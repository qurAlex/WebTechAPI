using Microsoft.EntityFrameworkCore;
using WebTechAPI.Models;

namespace WebTechAPI
{
    internal class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=SQL5111.site4now.net;Initial Catalog=db_a9fe5b_allekseiii0;
                                        user id=db_a9fe5b_allekseiii0_admin;
                                        pwd=webtechDB2; ");
            
        }
    }
}
