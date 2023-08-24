using AutoMapper;
using UserInfoService.Core.Dto;
using UserInfoService.Core.Models;

namespace UserInfoService.Core.Helpers
{
    public class ObjectMapper
    {
        private static IMapper mapper;

        public static IMapper Mapper
        {
            get
            {
                if (mapper == null)
                    return ConfigureMapping();

                return mapper;
            }
        }

        private static IMapper ConfigureMapping()
        {
            MapperConfiguration mapperConfiguration = new MapperConfiguration(config =>
            {
                config.CreateMap<AddOrUpdateUserInfoRequest, UserInfo>();
            });

            return mapper = mapperConfiguration.CreateMapper(); 
        }
    }
}
