using FavoritesAPI.Messaging.Events;

namespace FavoritesAPI.Services.Contracts;

public interface IRabbitMqPublisher
{
    void PublishFavoriteEvent(FavoriteFavoritedEvent message);
}

