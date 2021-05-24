using System;

namespace TourPlanner.Core.Models
{
    /// <summary>
    /// A step denotes one directional instruction.
    /// </summary>
    public record Step
    {
        /// <summary>
        /// Length of the step.
        /// Equal to distance to next step.
        /// </summary>
        public double Distance { get; init; }

        /// <summary>
        /// Text for the current step.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Path to an icon representing this step.
        /// </summary>
        public string IconPath { get; init; }
    }
}
