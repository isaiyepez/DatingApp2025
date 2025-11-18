using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPI.Data;
using RestAPI.DTOs;
using RestAPI.Entities;
using RestAPI.Extensions;
using RestAPI.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace RestAPI.Controllers
{
    
    public class AccountController(AppDbContext dbContext, ITokenService tokenService) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Email))
            {
                return BadRequest("Email is already taken");
            }

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            dbContext.Users.Add(user);

            await dbContext.SaveChangesAsync();

            //TODO : muestra a Brian esta sintaxis
            return new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
           var user = await dbContext.Users
                .SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }

            return user.ToDto(tokenService);
        }

        private async Task<bool> UserExists(string email)
        {
            return await dbContext.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}
