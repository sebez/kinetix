﻿using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection;
using Kinetix.Modeling.Annotations;

namespace Kinetix.Modeling;

/// <summary>
/// Fournit la description d'un bean.
/// </summary>
public static class BeanDescriptor
{
    /// <summary>
    /// Nom par défaut de la propriété par défaut d'un bean, pour l'affichage du libellé de l'objet.
    /// </summary>
    private const string DefaultPropertyDefaultName = "Libelle";

    private static readonly Dictionary<Type, BeanDefinition> _beanDefinitionDictionnary = new();
    private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _resourceTypeMap = new();

    /// <summary>
    /// Vérifie les contraintes sur un bean.
    /// </summary>
    /// <param name="bean">Bean à vérifier.</param>
    /// <param name="propertiesToCheck">Si renseigné, seules ces propriétés seront validées.</param>
    public static void Check(object bean, IEnumerable<string> propertiesToCheck = null)
    {
        if (bean != null)
        {
            GetDefinition(bean).Check(bean, propertiesToCheck);
        }
    }

    /// <summary>
    /// Retourne la definition des beans d'une collection générique.
    /// </summary>
    /// <param name="collection">Collection générique de bean.</param>
    /// <returns>Description des propriétés des beans.</returns>
    public static BeanDefinition GetCollectionDefinition(object collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException("collection");
        }

        var collectionType = collection.GetType();
        if (collectionType.IsArray)
        {
            return GetDefinition(collectionType.GetElementType());
        }

        if (!collectionType.IsGenericType)
        {
            throw new ArgumentException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    SR.ExceptionTypeDescription,
                    collection.GetType().FullName),
                "collection");
        }

        var genericDefinition = collectionType.GetGenericTypeDefinition();
        if (genericDefinition.GetInterface(typeof(ICollection<>).Name) == null)
        {
            throw new NotSupportedException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    SR.ExceptionNotSupportedGeneric,
                    genericDefinition.Name));
        }

        var objectType = collectionType.GetGenericArguments()[0];
        var coll = (ICollection)collection;
        if (typeof(ICustomTypeDescriptor).IsAssignableFrom(objectType) && coll.Count != 0)
        {
            var customObject = coll.Cast<object>().FirstOrDefault();
            return GetDefinition(customObject);
        }

        foreach (var obj in coll)
        {
            objectType = obj.GetType();
            break;
        }

        return GetDefinition(objectType, true);
    }

    /// <summary>
    /// Retourne la definition d'un bean.
    /// </summary>
    /// <param name="bean">Objet.</param>
    /// <returns>Description des propriétés.</returns>
    public static BeanDefinition GetDefinition(object bean)
    {
        if (bean == null)
        {
            throw new ArgumentNullException("bean");
        }

        return GetDefinitionInternal(bean.GetType(), bean);
    }

    /// <summary>
    /// Retourne la definition d'un bean.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <param name="ignoreCustomTypeDesc">Si true, retourne un définition même si le type implémente ICustomTypeDescriptor.</param>
    /// <returns>Description des propriétés.</returns>
    public static BeanDefinition GetDefinition(Type beanType, bool ignoreCustomTypeDesc = false)
    {
        if (beanType == null)
        {
            throw new ArgumentNullException("beanType");
        }

        if (!ignoreCustomTypeDesc && typeof(ICustomTypeDescriptor).IsAssignableFrom(beanType))
        {
            throw new NotSupportedException(SR.ExceptionICustomTypeDescriptorNotSupported);
        }

        return GetDefinitionInternal(beanType, null);
    }

    /// <summary>
    /// Efface la définition d'un bean du singleton.
    /// </summary>
    /// <param name="descriptionType">Type portant la description.</param>
    public static void ClearDefinition(Type descriptionType)
    {
        _beanDefinitionDictionnary.Remove(descriptionType);
    }

    /// <summary>
    /// Crée la collection des descripteurs de propriétés.
    /// </summary>
    /// <param name="properties">PropertyDescriptors.</param>
    /// <param name="defaultProperty">Propriété par défaut.</param>
    /// <param name="beanType">Type du bean.</param>
    /// <returns>Collection.</returns>
    private static BeanPropertyDescriptorCollection CreateCollection(PropertyDescriptorCollection properties, PropertyDescriptor defaultProperty, Type beanType)
    {
        var coll = new BeanPropertyDescriptorCollection(beanType);
        for (var i = 0; i < properties.Count; i++)
        {
            var property = properties[i];

            var keyAttr = (KeyAttribute)property.Attributes[typeof(KeyAttribute)];
            var displayAttr = (DisplayAttribute)property.Attributes[typeof(DisplayAttribute)];
            var attr = (ReferencedTypeAttribute)property.Attributes[typeof(ReferencedTypeAttribute)];
            var colAttr = (ColumnAttribute)property.Attributes[typeof(ColumnAttribute)];
            var domainAttr = (DomainAttribute)property.Attributes[typeof(DomainAttribute)];
            var requiredAttr = (RequiredAttribute)property.Attributes[typeof(RequiredAttribute)];

            string display = null;
            if (displayAttr != null)
            {
                if (displayAttr.ResourceType != null && displayAttr.Name != null)
                {
                    if (!_resourceTypeMap.TryGetValue(displayAttr.ResourceType, out var resourceProperties))
                    {
                        resourceProperties = new Dictionary<string, PropertyInfo>();
                        _resourceTypeMap[displayAttr.ResourceType] = resourceProperties;

                        foreach (var p in displayAttr.ResourceType.GetProperties(BindingFlags.Public | BindingFlags.Static))
                        {
                            resourceProperties.Add(p.Name, p);
                        }
                    }

                    display = resourceProperties[displayAttr.Name].GetValue(null, null).ToString();
                }
                else
                {
                    display = displayAttr.Name;
                }
            }

            var memberName = colAttr?.Name;
            var isPrimaryKey = keyAttr != null;
            var isRequired = requiredAttr != null;
            var domainName = domainAttr?.Name;
            var isDefault = property.Equals(defaultProperty) || DefaultPropertyDefaultName.Equals(property.Name) && defaultProperty == null;
            var referenceType = attr?.ReferenceType;
            var isBrowsable = property.IsBrowsable;
            var isReadonly = property.IsReadOnly;

            var description = new BeanPropertyDescriptor(
                property.Name,
                memberName,
                property.PropertyType,
                display,
                domainName,
                isPrimaryKey,
                isDefault,
                isRequired,
                referenceType,
                isReadonly,
                isBrowsable);

            coll.Add(description);
        }

        return coll;
    }

    /// <summary>
    /// Retourne la description des propriétés d'un objet sous forme d'une collection.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <param name="metadataType">Type portant les compléments de description.</param>
    /// <param name="bean">Bean dynamic.</param>
    /// <returns>Description des propriétés.</returns>
    private static BeanPropertyDescriptorCollection CreateBeanPropertyCollection(Type beanType, object bean)
    {
        PropertyDescriptor defaultProperty;
        PropertyDescriptorCollection properties;

        if (bean != null && bean is ICustomTypeDescriptor)
        {
            properties = TypeDescriptor.GetProperties(bean);
            defaultProperty = TypeDescriptor.GetDefaultProperty(bean);
        }
        else
        {
            properties = TypeDescriptor.GetProperties(beanType);
            defaultProperty = TypeDescriptor.GetDefaultProperty(beanType);
        }

        return CreateCollection(properties, defaultProperty, beanType);
    }

    /// <summary>
    /// Retourne la definition d'un bean.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <param name="bean">Bean.</param>
    /// <returns>Description des propriétés.</returns>
    private static BeanDefinition GetDefinitionInternal(Type beanType, object bean)
    {
        var descriptionType = beanType;

        if (!_beanDefinitionDictionnary.TryGetValue(descriptionType, out var definition))
        {
            var properties = CreateBeanPropertyCollection(beanType, bean);
            if (properties.Any())
            {
                var table = beanType.GetCustomAttribute<TableAttribute>();
                var contractName = table?.Name;

                var reference = beanType.GetCustomAttribute<ReferenceAttribute>();
                var isReference = reference != null;
                var isStatic = reference?.IsStatic ?? false;

                definition = new BeanDefinition(beanType, properties, contractName, isReference, isStatic);
                if (bean == null && !typeof(ICustomTypeDescriptor).IsAssignableFrom(beanType))
                {
                    _beanDefinitionDictionnary[descriptionType] = definition;
                }
            }
        }

        return definition;
    }
}
