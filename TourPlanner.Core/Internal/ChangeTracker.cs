using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TourPlanner.Core.Internal
{
    internal class ChangeTracker : IChangeTracking
    {
        private bool _changed = false;

        public bool IsChanged => _changed;

        public void AcceptChanges()
            => _changed = false;

        /// <summary>
        /// Checks if a property already matches a desired value.
        /// Sets the property and sets <see cref="IsChanged"/> only if necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to the property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <returns>Whether or not the value was changed.</returns>
        public bool SetProperty<T>(ref T storage, T value)
        {
            // no change if same value
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            // update and note
            storage = value;
            _changed = true;

            return true;
        }
    }
}
