using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AS.Service.Identity
{
    public class IdentityAppDbContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityAppDbContext(DbContextOptions<IdentityAppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
