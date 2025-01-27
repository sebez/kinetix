﻿using System.Linq.Expressions;
using ClosedXML.Excel;

namespace Kinetix.Reporting.Core.Excel;

/// <summary>
/// Constructeur d'onglet Excel (Worksheet).
/// </summary>
/// <typeparam name="T">Type de l'objet représenté dans la Worksheet.</typeparam>
public interface IWorksheetBuilder<T>
{
    /// <summary>
    /// Configure une nouvelle colonne dans la Worksheet.
    /// </summary>
    /// <param name="selector">Le champ à afficher dans la colonne.</param>
    /// <returns>Builder.</returns>
    IWorksheetBuilder<T> Column(Expression<Func<T, object>> selector);

    /// <summary>
    /// Configure une nouvelle colonne dans la Worksheet.
    /// </summary>
    /// <param name="label">Le libellé de la colonne.</param>
    /// <returns>Builder.</returns>
    IWorksheetBuilder<T> Column(string label = null);

    /// <summary>
    /// Configure une nouvelle colonne dans la Worksheet.
    /// </summary>
    /// <param name="label">Le libellé de la colonne.</param>
    /// <param name="selector">Le champ à afficher dans la colonne.</param>
    /// <returns>Builder.</returns>
    IWorksheetBuilder<T> Column(string label, Expression<Func<T, object>> selector);

    /// <summary>
    /// Configure la source de données pour la Worksheet.
    /// </summary>
    /// <param name="data">Les données.</param>
    /// <returns>Builder.</returns>
    IWorksheetBuilder<T> Data(IEnumerable<T> data);

    /// <summary>
    /// Configure la source de données pour la Worksheet.
    /// </summary>
    /// <param name="data">Les données.</param>
    /// <returns>Builder.</returns>
    IWorksheetBuilder<T> Data(IAsyncEnumerable<T> data);

    /// <summary>
    /// Configure le nombre de résultats maximum dans l'export.
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    IWorksheetBuilder<T> MaxResults(int maxResults);

    /// <summary>
    /// Transpose la feuille pour échanger les colonnes et les lignes.
    /// </summary>
    /// <returns>Builder.</returns>
    IWorksheetBuilder<T> Transpose();

    /// <summary>
    /// Crée la Worksheet à partir de ce qui a été configuré.
    /// </summary>
    /// <param name="postBuildAction">Actions manuelles à effectuer sur la worksheet après construction.</param>
    /// <returns>ExcelBuilder.</returns>
    Task<IExcelBuilder> Build(Func<IXLWorksheet, Task> postBuildAction = null);
}
