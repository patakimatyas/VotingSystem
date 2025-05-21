using AutoMapper;
using VotingSystem.Blazor.Infrastructure;
using VotingSystem.Blazor.ViewModels;
using VotingSystem.Shared.DTOs;

namespace VotingSystem.Blazor.Services
{
    public class PollService : IPollService
    {
        private readonly IHttpRequestUtility _http;
        private readonly IMapper _mapper;

        public PollService(IHttpRequestUtility http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<List<PollViewModel>> GetMyPollsAsync()
        {
            var response = await _http.ExecuteGetHttpRequestAsync<List<PollResponseDTO>>("api/polls/mine");
            return _mapper.Map<List<PollViewModel>>(response);
        }

        public async Task<PollDetailsViewModel?> GetPollAsync(string pollId)
        {
            var dto = await _http.ExecuteGetHttpRequestAsync<PollResponseDTO>($"api/polls/{pollId}");
            return dto == null ? null : _mapper.Map<PollDetailsViewModel>(dto);
        }

        public async Task<bool> CreatePollAsync(CreatePollViewModel viewModel)
        {
            try
            {
                var dto = _mapper.Map<PollRequestDTO>(viewModel);
                await _http.ExecutePostHttpRequestAsync<PollRequestDTO>("api/polls/create", dto);
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
