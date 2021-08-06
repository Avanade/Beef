/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using Beef.Mapper;
using Beef.Mapper.Converters;
using entities = Beef.Demo.Common.Entities;
using proto = Beef.Demo.Common.Grpc.Proto;

namespace Beef.Demo.Common.Grpc
{
    /// <summary>
    /// Provides entity to gRpc transformations (conversion and/or mapping).
    /// </summary>
    public static class Transformers
    {
        #region Converters
        
        /// <summary>
        /// Converts a <see cref="DateTime"/> to/from a <see cref="proto.DateOnly"/>.
        /// </summary>
        public static CustomConverter<DateTime, proto.DateOnly> DateTimeToDateOnly => new CustomConverter<DateTime, proto.DateOnly>(
            s => new proto.DateOnly { Year = s.Year, Month = s.Month, Day = s.Month },
            d => d == null ? new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) : new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified));

        /// <summary>
        /// Converts a <see cref="Nullable{DateTime}"/> to/from a <see cref="proto.DateOnly"/>.
        /// </summary>
        public static CustomConverter<DateTime?, proto.DateOnly> NullableDateTimeToDateOnly => new CustomConverter<DateTime?, proto.DateOnly>(
            s => s == null ? null! : new proto.DateOnly { Year = s.Value.Year, Month = s.Value.Month, Day = s.Value.Month },
            d => d == null ? (DateTime?)null : new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified));

        /// <summary>
        /// Converts a <see cref="DateTime"/> to/from a <see cref="Google.Protobuf.WellKnownTypes.Timestamp"/>.
        /// </summary>
        public static CustomConverter<DateTime, Google.Protobuf.WellKnownTypes.Timestamp> DateTimeToTimestamp => new CustomConverter<DateTime, Google.Protobuf.WellKnownTypes.Timestamp>(
            s => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(s.ToUniversalTime()),
            d => d == null ? DateTime.MinValue : d.ToDateTime());

        /// <summary>
        /// Converts a <see cref="Nullable{DateTime}"/> to/from a <see cref="Google.Protobuf.WellKnownTypes.Timestamp"/>.
        /// </summary>
        public static CustomConverter<DateTime?, Google.Protobuf.WellKnownTypes.Timestamp> NullableDateTimeToTimestamp => new CustomConverter<DateTime?, Google.Protobuf.WellKnownTypes.Timestamp>(
            s => s == null ? null! : Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(s.Value.ToUniversalTime()),
            d => d == null ? (DateTime?)null : d.ToDateTime());

        /// <summary>
        /// Converts a <see cref="Guid"/> to a <see cref="string"/>.
        /// </summary>
        public static CustomConverter<Guid, string> GuidToStringConverter = new CustomConverter<Guid, string>(
            s => s.ToString(),
            d => d == null ? Guid.Empty : new Guid(d));

        /// <summary>
        /// Converts a <see cref="Nullable{Guid}"/> to a <see cref="string"/>.
        /// </summary>
        public static CustomConverter<Guid?, string> NullableGuidToStringConverter = new CustomConverter<Guid?, string>(
            s => s == null ? null! : s.ToString(),
            d => d == null ? (Guid?)null : new Guid(d));

        /// <summary>
        /// Converts a <see cref="decimal"/> to a <see cref="proto.Decimal"/>.
        /// </summary>
        public static CustomConverter<decimal, proto.Decimal> DecimalToDecimalConverter = new CustomConverter<decimal, proto.Decimal>(
            s => new proto.Decimal { Units = decimal.ToInt64(s), Nanos = decimal.ToInt32((s - decimal.ToInt64(s)) * 1_000_000_000) },
            d => d == null ? 0m : d.Units + d.Nanos / 1_000_000_000);

        /// <summary>
        /// Converts a <see cref="Nullable"/> <see cref="decimal"/> to a <see cref="proto.Decimal"/>.
        /// </summary>
        public static CustomConverter<decimal?, proto.Decimal> NullableDecimalToDecimalConverter = new CustomConverter<decimal?, proto.Decimal>(
            s => s == null ? null! : new proto.Decimal { Units = decimal.ToInt64(s.Value), Nanos = decimal.ToInt32((s.Value - decimal.ToInt64(s.Value)) * 1_000_000_000) },
            d => d == null ? (decimal?)null : d.Units + d.Nanos / 1_000_000_000);
            
        /// <summary>
        /// Converts a <see cref="Beef.Entities.PagingArgs"/> to a <see cref="proto.PagingArgs"/>.
        /// </summary>
        public static CustomConverter<Beef.Entities.PagingArgs, proto.PagingArgs> PagingArgsToPagingArgsConverter = new Mapper.Converters.CustomConverter<Beef.Entities.PagingArgs, proto.PagingArgs>(
            s => s == null ? null! : new proto.PagingArgs { Skip = s.Skip, Take = s.Take, GetCount = s.IsGetCount },
            d => d == null ? null! : Beef.Entities.PagingArgs.CreateSkipAndTake(d.Skip, d.Take, d.GetCount));
            
         /// <summary>
        /// Converts a <see cref="Beef.Entities.PagingResult"/> to a <see cref="proto.PagingResult"/>.
        /// </summary>
        public static CustomConverter<Beef.Entities.PagingResult, proto.PagingResult> PagingResultToPagingResultConverter = new Mapper.Converters.CustomConverter<Beef.Entities.PagingResult, proto.PagingResult>(
            s => s == null ? null! : new proto.PagingResult { Skip = s.Skip, Take = s.Take, TotalCount = s.TotalCount },
            d => d == null ? null! : new Beef.Entities.PagingResult(Beef.Entities.PagingArgs.CreateSkipAndTake(d.Skip, d.Take), d.TotalCount));

        #endregion
        
        #region Mappers

        /// <summary>
        /// Represents the <c>AutoMapper</c> <see cref="Profile"/> for <i>gRPC</i>.
        /// </summary>
        public partial class AutoMapperProfile : AutoMapper.Profile
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AutoMapperProfile"/> class.
            /// </summary>
            public AutoMapperProfile()
            {
                CreateMap<Beef.Entities.ChangeLog, proto.ChangeLog>()
                    .ForMember(d => d.CreatedBy, o => o.MapFrom(s => s.CreatedBy))
                    .ForMember(d => d.CreatedDate, o => o.ConvertUsing(NullableDateTimeToTimestamp.ToDest, s => s.CreatedDate))
                    .ForMember(d => d.UpdatedBy, o => o.MapFrom(s => s.UpdatedBy))
                    .ForMember(d => d.UpdatedDate, o => o.ConvertUsing(NullableDateTimeToTimestamp.ToDest, s => s.UpdatedDate));

                CreateMap<proto.ChangeLog, Beef.Entities.ChangeLog>()
                    .ForMember(s => s.CreatedBy, o => o.MapFrom(d => d.CreatedBy))
                    .ForMember(s => s.CreatedDate, o => o.ConvertUsing(NullableDateTimeToTimestamp.ToSrce, d => d.CreatedDate))
                    .ForMember(s => s.UpdatedBy, o => o.MapFrom(d => d.UpdatedBy))
                    .ForMember(s => s.UpdatedDate, o => o.ConvertUsing(NullableDateTimeToTimestamp.ToSrce, d => d.UpdatedDate));

                CreateMap<entities.Person, proto.Person>()
                    .ForMember(d => d.Id, o => o.ConvertUsing(GuidToStringConverter.ToDest, s => s.Id))
                    .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                    .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                    .ForMember(d => d.UniqueCode, o => o.MapFrom(s => s.UniqueCode))
                    .ForMember(d => d.Gender, o => o.MapFrom(s => s.GenderSid))
                    .ForMember(d => d.EyeColor, o => o.MapFrom(s => s.EyeColorSid))
                    .ForMember(d => d.Birthday, o => o.ConvertUsing(DateTimeToDateOnly.ToDest, s => s.Birthday))
                    .ForMember(d => d.Address, o => o.MapFrom(s => s.Address))
                    .ForMember(d => d.Etag, o => o.MapFrom(s => s.ETag))
                    .ForMember(d => d.ChangeLog, o => o.MapFrom(s => s.ChangeLog));

                CreateMap<proto.Person, entities.Person>()
                    .ForMember(s => s.Id, o => o.ConvertUsing(GuidToStringConverter.ToSrce, s => s.Id))
                    .ForMember(s => s.FirstName, o => o.MapFrom(s => s.FirstName))
                    .ForMember(s => s.LastName, o => o.MapFrom(s => s.LastName))
                    .ForMember(s => s.UniqueCode, o => o.MapFrom(s => s.UniqueCode))
                    .ForMember(s => s.GenderSid, o => o.MapFrom(s => s.Gender))
                    .ForMember(s => s.EyeColorSid, o => o.MapFrom(s => s.EyeColor))
                    .ForMember(s => s.Birthday, o => o.ConvertUsing(DateTimeToDateOnly.ToSrce, s => s.Birthday))
                    .ForMember(s => s.Address, o => o.MapFrom(s => s.Address))
                    .ForMember(s => s.ETag, o => o.MapFrom(s => s.Etag))
                    .ForMember(s => s.ChangeLog, o => o.MapFrom(s => s.ChangeLog));

                CreateMap<entities.PersonCollectionResult, proto.PersonCollectionResult>()
                    .ConstructUsing(c => new proto.PersonCollectionResult())
                    .ForMember(d => d.Result, o => o.MapFrom(s => s.Result))
                    .ForMember(d => d.Paging, o => o.ConvertUsing(PagingResultToPagingResultConverter.ToDest, s => s.Paging));

                CreateMap<proto.PersonCollectionResult, entities.PersonCollectionResult>()
                    .ConstructUsing(c => new entities.PersonCollectionResult())
                    .ForMember(s => s.Result, o => o.MapFrom(d => d.Result))
                    .ForMember(s => s.Paging, o => o.ConvertUsing(PagingResultToPagingResultConverter.ToSrce, d => d.Paging));

                CreateMap<entities.Address, proto.Address>()
                    .ForMember(d => d.Street, o => o.MapFrom(s => s.Street))
                    .ForMember(d => d.City, o => o.MapFrom(s => s.City));

                CreateMap<proto.Address, entities.Address>()
                    .ForMember(s => s.Street, o => o.MapFrom(s => s.Street))
                    .ForMember(s => s.City, o => o.MapFrom(s => s.City));

                CreateMap<entities.Robot, proto.Robot>()
                    .ForMember(d => d.Id, o => o.ConvertUsing(GuidToStringConverter.ToDest, s => s.Id))
                    .ForMember(d => d.ModelNo, o => o.MapFrom(s => s.ModelNo))
                    .ForMember(d => d.SerialNo, o => o.MapFrom(s => s.SerialNo))
                    .ForMember(d => d.EyeColor, o => o.MapFrom(s => s.EyeColorSid))
                    .ForMember(d => d.PowerSource, o => o.MapFrom(s => s.PowerSourceSid))
                    .ForMember(d => d.Etag, o => o.MapFrom(s => s.ETag))
                    .ForMember(d => d.ChangeLog, o => o.MapFrom(s => s.ChangeLog));

                CreateMap<proto.Robot, entities.Robot>()
                    .ForMember(s => s.Id, o => o.ConvertUsing(GuidToStringConverter.ToSrce, s => s.Id))
                    .ForMember(s => s.ModelNo, o => o.MapFrom(s => s.ModelNo))
                    .ForMember(s => s.SerialNo, o => o.MapFrom(s => s.SerialNo))
                    .ForMember(s => s.EyeColorSid, o => o.MapFrom(s => s.EyeColor))
                    .ForMember(s => s.PowerSourceSid, o => o.MapFrom(s => s.PowerSource))
                    .ForMember(s => s.ETag, o => o.MapFrom(s => s.Etag))
                    .ForMember(s => s.ChangeLog, o => o.MapFrom(s => s.ChangeLog));

                CreateMap<entities.RobotCollectionResult, proto.RobotCollectionResult>()
                    .ConstructUsing(c => new proto.RobotCollectionResult())
                    .ForMember(d => d.Result, o => o.MapFrom(s => s.Result))
                    .ForMember(d => d.Paging, o => o.ConvertUsing(PagingResultToPagingResultConverter.ToDest, s => s.Paging));

                CreateMap<proto.RobotCollectionResult, entities.RobotCollectionResult>()
                    .ConstructUsing(c => new entities.RobotCollectionResult())
                    .ForMember(s => s.Result, o => o.MapFrom(d => d.Result))
                    .ForMember(s => s.Paging, o => o.ConvertUsing(PagingResultToPagingResultConverter.ToSrce, d => d.Paging));

                CreateMap<entities.RobotArgs, proto.RobotArgs>()
                    .ForMember(d => d.ModelNo, o => o.MapFrom(s => s.ModelNo))
                    .ForMember(d => d.SerialNo, o => o.MapFrom(s => s.SerialNo))
                    .ForMember(d => d.PowerSources, o => o.MapFrom(s => s.PowerSourcesSids));

                CreateMap<proto.RobotArgs, entities.RobotArgs>()
                    .ForMember(s => s.ModelNo, o => o.MapFrom(s => s.ModelNo))
                    .ForMember(s => s.SerialNo, o => o.MapFrom(s => s.SerialNo))
                    .ForMember(s => s.PowerSourcesSids, o => o.MapFrom(s => s.PowerSources));

                AutoMapperProfileCtor();
            }

            partial void AutoMapperProfileCtor(); // Enables the constructor to be extended.

        }

        #endregion
    }
}

#pragma warning restore
#nullable restore