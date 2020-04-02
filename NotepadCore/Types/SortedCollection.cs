using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NotepadCore.Types
{
    class SortedCollection<T> : IEnumerable<T>
    {
        private List<T> _collection;
        public Func<T, IComparable> Selector { get; set; }

        public int Count => _collection.Count;

        public SortedCollection(Func<T, IComparable> selector)
        {
            Selector = selector;
            _collection = new List<T>();
        }

        public SortedCollection(IEnumerable<T> collection, Func<T, IComparable> selector) : this(selector)
        {
            _collection = collection.ToList();
            _collection.OrderBy(Selector);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            int index = IndexOf(item);
            if (index == -1)
                _collection.Add(item);
            else
                _collection.Insert(index, item);
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Selector(this[i]).CompareTo(Selector(item)) > 0)
                    return i;
            }
            return -1;
        }

        public T this[int index] => _collection[index];
    }
}