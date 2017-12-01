using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.ReferenceTypes
{
    /// <summary>
    /// Represents an enumerated reference type pertaining to the domain.
    /// This is for types that classify a domain entity.  For example, 
    /// StatusType would be such an example.
    /// </summary>
    public abstract class ReferenceType
    {
        protected static List<ReferenceType> _referenceTypes = new List<ReferenceType>();

        /// <summary>
        /// Invoked by the plug-in module during the container startup phase.
        /// </summary>
        /// <param name="referenceTypes">List of all reference types.</param>
        public static void SetReferenceTypes(IEnumerable<ReferenceType> referenceTypes) 
        {
            _referenceTypes = referenceTypes.ToList();
        }

        public static void AddReferenceType(ReferenceType referenceType)
        {
            _referenceTypes.Add(referenceType);
        }

        public string ReferenceTypeId { get; private set; }
        public string ReferenceClrType { get; protected set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Represents an enumerated reference type with a custom type providing additional 
    /// meta-data for each enumerated value.
    /// </summary>
    /// <typeparam name="TEnum">The enumerated type.</typeparam>
    /// <typeparam name="TDescriptor">Class containing properties describing the enumerated value.</typeparam>
    public class ReferenceType<TEnum, TDescriptor> : ReferenceType
        where TEnum : struct
        where TDescriptor : ValueDescriptor<TEnum>
    {
        public ReferenceType()
        {
            this.ReferenceClrType = typeof(TEnum).FullName;
            this.Descriptors = new Dictionary<string, TDescriptor>();
        }

        /// <summary>
        /// List of descriptors keyed by the enumeration's named value.
        /// </summary>
        public IDictionary<string, TDescriptor> Descriptors { get; private set; }

        /// <summary>
        /// Returns a reference to the associated reference type.
        /// </summary>
        public static ReferenceType<TEnum, TDescriptor> InternalInstance
        {
            get {

                var clrType = typeof(TEnum).FullName;

                var referenceType = (ReferenceType<TEnum, TDescriptor>)_referenceTypes
                    .FirstOrDefault(rt => rt.ReferenceClrType == clrType);

                if (referenceType == null)
                {
                    throw new InvalidOperationException(
                        $"reference type for the enumeration of type {clrType} not found");
                }

                return referenceType;
            }
        }

        /// <summary>
        /// Associates meta-data with a given enumerated value.  This is not called by the
        /// consuming application but by an initialization script to seed the database.
        /// </summary>
        /// <param name="enumValue">The enumerated value being described.</param>
        /// <param name="descriptor">Object describing the enumerated value.</param>
        public void AddDescriptor(TEnum enumValue, TDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            var enumValueName = Enum.GetName(typeof(TEnum), enumValue);
            descriptor.Value = enumValue;
            Descriptors[enumValueName] = descriptor;
        }

        public TDescriptor GetDescriptor(TEnum enumValue)
        {
            var enumValueName = Enum.GetName(typeof(TEnum), enumValue);
            TDescriptor value = null;

            if(Descriptors.TryGetValue(enumValueName, out value))
            {
                return Descriptors[enumValueName];
            }

            throw new InvalidOperationException(
                $"descriptor for the enumerated value {enumValue} not found for enumerated type {enumValueName}");
        }

        public TDescriptor GetDiscriptorByExternalValue(string externalValue)
        {
            var descriptor = InternalInstance.Descriptors.Values
                .FirstOrDefault(d => d.ExternalValue == externalValue);

            if (descriptor == null)
            {

            }

            return descriptor;
        }
    }

    /// <summary>
    /// Represents an enumerated reference type providing additional meta-data 
    /// for each enumerated value.
    /// </summary>
    /// <typeparam name="TEnum">The enumerated type.</typeparam>
    public class ReferenceType<TEnum> : ReferenceType<TEnum, ValueDescriptor<TEnum>>
        where TEnum : struct
    {
        public void AddDescriptor(TEnum enumValue, ValueDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            var enumValueName = Enum.GetName(typeof(TEnum), enumValue);
            var typedDiscriptor = new ValueDescriptor<TEnum>(descriptor);

            typedDiscriptor.Value = enumValue;
            Descriptors[enumValueName] = typedDiscriptor;
        }
    }
}
