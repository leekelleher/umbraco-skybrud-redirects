﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Skybrud.Essentials.Strings.Extensions;
using Skybrud.Umbraco.Redirects.Config;
using Skybrud.Umbraco.Redirects.Models;
using Skybrud.Umbraco.Redirects.Models.Api;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Skybrud.Umbraco.Redirects.Helpers;

/// <summary>
/// Backoffice helper class for the redirects package.
/// </summary>
public class RedirectsBackOfficeHelper {

    #region Properties

    /// <summary>
    /// Gets a reference to the dependencies of this instance.
    /// </summary>
    protected RedirectsBackOfficeHelperDependencies Dependencies { get; }

    /// <summary>
    /// Gets the back-office URL of Umbraco.
    /// </summary>
    public string BackOfficeUrl => Dependencies.GlobalSettings.GetBackOfficePath(Dependencies.HostingEnvironment);

    /// <summary>
    /// Gets a reference to the current backoffice user.
    /// </summary>
    public IUser? CurrentUser => Dependencies.BackOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

    /// <summary>
    /// Gets a reference to the redirects settings.
    /// </summary>
    public RedirectsSettings Settings => Dependencies.RedirectsSettings.Value;

    /// <summary>
    /// Gets the current <see cref="CultureInfo"/>.
    /// </summary>
    public CultureInfo CurrentCulture {
        get {
            try {
                if (CurrentUser is { } user && user.Language.HasValue(out string? language)) {
                    return CultureInfo.GetCultureInfo(language);
                }
            } catch {
                // Hello there!
            }
            return CultureInfo.CurrentCulture;
        }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="dependencies"/>.
    /// </summary>
    /// <param name="dependencies">An instance of <see cref="RedirectsBackOfficeHelperDependencies"/>.</param>
    public RedirectsBackOfficeHelper(RedirectsBackOfficeHelperDependencies dependencies) {
        Dependencies = dependencies;
    }

    #endregion

    #region Member methods

    /// <summary>
    /// Returns the localized value for the key with the specified <paramref name="alias"/> within the <c>redirects</c> area.
    /// </summary>
    /// <param name="alias">The alias of the key.</param>
    /// <returns>The localized value.</returns>
    public string Localize(string alias) {
        return Localize(alias, "redirects");
    }

    /// <summary>
    /// Returns the localized value for the key with the specified <paramref name="alias"/> and <paramref name="area"/>.
    /// </summary>
    /// <param name="alias">The alias of the key.</param>
    /// <param name="area">The area in which the key is located.</param>
    /// <returns>The localized value.</returns>
    public string Localize(string alias, string area) {
        return Dependencies.TextService.Localize(area, alias);
    }

    /// <summary>
    /// Maps the specified <paramref name="result"/> to a corresponding object to be returned in the API.
    /// </summary>
    /// <param name="result">The search result to be mapped.</param>
    /// <returns>An instance of <see cref="object"/>.</returns>
    public virtual object Map(RedirectsSearchResult result) {

        Dictionary<Guid, ApiRootNode> rootNodeLookup = [];
        Dictionary<Guid, IContent> contentLookup = [];
        Dictionary<Guid, IMedia> mediaLookup = [];
        Dictionary<string, ILanguage> languageLookup = [];

        IEnumerable<ApiRedirect> items = result.Items
            .Select(redirect => Map(redirect, rootNodeLookup, contentLookup, mediaLookup, languageLookup));

        return new ApiRedirectList(result.Pagination, items);

    }

    /// <summary>
    /// Maps the specified <paramref name="redirect"/> to a corresponding <see cref="ApiRedirect"/> to be returned in the API.
    /// </summary>
    /// <param name="redirect">The redirect to be mapped.</param>
    /// <returns>An instance of <see cref="ApiRedirect"/>.</returns>
    public virtual ApiRedirect Map(IRedirect redirect) {
        Dictionary<Guid, ApiRootNode> rootNodeLookup = new();
        Dictionary<Guid, IContent> contentLookup = new();
        Dictionary<Guid, IMedia> mediaLookup = new();
        Dictionary<string, ILanguage> languageLookup = new();
        return Map(redirect, rootNodeLookup, contentLookup, mediaLookup, languageLookup);
    }

    /// <summary>
    /// Maps the specified collection of <paramref name="redirects"/> to a corresponding colelction of <see cref="ApiRedirect"/> to be returned in the API.
    /// </summary>
    /// <param name="redirects">The collection of redirects to be mapped.</param>
    /// <returns>A collection of <see cref="ApiRedirect"/>.</returns>
    public virtual IEnumerable<ApiRedirect> Map(IEnumerable<IRedirect> redirects) {
        Dictionary<Guid, ApiRootNode> rootNodeLookup = new();
        Dictionary<Guid, IContent> contentLookup = new();
        Dictionary<Guid, IMedia> mediaLookup = new();
        Dictionary<string, ILanguage> languageLookup = new();
        return redirects.Select(redirect => Map(redirect, rootNodeLookup, contentLookup, mediaLookup, languageLookup));
    }

    private ApiRedirect Map(IRedirect redirect, Dictionary<Guid, ApiRootNode> rootNodeLookup, Dictionary<Guid, IContent> contentLookup, Dictionary<Guid, IMedia> mediaLookup, Dictionary<string, ILanguage> languageLookup) {

        ApiRootNode? rootNode = null;
        if (redirect.RootKey != Guid.Empty) {

            if (!rootNodeLookup.TryGetValue(redirect.RootKey, out rootNode)) {

                if (!contentLookup.TryGetValue(redirect.RootKey, out IContent? content)) {
                    content = Dependencies.ContentService.GetById(redirect.RootKey);
                    if (content != null) contentLookup.Add(content.Key, content);
                }
                var domains = content == null ? null : Dependencies.DomainService.GetAssignedDomainsAsync(content.Key, false).Result.Select(x => x.DomainName).ToArray();
                rootNode = new ApiRootNode(redirect, content, domains);

                rootNodeLookup.Add(rootNode.Key, rootNode);

            }
        }

        ApiRedirectDestination destination;
        if (redirect.Destination.Type == RedirectDestinationType.Content) {

            if (!contentLookup.TryGetValue(redirect.Destination.Key, out IContent? content)) {
                content = Dependencies.ContentService.GetById(redirect.Destination.Key);
                if (content != null) contentLookup.Add(content.Key, content);
            }

            // Initialize a new destination object
            destination = new ApiRedirectDestination(redirect, content);

            // If the destination refers to a specific culture, we fetch some additional information about the culture
            if (redirect.Destination.Culture.HasValue(out string? culture)) {
                destination.Culture = culture;
                if (languageLookup.TryGetValue(culture, out ILanguage? language)) {
                    destination.CultureName = language.CultureName;
                } else {
                    language = Dependencies.LanguageService.GetAsync(culture).Result;
                    if (language is not null) {
                        languageLookup.Add(culture, language);
                        destination.CultureName = language.CultureName;
                    }
                }
            }

            // Look up the current URL and name of the destination (with respect for the selected culture)
            if (TryGetContent(redirect.Destination.Id, out IPublishedContent? published)) {

                string url = published.Url(destination.Culture);
                if (!string.IsNullOrEmpty(url)) destination.Url = url;

                string? name = published.Name(destination.Culture);
                if (!string.IsNullOrEmpty(name)) destination.Name = name;

            }

            // Set the backoffice URL of the page
            destination.BackOfficeUrl = $"/umbraco/section/content/workspace/document/edit/{redirect.Destination.Id}";

        } else if (redirect.Destination.Type == RedirectDestinationType.Media) {

            if (!mediaLookup.TryGetValue(redirect.Destination.Key, out IMedia? media)) {
                media = Dependencies.MediaService.GetById(redirect.Destination.Key);
                if (media != null) mediaLookup.Add(media.Key, media);
            }

            // Initialize a new destination object
            destination = new ApiRedirectDestination(redirect, media);

            // Look up the current URL of the destination
            if (TryGetMedia(redirect.Destination.Id, out IPublishedContent? published)) {
                string url = published.Url();
                if (!string.IsNullOrEmpty(url)) destination.Url = url;
            }

            // Set the backoffice URL of the media
            destination.BackOfficeUrl = $"/umbraco/section/content/workspace/document/edit/{redirect.Destination.Id}";

        } else {

            destination = new ApiRedirectDestination(redirect);

        }

        return new ApiRedirect(redirect, rootNode, destination);

    }
    /// <summary>
    /// Attempts to get the <see cref="IPublishedContent"/> with the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The numeric ID.</param>
    /// <param name="result">When this method returns, holds the <see cref="IPublishedContent"/> if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryGetContent(int id, [NotNullWhen(true)] out IPublishedContent? result) {

        if (Dependencies.UmbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? context)) {
            result = context.Content?.GetById(id);
            return result is not null;
        }

        result = null;
        return false;

    }

    /// <summary>
    /// Attempts to get the <see cref="IPublishedContent"/> with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The GUID key.</param>
    /// <param name="result">When this method returns, holds the <see cref="IPublishedContent"/> if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryGetContent(Guid key, [NotNullWhen(true)] out IPublishedContent? result) {

        if (Dependencies.UmbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? context)) {
            result = context.Content?.GetById(key);
            return result is not null;
        }

        result = null;
        return false;

    }

    /// <summary>
    /// Attempts to get the media with the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The numeric ID of the media.</param>
    /// <param name="result">When this method returns, holds the <see cref="IPublishedContent"/> representing the media if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryGetMedia(int id, [NotNullWhen(true)] out IPublishedContent? result) {

        if (Dependencies.UmbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? context)) {
            result = context.Media?.GetById(id);
            return result is not null;
        }

        result = null;
        return false;

    }

    #endregion

}