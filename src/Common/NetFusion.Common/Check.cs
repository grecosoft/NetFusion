using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NetFusion.Common
{
    public static class Check
    {
        [DebuggerStepThrough]
        public static void IsTrue(bool predicate, string parameterName, string message)
        {
            if (!predicate)
            {
                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        public static void NotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        [DebuggerStepThrough]
        public static void NotNull<T>(T value, string parameterName, string message) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName, message);
            }
        }

        [DebuggerStepThrough]
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            Check.NotNull(value, parameterName);
            if (value.Length == 0)
            {
                throw new ArgumentException("String value must have at least one character.", parameterName);
            }
        }

        [DebuggerStepThrough]
        public static void NotNullOrWhiteSpace(string value, string parameterName)
        {
            Check.NotNull(value, parameterName);
            if (String.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("String value must have at least one character.", parameterName);
            }
        }
    }
}
