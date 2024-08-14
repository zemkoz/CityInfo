using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;

[ApiController]
[Route("api/cities/{cityId}/pointsofinterest")]
public class PointsOfInterestController : ControllerBase
{
    private readonly ILogger<PointsOfInterestController> _logger;
    private readonly ICityInfoRepository _cityInfoRepository;
    private readonly IMailService _mailService;
    private readonly IMapper _mapper;

    public PointsOfInterestController(
        ILogger<PointsOfInterestController> logger,
        ICityInfoRepository cityInfoRepository,
        IMailService mailService, 
        IMapper mapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
        _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    public async Task<ActionResult<List<PointOfInterestDto>>> GetAllPointsOfInterest(int cityId)
    {
        if (!await _cityInfoRepository.DoesCityExistsAsync(cityId))
        {
            _logger.LogDebug("City with id {cityId} wasn't found when accessing points of interest.", cityId);
            return NotFound();
        }
        
        var pointsOfInterest = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
        return Ok(_mapper.Map<List<PointOfInterestDto>>(pointsOfInterest));
    }
    
    [HttpGet("{pointOfInterestId}")]
    public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointOfInterestId)
    {
        if (!await _cityInfoRepository.DoesCityExistsAsync(cityId))
        {
            return NotFound();
        }
        
        var pointOfInterest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
        if (pointOfInterest == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
    }
    
    [HttpPost]
    public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(
        int cityId, 
        [FromBody] PointOfInterestForCreationDto pointOfInterest)
    {
        if (!await _cityInfoRepository.DoesCityExistsAsync(cityId))
        {
            return NotFound();
        }
        
        var finalPointOfInterest = _mapper.Map<PointOfInterest>(pointOfInterest);
        await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
        await _cityInfoRepository.SaveChanges();
        
        var createPointOfInterestDto = _mapper.Map<PointOfInterestDto>(finalPointOfInterest); 
        
        return CreatedAtAction(
            "GetPointOfInterest",
            new { cityId, pointOfInterestId = finalPointOfInterest.Id },
            createPointOfInterestDto);
    }
    
    [HttpPut("{pointOfInterestId}")]
    public async Task<ActionResult> UpdatePointOfInterest(
        int cityId, 
        int pointOfInterestId,
        [FromBody] PointOfInterestForUpdateDto pointOfInterest)
    {
        if (!await _cityInfoRepository.DoesCityExistsAsync(cityId))
        {
            return NotFound();
        }
        
        var pointOfInterestEntity = 
            await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
        if (pointOfInterestEntity == null)
        {
            return NotFound();
        }

        _mapper.Map(pointOfInterest, pointOfInterestEntity);
        await _cityInfoRepository.SaveChanges();

        return NoContent();
    }
    
    [HttpPatch("{pointOfInterestId}")]
    public async Task<ActionResult> PartiallyUpdatePointOfInterest(
        int cityId, 
        int pointOfInterestId,
        [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
    {
        if (!await _cityInfoRepository.DoesCityExistsAsync(cityId))
        {
            return NotFound();
        }
        
        var pointOfInterestEntity = 
            await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
        if (pointOfInterestEntity == null)
        {
            return NotFound();
        }

        var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);
        patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);
        if (!ModelState.IsValid || !TryValidateModel(pointOfInterestToPatch))
        {
            return BadRequest(ModelState);
        }
        
        _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);
        await _cityInfoRepository.SaveChanges();
        
        return NoContent();
    }
    
    [HttpDelete("{pointOfInterestId}")]
    public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
    {
        if (!await _cityInfoRepository.DoesCityExistsAsync(cityId))
        {
            return NotFound();
        }
        
        var pointOfInterestEntity = 
            await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
        if (pointOfInterestEntity == null)
        {
            return NotFound();
        }
        
        _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
        await _cityInfoRepository.SaveChanges();
        
        _mailService.Send("Point of interest deleted.", 
            $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");
        return NoContent();
    }
}