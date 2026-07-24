using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface IOrganizationSettingsRepository
{
    Task<OrganizationSettings?> GetAsync();
    Task UpdateAsync(OrganizationSettings settings);
}
