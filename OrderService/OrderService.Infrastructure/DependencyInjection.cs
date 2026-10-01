using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Repositories;   

namespace OrderService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<OrderDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString(
                        "DefaultConnection")));

            services.AddScoped<IOrderRepository,
                OrderRepository>();

            services.AddHttpClient<IProductServiceClient,
                ProductServiceClient>((serviceProvider, client) =>
                {
                    var configuration =
                        serviceProvider.GetRequiredService<
                            IConfiguration>();

                    var baseUrl =
                        configuration["Services:ProductService"];

                    client.BaseAddress =
                        new Uri(baseUrl!);
                });

            var rabbitMQHost = configuration["RabbitMQ:Host"] ?? "localhost";

            services.AddSingleton<IOrderEventPublisher>(_ =>
                RabbitMqOrderEventPublisher.CreateAsync(rabbitMQHost).GetAwaiter().GetResult()
            );

            return services;
        }
    }
}
