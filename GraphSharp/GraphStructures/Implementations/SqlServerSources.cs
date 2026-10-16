using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace GraphSharp.Graphs
{
    // Simple thread-safe LRU cache that tracks approximate byte usage and evicts least-recently-used entries
    internal class LruCache<TKey, TValue>
        where TKey : notnull
    {
        private readonly object _lock = new();
        private readonly LinkedList<TKey> _list = new();
        private readonly Dictionary<TKey, (TValue Value, long Size, LinkedListNode<TKey> Node)> _map = new();
        private long _currentBytes;
        private readonly long _maxBytes; // <=0 => unlimited

        public LruCache(long maxBytes)
        {
            _maxBytes = maxBytes;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out var entry))
                {
                    // move to front
                    _list.Remove(entry.Node);
                    _list.AddFirst(entry.Node);
                    value = entry.Value;
                    return true;
                }
            }
            value = default!;
            return false;
        }

        // Add or replace a value with its size (in bytes). Caller must compute size.
        public void AddOrReplace(TKey key, TValue value, long size)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out var existing))
                {
                    // replace
                    _currentBytes -= existing.Size;
                    _list.Remove(existing.Node);
                }

                var node = new LinkedListNode<TKey>(key);
                _list.AddFirst(node);
                _map[key] = (value, size, node);
                _currentBytes += size;

                EvictIfNeeded();
            }
        }

        // Try remove a specific key
        public bool TryRemove(TKey key)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out var entry))
                {
                    _map.Remove(key);
                    _list.Remove(entry.Node);
                    _currentBytes -= entry.Size;
                    return true;
                }
                return false;
            }
        }

        // Remove least-recently-used until under limit
        private void EvictIfNeeded()
        {
            if (_maxBytes <= 0) return;
            while (_currentBytes > _maxBytes && _list.Count > 0)
            {
                var last = _list.Last!;
                var key = last.Value;
                var entry = _map[key];
                _map.Remove(key);
                _list.RemoveLast();
                _currentBytes -= entry.Size;
                // If TValue holds disposable resources, caller should handle them via stored value or mapping
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _map.Clear();
                _list.Clear();
                _currentBytes = 0;
            }
        }

        public long CurrentBytes
        {
            get { lock (_lock) { return _currentBytes; } }
        }
    }

    /// <summary>
    /// SQL Server-backed node source with custom mapping and LRU eviction.
    /// </summary>
    public class SqlServerNodeSource<TNode> : INodeSource<TNode>
        where TNode : INode
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly Func<IDataRecord, TNode> _nodeFactory;
        private readonly Action<TNode, IDataRecord>? _nodeMapper;
        private readonly Func<TNode, int> _sizeEstimator;
        private readonly LruCache<int, TNode>? _cache; // keyed by node id

        // defaults: approximate sizes (bytes). You should provide realistic estimators for your node type if accurate memory control is required.
        private const int DefaultNodeEstimate = 200;

        public SqlServerNodeSource(
            string connectionString,
            string tableName,
            Func<IDataRecord, TNode> nodeFactory,
            Action<TNode, IDataRecord>? nodeMapper = null,
            Func<TNode, int>? nodeSizeEstimator = null,
            long maxCacheBytes = 0 // 0 or negative => unlimited
        )
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
            _nodeMapper = nodeMapper;
            _sizeEstimator = nodeSizeEstimator ?? (_ => DefaultNodeEstimate);
            _cache = maxCacheBytes > 0 ? new LruCache<int, TNode>(maxCacheBytes) : null;
        }

        public TNode this[int nodeId]
        {
            get
            {
                if (_cache != null && _cache.TryGet(nodeId, out var cached))
                    return cached;

                var loaded = LoadNode(nodeId);
                if (loaded is null) throw new KeyNotFoundException($"Node {nodeId} not found in {_tableName}.");
                if (_cache != null) _cache.AddOrReplace(nodeId, loaded, _sizeEstimator(loaded));
                return loaded;
            }
            set => throw new NotSupportedException("SqlServerNodeSource is read-only.");
        }

        public int Count => QueryScalarInt($"SELECT COUNT(*) FROM [{_tableName}]");
        public int MaxNodeId => QueryScalarIntOrDefault($"SELECT MAX(Id) FROM [{_tableName}]");
        public int MinNodeId => QueryScalarIntOrDefault($"SELECT MIN(Id) FROM [{_tableName}]");
        public bool IsReadOnly => true;

        public void Add(TNode node) => throw new NotSupportedException("SqlServerNodeSource is read-only.");
        public bool Remove(TNode node) => throw new NotSupportedException("SqlServerNodeSource is read-only.");
        public bool Remove(int nodeId) => throw new NotSupportedException("SqlServerNodeSource is read-only.");
        public bool Move(TNode node, int newId) => throw new NotSupportedException("SqlServerNodeSource is read-only.");
        public bool Move(int nodeId, int newId) => throw new NotSupportedException("SqlServerNodeSource is read-only.");
        public void Clear() => throw new NotSupportedException("SqlServerNodeSource is read-only.");

        public IEnumerator<TNode> GetEnumerator()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM [{_tableName}]";
            using var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            while (rdr.Read())
            {
                // create and map
                var node = _nodeFactory(rdr);
                _nodeMapper?.Invoke(node, rdr);
                if (_cache != null) _cache.AddOrReplace(node.Id, node, _sizeEstimator(node));
                yield return node;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGetNode(int nodeId, out TNode? node)
        {
            if (_cache != null && _cache.TryGet(nodeId, out var cached))
            {
                node = cached;
                return true;
            }

            var loaded = LoadNode(nodeId);
            if (loaded is not null)
            {
                if (_cache != null) _cache.AddOrReplace(nodeId, loaded, _sizeEstimator(loaded));
                node = loaded;
                return true;
            }

            node = default;
            return false;
        }

        public bool Contains(int nodeId)
        {
            if (_cache != null && _cache.TryGet(nodeId, out _)) return true;
            var found = QueryScalarIntOrDefault($"SELECT TOP 1 1 FROM [{_tableName}] WHERE Id = @id", new SqlParameter("@id", SqlDbType.Int) { Value = nodeId });
            return found == 1;
        }

        public bool Contains(TNode node) => Contains(node.Id);

        public void CopyTo(TNode[] array, int arrayIndex)
        {
            foreach (var n in this) array[arrayIndex++] = n;
        }

        private TNode? LoadNode(int nodeId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM [{_tableName}] WHERE Id = @id";
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = nodeId });
            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read()) return default;
            var node = _nodeFactory(rdr);
            _nodeMapper?.Invoke(node, rdr);
            return node;
        }

        private int QueryScalarInt(string sql)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var val = cmd.ExecuteScalar();
            return Convert.ToInt32(val);
        }

        private int QueryScalarIntOrDefault(string sql, SqlParameter? param = null)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (param != null) cmd.Parameters.Add(param);
            var val = cmd.ExecuteScalar();
            return val == DBNull.Value || val == null ? -1 : Convert.ToInt32(val);
        }
    }

    /// <summary>
    /// SQL Server-backed edge source with custom mapping and LRU eviction.
    /// Each node cache entry holds outEdges and inEdges sets for that node. Eviction removes least-recently-used node entries.
    /// </summary>
    public class SqlServerEdgeSource<TEdge> : BaseEdgeSource<TEdge>, IEdgeSource<TEdge>
        where TEdge : IEdge
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly Func<IDataRecord, TEdge> _edgeFactory;
        private readonly Action<TEdge, IDataRecord>? _edgeMapper;
        private readonly Func<TEdge, int> _edgeSizeEstimator;
        private readonly LruCache<int, (HashSet<TEdge> Out, HashSet<TEdge> In)>? _cache;

        private const int DefaultEdgeEstimate = 64;

        public SqlServerEdgeSource(
            string connectionString,
            string tableName,
            Func<IDataRecord, TEdge> edgeFactory,
            Action<TEdge, IDataRecord>? edgeMapper = null,
            Func<TEdge, int>? edgeSizeEstimator = null,
            long maxCacheBytes = 0)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _edgeFactory = edgeFactory ?? throw new ArgumentNullException(nameof(edgeFactory));
            _edgeMapper = edgeMapper;
            _edgeSizeEstimator = edgeSizeEstimator ?? (_ => DefaultEdgeEstimate);
            _cache = maxCacheBytes > 0 ? new LruCache<int, (HashSet<TEdge> Out, HashSet<TEdge> In)>(maxCacheBytes) : null;
            AllowParallelEdges = true;
        }

        public override IEnumerable<TEdge> OutEdges(int sourceId)
        {
            if (_cache != null && _cache.TryGet(sourceId, out var holder))
                return holder.Out;

            var outSet = LoadOutEdges(sourceId);
            if (_cache != null)
            {
                var size = outSet.Sum(e => _edgeSizeEstimator(e));
                // also include current In set if exists (0 here)
                _cache.AddOrReplace(sourceId, (outSet, new HashSet<TEdge>()), size);
            }
            return outSet;
        }

        public override IEnumerable<TEdge> InEdges(int targetId)
        {
            if (_cache != null && _cache.TryGet(targetId, out var holder))
                return holder.In;

            var inSet = LoadInEdges(targetId);
            if (_cache != null)
            {
                var size = inSet.Sum(e => _edgeSizeEstimator(e));
                _cache.AddOrReplace(targetId, (new HashSet<TEdge>(), inSet), size);
            }
            return inSet;
        }

        public override (IEnumerable<TEdge> OutEdges, IEnumerable<TEdge> InEdges) BothEdges(int nodeId)
        {
            // ensure both loaded
            var outE = OutEdges(nodeId);
            var inE = InEdges(nodeId);
            return (outE, inE);
        }

        public override IEnumerable<TEdge> InOutEdges(int nodeId)
            => OutEdges(nodeId).Concat(InEdges(nodeId)).Distinct();

        public override IEnumerator<TEdge> GetEnumerator()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM [{_tableName}]";
            using var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            while (rdr.Read())
            {
                var e = _edgeFactory(rdr);
                _edgeMapper?.Invoke(e, rdr);
                // Register in caches for source and target
                RegisterEdgeInCaches(e);
                yield return e;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public new bool IsReadOnly => true;
        public new int Count => QueryScalarInt($"SELECT COUNT(*) FROM [{_tableName}]");

        public void Trim()
        {
            if (_cache == null) return;
            // remove holders that are empty
            // LruCache does not expose keys; we rely on eviction by size.
        }

        public void Add(TEdge edge) => throw new NotSupportedException("SqlServerEdgeSource is read-only.");
        public bool Remove(TEdge edge) => throw new NotSupportedException("SqlServerEdgeSource is read-only.");
        public bool Remove(int sourceId, int targetId) => throw new NotSupportedException("SqlServerEdgeSource is read-only.");
        public void Clear() => throw new NotSupportedException("SqlServerEdgeSource is read-only.");

        public bool Contains(TEdge item)
        {
            // Check cache first
            if (_cache != null && _cache.TryGet(item.SourceId, out var holder))
                return holder.Out.Contains(item);
            // fallback to DB
            var p1 = new SqlParameter("@s", SqlDbType.Int) { Value = item.SourceId };
            var p2 = new SqlParameter("@t", SqlDbType.Int) { Value = item.TargetId };
            var found = QueryScalarIntOrDefault($"SELECT TOP 1 1 FROM [{_tableName}] WHERE SourceId = @s AND TargetId = @t", p1, p2);
            return found == 1;
        }

        public void CopyTo(TEdge[] array, int arrayIndex)
        {
            foreach (var e in this) array[arrayIndex++] = e;
        }

        // Loads out edges from DB and registers them into caches (source and corresponding target holders)
        private HashSet<TEdge> LoadOutEdges(int sourceId)
        {
            var set = new HashSet<TEdge>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM [{_tableName}] WHERE SourceId = @id";
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = sourceId });

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var e = _edgeFactory(rdr);
                _edgeMapper?.Invoke(e, rdr);
                set.Add(e);
                // also register in target's In set
                RegisterEdgeInTargetCache(e);
            }
            return set;
        }

        private HashSet<TEdge> LoadInEdges(int targetId)
        {
            var set = new HashSet<TEdge>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM [{_tableName}] WHERE TargetId = @id";
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = targetId });

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var e = _edgeFactory(rdr);
                _edgeMapper?.Invoke(e, rdr);
                set.Add(e);
                RegisterEdgeInSourceCache(e);
            }
            return set;
        }

        // Register edge into both source and target caches, keeping cache sizes updated.
        private void RegisterEdgeInCaches(TEdge e)
        {
            // Source: add to Out
            RegisterEdgeInSourceCache(e);
            // Target: add to In
            RegisterEdgeInTargetCache(e);
        }

        private void RegisterEdgeInSourceCache(TEdge e)
        {
            if (_cache == null) return;
            // Try get existing holder
            if (_cache.TryGet(e.SourceId, out var existing))
            {
                // create new holder with updated Out set
                var newOut = new HashSet<TEdge>(existing.Out) { e };
                var newIn = existing.In;
                var newSize = newOut.Sum(x => _edgeSizeEstimator(x)) + newIn.Sum(x => _edgeSizeEstimator(x));
                _cache.AddOrReplace(e.SourceId, (newOut, newIn), newSize);
            }
            else
            {
                var outSet = new HashSet<TEdge> { e };
                var inSet = new HashSet<TEdge>();
                var size = outSet.Sum(x => _edgeSizeEstimator(x));
                _cache.AddOrReplace(e.SourceId, (outSet, inSet), size);
            }
        }

        private void RegisterEdgeInTargetCache(TEdge e)
        {
            if (_cache == null) return;
            if (_cache.TryGet(e.TargetId, out var existing))
            {
                var newOut = existing.Out;
                var newIn = new HashSet<TEdge>(existing.In) { e };
                var newSize = newOut.Sum(x => _edgeSizeEstimator(x)) + newIn.Sum(x => _edgeSizeEstimator(x));
                _cache.AddOrReplace(e.TargetId, (newOut, newIn), newSize);
            }
            else
            {
                var outSet = new HashSet<TEdge>();
                var inSet = new HashSet<TEdge> { e };
                var size = inSet.Sum(x => _edgeSizeEstimator(x));
                _cache.AddOrReplace(e.TargetId, (outSet, inSet), size);
            }
        }

        private int QueryScalarInt(string sql)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var val = cmd.ExecuteScalar();
            return Convert.ToInt32(val);
        }

        private int QueryScalarIntOrDefault(string sql, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            foreach (var p in parameters) cmd.Parameters.Add(p);
            var val = cmd.ExecuteScalar();
            return val == DBNull.Value || val == null ? -1 : Convert.ToInt32(val);
        }
    }

    /// <summary>
    /// Helper extensions for IDataRecord
    /// </summary>
    public static class DataRecordExtensions
    {
        public static int GetOrdinalSafe(this IDataRecord rdr, string name)
        {
            try { return rdr.GetOrdinal(name); }
            catch (IndexOutOfRangeException) { return -1; }
        }

        public static bool HasColumn(this IDataRecord rdr, string name)
        {
            return rdr.GetOrdinalSafe(name) >= 0;
        }
    }
}
