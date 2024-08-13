namespace CityInfo.API.Models;

public class CitiesDataStore
{
    public List<CityDto> Cities { get; set; }

    public CitiesDataStore()
    {
        Cities = new List<CityDto>()
        {
            new()
            {
                Id = 1,
                Name = "Paris",
                Description = "The one with that big tower",
                PointsOfInterest = new() {
                    new PointOfInterestDto()
                    {
                        Id = 1,
                        Name = "Eiffel Tower",
                        Description = "A wrought iron lattice tower on the Champ de Mars." 
                    },
                    new PointOfInterestDto()
                    {
                        Id = 2,
                        Name = "The Louvre",
                        Description = "The world's largest museum."
                    }
                }
            },
            new()
            {
                Id = 2,
                Name = "Prague",
                Description = "The capital city of Czechia.",
                PointsOfInterest = new()
                {
                    new PointOfInterestDto()
                    {
                        Id = 3,
                        Name = "Charles bridge",
                        Description = "Charles Bridge is a medieval stone arch bridge that crosses the Vltava river"
                    }
                }
                
            }
        };
    }
}