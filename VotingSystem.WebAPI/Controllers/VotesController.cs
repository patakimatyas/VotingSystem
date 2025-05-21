using Microsoft.AspNetCore.Mvc;
using VotingSystem.DataAccess.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using VotingSystem.Shared.DTOs;

namespace VotingSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotesController : Controller
    {
        private readonly IVoteService _voteService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public VotesController(IVoteService voteService, IUserService userService, IMapper mapper)
        {
            _voteService = voteService;
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>
        /// Casts a vote for a specific option in a poll.
        /// </summary>
        /// <param name="dto">The vote request containing PollId and OptionId.</param>
        /// <response code="204">Vote recorded successfully; no content returned.</response>
        /// <response code="400">Bad request, e.g. invalid poll, closed poll, invalid option, or duplicate vote.</response>
        /// <response code="401">Unauthorized if the user is not authenticated.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // POST: api/votes
        public async Task<IActionResult> Vote([FromBody] VoteRequestDTO dto)
        {
            var userId = _userService.GetCurrentUserId();

            try
            {
                await _voteService.VoteAsync(dto.PollId, dto.OptionId, userId!);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
