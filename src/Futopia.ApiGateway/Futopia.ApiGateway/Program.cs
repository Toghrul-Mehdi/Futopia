using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Futopia.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Servisləri əlavə et
            builder.Services.AddRouting(); // Routing üçün
            builder.Services.AddHttpClient(); // Microservice çağırışları üçün HttpClient
            builder.Services.AddHealthChecks(); // Health check əlavə et

            var app = builder.Build();

            // Middleware-lər (gələcəkdə əlavə ediləcək)
            app.UseRouting();

            // Health check endpoint
            app.MapHealthChecks("/health");

            // Default route
            app.MapGet("/", () => "Futopia API Gateway is running!");

            // Placeholder microservice proxy route nümunəsi
            app.MapGet("/user-service/{*path}", async (HttpContext context, IHttpClientFactory clientFactory) =>
            {
                var client = clientFactory.CreateClient();
                var path = context.Request.Path.ToString().Replace("/user-service", "");
                var query = context.Request.QueryString.ToString();
                var response = await client.GetAsync($"http://localhost:5001{path}{query}"); // user service URL
                var content = await response.Content.ReadAsStringAsync();
                return Results.Content(content, "application/json");
            });

            app.Run();
        }
    }
}
