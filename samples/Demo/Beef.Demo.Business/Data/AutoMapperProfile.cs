namespace Beef.Demo.Business.Data
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Model.PostalInfo, Common.Entities.PostalInfo>();
            CreateMap<Model.PlaceInfo, Common.Entities.PlaceInfo>();
        }
    }
}