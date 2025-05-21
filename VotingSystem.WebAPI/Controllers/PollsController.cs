using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotingSystem.DataAccess.Services;
using AutoMapper;
using VotingSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Formats.Asn1;
using Azure.Core;
using VotingSystem.DataAccess.Models;
using Microsoft.AspNetCore.Identity;

namespace VotingSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PollsController : ControllerBase
    {
        private readonly IPollService _pollService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;


        public PollsController(IPollService pollService,IMapper mapper, IUserService userService)
        {
            _pollService = pollService;
            _mapper = mapper;
            _userService = userService;
        }

        /// <summary>
        /// Returns a single poll by its ID, including vote status.
        /// Owners also see the full voter list.
        /// </summary>
        /// <param name="id">The poll identifier.</param>
        /// <returns>The poll details.</returns>
        /// <response code="200">Returns the poll.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If no poll with the given ID exists.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PollResponseDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // GET: api/polls/{id}
        public async Task<ActionResult<PollResponseDTO>> GetPollById(int id)
        {
            var userId = _userService.GetCurrentUserId();

            var poll = await _pollService.GetByIdAsync(id);
            if (poll == null) return NotFound();

            var hasVoted = poll.Voters.Any(v => v.UserId == userId);

             var optionsResult = poll.Options.Select(option => new OptionResponseDTO
            {
                Id = option.Id,
                Text = option.Text,
                VoteCount = poll.Votes.Count(v => v.OptionId == option.Id)
            }).ToList();

            var result = _mapper.Map<PollResponseDTO>(poll);
            result.HasVoted = hasVoted;
            result.Options = optionsResult;


            var ownedPolls = await _pollService.GetPollsByUserAsync(userId!);
            bool userOwnsPoll = ownedPolls.Any(p => p.Id == id);

            if (userOwnsPoll)
            {
                var allUsers = await _userService.GetAllAsync();
                var votedUserIds = poll.Voters.Select(v => v.UserId).ToHashSet();

                result.Voters = allUsers.Select(user => new VoterStatusDTO
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    HasVoted = votedUserIds.Contains(user.Id)
                }).ToList();
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns only polls that are currently active (start ≤ now ≤ end).
        /// </summary>
        /// <returns>List of active polls, each with HasVoted flag.</returns>
        /// <response code="200">Returns the list of active polls.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PollResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // GET: api/polls/active
        public async Task<ActionResult<List<PollResponseDTO>>> GetActivePolls()
        {
            var userId = _userService.GetCurrentUserId();

            var polls = await _pollService.GetActivePollsAsync();
            var result = _mapper.Map<List<PollResponseDTO>>(polls);

            foreach (var pollDto in result)
            {
                // Find poll in loaded entities, match by ID
                var poll = polls.FirstOrDefault(p => p.Id == pollDto.Id);
                pollDto.HasVoted = poll?.Voters.Any(v => v.UserId == userId) ?? false;
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns polls that have ended (EndDate &lt; now), optionally filtered.
        /// </summary>
        /// <param name="text">Optional substring to match in question.</param>
        /// <param name="from">Optional earliest end‐date (inclusive).</param>
        /// <param name="to">Optional latest end‐date (inclusive).</param>
        /// <returns>List of closed polls matching filters.</returns>
        /// <response code="200">Returns the list of closed polls.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("closed")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PollResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // GET: api/polls/closed
        public async Task<ActionResult<List<PollResponseDTO>>> GetClosedPolls
            (
            [FromQuery] string? text,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to
            )
        {

            var polls = await _pollService.GetClosedPollsAsync(text, from, to);
            var result = _mapper.Map<List<PollResponseDTO>>(polls);
            return Ok(result);
        }

        /// <summary>
        /// Returns detailed results for a single closed poll.
        /// </summary>
        /// <param name="id">The poll ID. Poll must be closed.</param>
        /// <returns>Closed‐poll result including vote percentages.</returns>
        /// <response code="200">Returns the closed poll result.</response>
        /// <response code="404">If the poll does not exist or is not closed yet.</response>
        [HttpGet("closed/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClosedPollResponseDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClosedPollResponseDTO>> GetClosedPollResult(int id)
        {
            var poll = await _pollService.GetByIdAsync(id);
            if (poll == null || poll.EndDate > DateTime.UtcNow)
                return NotFound();

            var totalVotes = poll.Votes.Count;

            var optionsResult = poll.Options.Select(option => new OptionResponseDTO
            {
                Id = option.Id,
                Text = option.Text,
                VoteCount = poll.Votes.Count(v => v.OptionId == option.Id),
                VotePercentage = totalVotes == 0 ? 0 : Math.Round((double)poll.Votes.Count(v => v.OptionId == option.Id) / totalVotes * 100, 2)
            }).ToList();

            var result = new ClosedPollResponseDTO
            {
                Id = poll.Id,
                Question = poll.Question,
                StartDate = poll.StartDate,
                EndDate = poll.EndDate,
                Options = optionsResult
            };

            return Ok(result);
        }

        /// <summary>
        /// Returns the polls created by the current user.
        /// </summary>
        /// <returns>List of polls owned by you.</returns>
        /// <response code="200">Returns the list of your polls.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("mine")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PollResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // GET: api/polls/mine
        public async Task<ActionResult<List<PollResponseDTO>>> GetMyPolls()
        {
            var userId = _userService.GetCurrentUserId();

            var polls = await _pollService.GetPollsByUserAsync(userId!);
            var result = _mapper.Map<List<PollResponseDTO>>(polls);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new poll under your user account.
        /// </summary>
        /// <param name="dto">The question text and list of options.</param>
        /// <response code="200">Poll created successfully.</response>
        /// <response code="400">Bad request, e.g. model validation failed.</response>
        /// <response code="401">If you are not authenticated.</response>
        /// <response code="500">Unexpected server error.</response>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePoll([FromBody] PollRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            try
            {

                var poll = _mapper.Map<Poll>(dto);
                poll.CreatedByUserId = userId;
                poll.IsClosed = false;

                await _pollService.CreatePollAsync(poll);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");

            }
        }



    }
}
