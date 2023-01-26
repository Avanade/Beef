/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <see cref="Address"/> data access.
    /// </summary>
    public partial class AddressData
    {

        /// <summary>
        /// Provides the <see cref="Address"/> property and database column mapping.
        /// </summary>
        public partial class DbMapper : DatabaseMapper<Address, DbMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DbMapper"/> class.
            /// </summary>
            public DbMapper()
            {
                Property(s => s.Street);
                Property(s => s.City);
                DbMapperCtor();
            }
            
            partial void DbMapperCtor(); // Enables the DbMapper constructor to be extended.
        }

        /// <summary>
        /// Provides the <see cref="Address"/> to Entity Framework <see cref="EfModel.Person"/> mapping.
        /// </summary>
        public partial class EntityToModelEfMapper : Mapper<Address, EfModel.Person>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EntityToModelEfMapper"/> class.
            /// </summary>
            public EntityToModelEfMapper()
            {
                Map((s, d) => d.Street = s.Street);
                Map((s, d) => d.City = s.City);
                EntityToModelEfMapperCtor();
            }

            partial void EntityToModelEfMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(Address s)
                => s.Street == default
                && s.City == default;
        }

        /// <summary>
        /// Provides the Entity Framework <see cref="EfModel.Person"/> to <see cref="Address"/> mapping.
        /// </summary>
        public partial class ModelToEntityEfMapper : Mapper<EfModel.Person, Address>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModelToEntityEfMapper"/> class.
            /// </summary>
            public ModelToEntityEfMapper()
            {
                Map((s, d) => d.Street = (string?)s.Street);
                Map((s, d) => d.City = (string?)s.City);
                ModelToEntityEfMapperCtor();
            }

            partial void ModelToEntityEfMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(EfModel.Person s)
                => s.Street == default
                && s.City == default;
        }
    }
}

#pragma warning restore
#nullable restore