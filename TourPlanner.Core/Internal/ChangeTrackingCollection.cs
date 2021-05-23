using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TourPlanner.Core.Interfaces;

namespace TourPlanner.Core.Internal
{
    /// <summary>
    /// A collection that automatically keeps track of changed items.
    /// It is assumed that every object is unique.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    internal class ChangeTrackingCollection<T> : IChangeTrackingCollection<T> where T : IChangeTracking
    {
        private readonly List<T>
            _currentItems = new(),
            _newItems = new(),
            _removedItems = new();

        public IReadOnlyCollection<T> NewItems => _newItems;

        public IReadOnlyCollection<T> RemovedItems => _removedItems;

        public IReadOnlyCollection<T> ChangedItems => _currentItems.Except(_removedItems).Where(i => i.IsChanged).ToArray();

        public int Count => _currentItems.Count + _newItems.Count - _removedItems.Count;

        public bool IsReadOnly => false;

        public bool IsChanged => _newItems.Any() || _removedItems.Any() || _currentItems.Any(i => i.IsChanged); // no need to check new items for changes

        public ChangeTrackingCollection()
        { }

        public ChangeTrackingCollection(ICollection<T> initialItems)
            => _currentItems.AddRange(initialItems);

        public void Add(T item)
        {
            if (_removedItems.Remove(item))
                return;

            if (_newItems.Contains(item) || _currentItems.Contains(item))
                throw new InvalidOperationException("This item has already been stored.");

            _newItems.Add(item);
        }

        public void AddRange(ICollection<T> items)
            => _newItems.AddRange(items);

        public void Clear()
        {
            _newItems.Clear();
            _removedItems.Clear();
            _removedItems.AddRange(_currentItems);
        }

        public bool Contains(T item)
            => _newItems.Contains(item) || (_currentItems.Contains(item) && !_removedItems.Contains(item));

        public void CopyTo(T[] array, int arrayIndex)
        {
            _currentItems.Except(_removedItems).ToArray().CopyTo(array, arrayIndex);
            _newItems.CopyTo(array, _currentItems.Count - _removedItems.Count);
        }

        public IEnumerator<T> GetEnumerator()
            => _currentItems.Except(_removedItems).Concat(_newItems).GetEnumerator();

        public bool Remove(T item)
        {
            // not yet accepted, in new
            if (_newItems.Remove(item))
                return true;

            // accepted
            if (_currentItems.Contains(item) && !_removedItems.Contains(item))
            {
                _removedItems.Add(item);
                return true;
            }

            // not yet accepted, already removed
            return _removedItems.Contains(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void AcceptChanges()
        {
            foreach (var item in _removedItems)
                _currentItems.Remove(item);

            _currentItems.AddRange(_newItems);
            _removedItems.Clear();
            _newItems.Clear();

            foreach (var item in _currentItems)
                item.AcceptChanges();
        }
    }
}
