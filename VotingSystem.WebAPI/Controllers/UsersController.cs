using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotingSystem.DataAccess.Services;
using VotingSystem.Shared.DTOs;
using VotingSystem.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace VotingSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="dto">User registration details: email, username, and password.</param>
        /// <response code="201">User created successfully; returns the created user DTO.</response>
        /// <response code="400">Bad request, e.g. invalid email/username or password.</response>
        /// <response code="409">Conflict, e.g. email or username already in use.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponseDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRequestDTO dto)
        {
            try
            {
                var user = _mapper.Map<ApplicationUser>(dto);

                await _userService.RegisterUserAsync(user, dto.Password);

                var response = _mapper.Map<UserResponseDTO>(user);

                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT + refresh token.
        /// </summary>
        /// <param name="dto">Login credentials: email and password.</param>
        /// <response code="200">Login successful; returns authentication tokens and user ID.</response>
        /// <response code="400">Bad request, e.g. missing email or password.</response>
        /// <response code="403">Forbidden, e.g. invalid credentials or account locked.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            try
            {
                var (authToken, refreshToken, userId) =
                    await _userService.LoginAsync(dto.Email, dto.Password);

                var response = new LoginResponseDTO
                {
                    AuthToken = authToken,
                    RefreshToken = refreshToken,
                    UserId = userId
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AccessViolationException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }


        }

        /// <summary>
        /// Logs out the current user by invalidating their refresh token.
        /// </summary>
        /// <response code="204">Logout successful; no content returned.</response>
        /// <response code="401">Unauthorized if the user is not authenticated.</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();

            return NoContent();
        }
    }
}
