using System;
using System.Text.RegularExpressions;

namespace JapaneseLearningWeb.Services
{
    /// <summary>
    /// Utility functions for various operations like validation, string processing, and calculations
    /// </summary>
    public class UtilityFunctions
    {
        /// <summary>
        /// Validates if a string is a valid email address
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if a string is a valid Vietnamese phone number
        /// </summary>
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Vietnamese phone numbers typically start with 0 and have 10-11 digits
            string pattern = @"^0\d{9,10}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        /// <summary>
        /// Calculates the total price with discount
        /// </summary>
        public static decimal CalculateDiscountedPrice(decimal originalPrice, decimal discountPercent)
        {
            if (originalPrice < 0)
                throw new ArgumentException("Original price cannot be negative");

            if (discountPercent < 0 || discountPercent > 100)
                throw new ArgumentException("Discount percent must be between 0 and 100");

            return originalPrice * (1 - (discountPercent / 100));
        }

        /// <summary>
        /// Reverses a string
        /// </summary>
        public static string ReverseString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Parses an integer from a string with error handling
        /// </summary>
        public static bool TryParseInteger(string input, out int result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            return int.TryParse(input.Trim(), out result);
        }
    }
}
