using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Models
{
    public class ApplicationDbContext : DbContext
    {
        public static string LocalDatabaseName { get; set; } = "Emaildb";
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public ApplicationDbContext() { }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            Configure(options);
        }
        public static void Configure(DbContextOptionsBuilder options)
        {
            options.UseLazyLoadingProxies();
            options.UseSqlServer($"Server=(localdb)\\mssqllocaldb;" +
                                 $"Database={Directory.GetCurrentDirectory()}\\{LocalDatabaseName};" +
                                 $"Trusted_Connection=True;");
        }
    }
}

