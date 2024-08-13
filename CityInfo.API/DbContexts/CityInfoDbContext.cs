using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.DbContexts;

public class CityInfoDbContext : DbContext
{
    public DbSet<City> Cities { get; set; }
    public DbSet<PointOfInterest> PointsOfInterest { get; set; }

    public CityInfoDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>().HasData(
            new City("Paris")
            {
                Id = 1,
                Description = "The one with that big tower"
            },
            new City("Prague")
            {
                Id = 2,
                Description = "The capital city of Czechia."
            });
        modelBuilder.Entity<PointOfInterest>().HasData(
            new PointOfInterest("Eiffel Tower")
            {
                Id = 1,
                CityId = 1,
                Description = "A wrought iron lattice tower on the Champ de Mars."
            },
            new PointOfInterest("The Louvre")
            {
                Id = 2,
                CityId = 1,
                Description = "The world's largest museum."
            },
            new PointOfInterest("Charles bridge")
            {
                Id = 3,
                CityId = 2,
                Description = "Charles Bridge is a medieval stone arch bridge that crosses the Vltava river"
            }
        );
        
        base.OnModelCreating(modelBuilder);
    }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseSqlite("connectionstring");
    //     base.OnConfiguring(optionsBuilder);
    // }
}