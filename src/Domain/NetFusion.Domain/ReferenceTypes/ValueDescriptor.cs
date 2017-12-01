using System;

namespace NetFusion.Domain.ReferenceTypes
{
    /// <summary>
    /// Provides a base class with a list of name/value pairs used to 
    /// describe and enumerated type.
    /// </summary>
    public class ValueDescriptor
    {
        public string DisplayValue { get; set; }
        public string Description { get; set; }
        public string ExternalValue { get; set; }
    }

    /// <summary>
    /// Can be derived from to provide a custom class defining
    /// properties associated with the enumerated value.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumerated value.</typeparam>
    public class ValueDescriptor<TEnum> : ValueDescriptor
    {
        public TEnum Value { get; set; }

        public ValueDescriptor() : base() { }

        public ValueDescriptor(ValueDescriptor descriptor) : base()
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            DisplayValue = descriptor.DisplayValue;
            Description = descriptor.Description;
            ExternalValue = descriptor.ExternalValue;
        }
    }
}

