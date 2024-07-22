using Skybrud.Umbraco.Redirects.Models.Dtos;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Skybrud.Umbraco.Redirects.Notifications.Handlers;

public class MediaSavedNotificationHandler : INotificationHandler<MediaSavedNotification> {

    private readonly IScopeProvider _scopeProvider;

    public MediaSavedNotificationHandler(IScopeProvider scopeProvider) {
        _scopeProvider = scopeProvider;
    }

    public void Handle(MediaSavedNotification notification) {
        using var scope = _scopeProvider.CreateScope();

        foreach (var media in notification.SavedEntities) {

            var sql = scope.SqlContext
                .Sql()
                .Update<RedirectDto>()
                .Update<RedirectDto>(x => x.Set(y => y.DestinationName, media.Name))
                .Where<RedirectDto>(x => x.Key == media.Key);

            scope.Database.Execute(sql);

        }

        scope.Complete();

    }

}