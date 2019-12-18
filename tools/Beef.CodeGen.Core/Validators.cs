﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace Beef.CodeGen
{
    /// <summary>
    /// Validates either existence of file or embedded resource.
    /// </summary>
    public class FileResourceValidator : IOptionValidator
    {
        /// <summary>
        /// Performs the validation.
        /// </summary>
        /// <param name="option">The <see cref="CommandOption"/>.</param>
        /// <param name="context">The <see cref="ValidationContext"/>.</param>
        /// <returns>The <see cref="ValidationResult"/>.</returns>
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (option.Value() != null && !File.Exists(option.Value()) && ResourceManager.GetScriptContent(option.Value()) == null)
                return new ValidationResult($"The file or embedded resource '{option.Value()}' does not exist.");

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Validate the Params to ensure format is correct and values are not duplicated.
    /// </summary>
    public class ParamsValidator : IOptionValidator
    {
        /// <summary>
        /// Performs the validation.
        /// </summary>
        /// <param name="option">The <see cref="CommandOption"/>.</param>
        /// <param name="context">The <see cref="ValidationContext"/>.</param>
        /// <returns>The <see cref="ValidationResult"/>.</returns>
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var pd = new Dictionary<string, string>();

            foreach (var p in option.Values)
            {
                string[] parts = CodeGenConsole.CreateKeyValueParts(p);
                if (parts.Length != 2)
                    return new ValidationResult($"The parameter '{p}' is not valid; must be formatted as Name=value.");

                if (pd.ContainsKey(parts[0]))
                    return new ValidationResult($"The parameter '{p}' is not valid; name has been specified more than once.");

                pd.Add(parts[0], parts[1]);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Validates the assembly name(s).
    /// </summary>
    public class AssemblyValidator : IOptionValidator
    {
        /// <summary>
        /// Initilizes a new instance of the <see cref="AssemblyValidator"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies list to update.</param>
        public AssemblyValidator(List<Assembly> assemblies) => Assemblies = assemblies;

        /// <summary>
        /// Gets the list of assemblies.
        /// </summary>
        public List<Assembly> Assemblies { get; private set; }

        /// <summary>
        /// Performs the validation.
        /// </summary>
        /// <param name="option">The <see cref="CommandOption"/>.</param>
        /// <param name="context">The <see cref="ValidationContext"/>.</param>
        /// <returns>The <see cref="ValidationResult"/>.</returns>
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            foreach (var name in option.Values)
            {
                try
                {
                    Assemblies.Add(Assembly.Load(name));
                }
#pragma warning disable CA1031 // Do not catch general exception types; by-design.
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    return new ValidationResult($"The specified assembly '{name}' is invalid: {ex.Message}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
