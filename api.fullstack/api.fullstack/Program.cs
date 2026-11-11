using api.fullstack.Services;

namespace api.fullstack;

public class Program
{
    private const string WebAppCorsPolicy = "WebApp";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Typed HttpClient for the posts API. IHttpClientFactory manages the handler lifetime.
        var postsApiBaseUrl = builder.Configuration["PostsApi:BaseUrl"]
            ?? throw new InvalidOperationException("Missing configuration value 'PostsApi:BaseUrl'.");
        builder.Services.AddHttpClient<IPostService, PostService>(client =>
        {
            client.BaseAddress = new Uri(postsApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Allow the Nuxt front end (a different origin) to call this API from the browser.
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(WebAppCorsPolicy, policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .WithMethods("GET"));
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseCors(WebAppCorsPolicy);

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
