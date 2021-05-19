using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TourPlanner.Core.Interfaces
{
    public interface IChangeTrackingCollection<T> : ICollection<T>, IChangeTracking where T : IChangeTracking
    {
        /// <summary>
        /// A collection of all added items.
        /// </summary>
        public IReadOnlyCollection<T> NewItems { get; }

        /// <summary>
        /// A collection of all removed items.
        /// </summary>
        public IReadOnlyCollection<T> RemovedItems { get; }

        /// <summary>
        /// A collection of all modified items.
        /// </summary>
        public IReadOnlyCollection<T> ChangedItems { get; }
    }
}
