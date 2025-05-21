using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VotingSystem.DataAccess.Models;
using System.Data;
using VotingSystem.DataAccess.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace VotingSystem.DataAccess.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = new HttpContextAccessor();
        }

        public async Task RegisterUserAsync(ApplicationUser user, string password)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("User must have an email.", nameof(user.Email));
            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new ArgumentException("User must have a user name.", nameof(user.UserName));

            user.RefreshToken = Guid.NewGuid();

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
                return;

            // We failed—decide which exception to throw.
            var error = result.Errors.First();
            // duplicate‐user errors come back with Codes like "DuplicateUserName" or "DuplicateEmail"
            if (error.Code?.StartsWith("Duplicate", StringComparison.OrdinalIgnoreCase) == true)
            {
                // this will be caught by your controller’s `catch(InvalidOperationException)`
                throw new InvalidOperationException(error.Description);
            }
            else
            {
                // all other Identity failures are bad requests
                throw new ArgumentException(error.Description);
            }

        }

        public async Task<(string authToken, string refreshToken, string userId)> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new AccessViolationException("Email or password is invalid");

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, false, true);
            if (result.IsLockedOut)
                throw new AccessViolationException("Too many failed attempt. User is locked out");
            if (!result.Succeeded)
                throw new AccessViolationException("Email or password is invalid");

            var accessToken = await GenerateJwtTokenAsync(user);

            return (accessToken, user.RefreshToken.ToString()!, user.Id);
        }


        public async Task LogoutAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return;

            await _signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return null;

            return await _userManager.FindByIdAsync(userId);
        }

        public string? GetCurrentUserId()
        {
            var id = _httpContextAccessor.HttpContext?.User.FindFirstValue("id");
            if (id == null)
                return null;

            return id;
        }

        public async Task<List<ApplicationUser>> GetAllAsync()
        {
            var res = await _userManager.Users.ToListAsync();
            return res;
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
                throw new InvalidOperationException("JWT SecretKey is not configured.");

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Sub, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("id", user.Id),
            new("username", user.UserName!),
        };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
