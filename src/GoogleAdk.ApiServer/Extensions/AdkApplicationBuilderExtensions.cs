using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Routing;

namespace GoogleAdk.ApiServer;

public static class AdkApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAdk(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<AdkServerOptions>();
        
        if (options.ShowSwaggerUI)
        {
            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ADK Server API v1");
            });
        }

        app.UseWebSockets();
        app.UseCors();

        if (options.ShowAdkWebUI)
        {
            var embeddedProvider = new EmbeddedFileProvider(
                typeof(AdkServer).Assembly, "GoogleAdk.ApiServer.wwwroot");

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = embeddedProvider,
                RequestPath = "/dev-ui",
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedProvider,
                RequestPath = "/dev-ui",
                ServeUnknownFileTypes = false,
            });
        }

        return app;
    }

    public static IEndpointRouteBuilder MapAdk(this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<AdkServerOptions>();
        
        // We must cast to WebApplication because the endpoint mappings rely on WebApplication 
        // to map endpoints.
        if (endpoints is WebApplication app)
        {
            app.MapAdkApi();
            if (options.EnableA2a)
                app.MapA2aApi();
        }

        if (options.ShowAdkWebUI)
        {
            endpoints.MapGet("/", () => Results.Redirect("/dev-ui"));
        }

        return endpoints;
    }
}
