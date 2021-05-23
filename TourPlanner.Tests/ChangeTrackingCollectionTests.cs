using NUnit.Framework;
using System;
using TourPlanner.Core.Internal;
using TourPlanner.Core.Models;

namespace TourPlanner.Tests
{
    public class ChangeTrackingCollectionTests
    {
        private ChangeTrackingCollection<Tour> _collection;
        private Tour _item1, _item2, _item3;

        [OneTimeSetUp]
        public void Setup()
        {
            _collection = new ChangeTrackingCollection<Tour>();
            _item1 = new Tour();
            _item2 = new Tour();
            _item3 = new Tour();
        }

        [Test, Order(1)]
        public void AddFirstItem()
        {
            _collection.Add(_item1);

            Assert.IsTrue(_collection.IsChanged);
            Assert.AreEqual(1, _collection.Count);
            Assert.AreEqual(1, _collection.NewItems.Count);
            Assert.AreEqual(0, _collection.ChangedItems.Count);
            Assert.AreEqual(0, _collection.RemovedItems.Count);
        }

        [Test, Order(2)]
        public void AcceptFirstItem()
        {
            _collection.AcceptChanges();

            Assert.IsFalse(_collection.IsChanged);
            Assert.AreEqual(1, _collection.Count);
            Assert.AreEqual(0, _collection.NewItems.Count);
            Assert.AreEqual(0, _collection.ChangedItems.Count);
            Assert.AreEqual(0, _collection.RemovedItems.Count);
        }

        [Test, Order(3)]
        public void AddSecondItem()
        {
            _collection.Add(_item2);

            Assert.IsTrue(_collection.IsChanged);
            Assert.AreEqual(2, _collection.Count);
            Assert.AreEqual(1, _collection.NewItems.Count);
            Assert.AreEqual(0, _collection.ChangedItems.Count);
            Assert.AreEqual(0, _collection.RemovedItems.Count);
        }

        [Test, Order(4)]
        public void RemoveSecondItem()
        {
            _collection.Remove(_item2);

            Assert.IsFalse(_collection.IsChanged);
            Assert.AreEqual(1, _collection.Count);
            Assert.AreEqual(0, _collection.NewItems.Count);
            Assert.AreEqual(0, _collection.ChangedItems.Count);
            Assert.AreEqual(0, _collection.RemovedItems.Count);
        }

        [Test, Order(5)]
        public void RemoveFirstItem()
        {
            _collection.Remove(_item1);

            Assert.IsTrue(_collection.IsChanged);
            Assert.AreEqual(0, _collection.Count);
            Assert.AreEqual(0, _collection.NewItems.Count);
            Assert.AreEqual(0, _collection.ChangedItems.Count);
            Assert.AreEqual(1, _collection.RemovedItems.Count);
        }

        [Test, Order(6)]
        public void ClearUncommittedRemovedItem()
        {
            _collection.Clear();

            Assert.IsTrue(_collection.IsChanged);
            Assert.AreEqual(0, _collection.Count);
            Assert.AreEqual(0, _collection.NewItems.Count);
            Assert.AreEqual(0, _collection.ChangedItems.Count);
            Assert.AreEqual(1, _collection.RemovedItems.Count);
        }

        [Test, Order(7)]
        public void ReAddRemovedItem()
        {
            _collection.Add(_item1);

            Assert.IsFalse(_collection.IsChanged);
            Assert.AreEqual(1, _collection.Count);
            Assert.AreEqual(0, _collection.NewItems.Count);
            Assert.AreEqual(0, _collection.ChangedItems.Count);
            Assert.AreEqual(0, _collection.RemovedItems.Count);
        }

        [Test, Order(8)]
        public void ClearCommittedItems()
        {
            _collection.Add(_item2);
            _collection.Add(_item3);
            _collection.AcceptChanges();
            _collection.Clear();

            Assert.IsTrue(_collection.IsChanged);
            Assert.AreEqual(0, _collection.Count);
            Assert.AreEqual(0, _collection.NewItems.Count);
            Assert.AreEqual(0, _collection.ChangedItems.Count);
            Assert.AreEqual(3, _collection.RemovedItems.Count);
        }

        [Test, Order(9)]
        public void ChangeOnCurrentItem()
        {
            _collection.Add(_item1);
            _collection.AcceptChanges();
            _item1.CustomDescription = "changed";

            Assert.IsTrue(_collection.IsChanged);
            Assert.AreEqual(1, _collection.Count);
            Assert.AreEqual(0, _collection.NewItems.Count);
            Assert.AreEqual(1, _collection.ChangedItems.Count);
            Assert.AreEqual(0, _collection.RemovedItems.Count);
        }

        [Test, Order(10)]
        public void ChangeOnRemovedItem()
        {
            _collection.Remove(_item1);

            Assert.IsTrue(_collection.IsChanged);
            Assert.AreEqual(0, _collection.Count);
            Assert.AreEqual(0, _collection.NewItems.Count);
            Assert.AreEqual(0, _collection.ChangedItems.Count);
            Assert.AreEqual(1, _collection.RemovedItems.Count);
        }
    }
}
