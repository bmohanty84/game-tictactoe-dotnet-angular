using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // Keep enums as readable strings in JSON (e.g. "InProgress" instead of 0)
    opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Swagger / OpenAPI (helps the panel explore the API contract quickly)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Game + scoreboard state is in-memory and lives for the lifetime of the process,
// so both services are registered as singletons and shared across all requests.
builder.Services.AddSingleton<IScoreboardService, ScoreboardService>();
builder.Services.AddSingleton<IGameService, GameService>();

// Allow the local Angular dev server (ng serve, default port 4200) to call this API.
const string CorsPolicy = "AllowAngularDevClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory<Program> can be used from integration tests, if added later.
public partial class Program { }
