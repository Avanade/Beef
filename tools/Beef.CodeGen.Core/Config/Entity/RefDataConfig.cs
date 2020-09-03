﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Provides Reference Data specific properties.
    /// </summary>
    public class RefDataConfig : ConfigBase<CodeGenConfig, CodeGenConfig>
    {
        /// <summary>
        /// Indicates whether auto-implementing 'Database'.
        /// </summary>
        public bool UsesDatabase => Parent!.RefDataEntities.Any(x => x.AutoImplement == "Database");

        /// <summary>
        /// Indicates whether auto-implementing 'EntityFramework'.
        /// </summary>
        public bool UsesEntityFramework => Parent!.RefDataEntities.Any(x => x.AutoImplement == "EntityFramework");

        /// <summary>
        /// Indicates whether auto-implementing 'Cosmos'.
        /// </summary>
        public bool UsesCosmos => Parent!.RefDataEntities.Any(x => x.AutoImplement == "Cosmos");

        /// <summary>
        /// Indicates whether auto-implementing 'OData'.
        /// </summary>
        public bool UsesOData => Parent!.RefDataEntities.Any(x => x.AutoImplement == "OData");

        /// <summary>
        /// Gets the Data constructor parameters.
        /// </summary>
        public List<ParameterConfig> DataConstructorParameters { get; } = new List<ParameterConfig>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            var oc = new OperationConfig();
            oc.Prepare(Root!, new EntityConfig { Name = "RefData" });

            // Data constructors.
            if (UsesDatabase)
                DataConstructorParameters.Add(new ParameterConfig { Name = "Db", Type = Root!.DatabaseName, Text = $"{{{{{Root!.DatabaseName}}}}}" });

            if (UsesEntityFramework)
                DataConstructorParameters.Add(new ParameterConfig { Name = "Ef", Type = Root!.EntityFrameworkName, Text = $"{{{{{Root!.EntityFrameworkName}}}}}" });

            if (UsesCosmos)
                DataConstructorParameters.Add(new ParameterConfig { Name = "Cosmos", Type = Root!.CosmosName, Text = $"{{{{{Root!.CosmosName}}}}}" });

            if (UsesOData)
                DataConstructorParameters.Add(new ParameterConfig { Name = "OData", Type = Root!.ODataName, Text = $"{{{{{Root!.ODataName}}}}}" });

            foreach (var ctor in DataConstructorParameters)
            {
                ctor.Prepare(Root!, oc);
            }
        }
    }
}