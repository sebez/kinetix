﻿using System;
using System.Linq.Expressions;

namespace Kinetix.Search.Model
{
    /// <summary>
    /// Facette de champ renseigné.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    public class ExistsFacet<TDocument> : TermFacet<TDocument>
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="code">Code de la facette.</param>
        /// <param name="label">Libellé de la facette.</param>
        /// <param name="field">Champ sur lequel agit la facette.</param>
        public ExistsFacet(string code, string label, Expression<Func<TDocument, object>> field)
            : base(code, label, field)
        {
        }

        /// <inheritdoc cref="IFacetDefinition.IsMultiSelectable" />
        public override bool IsMultiSelectable => false;

        /// <inheritdoc cref="IFacetDefinition.CanExclude" />
        public override bool CanExclude => false;

        /// <inheritdoc cref="IFacetDefinition.HasMissing" />
        public override bool HasMissing => true;

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public override string ResolveLabel(string primaryKey)
        {
            return null;
        }
    }
}