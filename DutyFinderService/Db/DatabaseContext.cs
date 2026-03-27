using DutyFinderService.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace DutyFinderService.Db;

public class DatabaseContext : DbContext
{
    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    #region DbSets

    internal DbSet<Image> Images => Set<Image>();

    #endregion

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var conn = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);
            optionsBuilder.UseNpgsql(conn).UseSnakeCaseNamingConvention();
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly); // load models from assembly
        modelBuilder.UseIdentityAlwaysColumns(); // always generate identity column, do not allow user values

        base.OnModelCreating(modelBuilder);
    }
}