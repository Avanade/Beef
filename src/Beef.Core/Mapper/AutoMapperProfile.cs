// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using Beef.Entities;
using System.Reflection;

namespace Beef.Mapper
{
    /// <summary>
    /// Represents the core <c>AutoMapper</c> <see cref="Profile"/> for <i>Beef</i>.
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        /// <summary>
        /// Gets the <i>Beef</i> <see cref="AutoMapperProfile"/> <see cref="System.Reflection.Assembly"/>.
        /// </summary>
        public static Assembly Assembly => typeof(AutoMapperProfile).Assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMapperProfile"/> class.
        /// </summary>
        public AutoMapperProfile()
        {
            // Never map property EntityBasicBase.NotifyChangesWhenSameValue.
            ForAllPropertyMaps(pm => pm.SourceMember.Name == nameof(EntityBasicBase.NotifyChangesWhenSameValue)
                && (pm.TypeMap.SourceType == typeof(EntityBase) || typeof(EntityBasicBase).IsAssignableFrom(pm.TypeMap.SourceType) 
                || pm.TypeMap.DestinationType == typeof(EntityBase) || typeof(EntityBasicBase).IsAssignableFrom(pm.TypeMap.DestinationType)), (pm, opt) => opt.Ignore());

            // Standardize ChangeLog -> Changelog mapping with the OperationTypes condition.
            CreateMap<ChangeLog, ChangeLog>()
                .ForMember(d => d.CreatedBy, o => o.OperationTypes(OperationTypes.AnyExceptUpdate).MapFrom(s => s.CreatedBy))
                .ForMember(d => d.CreatedDate, o => o.OperationTypes(OperationTypes.AnyExceptUpdate).MapFrom(s => s.CreatedDate))
                .ForMember(d => d.UpdatedBy, o => o.OperationTypes(OperationTypes.AnyExceptCreate).MapFrom(s => s.UpdatedBy))
                .ForMember(d => d.UpdatedDate, o => o.OperationTypes(OperationTypes.AnyExceptCreate).MapFrom(s => s.UpdatedDate));
        }
    }
}