using NUnit.Framework;
using System;
using TourPlanner.Core.Internal;
using TourPlanner.Core.Models;

namespace TourPlanner.Tests
{
    public class ChangeTrackingListTests
    {
        private ChangeTrackingList<Tour> _list;
        private Tour _item1, _item2, _item3;

        [OneTimeSetUp]
        public void Setup()
        {
            _list = new ChangeTrackingList<Tour>();
            _item1 = new Tour();
            _item2 = new Tour();
            _item3 = new Tour();
        }

        [Test, Order(1)]
        public void AddFirstItem()
        {
            _list.Add(_item1);

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(1, _list.Count);
            Assert.AreEqual(1, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(0, _list.RemovedItems.Count);
        }

        [Test, Order(2)]
        public void AcceptFirstItem()
        {
            _list.AcceptChanges();

            Assert.IsFalse(_list.IsChanged);
            Assert.AreEqual(1, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(0, _list.RemovedItems.Count);
        }

        [Test, Order(3)]
        public void AddSecondItem()
        {
            _list.Add(_item2);

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(2, _list.Count);
            Assert.AreEqual(1, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(0, _list.RemovedItems.Count);
        }

        [Test, Order(4)]
        public void RemoveSecondItem()
        {
            _list.Remove(_item2);

            Assert.IsFalse(_list.IsChanged);
            Assert.AreEqual(1, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(0, _list.RemovedItems.Count);
        }

        [Test, Order(5)]
        public void RemoveFirstItem()
        {
            _list.Remove(_item1);

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(0, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(1, _list.RemovedItems.Count);
        }

        [Test, Order(6)]
        public void ClearUncommittedRemovedItem()
        {
            _list.Clear();

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(0, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(1, _list.RemovedItems.Count);
        }

        [Test, Order(7)]
        public void ReAddRemovedItem()
        {
            _list.Add(_item1);

            Assert.IsFalse(_list.IsChanged);
            Assert.AreEqual(1, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(0, _list.RemovedItems.Count);
        }

        [Test, Order(8)]
        public void ClearCommittedItems()
        {
            _list.Add(_item2);
            _list.Add(_item3);
            _list.AcceptChanges();
            _list.Clear();

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(0, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(3, _list.RemovedItems.Count);
        }

        [Test, Order(9)]
        public void ChangeOnCurrentItem()
        {
            _list.Add(_item1);
            _list.AcceptChanges();
            _item1.CustomDescription = "changed";

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(1, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(1, _list.ChangedItems.Count);
            Assert.AreEqual(0, _list.RemovedItems.Count);
        }

        [Test, Order(10)]
        public void ChangeOnRemovedItem()
        {
            _list.Remove(_item1);

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(0, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(1, _list.RemovedItems.Count);
        }
    }
}
