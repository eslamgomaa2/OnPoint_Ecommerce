using AutoMapper;
using Onpoint.Store.Application.DTOs.Contactmessage;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class MessageMappingProfile : Profile
    {
        public MessageMappingProfile()
        {
            CreateMap<ContactMessage, ContactMessageReadDto>();

            CreateMap<ContactMessageCreateDto, ContactMessage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsResolved, opt => opt.MapFrom(_ => false));

            CreateMap<ContactMessageUpdateDto, ContactMessage>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
