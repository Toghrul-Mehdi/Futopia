using System.Text;

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

            // CORS əlavə et (frontend sorğular üçün)
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            var app = builder.Build();

            // Middleware-lər
            app.UseRouting();
            app.UseCors();

            // Health check endpoint
            app.MapHealthChecks("/health");

            // Default route
            app.MapGet("/", () => "Futopia API Gateway is running!");

            // Universal proxy route (GET, POST, PUT, DELETE)
            app.Map("/{service}/{*path}", async (HttpContext context, IHttpClientFactory clientFactory) =>
            {
                var client = clientFactory.CreateClient();
                var path = context.Request.RouteValues["path"]?.ToString() ?? "";

                var requestMessage = new HttpRequestMessage
                {
                    Method = new HttpMethod(context.Request.Method),
                    // HTTPS portunu istifadə edirik
                    RequestUri = new Uri($"https://localhost:7198/{path}{context.Request.QueryString}")
                };

                // Body varsa oxu və əlavə et
                if (context.Request.ContentLength > 0)
                {
                    using var reader = new StreamReader(context.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    requestMessage.Content = new StringContent(body, Encoding.UTF8, context.Request.ContentType);
                }

                // Authorization header varsa əlavə et
                if (context.Request.Headers.ContainsKey("Authorization"))
                {
                    requestMessage.Headers.Add("Authorization", context.Request.Headers["Authorization"].ToString());
                }

                var response = await client.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                context.Response.StatusCode = (int)response.StatusCode;
                await context.Response.WriteAsync(responseContent);
            });

            app.Run();
        }
    }
}
