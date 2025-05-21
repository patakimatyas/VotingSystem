using AutoMapper;
using VotingSystem.Shared.DTOs;
using VotingSystem.DataAccess.Models;

namespace VotingSystem.WebAPI.Infrastructure
{
    public class WebApiMappingProfile : Profile
    {
        public WebApiMappingProfile() 
        {
            CreateMap<Option, OptionResponseDTO>(MemberList.Destination)
            .ForMember(dest => dest.VoteCount, opt => opt.Ignore())
            .ForMember(dest => dest.VotePercentage, opt => opt.Ignore());

            CreateMap<Poll,PollResponseDTO>(MemberList.Destination)
            .ForMember(dest => dest.HasVoted, opt => opt.Ignore())
            .ForMember(dest => dest.Voters, opt => opt.Ignore());

            CreateMap<PollRequestDTO, Poll>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src =>
                src.Options.Select(o => new Option { Text = o })))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.IsClosed, opt => opt.Ignore())
            .ForMember(dest => dest.Votes, opt => opt.Ignore())
            .ForMember(dest => dest.Voters, opt => opt.Ignore());

            CreateMap<UserRequestDTO, ApplicationUser>(MemberList.Source)
            .ForSourceMember(src => src.Password, opt => opt.DoNotValidate())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<ApplicationUser, UserResponseDTO>(MemberList.Destination);

        }
    }
}
