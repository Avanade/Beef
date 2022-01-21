// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace OnRamp.Console
{
    /// <summary>
    /// Validates the assembly name(s).
    /// </summary>
    public class AssemblyValidator : IOptionValidator
    {
        private readonly CodeGeneratorArgsBase _args;

        /// <summary>
        /// Initilizes a new instance of the <see cref="AssemblyValidator"/> class.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgs"/> to update.</param>
        public AssemblyValidator(CodeGeneratorArgsBase args) => _args = args ?? throw new ArgumentNullException(nameof(args));

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

            var list = new List<Assembly>();
            foreach (var name in option.Values.Where(x => !string.IsNullOrEmpty(x)))
            {
                try
                {
                    list.Add(Assembly.Load(name!));
                }
                catch (Exception ex)
                {
                    return new ValidationResult($"The specified assembly '{name}' is invalid: {ex.Message}");
                }
            }

            _args.Assemblies.InsertRange(0, list.ToArray());
            return ValidationResult.Success!;
        }
    }
}