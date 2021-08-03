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
using Microsoft.Azure.Cosmos;
using Beef;
using Beef.Business;
using Beef.Data.Cosmos;
using Beef.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using Cdr.Banking.Common.Entities;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Business.Data
{
    /// <summary>
    /// Provides the <see cref="AccountDetail"/> data access.
    /// </summary>
    public partial class AccountDetailData
    {

        /// <summary>
        /// Provides the <see cref="AccountDetail"/> and Entity Framework <see cref="CosmoskModel"/> <i>AutoMapper</i> mapping.
        /// </summary>
        public partial class CosmosMapperProfile : AutoMapper.Profile
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CosmosMapperProfile"/> class.
            /// </summary>
            public CosmosMapperProfile()
            {
                var s2d = CreateMap<AccountDetail, Model.Account>();
                s2d.ForMember(d => d.Bsb, o => o.MapFrom(s => s.Bsb));
                s2d.ForMember(d => d.AccountNumber, o => o.MapFrom(s => s.AccountNumber));
                s2d.ForMember(d => d.BundleName, o => o.MapFrom(s => s.BundleName));
                s2d.ForMember(d => d.SpecificAccountUType, o => o.MapFrom(s => s.SpecificAccountUTypeSid));
                s2d.ForMember(d => d.TermDeposit, o => o.MapFrom(s => s.TermDeposit));
                s2d.ForMember(d => d.CreditCard, o => o.MapFrom(s => s.CreditCard));

                var d2s = CreateMap<Model.Account, AccountDetail>();
                d2s.ForMember(s => s.Bsb, o => o.MapFrom(d => d.Bsb));
                d2s.ForMember(s => s.AccountNumber, o => o.MapFrom(d => d.AccountNumber));
                d2s.ForMember(s => s.BundleName, o => o.MapFrom(d => d.BundleName));
                d2s.ForMember(s => s.SpecificAccountUTypeSid, o => o.MapFrom(d => d.SpecificAccountUType));
                d2s.ForMember(s => s.TermDeposit, o => o.MapFrom(d => d.TermDeposit));
                d2s.ForMember(s => s.CreditCard, o => o.MapFrom(d => d.CreditCard));

                CosmosMapperProfileCtor(s2d, d2s);
            }

            partial void CosmosMapperProfileCtor(AutoMapper.IMappingExpression<AccountDetail, Model.Account> s2d, AutoMapper.IMappingExpression<Model.Account, AccountDetail> d2s); // Enables the constructor to be extended.
        }
    }
}

#pragma warning restore
#nullable restore