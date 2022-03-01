using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphSharp.Extensions
{
    public class ConvertableList<T, TBase> : IList<TBase>
    where T : TBase
    {
        protected IList<T> Source{get;}

        public int Count => Source.Count;

        public bool IsReadOnly => Source.IsReadOnly;

        int ICollection<TBase>.Count => Source.Count;

        bool ICollection<TBase>.IsReadOnly => Source.IsReadOnly;
        public TBase this[int index] { 
            get => Source[index];
            set {
                if(value is T t)
                Source[index] = t;
            }
        }

        public ConvertableList(IList<T> source)
        {
            Source = source;
        }

       
        IEnumerator<TBase> IEnumerable<TBase>.GetEnumerator()
        {
            int index = 0;
            while(index<this.Count)
                yield return Source[index++];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Source).GetEnumerator();
        }

        int IList<TBase>.IndexOf(TBase item)
        {
            throw new NotImplementedException();
        }

        void IList<TBase>.Insert(int index, TBase item)
        {
            throw new NotImplementedException();
        }

        void IList<TBase>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection<TBase>.Add(TBase item)
        {
            throw new NotImplementedException();
        }

        void ICollection<TBase>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<TBase>.Contains(TBase item)
        {
            throw new NotImplementedException();
        }

        void ICollection<TBase>.CopyTo(TBase[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<TBase>.Remove(TBase item)
        {
            throw new NotImplementedException();
        }

    }
}