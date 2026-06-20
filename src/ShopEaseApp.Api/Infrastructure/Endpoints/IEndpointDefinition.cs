namespace ShopEaseApp.Api.Infrastructure.Endpoints;

public interface IEndpointDefinition
{
    void RegisterEndpoints(WebApplication app);
}

public static class EndpointDefinitionExtensions
{
    public static void AddEndpointDefinitions(this IServiceCollection services, Type scanMarker)
    {
        var definitions = scanMarker.Assembly
            .GetTypes()
            .Where(t => typeof(IEndpointDefinition).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IEndpointDefinition>();

        foreach (var definition in definitions)
            services.AddSingleton(definition);
    }

    public static void UseEndpointDefinitions(this WebApplication app)
    {
        var definitions = app.Services.GetServices<IEndpointDefinition>();
        foreach (var definition in definitions)
            definition.RegisterEndpoints(app);
    }
}
