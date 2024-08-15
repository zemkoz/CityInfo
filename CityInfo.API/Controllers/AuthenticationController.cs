using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CityInfo.API.Controllers;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthenticationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("authenticate")]
    public ActionResult<string> Authenticate(AuthenticationRequestDto request)
    {
        var user = ValidateUser(request.Username, request.Password);
        if (user == null)
        {
            return Unauthorized();
        }
        
        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:SecretKey"]));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _configuration["Authentication:Issuer"],
            audience: _configuration["Authentication:Audience"],
            claims: new[]
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("given_name", user.FirstName),
                new Claim("family_name", user.LastName),
                new Claim("city", user.City)
            },
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: signingCredentials
        );

        var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        
        return Ok(tokenToReturn);
    }

    private CityInfoUser? ValidateUser(string? username, string? password)
    {
        // Because we have no database, we'll just return a dummy user
        return new CityInfoUser(
            1,
            username ?? "",
            "John",
            "Doe",
            "Prague"
        );
    }
}