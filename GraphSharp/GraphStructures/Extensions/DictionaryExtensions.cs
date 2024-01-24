using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GraphSharp.Extensions;
/// <summary>
/// Dictionary extensions
/// </summary>
public static class DictionaryExtensions
{
    /// <returns>Found value in dictionary, or default value if key not found</returns>
    public static TValue? GetOrDefault<TValue,TKey>(this IDictionary<TKey,TValue> dict, TKey key){
        if(dict.TryGetValue(key,out var result)){
            return result;
        }
        return default;
    }
    /// <returns>Found value in dictionary, or default value if key not found</returns>
    public static TValue GetOrDefault<TValue,TKey>(this IDictionary<TKey,TValue> dict, TKey key, TValue defaultV){
        if(dict.TryGetValue(key,out var result)){
            return result;
        }
        return defaultV;
    }
    /// <summary>
    /// Tries to make deep copy of dictionary
    /// </summary>
    public static ConcurrentDictionary<TKey,TValue> CloneConcurrent<TValue,TKey>(this IDictionary<TKey,TValue> dict)
    where TKey : notnull
    {
        var result = new ConcurrentDictionary<TKey,TValue>();
        foreach(var pair in dict){
            var key = pair.Key is ICloneable cKey ? cKey.Clone() : pair.Key;
            var value = pair.Value is ICloneable cValue ? cValue.Clone() : pair.Key;
            if(key is TKey k && value is TValue v){
                result[k] = v;
            }
            else{
                result[pair.Key] = pair.Value;
            }
        }
        return result;
    }
    /// <summary>
    /// Tries to make deep copy of dictionary
    /// </summary>
    public static Dictionary<TKey,TValue> Clone<TValue,TKey>(this IDictionary<TKey,TValue> dict)
    where TKey : notnull
    {
        var result = new Dictionary<TKey,TValue>();
        foreach(var pair in dict){
            var key = pair.Key is ICloneable cKey ? cKey.Clone() : pair.Key;
            var value = pair.Value is ICloneable cValue ? cValue.Clone() : pair.Key;
            if(key is TKey k && value is TValue v){
                result[k] = v;
            }
            else{
                result[pair.Key] = pair.Value;
            }
        }
        return result;
    }
}