using Microsoft.EntityFrameworkCore;
using CartonCapsApi.Data.Entities;

namespace CartonCapsApi.Data
{
    public class ReferralContext : DbContext
    {
        public ReferralContext(DbContextOptions<ReferralContext> options) : base(options) { }

        public DbSet<ReferredUser> ReferredUsers { get; set; } = default!;
    }
}
