using Consul;

namespace ServerOne;

public static class ServiceCollectionExtensions
{
    public static void AddConsulClient(this IServiceCollection services, Uri consulServer)
    {
        services.AddSingleton(services => new ConsulClient(c =>
        {
            c.Address = consulServer;
        }));
    }
}