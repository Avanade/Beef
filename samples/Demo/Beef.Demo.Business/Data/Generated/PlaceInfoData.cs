/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <see cref="PlaceInfo"/> data access.
    /// </summary>
    public partial class PlaceInfoData
    {

        /// <summary>
        /// Provides the <see cref="PlaceInfo"/> to Entity Framework <see cref="Model.PlaceInfo"/> mapping.
        /// </summary>
        public partial class EntityToModelHttpAgentMapper : Mapper<PlaceInfo, Model.PlaceInfo>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EntityToModelHttpAgentMapper"/> class.
            /// </summary>
            public EntityToModelHttpAgentMapper()
            {
                Map((s, d) => d.Name = s.Name);
                Map((s, d) => d.PostCode = s.PostCode);
                EntityToModelHttpAgentMapperCtor();
            }

            partial void EntityToModelHttpAgentMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(PlaceInfo s)
                => s.Name == default
                && s.PostCode == default;
        }

        /// <summary>
        /// Provides the Entity Framework <see cref="Model.PlaceInfo"/> to <see cref="PlaceInfo"/> mapping.
        /// </summary>
        public partial class ModelToEntityHttpAgentMapper : Mapper<Model.PlaceInfo, PlaceInfo>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModelToEntityHttpAgentMapper"/> class.
            /// </summary>
            public ModelToEntityHttpAgentMapper()
            {
                Map((s, d) => d.Name = (string?)s.Name);
                Map((s, d) => d.PostCode = (string?)s.PostCode);
                ModelToEntityHttpAgentMapperCtor();
            }

            partial void ModelToEntityHttpAgentMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(Model.PlaceInfo s)
                => s.Name == default
                && s.PostCode == default;
        }
    }
}

#pragma warning restore
#nullable restore