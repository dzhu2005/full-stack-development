using api.fullstack.Services;

namespace api.fullstack;

public class Program
{
    private const string FrontendCorsPolicy = "frontend";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Typed client: base address and timeout come from configuration, while connection pooling
        // and handler lifetime are managed by IHttpClientFactory.
        builder.Services.AddHttpClient<IPostSearchService, PostSearchService>(client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["PostsApi:BaseUrl"] ?? "https://jsonplaceholder.typicode.com/",
                UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Allow the Nuxt dev server (a different origin) to call this API from the browser.
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? ["http://localhost:3000"];

        builder.Services.AddCors(options =>
            options.AddPolicy(FrontendCorsPolicy, policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseCors(FrontendCorsPolicy);

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}