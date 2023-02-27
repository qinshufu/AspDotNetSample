using Consul;
using System.Reflection;

namespace ServerOne;

public record ConsulRegistrationConfigration
{
    public required string ServiceName { get; init; }

    public required string ServerAddress { get; init; }

    public required int ServerPort { get; init; }

    public required string HealthCheckUri { get; init; }
}




public static class WebApplicationExtensions
{
    public static void UseConsul(this WebApplication app, ConsulRegistrationConfigration configration)
    {
        var consul = app.Services.GetRequiredService<ConsulClient>();
        var serviceId = Guid.NewGuid().ToString()[..8];

        var resultTask = consul.Agent.ServiceRegister(new AgentServiceRegistration()
        {
            ID = serviceId,
            Address = configration.ServerAddress,
            Port = configration.ServerPort,
            Name = Assembly.GetExecutingAssembly().GetName().Name,
            Check = new AgentServiceCheck()
            {
                HTTP = configration.HealthCheckUri,
                Interval = TimeSpan.FromSeconds(2),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(2)
            }
        });

        if (resultTask.Result.StatusCode is not System.Net.HttpStatusCode.OK)
            throw new ApplicationException("无法注册到 Consul");

        app.Lifetime.ApplicationStopped.Register(() => consul.Agent.ServiceDeregister(serviceId));
    }
}