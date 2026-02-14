using Microsoft.EntityFrameworkCore;
using StudentOnboard.Api.Models;

namespace StudentOnboard.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<StudentProfile> StudentProfiles { get; set; }
    }
}
