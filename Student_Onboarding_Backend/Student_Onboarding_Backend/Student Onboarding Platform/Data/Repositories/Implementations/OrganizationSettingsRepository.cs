using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class OrganizationSettingsRepository : IOrganizationSettingsRepository
{
    private readonly DbConnectionFactory _db;

    public OrganizationSettingsRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<OrganizationSettings?> GetAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<OrganizationSettings>(
            "SELECT * FROM OrganizationSettings ORDER BY UpdatedAt DESC OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY");
    }

    public async Task UpdateAsync(OrganizationSettings s)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            UPDATE OrganizationSettings SET
                OrgName = @OrgName, AddressLine1 = @AddressLine1, AddressLine2 = @AddressLine2,
                City = @City, State = @State, PostalCode = @PostalCode, Country = @Country,
                Phone = @Phone, Email = @Email, Website = @Website, TaxRegNo = @TaxRegNo,
                LogoUrl = @LogoUrl, CurrencyCode = @CurrencyCode, CurrencySymbol = @CurrencySymbol,
                DefaultTaxPercent = @DefaultTaxPercent, InvoicePrefix = @InvoicePrefix,
                DefaultNotes = @DefaultNotes, DefaultTerms = @DefaultTerms, FooterNote = @FooterNote,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id", s);
    }
}
