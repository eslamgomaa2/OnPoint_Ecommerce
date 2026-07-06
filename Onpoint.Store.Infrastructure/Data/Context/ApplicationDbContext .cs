using Microsoft.EntityFrameworkCore;

namespace Onpoint.Store.Infrastructure.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            
        }

       

       
    }
}
