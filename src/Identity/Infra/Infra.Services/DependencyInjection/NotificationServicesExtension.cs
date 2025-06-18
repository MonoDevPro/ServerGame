using Infra.Notification.Dispatchers;
using Infra.Notification.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerGame.Application.Common.Interfaces.Notification.Dispatchers;

namespace Infra.Services.DependencyInjection;

public static class NotificationServicesExtension
{
    public static IHostApplicationBuilder ConfigureNotificationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(INotificationDispatcher<>), typeof(NotificationDispatcher<>));
        
        builder.Services.AddScoped<ISaveChangesInterceptor, NotificationInterceptor>();

        return builder;
    }
}
