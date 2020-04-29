using Beef.Demo.Common.Entities;
using Beef.Mapper.Converters;

namespace Beef.Demo.Business.Data
{
    public class GenderTripCodeConverter : ReferenceDataMappingConverter<GenderTripCodeConverter, Gender, string>
    {
        public GenderTripCodeConverter() : base(nameof(Gender.TripCode)) { }
    }
}