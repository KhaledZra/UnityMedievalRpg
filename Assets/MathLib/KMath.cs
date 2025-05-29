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
        /// Calculates the magnitude value from the given Vector3.
        /// </summary>
        /// <param name="value">The value to calculate magnitude.</param>
        /// <returns>The Magnitude of value.</returns>
        public static float Magnitude(Vector3 value)
        {
            return SquareRoot(
                Power(value.x, 2) +
                Power(value.y, 2) +
                Power(value.z, 2));
        }

        /// <summary>
        /// Calculate the square root of a number using the Newton-Raphson method.
        /// </summary>
        /// <param name="value">The value to calculate square root.</param>
        /// <returns>The square root of value</returns>
        public static float SquareRoot(float value)
        {
            if (value < 0) throw new ArgumentException("Negative input");

            float x = value / 2f;
            float tolerance = 0.00001f;

            while (Absolute(x * x - value) > tolerance)
            {
                x = 0.5f * (x + value / x);
            }

            return x;
        }

        /// <summary>
        /// Calculates the power of a base value raised to an exponent using exponentiation by squaring.
        /// </summary>
        /// <param name="baseValue">the base value</param>
        /// <param name="exponent">The exponent value</param>
        /// <returns>The result of baseValue raised to the power of exponent</returns>
        public static float Power(float baseValue, int exponent)
        {
            if (exponent == 0) return 1;
            if (exponent < 0) return 1 / Power(baseValue, -exponent);

            float result = 1;
            while (exponent > 0)
            {
                if ((exponent % 2) == 1)
                    result *= baseValue;
                baseValue *= baseValue;
                exponent /= 2;
            }

            return result;
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