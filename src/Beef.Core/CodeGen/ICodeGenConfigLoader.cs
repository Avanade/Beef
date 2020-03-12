// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Threading.Tasks;

namespace Beef.CodeGen
{
    /// <summary>
    /// Provides for extended <see cref="LoadBeforeChildrenAsync"/> and <see cref="LoadAfterChildrenAsync"/> activities on a <see cref="CodeGenConfig"/> item.
    /// </summary>
    public interface ICodeGenConfigLoader
    {
        /// <summary>
        /// Gets the loader name (is the unique element name).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Performs extended load activities for a <see cref="CodeGenConfig"/> item before the corresponding <see cref="CodeGenConfig.Children"/> are loaded.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        /// <remarks>All <see cref="CodeGenConfig.Attributes"/> configuration will have been loaded.</remarks>
        Task LoadBeforeChildrenAsync(CodeGenConfig config) => Task.CompletedTask;

        /// <summary>
        /// Performs extended load activities for a <see cref="CodeGenConfig"/> item after the corresponding <see cref="CodeGenConfig.Children"/> are loaded.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        /// <remarks>All <see cref="CodeGenConfig.Attributes"/> configuration will have been loaded.</remarks>
        Task LoadAfterChildrenAsync(CodeGenConfig config) => Task.CompletedTask;
    }

    /// <summary>
    /// Provides the <see cref="GetLoaders"/> implementation.
    /// </summary>
    public interface ICodeGenConfigGetLoaders
    {
        /// <summary>
        /// Gets the corresponding loaders.
        /// </summary>
        /// <returns>An <see cref="ICodeGenConfigLoader"/> array.</returns>
        ICodeGenConfigLoader[] GetLoaders();
    }
}
