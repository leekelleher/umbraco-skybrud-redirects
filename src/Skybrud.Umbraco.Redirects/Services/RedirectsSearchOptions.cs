using System;
using System.Collections.Generic;
using Skybrud.Umbraco.Redirects.Models;

namespace Skybrud.Umbraco.Redirects.Services;

/// <summary>
/// Class with options for searching through the created redirects.
/// </summary>
public class RedirectsSearchOptions {

    /// <summary>
    /// Gets or sets the page to be returned. Default is <c>1</c>.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the maximum amount of redirects to be returned per page. Default is <c>20</c>.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the key the returned redirects should match. <see cref="Guid.Empty"/> indicates all global
    /// redirects. Default is <c>null</c>, in which case this filter is disabled.
    /// </summary>
    public Guid? RootNodeKey { get; set; }

    /// <summary>
    /// Gets or sets the types the returned redirects should match.
    /// </summary>
    public List<RedirectDestinationType> Type {  get; set; } = [];

    /// <summary>
    /// Gets or sets a string value that should be present in either the text or URL of the returned redirects. Default is <see langword="null"/>.
    /// </summary>
    public string? Text { get; set; }

}
