using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace TourPlanner.Core.Interfaces
{
    public interface IChangeTrackingList<T> : IList<T>, IList, IChangeTrackingCollection<T> where T : IChangeTracking
    {
    }
}
