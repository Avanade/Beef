// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Beef.CodeGen
{
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

            foreach (var p in option.Values.Where(x => !string.IsNullOrEmpty(x)))
            {
                string[] parts = CodeGenConsole.CreateKeyValueParts(p!);
                if (parts.Length != 2)
                    return new ValidationResult($"The parameter '{p}' is not valid; must be formatted as Name=Value.");

                if (pd.ContainsKey(parts[0]))
                    return new ValidationResult($"The parameter '{p}' is not valid; name has been specified more than once.");

                pd.Add(parts[0], parts[1]);
            }

            return ValidationResult.Success!;
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

            foreach (var name in option.Values.Where(x => !string.IsNullOrEmpty(x)))
            {
                try
                {
                    Assemblies.Add(Assembly.Load(name!));
                }
                catch (Exception ex)
                {
                    return new ValidationResult($"The specified assembly '{name}' is invalid: {ex.Message}");
                }
            }

            return ValidationResult.Success!;
        }
    }
}