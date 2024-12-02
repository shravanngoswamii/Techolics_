using System;

namespace Techolics_.PolicyManagement
{
    public static class PolicyValueConverter
    {
        /// <summary>
        /// Converts the policy value based on its value type.
        /// For Boolean types, converts "true"/"false" to "1"/"0".
        /// Otherwise, returns the original value.
        /// </summary>
        /// <param name="value">The original value.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <returns>The converted value as string.</returns>
        public static string ConvertForConfiguration(string value, string valueType)
        {
            if (string.Equals(valueType, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return "1";
                }
                else if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    return "0";
                }
                else
                {
                    throw new ArgumentException($"Invalid Boolean value: {value}");
                }
            }

            // Add more conversions if needed for other types
            return value;
        }

        /// <summary>
        /// Converts the policy value from configuration format to display format.
        /// For Boolean types, converts "1"/"0" to "true"/"false".
        /// Otherwise, returns the original value.
        /// </summary>
        /// <param name="value">The value from configuration.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <returns>The converted value as string.</returns>
        public static string ConvertForDisplay(string value, string valueType)
        {
            if (string.Equals(valueType, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                if (value == "1")
                {
                    return "true";
                }
                else if (value == "0")
                {
                    return "false";
                }
                else
                {
                    return value; // Return as-is if it's neither 1 nor 0
                }
            }

            // Add more conversions if needed for other types
            return value;
        }
    }
}
