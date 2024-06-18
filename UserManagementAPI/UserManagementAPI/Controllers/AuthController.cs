using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly UserService _userService;
		private readonly IConfiguration _configuration;

		public AuthController(UserService userService, IConfiguration configuration)
		{
			_userService = userService;
			_configuration = configuration;
		}

		[HttpPost("register")]
		public IActionResult Register(User user)
		{
			var existingUser = _userService.GetByEmail(user.Email);
			if (existingUser != null)
			{
				return Conflict("Email already in use");
			}

			_userService.Create(user);

			return Ok();
		}

		[HttpPost("login")]
		public IActionResult Login([FromBody] UserRequest login)
		{
			var user = _userService.GetByEmail(login.Email);
			

			if (user == null)
			{
				return Unauthorized("No user found");
			}
			else if (user.Password != login.Password)
			{
				return Unauthorized("Invalid password!");
			}
			

			var token = GenerateJwtToken(user);
			
			return Ok(new { token });
		}

		private string GenerateJwtToken(User user)
		{
			var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddDays(_configuration.GetValue<int>("Jwt:ExpiryDays")),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
