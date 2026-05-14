using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Student_Onboarding_Platform.Data;
using Student_Onboarding_Platform.Data.Repositories.Implementations;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Settings;
using Student_Onboarding_Platform.Services.Implementations;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Settings
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.Configure<OtpSettings>(configuration.GetSection("OtpSettings"));
        services.Configure<BytescaleSettings>(configuration.GetSection("BytescaleSettings"));

        // Data
        services.AddSingleton<DbConnectionFactory>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ICourseRegistrationRepository, CourseRegistrationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICourseReviewRepository, CourseReviewRepository>();
        services.AddScoped<IFaqRepository, FaqRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddHttpClient("SendGrid");
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ILoginAttemptService, LoginAttemptService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFaqService, FaqService>();
        services.AddHttpClient("Bytescale");
        services.AddScoped<IFileStorageService, BytescaleStorageService>();

        // Background services
        services.AddHostedService<BirthdayNotificationService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings is not configured.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }
}
