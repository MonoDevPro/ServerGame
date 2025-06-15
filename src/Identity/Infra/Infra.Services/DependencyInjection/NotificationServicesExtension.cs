using Infra.Notification.Dispatchers;
using Infra.Notification.EventBus;
using Infra.Notification.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Application.Common.Interfaces.Notification.Dispatchers;
using ServerGame.Application.Common.Interfaces.Notification.EventBus;

namespace Infra.Services.DependencyInjection;

public static class NotificationServicesExtension
{
    public static IHostApplicationBuilder ConfigureNotificationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(INotificationDispatcher<>), typeof(NotificationDispatcher<>));
        builder.Services.AddScoped(typeof(IEventDispatcher<>), typeof(EventDispatcher<>));
        
        builder.Services.AddScoped<IEventBus, InMemoryEventBus>();
        
        builder.Services.AddScoped<ISaveChangesInterceptor, EventInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, NotificationInterceptor>();

        return builder;
    }
}
