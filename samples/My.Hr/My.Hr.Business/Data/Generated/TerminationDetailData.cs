/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Data.Database;
using Beef.Data.EntityFrameworkCore;
using Beef.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using My.Hr.Business.Entities;
using RefDataNamespace = My.Hr.Business.Entities;

namespace My.Hr.Business.Data
{
    /// <summary>
    /// Provides the <see cref="TerminationDetail"/> data access.
    /// </summary>
    public partial class TerminationDetailData
    {

        /// <summary>
        /// Provides the <see cref="TerminationDetail"/> property and database column mapping.
        /// </summary>
        public partial class DbMapper : DatabaseMapper<TerminationDetail, DbMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DbMapper"/> class.
            /// </summary>
            public DbMapper()
            {
                Property(s => s.Date, "TerminationDate");
                Property(s => s.ReasonSid, "TerminationReasonCode");
                AddStandardProperties();
                DbMapperCtor();
            }
            
            partial void DbMapperCtor(); // Enables the DbMapper constructor to be extended.
        }

        /// <summary>
        /// Provides the <see cref="TerminationDetail"/> and Entity Framework <see cref="EfModel.Employee"/> <i>AutoMapper</i> mapping.
        /// </summary>
        public partial class EfMapperProfile : AutoMapper.Profile
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EfMapperProfile"/> class.
            /// </summary>
            public EfMapperProfile()
            {
                var s2d = CreateMap<TerminationDetail, EfModel.Employee>();
                s2d.ForMember(d => d.TerminationDate, o => o.MapFrom(s => s.Date));
                s2d.ForMember(d => d.TerminationReasonCode, o => o.MapFrom(s => s.ReasonSid));

                var d2s = CreateMap<EfModel.Employee, TerminationDetail>();
                d2s.ForMember(s => s.Date, o => o.MapFrom(d => d.TerminationDate));
                d2s.ForMember(s => s.ReasonSid, o => o.MapFrom(d => d.TerminationReasonCode));

                EfMapperProfileCtor(s2d, d2s);
            }

            partial void EfMapperProfileCtor(AutoMapper.IMappingExpression<TerminationDetail, EfModel.Employee> s2d, AutoMapper.IMappingExpression<EfModel.Employee, TerminationDetail> d2s); // Enables the constructor to be extended.
        }
    }
}

#pragma warning restore
#nullable restore