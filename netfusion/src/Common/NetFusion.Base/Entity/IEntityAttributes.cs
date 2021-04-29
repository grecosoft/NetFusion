using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NetFusion.Base.Entity
{
    /// <summary>
    /// Represents a set of dynamic key/value pairs that can be associated with an entity.
    /// </summary>
    public interface IEntityAttributes
    {
        /// <summary>
        /// Provides dynamic access to the entity's attributes.  This property is a .NET dynamic type
        /// allows arbitrary properties, not statically defined, to be associated with an object.
        /// </summary>
        dynamic Values { get; }

        /// <summary>
        /// Sets the underlying values of the entity.
        /// </summary>
        /// <param name="values">Dictionary of key/value pairs.</param>
        void SetValues(IDictionary<string, object> values);

        /// <summary>
        /// Gets all of the entity's attributes.
        /// </summary>
        /// <returns>Dictionary of key/value pairs.</returns>
        IDictionary<string, object> GetValues();

        /// <summary>
        /// Set the value of an entity's attribute property.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <param name="overrideIfPresent">Determines if existing attribute value is overridden.</param>
        void SetValue(string name, object value, bool overrideIfPresent = true);

        /// <summary>
        /// Returns the value of an attribute.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The current value or an exception if not present.</returns>
        object GetValue(string name);

        /// <summary>
        /// Allows returning a value if present.  The return value indicates if the
        /// named value was present.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <returns>True if the named value exists.  Otherwise false.</returns>
        bool TryGetValue(string name, out object value);

        /// <summary>
        /// Returns the value of an attribute or the specified default value if not present.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="defaultValue">The value to return if not present.</param>
        /// <returns>The current value or the specified default if not present.</returns>
        object GetValueOrDefault(string name, object defaultValue = default);

        /// <summary>
        /// Returns the value of an attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute value.</typeparam>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The current value or an exception if not present.</returns>
        T GetValue<T>(string name);
        
        /// <summary>
        /// Allows returning a value if present.  The return value indicates if the
        /// named value was present.
        /// </summary>
        /// <typeparam name="T">The type of the attribute value.</typeparam>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <returns>True if the named value exists.  Otherwise false.</returns>
        bool TryGetValue<T>(string name, out T value);
        
        /// <summary>
        /// Returns the value of an attribute or the specified default value if not present.
        /// </summary>
        /// <typeparam name="T">The type of the attribute value.</typeparam>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="defaultValue">The value to return if not present.</param>
        /// <returns>The current value or the specified default if not present.</returns>
        T GetValueOrDefault<T>(string name, T defaultValue = default);

        /// <summary>
        /// Determines the entity has an associated attribute.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>True if the entity has the attribute.  Otherwise, false.</returns>
        bool Contains(string name);

        /// <summary>
        /// Removes an attribute from the entity.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>True if the attribute was present.  Otherwise, false.</returns>
        bool Delete(string name);

        /// <summary>
        /// Set the value of an entity's attribute property.
        /// </summary>
        /// <typeparam name="T">The type of the attribute value.</typeparam>
        /// <param name="value">The value of the attribute.</param>
        /// <param name="overrideIfPresent">Determines if existing attribute value is overridden.</param>
        /// <param name="callerName">The name of the attribute.</param>
        void SetMemberValue<T>(T value, bool overrideIfPresent = true,
            [CallerMemberName] string callerName = null);

        /// <summary>
        /// Returns the value of an attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute value.</typeparam>
        /// <param name="defaultValue"></param>
        /// <param name="callerName">The name of the attribute.</param>
        /// <returns>The current value or the specified default if not present.</returns>
        T GetMemberValueOrDefault<T>(T defaultValue = default, 
            [CallerMemberName] string callerName = null);
    }
}
