using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TourPlanner.Core.Interfaces;

namespace TourPlanner.Core.Internal
{
    internal class ChangeTrackingList<T> : IChangeTrackingList<T> where T : class, IChangeTracking
    {
        private readonly object _syncRoot = new();
        private readonly List<(ChangeState state, T item)> _items = new();

        private IEnumerable<T> CurrentItems
            => _items.Where(i => i.state != ChangeState.Removed).Select(i => i.item);

        private int InternalIndexOf(T item)
        {
            for (int i = 0; i < _items.Count; ++i)
            {
                if (Object.ReferenceEquals(_items[i].item, item))
                    return i;
            }

            return -1;
        }

        private int PublicIndexToInternalIndex(int publicIndex)
        {
            int count = 0;
            int rem = publicIndex + 1;
            do
            {
                int temp = rem;
                rem = _items.Skip(count).Take(rem).Count(i => i.state == ChangeState.Removed);
                count += temp;
            } while (rem > 0);

            return count - 1;
        }

        public ChangeTrackingList()
        { }

        public ChangeTrackingList(IList<T> initialItems)
            => _items.AddRange(initialItems.Select(i => (ChangeState.Current, i)));

        public T this[int index]
        {
            get => CurrentItems.ElementAt(index);
            set
            {
                RemoveAt(index);
                Insert(index, value);
            }
        }

        public IReadOnlyCollection<T> NewItems => _items.Where(i => i.state == ChangeState.New).Select(i => i.item).ToArray();

        public IReadOnlyCollection<T> RemovedItems => _items.Where(i => i.state == ChangeState.Removed).Select(i => i.item).ToArray();

        public IReadOnlyCollection<T> ChangedItems => _items.Where(i => i.item.IsChanged).Select(i => i.item).ToArray();

        public int Count => _items.Count(i => i.state != ChangeState.Removed);

        public bool IsReadOnly => false;

        public bool IsChanged => _items.Any(i => i.state != ChangeState.Current || i.item.IsChanged);

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => _syncRoot;

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is not T item)
                    throw new ArgumentException("Given argument is of wrong type, expected " + typeof(T).Name + " got " + value?.GetType().Name, nameof(value));

                this[index] = item;
            }
        }

        public void AcceptChanges()
        {
            var items = CurrentItems.Select(i => (ChangeState.Current, i)).ToArray();
            _items.Clear();
            _items.AddRange(items);
            _items.ForEach(i => i.item.AcceptChanges());
        }

        public void Add(T item)
            => _items.Add((ChangeState.New, item));

        public void Clear()
            => _items.ForEach(i => i.state = ChangeState.Removed);

        public bool Contains(T item)
            => CurrentItems.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
            => CurrentItems.ToArray().CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator()
            => CurrentItems.GetEnumerator();

        public int IndexOf(T item)
        {
            for (int i = 0, pi = 0; i < _items.Count; ++i)
            {
                if (_items[i].state != ChangeState.Removed)
                {
                    if (Object.ReferenceEquals(_items[i].item, item))
                        return pi;

                    ++pi;
                }
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            var internalIndex = PublicIndexToInternalIndex(index);
            _items.Insert(internalIndex, (ChangeState.New, item));
        }

        public bool Remove(T item)
        {
            var internalIndex = InternalIndexOf(item);

            if (internalIndex == -1 || _items[internalIndex].state == ChangeState.Removed)
                return false;

            // if not yet accepted
            if (_items[internalIndex].state == ChangeState.New)
            {
                _items.RemoveAt(internalIndex);
                return true;
            }

            _items[internalIndex] = (ChangeState.Removed, _items[internalIndex].item);
            return true;
        }

        public void RemoveAt(int index)
        {
            var internalIndex = PublicIndexToInternalIndex(index);

            if (internalIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(index));

            // if not yet accepted
            if (_items[internalIndex].state == ChangeState.New)
            {
                _items.RemoveAt(internalIndex);
                return;
            }

            _items[internalIndex] = (ChangeState.Removed, _items[internalIndex].item);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        int IList.Add(object value)
        {
            if (value is null)
                Add(null);

            Add(value as T ?? throw new ArgumentException("Given argument is of wrong type, expected " + typeof(T).Name + " got " + value?.GetType().Name, nameof(value)));
            return IndexOf((T)value);
        }

        bool IList.Contains(object value)
        {
            if (value is null)
                return Contains(null);

            return Contains(value as T ?? throw new ArgumentException("Given argument is of wrong type, expected " + typeof(T).Name + " got " + value?.GetType().Name, nameof(value)));
        }

        int IList.IndexOf(object value)
        {
            if (value is null)
                return IndexOf(null);

            return IndexOf(value as T ?? throw new ArgumentException("Given argument is of wrong type, expected " + typeof(T).Name + " got " + value?.GetType().Name, nameof(value)));
        }

        void IList.Insert(int index, object value)
        {
            if (value is null)
            {
                Insert(index, null);
                return;
            }

            Insert(index, value as T ?? throw new ArgumentException("Given argument is of wrong type, expected " + typeof(T).Name + " got " + value?.GetType().Name, nameof(value)));
        }

        void IList.Remove(object value)
        {
            if (value is null)
            {
                Remove(null);
                return;
            }

            Remove(value as T ?? throw new ArgumentException("Given argument is of wrong type, expected " + typeof(T).Name + " got " + value?.GetType().Name, nameof(value)));
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));

            CopyTo(array as T[] ?? throw new ArgumentException("Given argument is of wrong type, expected " + typeof(T).Name + " got " + array?.GetType().Name, nameof(array)), index);
        }
    }
}
