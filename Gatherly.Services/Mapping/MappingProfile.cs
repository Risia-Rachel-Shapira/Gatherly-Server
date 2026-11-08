using AutoMapper;

using Gatherly.Core.Models;

using Gatherly.Core.DTOs.Users;
using Gatherly.Core.DTOs.Events;
using Gatherly.Core.DTOs.Items;
using Gatherly.Core.DTOs.ItemClaims;
using Gatherly.Core.DTOs.Expenses;
using Gatherly.Core.DTOs.Contributions;

namespace Gatherly.Services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EventMember, EventMemberResponseDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<CreateExpenseDto, Expense>();
            CreateMap<CreateContributionDto, Contribution>();
            CreateMap<CreateItemDto, Item>();
            CreateMap<UpdateItemDto, Item>();
            CreateMap<CreateEventDto, Event>();
            CreateMap<UpdateEventDto, Event>();
            CreateMap<User, UserResponseDto>();
            CreateMap<Event, EventResponseDto>()
                .ForMember(dest => dest.OrganizerName,
                    opt => opt.MapFrom(src => src.Organizer.FullName));
            CreateMap<Item, ItemResponseDto>()
                .ForMember(dest => dest.ClaimedQuantity,
                    opt => opt.Ignore())
                .ForMember(dest => dest.RemainingQuantity,
                    opt => opt.Ignore());
            CreateMap<ItemClaim, ItemClaimResponseDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<Expense, ExpenseResponseDto>()
                .ForMember(dest => dest.PaidByUserName,
                    opt => opt.MapFrom(src => src.PaidByUser.FullName));
            CreateMap<Contribution, ContributionResponseDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.FullName));
        }
    }
}