using CityInfo.API.Entities;

namespace CityInfo.API.Services;

public interface ICityInfoRepository
{
    Task<IEnumerable<City>> GetCitiesAsync();
    
    Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);
    
    Task<bool> DoesCityExistsAsync(int cityId);
    
    Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);
    
    Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId);
    
    Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);

    Task<bool> SaveChanges(); 
    
    void DeletePointOfInterest(PointOfInterest pointOfInterest);
}