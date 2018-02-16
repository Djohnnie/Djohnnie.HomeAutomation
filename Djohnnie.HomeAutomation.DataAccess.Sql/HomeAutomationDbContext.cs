using Djohnnie.HomeAutomation.DataAccess.Sql.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Djohnnie.HomeAutomation.DataAccess.Sql
{
    public class HomeAutomationDbContext : DbContext
    {
        public DbSet<Light> Lights { get; set; }
        public DbSet<Plug> Plugs { get; set; }
        public DbSet<PowerMeter> PowerMeters { get; set; }
        public DbSet<Thermostat> Thermostats { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<Light> lightBuilder = modelBuilder.Entity<Light>().ToTable("LIGHTS");
            lightBuilder.HasKey(e => e.Id).ForSqlServerIsClustered(false);
            lightBuilder.Property(e => e.SysId).ValueGeneratedOnAdd();
            lightBuilder.HasIndex(e => e.SysId).IsUnique().ForSqlServerIsClustered(true);

            EntityTypeBuilder<Plug> plugBuilder = modelBuilder.Entity<Plug>().ToTable("PLUGS");
            plugBuilder.HasKey(e => e.Id).ForSqlServerIsClustered(false);
            plugBuilder.Property(e => e.SysId).ValueGeneratedOnAdd();
            plugBuilder.HasIndex(e => e.SysId).IsUnique().ForSqlServerIsClustered(true);

            EntityTypeBuilder<PowerMeter> powerMeterBuilder = modelBuilder.Entity<PowerMeter>().ToTable("POWERMETERS");
            powerMeterBuilder.HasKey(e => e.Id).ForSqlServerIsClustered(false);
            powerMeterBuilder.Property(e => e.SysId).ValueGeneratedOnAdd();
            powerMeterBuilder.HasIndex(e => e.SysId).IsUnique().ForSqlServerIsClustered(true);

            EntityTypeBuilder<Thermostat> thermostatBuilder = modelBuilder.Entity<Thermostat>().ToTable("THERMOSTATS");
            thermostatBuilder.HasKey(e => e.Id).ForSqlServerIsClustered(false);
            thermostatBuilder.Property(e => e.SysId).ValueGeneratedOnAdd();
            thermostatBuilder.HasIndex(e => e.SysId).IsUnique().ForSqlServerIsClustered(true);
        }
    }
}