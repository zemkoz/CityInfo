using System.Collections;
using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services;

public class CityInfoRepository : ICityInfoRepository
{
    private readonly CityInfoDbContext _dbContext;

    public CityInfoRepository(CityInfoDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(
        string? name, 
        string? searchQuery, 
        int pageNumber, 
        int pageSize)
    {
        var query = _dbContext.Cities as IQueryable<City>;
        if (!string.IsNullOrWhiteSpace(name))
        {
            name = name.Trim();
            query = query.Where(c => c.Name == name);
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            searchQuery = searchQuery.Trim();
            query = query.Where(a => a.Name.Contains(searchQuery)
                || a.Description != null && a.Description.Contains(searchQuery));
        }

        var totalItemCount = await query.CountAsync();
        var cities = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (cities, new PaginationMetadata(totalItemCount, pageSize, pageNumber));
    }

    public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
    {
        if (includePointsOfInterest)
        {
            return await _dbContext.Cities
                .Include(c => c.PointsOfInterest)
                .FirstOrDefaultAsync(c => c.Id == cityId);
        }

        return await _dbContext.Cities
            .FirstOrDefaultAsync(c => c.Id == cityId);
    }

    public async Task<bool> DoesCityExistsAsync(int cityId)
    {
        return await _dbContext.Cities
            .AnyAsync(c => c.Id == cityId);
    }

    public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
    {
        return await _dbContext.PointsOfInterest
            .Where(p => p.CityId == cityId)
            .ToListAsync();
    }

    public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
    {
        return await _dbContext.PointsOfInterest
            .FirstOrDefaultAsync(p => p.CityId == cityId && p.Id == pointOfInterestId);
    }

    public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
    {
        var city = await GetCityAsync(cityId, false);
        if (city != null)
        {
            city.PointsOfInterest.Add(pointOfInterest);
        }
    }

    public void DeletePointOfInterest(PointOfInterest pointOfInterest)
    {
        _dbContext.PointsOfInterest.Remove(pointOfInterest);
    }

    public async Task<bool> SaveChanges()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }
}