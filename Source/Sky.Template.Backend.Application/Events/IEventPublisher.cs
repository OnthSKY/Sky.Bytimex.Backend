namespace Sky.Template.Backend.Application.Events;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event);
}
