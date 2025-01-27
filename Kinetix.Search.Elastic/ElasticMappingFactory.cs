﻿using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Elastic.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Usine à mapping ElasticSearch.
/// </summary>
public sealed class ElasticMappingFactory
{
    private readonly IServiceProvider _provider;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="provider"></param>
    public ElasticMappingFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Effectue le mapping pour les champs d'un document.
    /// </summary>
    /// <param name="selector">Descripteur des propriétés.</param>
    /// <param name="fields">Les champs.</param>
    /// <returns>Mapping de champ.</returns>
    /// <typeparam name="T">Type du document.</typeparam>
    public PropertiesDescriptor<T> AddFields<T>(PropertiesDescriptor<T> selector, DocumentFieldDescriptorCollection fields)
         where T : class
    {
        foreach (var field in fields.OrderBy(field => field.FieldName))
        {
            AddField(selector, field);
        }

        return selector;
    }

    /// <summary>
    /// Effectue le mapping pour un champ d'un document.
    /// </summary>
    /// <param name="selector">Descripteur des propriétés.</param>
    /// <param name="field">Le champ.</param>
    /// <returns>Mapping de champ.</returns>
    /// <typeparam name="T">Type du document.</typeparam>
    public PropertiesDescriptor<T> AddField<T>(PropertiesDescriptor<T> selector, DocumentFieldDescriptor field)
        where T : class
    {
        if (!(_provider.GetService(typeof(IElasticMapping<>).MakeGenericType(field.PropertyType)) is IElasticMapping mapper))
        {
            mapper = _provider.GetService<IElasticMapping<string>>();
        }

        return mapper.Map(selector, field);
    }
}
