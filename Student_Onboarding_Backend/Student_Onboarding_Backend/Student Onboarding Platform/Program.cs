using Serilog;
using Student_Onboarding_Platform.Extensions;
using Student_Onboarding_Platform.Middleware;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Disable file-watching reloadOnChange to avoid inotify limits in containers
builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Render sets PORT env variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "StudentOnboardingPlatform")
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application services (DI registration)
builder.Services.AddApplicationServices(builder.Configuration);

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// FluentValidation
builder.Services.AddFluentValidationServices();

// CORS (allow mobile app and web frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();
// HTTPS redirect disabled — Render handles SSL at proxy level
//if (!app.Environment.IsProduction())
    //app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve uploaded photos from wwwroot
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();




Log.Information("Student Onboarding Platform API starting...");
app.Run();
