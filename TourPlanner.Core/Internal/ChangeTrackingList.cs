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
        private record ItemEntry<T> { public ChangeState State; public T Item; };
        private readonly object _syncRoot = new();
        private readonly List<ItemEntry<T>> _items = new();

        private IEnumerable<T> CurrentItems
            => _items.Where(i => i.State != ChangeState.Removed).Select(i => i.Item);

        private int InternalIndexOf(T item)
        {
            for (int i = 0; i < _items.Count; ++i)
            {
                if (Object.ReferenceEquals(_items[i].Item, item))
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
                rem = _items.Skip(count).Take(rem).Count(i => i.State == ChangeState.Removed);
                count += temp;
            } while (rem > 0);

            return count - 1;
        }

        public ChangeTrackingList()
        { }

        public ChangeTrackingList(IList<T> initialItems)
            => _items.AddRange(initialItems.Select(i => new ItemEntry<T> { State = ChangeState.Current, Item = i }));

        public T this[int index]
        {
            get => CurrentItems.ElementAt(index);
            set
            {
                RemoveAt(index);
                Insert(index, value);
            }
        }

        public IReadOnlyCollection<T> NewItems => _items.Where(i => i.State == ChangeState.New).Select(i => i.Item).ToArray();

        public IReadOnlyCollection<T> RemovedItems => _items.Where(i => i.State == ChangeState.Removed).Select(i => i.Item).ToArray();

        public IReadOnlyCollection<T> ChangedItems => _items.Where(i => i.State == ChangeState.Current && i.Item.IsChanged).Select(i => i.Item).ToArray();

        public int Count => _items.Count(i => i.State != ChangeState.Removed);

        public bool IsReadOnly => false;

        public bool IsChanged => _items.Any(i => i.State != ChangeState.Current || i.Item.IsChanged);

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
            var items = _items.Where(i => i.State != ChangeState.Removed).ToArray();
            _items.Clear();
            _items.AddRange(items);
            _items.ForEach(i => { i.State = ChangeState.Current; i.Item.AcceptChanges(); });
        }

        public void Add(T item)
        {
            if (_items.Find(i => i.Item == item) is var itemEntry and not null)
            {
                // undo removal and move to end
                if (itemEntry.State == ChangeState.Removed)
                {
                    itemEntry.State = ChangeState.Current;
                    _items.Remove(itemEntry);
                    _items.Add(itemEntry);
                    return;
                }

                // double entry
                throw new InvalidOperationException("This item has already been stored.");
            }

            _items.Add(new() { State = ChangeState.New, Item = item });
        }

        public void Clear()
            => _items.ForEach(i => i.State = ChangeState.Removed);

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
                if (_items[i].State != ChangeState.Removed)
                {
                    if (Object.ReferenceEquals(_items[i].Item, item))
                        return pi;

                    ++pi;
                }
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            var internalIndex = PublicIndexToInternalIndex(index);

            if (_items.Find(i => i.Item == item) is var itemEntry and not null)
            {
                // undo removal and move to new position
                if (itemEntry.State == ChangeState.Removed)
                {
                    itemEntry.State = ChangeState.Current;
                    var oldIndex = _items.IndexOf(itemEntry);
                    _items.Remove(itemEntry);
                    _items.Insert(internalIndex > oldIndex ? internalIndex - 1 : internalIndex, itemEntry);
                    return;
                }

                // double entry
                throw new InvalidOperationException("This item has already been stored.");
            }

            _items.Insert(internalIndex, new() { State = ChangeState.New, Item = item });
        }

        public bool Remove(T item)
        {
            var internalIndex = InternalIndexOf(item);

            if (internalIndex == -1 || _items[internalIndex].State == ChangeState.Removed)
                return false;

            // if not yet accepted
            if (_items[internalIndex].State == ChangeState.New)
            {
                _items.RemoveAt(internalIndex);
                return true;
            }

            _items[internalIndex].State = ChangeState.Removed;
            return true;
        }

        public void RemoveAt(int index)
        {
            var internalIndex = PublicIndexToInternalIndex(index);

            if (internalIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(index));

            // if not yet accepted
            if (_items[internalIndex].State == ChangeState.New)
            {
                _items.RemoveAt(internalIndex);
                return;
            }

            _items[internalIndex].State = ChangeState.Removed;
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
