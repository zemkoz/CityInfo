﻿using CityInfo.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;

[ApiController]
[Route("api/cities/{cityId}/pointsofinterest")]
public class PointsOfInterestController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<PointOfInterestDto>> GetAllPointsOfInterest(int cityId)
    {
        var city = CitiesDataStore.Current.Cities
            .FirstOrDefault(c => c.Id == cityId);
        
        if (city == null)
        {
            return NotFound();
        }
        return Ok(city.PointsOfInterest);
    }
    
    [HttpGet("{pointOfInterestId}")]
    public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
    {
        var city = CitiesDataStore.Current.Cities
            .FirstOrDefault(c => c.Id == cityId);
        
        if (city == null)
        {
            return NotFound();
        }
        
        var pointOfInterest = city.PointsOfInterest
            .FirstOrDefault(p => p.Id == pointOfInterestId);
        if (pointOfInterest == null)
        {
            return NotFound();
        }

        return Ok(pointOfInterest);
    }
    
    [HttpPost]
    public ActionResult<PointOfInterestDto> CreatePointOfInterest(
        int cityId, 
        [FromBody] PointOfInterestForCreationDto pointOfInterest)
    {
        
        var city = CitiesDataStore.Current.Cities
            .FirstOrDefault(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }
        
        var maxPointOfInterestId = CitiesDataStore.Current.Cities
            .SelectMany(c => c.PointsOfInterest)
            .Max(p => p.Id);

        var finalPointOfInterest = new PointOfInterestDto()
        {
            Id = ++maxPointOfInterestId,
            Name = pointOfInterest.Name,
            Description = pointOfInterest.Description

        };
        
        city.PointsOfInterest.Add(finalPointOfInterest);
        
        return CreatedAtAction(
            "GetPointOfInterest",
            new { cityId, pointOfInterestId = finalPointOfInterest.Id },
            finalPointOfInterest);
    }
    
    [HttpPut("{pointOfInterestId}")]
    public ActionResult UpdatePointOfInterest(
        int cityId, 
        int pointOfInterestId,
        [FromBody] PointOfInterestForUpdateDto pointOfInterest)
    {
        var city = CitiesDataStore.Current.Cities
            .FirstOrDefault(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }
        
        var pointOfInterestFromStore = city.PointsOfInterest
            .FirstOrDefault(p => p.Id == pointOfInterestId);
        if (pointOfInterestFromStore == null)
        {
            return NotFound();
        }
        
        pointOfInterestFromStore.Name = pointOfInterest.Name;
        pointOfInterestFromStore.Description = pointOfInterest.Description;

        return NoContent();
    }
    
    [HttpPatch("{pointOfInterestId}")]
    public ActionResult PartiallyUpdatePointOfInterest(
        int cityId, 
        int pointOfInterestId,
        [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
    {
        var city = CitiesDataStore.Current.Cities
            .FirstOrDefault(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }
        
        var pointOfInterestFromStore = city.PointsOfInterest
            .FirstOrDefault(p => p.Id == pointOfInterestId);
        if (pointOfInterestFromStore == null)
        {
            return NotFound();
        }
        
        var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
        {
            Name = pointOfInterestFromStore.Name,
            Description = pointOfInterestFromStore.Description
        };
        
        patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);
        if (!ModelState.IsValid || !TryValidateModel(pointOfInterestToPatch))
        {
            return BadRequest(ModelState);
        }
        
        pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
        pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;
        
        return NoContent();
    }
    
    [HttpDelete("{pointOfInterestId}")]
    public ActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
    {
        var city = CitiesDataStore.Current.Cities
            .FirstOrDefault(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }
        
        var pointOfInterestFromStore = city.PointsOfInterest
            .FirstOrDefault(p => p.Id == pointOfInterestId);
        if (pointOfInterestFromStore == null)
        {
            return NotFound();
        }
        
        city.PointsOfInterest.Remove(pointOfInterestFromStore);
        return NoContent();
    }
}