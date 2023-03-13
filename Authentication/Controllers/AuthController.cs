using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication.Dtos;
using Configuration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtBearerConfiguration _jwtBearerTokenSettings;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthController(IOptions<JwtBearerConfiguration> jwtTokenOptions, UserManager<IdentityUser> userManager)
    {
        _jwtBearerTokenSettings = jwtTokenOptions.Value;
        _userManager = userManager;
    }

    // create new account
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] Register userData)
    {
        IdentityUser identityUser = new() { UserName = userData.UserName, Email = userData.Email };
        var result = await _userManager.CreateAsync(identityUser, userData.Password);

        if (!result.Succeeded)
        {
            List<string> errors = new();

            foreach (var error in result.Errors)
            {
                errors.Add(error.Description);
            }

            return BadRequest(new { Errors = errors });
        }

        return Ok();
    }

    // logs in to existing account
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] Login userData)
    {
        // get user
        var user = await _userManager.FindByNameAsync(userData.UserName);

        // user does not exist
        if (user == null)
        {
            return NotFound();
        }

        // incorrect password
        if (_userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, userData.Password) ==
            PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        // generate token
        var token = GenerateToken(user);

        return Ok(new { Token = token });
    }

    // generates jwt token
    private string GenerateToken(IdentityUser identityUser)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtBearerTokenSettings.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new(ClaimTypes.NameIdentifier, identityUser.Id),
                new Claim(ClaimTypes.Name, identityUser.UserName),
                new Claim(ClaimTypes.Email, identityUser.Email)
            }),

            Expires = DateTime.UtcNow.AddSeconds(_jwtBearerTokenSettings.ExpiryTimeInSeconds),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Audience = _jwtBearerTokenSettings.Audience,
            Issuer = _jwtBearerTokenSettings.Issuer
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}