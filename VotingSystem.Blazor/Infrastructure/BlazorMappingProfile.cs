using AutoMapper;
using VotingSystem.Shared.DTOs;
using VotingSystem.Blazor.ViewModels;

namespace VotingSystem.Blazor.Infrastructure
{
    public class BlazorMappingProfile : Profile
    {
        public BlazorMappingProfile() 
        {
            CreateMap<LoginViewModel, LoginRequestDTO>(MemberList.Destination);
            CreateMap<PollResponseDTO, PollViewModel>(MemberList.Source);

            CreateMap<CreatePollViewModel, PollRequestDTO>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options.Select(o => o.Text).ToList()));
            
            CreateMap<PollResponseDTO, PollDetailsViewModel>();
            CreateMap<OptionResponseDTO, OptionViewModel>();
            CreateMap<VoterStatusDTO, VoterStatusViewModel>();
        }
    }
}
