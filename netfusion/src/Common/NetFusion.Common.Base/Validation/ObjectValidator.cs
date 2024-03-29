﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetFusion.Common.Base.Validation;

/// <summary>
/// Validation based on Microsoft DataAnnotations.  This is the IObjectValidator implementation
/// used by default if not overridden when bootstrapping the application container.
/// </summary>
public class ObjectValidator(object obj) : IObjectValidator
{
    public object Object { get; } = obj ?? throw new ArgumentNullException(nameof(obj));
    
    private readonly List<ValidationItem> _items = [];
    private readonly List<IObjectValidator> _children = [];
    
    public IEnumerable<ValidationItem> Validations => _items;
    public IEnumerable<IObjectValidator> Children => _children;

    public bool IsValid =>
        _items.All(i => i.ValidationType != ValidationTypes.Error) &&
        _children.All(cv => cv.IsValid);

    public ValidationResultSet Validate()
    {
        ValidateObject();
        return new ValidationResultSet(Object, this);
    }

    private void ValidateObject()
    {
        var valResults = new List<ValidationResult>();
        var context = new ValidationContext(Object);

        Validator.TryValidateObject(Object, context, valResults, true);

        var validationItems = valResults.Select(r => new ValidationItem(
            r.ErrorMessage ?? string.Empty,
            r.MemberNames,
            ValidationTypes.Error)).ToArray();

        _items.AddRange(validationItems);

        // If the object being validated is valid and it supports custom
        // validation, invoke the Validate method on the object.
        if (validationItems.Length == 0 && Object is IValidatableType validatable)
        {
            validatable.Validate(this);
        }
    }

    public bool Verify(bool predicate, string message, 
        ValidationTypes level = ValidationTypes.Error,
        params string[] propertyNames)
    {
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(nameof(message),
            "Message cannot be null or empty string.");

        if (! predicate)
        {
            _items.Add(new ValidationItem(message, propertyNames, level));
        }
        return predicate;
    }

    // Enlists a child object with its parent to be validated.
    public IObjectValidator AddChild(object childObject)
    {
        if (childObject == null) throw new ArgumentNullException(nameof(childObject), 
            "Child object to validate cannot be null.");

        var validator = new ObjectValidator(childObject);
        validator.ValidateObject();

        _children.Add(validator);
        return validator;
    }

    public void AddChildren(IEnumerable<object> childObjects)
    {
        foreach (object childObject in childObjects)
        {
            AddChild(childObject);
        }
    }
        
    public void AddChildren(params object[] childObjects)
    {
        foreach (object childObject in childObjects)
        {
            AddChild(childObject);
        }
    }
}