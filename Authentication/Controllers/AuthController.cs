using Authentication.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Configuration.Models;

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

    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register([FromBody] AuthRegister userData)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        IdentityUser identityUser = new IdentityUser { UserName = userData.UserName, Email = userData.Email };
        IdentityResult? result = await _userManager.CreateAsync(identityUser, userData.Password);

        if (!result.Succeeded)
        {
            List<string> errors = new List<string>();

            foreach (IdentityError? error in result.Errors)
            {
                errors.Add(error.Description);
            }

            return BadRequest(new { Errors = errors });
        }

        return Ok();
    }

    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] AuthLogin userData)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        IdentityUser? user = await _userManager.FindByNameAsync(userData.UserName);

        if (user == null)
        {
            return NotFound();
        }

        if (_userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, userData.Password) ==
            PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        var token = GenerateToken(user);
        return Ok(new { Token = token });
    }

    private string GenerateToken(IdentityUser identityUser)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtBearerTokenSettings.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, identityUser.Id),
                new Claim(ClaimTypes.Name, identityUser.UserName),
                new Claim(ClaimTypes.Email, identityUser.Email)
            }),

            Expires = DateTime.UtcNow.AddSeconds(_jwtBearerTokenSettings.ExpiryTimeInSeconds),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Audience = _jwtBearerTokenSettings.Audience,
            Issuer = _jwtBearerTokenSettings.Issuer
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}