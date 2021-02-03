// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Provides a means to edit (extend) the root <see cref="ConfigBase"/> and any of its children prior to code-generation. This is used as a means to extend/customize the configuration
    /// outside of core <i>Beef</i> for use with the likes of customized templates.
    /// </summary>
    public interface IConfigEditor
    {
        /// <summary>
        /// Edit the <paramref name="config"/> directly after the <see cref="ConfigBase.Prepare(object, object)"/> has been performed.
        /// </summary>
        /// <param name="config">The root <see cref="ConfigBase"/>.</param>
        /// <remarks>Any additional properties added to the configuration file will be automatically added to <see cref="ConfigBase.ExtraProperties"/>; the <see cref="ConfigBase.CustomProperties"/> is provided
        /// to enable additional custom configuration to be added that can be referenced directly by the code-generation templates.</remarks>
        void EditConfig(ConfigBase config);
    }
}