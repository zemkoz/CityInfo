using CityInfo.API.Entities;
using CityInfo.API.Models;

namespace CityInfo.API.Services;

public interface ICityInfoRepository
{
    Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize);
    
    Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);
    
    Task<bool> DoesExistCityByIdAndName(int cityId, string cityName);
    
    Task<bool> DoesCityExistsAsync(int cityId);
    
    Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);
    
    Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId);
    
    Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);

    Task<bool> SaveChanges(); 
    
    void DeletePointOfInterest(PointOfInterest pointOfInterest);
}