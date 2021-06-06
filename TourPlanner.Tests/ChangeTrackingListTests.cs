using NUnit.Framework;
using System;
using System.Linq;
using TourPlanner.Core.Internal;
using TourPlanner.Core.Models;

namespace TourPlanner.Tests
{
    public class ChangeTrackingListTests
    {
        private ChangeTrackingList<Tour> _list;
        private Tour _item1, _item2, _item3, _item4;

        [OneTimeSetUp]
        public void Setup()
        {
            _list = new ChangeTrackingList<Tour>();
            _item1 = new Tour();
            _item2 = new Tour();
            _item3 = new Tour();
            _item4 = new Tour();
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

        [Test, Order(11)]
        public void InsertAtFirstPositionWhenEmpty()
        {
            _list.Insert(0, _item2);

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(1, _list.Count);
            Assert.AreEqual(1, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(1, _list.RemovedItems.Count);
            Assert.AreEqual(_item2, _list[0]);
        }

        [Test, Order(12)]
        public void InsertAtLastPosition()
        {
            _list.Insert(1, _item3);

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(2, _list.Count);
            Assert.AreEqual(2, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(1, _list.RemovedItems.Count);
            Assert.AreEqual(_item2, _list[0]);
            Assert.AreEqual(_item3, _list[1]);
        }

        [Test, Order(13)]
        public void InsertAtFirstPosition()
        {
            _list.Insert(0, _item4);

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(3, _list.Count);
            Assert.AreEqual(3, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(1, _list.RemovedItems.Count);
            Assert.AreEqual(_item2, _list[1]);
            Assert.AreEqual(_item3, _list[2]);
            Assert.AreEqual(_item4, _list[0]);
            Assert.IsFalse(_list.Contains(_item1));
            Assert.IsTrue(_list.Contains(_item2));
            Assert.IsTrue(_list.Contains(_item3));
            Assert.IsTrue(_list.Contains(_item4));
        }

        [Test, Order(14)]
        public void InsertRemovedItems()
        {
            // before: i4, i2, i3; (i1)
            _list.Remove(_item2);
            _list.Insert(1, _item1);
            _list.Insert(0, _item2);
            // after: i2, i4, i1, i3

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(4, _list.Count);
            Assert.AreEqual(3, _list.NewItems.Count);
            Assert.AreEqual(1, _list.ChangedItems.Count);
            Assert.AreEqual(0, _list.RemovedItems.Count);
            Assert.AreEqual(_item1, _list[2]);
            Assert.AreEqual(_item2, _list[0]);
            Assert.AreEqual(_item3, _list[3]);
            Assert.AreEqual(_item4, _list[1]);
        }

        [Test, Order(15)]
        public void RemoveItemsAtLocations()
        {
            _list.RemoveAt(1);
            _list.RemoveAt(1);

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(2, _list.Count);
            Assert.AreEqual(2, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(1, _list.RemovedItems.Count);
            Assert.AreEqual(_item2, _list[0]);
            Assert.AreEqual(_item3, _list[1]);
            Assert.IsFalse(_list.Contains(_item1));
            Assert.IsFalse(_list.Contains(_item4));
        }

        [Test, Order(16)]
        public void ReAddRemovedItemAndCheckPosition()
        {
            _list.Add(_item4);
            _list.Add(_item1);

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(4, _list.Count);
            Assert.AreEqual(3, _list.NewItems.Count);
            Assert.AreEqual(1, _list.ChangedItems.Count);
            Assert.AreEqual(0, _list.RemovedItems.Count);
            Assert.AreEqual(_item1, _list[3]);
            Assert.AreEqual(_item2, _list[0]);
            Assert.AreEqual(_item3, _list[1]);
            Assert.AreEqual(_item4, _list[2]);
            Assert.IsTrue(_list.Contains(_item1));
            Assert.IsTrue(_list.Contains(_item2));
            Assert.IsTrue(_list.Contains(_item3));
            Assert.IsTrue(_list.Contains(_item4));
        }

        [Test, Order(17)]
        public void AcceptAllItems()
        {
            _list.AcceptChanges();

            Assert.IsFalse(_list.IsChanged);
            Assert.AreEqual(4, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(0, _list.RemovedItems.Count);
            Assert.IsTrue(_list.All(i => !i.IsChanged));
        }

        [Test, Order(18)]
        public void RemoveRemovedItem()
        {
            _list.Remove(_item2);

            Assert.IsFalse(_list.Remove(_item2));
        }

        [Test, Order(19)]
        public void RemoveOutOfBoundsItem()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _list.RemoveAt(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => _list.RemoveAt(-1));
        }

        [Test, Order(20)]
        public void ReplaceItemViaIndex()
        {
            _list[1] = _item2;

            Assert.IsTrue(_list.IsChanged);
            Assert.AreEqual(3, _list.Count);
            Assert.AreEqual(0, _list.NewItems.Count);
            Assert.AreEqual(0, _list.ChangedItems.Count);
            Assert.AreEqual(1, _list.RemovedItems.Count);
            Assert.AreEqual(_item1, _list[2]);
            Assert.AreEqual(_item2, _list[1]);
            Assert.AreEqual(_item3, _list[0]);
            Assert.IsTrue(_list.Contains(_item1));
            Assert.IsTrue(_list.Contains(_item2));
            Assert.IsTrue(_list.Contains(_item3));
            Assert.IsFalse(_list.Contains(_item4));
        }
    }
}
