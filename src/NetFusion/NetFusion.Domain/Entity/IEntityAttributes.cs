using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NetFusion.Domain.Entity
{
    /// <summary>
    /// Represents a set of dynamic key/value pairs that can be associated
    /// with an entity.
    /// </summary>
    public interface IEntityAttributes
    {
        /// <summary>
        /// Allows access to the dynamic attributes using C# syntax.
        /// </summary>
        dynamic Values { get; }

        /// <summary>
        /// Sets the entity attribute values.
        /// </summary>
        /// <param name="data"></param>
        void SetValues(IDictionary<string, object> values);

        /// <summary>
        /// Returns all of the entity attribute values.
        /// </summary>
        /// <returns>Key value pairs.</returns>
        IDictionary<string, object> GetValues();

        /// <summary>
        /// Used to set a given attribute value for the entity.  If the
        /// attribute exists, the value will be overridden.</summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        void SetValue(string name, object value);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Sets a value that is tagged with a given scope.</typeparam>
        /// <param name="value">The value to set.</param>
        /// <param name="context">The type representing the scope associated with the value.</param>
        /// <param name="name">The name of the attribute.  If null, the name is the calling member's name.</param>
        void SetValue<T>(T value, Type context = null, [CallerMemberName] string name = null);

        /// <summary>
        /// Returns a named attribute value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute.</typeparam>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The value cast as the specified type.</returns>
        T GetValue<T>(string name);

        /// <summary>
        /// Returns a named attribute value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute.</typeparam>
        /// <param name="name">The name of the attribute</param>
        /// <param name="defaultValue">The default value to use if the attribute is not present.</param>
        /// <returns>The value cast as the specified type.</returns>
        T GetValueOrDefault<T>(string name, T defaultValue = default(T));

        /// <summary>
        /// Returns a named attributed tagged with a given scope.
        /// </summary>
        /// <typeparam name="T">The type of the attribute.</typeparam>
        /// <param name="defaultValue">The default value to use if the attribute is not present.</param>
        /// <param name="context">The type representing the scope associated with the value.</param>
        /// <param name="name">The name of the attribute.  If null, the name is the calling member's name.</param>
        /// <returns></returns>
        T GetValue<T>(T defaultValue, Type context = null, [CallerMemberName]string name = null);

        /// <summary>
        /// Deletes an existing entity attribute.
        /// </summary>
        /// <param name="name">The name of the attribute to delete.</param>
        /// <returns>True if deleted.  Otherwise, False.</returns>
        bool Delete(string name);

        /// <summary>
        /// Determines if a named attribute exists.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>True if present.  Otherwise, False.</returns>
        bool Contains(string name);
    }
}
