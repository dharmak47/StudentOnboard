using System;
using System.IO;
using Npgsql;

class Program
{
    static void Main(string[] args)
    {
        string connString = "Host=aws-1-ap-south-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.fdlpaolzdtpkrichitzn;Password=dharmaraj2026;SSL Mode=Require;Trust Server Certificate=true";
        string scriptPath = @"C:\Users\HP\Desktop\Downloads\what\StudentOnboard-Repo\Student_Onboarding_Backend\Student_Onboarding_Backend\sql\postgres\_run_all_postgres.sql";

        Console.WriteLine("Reading SQL script...");
        string script = File.ReadAllText(scriptPath);

        Console.WriteLine("Connecting to Supabase...");
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        Console.WriteLine("Executing script...");
        using var cmd = new NpgsqlCommand(script, conn);
        cmd.ExecuteNonQuery();

        Console.WriteLine("Script executed successfully!");
    }
}
