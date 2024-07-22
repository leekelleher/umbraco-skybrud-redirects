using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Skybrud.Umbraco.Redirects.Models.Api;

public class ApiRedirectList {

    [JsonPropertyName("pagination")]
    public RedirectsSearchResultPagination Pagination { get; }

    [JsonPropertyName("items")]
    public IEnumerable<ApiRedirect> Items { get; }

    public ApiRedirectList(RedirectsSearchResultPagination pagination, IEnumerable<ApiRedirect> items) {
        Pagination = pagination;
        Items = items;
    }

}