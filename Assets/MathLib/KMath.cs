using System;
using UnityEngine;

namespace Khaled.MathLib
{
    /// <summary>
    /// A static class that provides mathematical utility functions for various calculations focused on Unity.
    /// </summary>
    public static class KMath
    {
        /// <summary>
        /// Calculates the normalized value of a float within a specified range.
        /// </summary>
        /// <param name="value">The value to normalize.</param>
        /// <param name="min">The minimum range.</param>
        /// <param name="max">The maximum range.</param>
        /// <returns>The Normalized float value.</returns>
        public static float Normalize(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        /// <summary>
        /// Calculates the absolute value of a float. Basically removes the sign of the value.
        /// </summary>
        /// <param name="value">the value to return the absolute value of</param>
        /// <returns>The absolute result of given value.</returns>
        public static float Absolute(float value)
        {
            return (value >= 0) ? value : -value;
        }

        /// <summary>
        /// Checks if the given value is between two bounds.
        /// </summary>
        /// <param name="value">the value to check.</param>
        /// <param name="bound1">the minimum bound.</param>
        /// <param name="bound2">the maximum bound.</param>
        /// <returns>If the value is within the two bound ranges.</returns>
        public static bool IsBetween(float value, float bound1, float bound2)
        {
            return (value >= Minimum(bound1, bound2) && value <= Maximum(bound1, bound2));
        }


        /// <summary>
        /// Calculates the minimum of two float values.
        /// </summary>
        /// <param name="value1">the first value to check.</param>
        /// <param name="value2">the first value to check.</param>
        /// <returns>The minimum value out of the two values.</returns>
        public static float Minimum(float value1, float value2)
        {
            return (value1 < value2) ? value1 : value2;
        }

        /// <summary>
        /// Calculates the maximum of two float values.
        /// </summary>
        /// <param name="value1">the first value to check.</param>
        /// <param name="value2">the first value to check.</param>
        /// <returns>The maximum value out of the two values.</returns>
        public static float Maximum(float value1, float value2)
        {
            return (value1 > value2) ? value1 : value2;
        }
    }
}