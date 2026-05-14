using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Npgsql;
using Student_Onboarding_Platform.Models.Settings;

namespace Student_Onboarding_Platform.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;
    private readonly bool _isProduction;

    public DbConnectionFactory(IOptions<AppSettings> appSettings, IConfiguration configuration)
    {
        _isProduction = appSettings.Value.IsProduction;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string is not configured.");
    }

    public IDbConnection CreateConnection()
    {
        if (_isProduction)
            return new NpgsqlConnection(_connectionString);

        return new SqlConnection(_connectionString);
    }

    public bool IsProduction => _isProduction;
}
