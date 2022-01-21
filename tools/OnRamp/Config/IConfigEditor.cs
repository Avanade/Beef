// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

namespace OnRamp.Config
{
    /// <summary>
    /// Provides a means to edit (extend) the root <see cref="ConfigBase"/> and any of its children prior to code-generation. This is used as a means to extend/customize the configuration
    /// outside of the core for use with the likes of customized templates.
    /// </summary>
    public interface IConfigEditor
    {
        /// <summary>
        /// Edit the <paramref name="config"/> <i>before</i> the <see cref="ConfigBase.Prepare(object, object)"/> has been performed.
        /// </summary>
        /// <param name="config">The root <see cref="ConfigRootBase{TRoot}"/>.</param>
        /// <remarks>Any additional properties added to the configuration file will be automatically added to <see cref="ConfigBase.ExtraProperties"/> when deserialized from JSON/YAML; the <see cref="ConfigBase.CustomProperties"/> is also 
        /// provided to enable additional custom configuration to be added that can be referenced directly by the code-generation templates.</remarks>
        void BeforePrepare(IRootConfig config) { }

        /// <summary>
        /// Edit the <paramref name="config"/> <i>after</i> the <see cref="ConfigBase.Prepare(object, object)"/> has been performed.
        /// </summary>
        /// <param name="config">The root <see cref="ConfigRootBase{TRoot}"/>.</param>
        /// <remarks>Any additional properties added to the configuration file will be automatically added to <see cref="ConfigBase.ExtraProperties"/> when deserialized from JSON/YAML; the <see cref="ConfigBase.CustomProperties"/> is also 
        /// provided to enable additional custom configuration to be added that can be referenced directly by the code-generation templates.</remarks>
        void AfterPrepare(IRootConfig config) { }
    }
}