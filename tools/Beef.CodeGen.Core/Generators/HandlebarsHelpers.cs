// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using HandlebarsDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Provides the <b>Handlebars.Net</b> <see cref="RegisterHelpers"/> capability.
    /// </summary>
    public static class HandlebarsHelpers
    {
        private static bool _areRegistered = false;

        /// <summary>
        /// Registers all of the required Handlebars helpers.
        /// </summary>
        public static void RegisterHelpers()
        {
            if (_areRegistered)
                return;

            _areRegistered = true;

            // Will check that the first argument equals at least one of the subsequent arguments.
            Handlebars.RegisterHelper("ifeq", (writer, context, parameters, args) =>
            {
                if (!CheckArgs("ifeq", writer, args))
                    return;

                if (IfEq(args))
                    context.Template(writer, parameters);
                else
                    context.Inverse(writer, parameters);
            });

            // Will check that the first argument does not equal any of the subsequent arguments.
            Handlebars.RegisterHelper("ifne", (writer, context, parameters, args) =>
            {
                if (!CheckArgs("ifne", writer, args))
                    return;

                if (IfEq(args))
                    context.Inverse(writer, parameters);
                else
                    context.Template(writer, parameters);
            });

            // Will check that the first argument is less than or equal to the subsequent arguments.
            Handlebars.RegisterHelper("ifle", (writer, context, parameters, args) =>
            {
                if (!CheckArgs("ifle", writer, args))
                    return;

                if (IfLe(args))
                    context.Template(writer, parameters);
                else
                    context.Inverse(writer, parameters);
            });

            // Will check that the first argument is greater than or equal to the subsequent arguments.
            Handlebars.RegisterHelper("ifge", (writer, context, parameters, args) =>
            {
                if (!CheckArgs("ifge", writer, args))
                    return;

                if (IfGe(args))
                    context.Template(writer, parameters);
                else
                    context.Inverse(writer, parameters);
            });

            // Will check that all of the arguments have a non-<c>null</c> value.
            Handlebars.RegisterHelper("ifval", (writer, context, parameters, args) =>
            {
                if (!CheckArgs("ifval", writer, args))
                    return;

                foreach (var arg in args)
                {
                    if (arg == null)
                    {
                        context.Inverse(writer, parameters);
                        return;
                    }
                }

                context.Template(writer, parameters);
            });

            // Will check that all of the arguments have a <c>null</c> value.
            Handlebars.RegisterHelper("ifnull", (writer, context, parameters, args) =>
            {
                if (!CheckArgs("ifnull", writer, args))
                    return;

                foreach (var arg in args)
                {
                    if (arg != null)
                    {
                        context.Inverse(writer, parameters);
                        return;
                    }
                }

                context.Template(writer, parameters);
            });

            // Will check that any of the arguments have a <c>true</c> value where bool; otherwise, non-null.
            Handlebars.RegisterHelper("ifor", (writer, context, parameters, args) =>
            {
                if (!CheckArgs("ifor", writer, args))
                    return;

                foreach (var arg in args)
                {
                    if (arg is bool opt)
                    {
                        if (opt)
                        {
                            context.Template(writer, parameters);
                            return;
                        }
                    }
                    else if (arg != null)
                    {
                        context.Template(writer, parameters);
                        return;
                    }
                }

                context.Inverse(writer, parameters);
            });

            // Converts a value to lowercase.
            Handlebars.RegisterHelper("lower", (writer, context, parameters) => writer.WriteSafeString(parameters.FirstOrDefault()?.ToString()?.ToLowerInvariant() ?? ""));

            // Converts a value to camelcase.
            Handlebars.RegisterHelper("camel", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToCamelCase(parameters.FirstOrDefault()?.ToString()) ?? ""));

            // Converts a value to pascalcase.
            Handlebars.RegisterHelper("pascal", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToPascalCase(parameters.FirstOrDefault()?.ToString()) ?? ""));

            // Converts a value to private case.
            Handlebars.RegisterHelper("private", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToPrivateCase(parameters.FirstOrDefault()?.ToString()) ?? ""));

            // Converts a value to the c# '<see cref="value"/>' comments equivalent.
            Handlebars.RegisterHelper("seecomments", (writer, context, parameters) => writer.WriteSafeString(ConfigBase.ToSeeComments(parameters.FirstOrDefault()?.ToString())));

            // Inserts indent spaces based on the passed index value.
            Handlebars.RegisterHelper("indent", (writer, context, parameters) => writer.WriteSafeString(new string(' ', 4 * (int)(parameters.FirstOrDefault() ?? 0))));

            // Adds a value to a value.
            Handlebars.RegisterHelper("add", (writer, context, parameters) =>
            {
                int sum = 0;
                foreach (var p in parameters)
                {
                    if (p is int pi)
                        sum += pi;
                    else if (p is string ps)
                        sum += int.Parse(ps, NumberStyles.Integer, CultureInfo.InvariantCulture);
                    else
                        writer.WriteSafeString("!!! add with invalid integer !!!");
                }

                writer.WriteSafeString(sum);
            });
        }

        /// <summary>
        /// Perform the actual IfEq equality check.
        /// </summary>
        private static bool IfEq(Arguments args)
        {
            bool func()
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (Comparer.Default.Compare(args[0], RValConvert(args[0], args[i])) == 0)
                        return true;
                }

                return false;
            }

            return args.Length switch
            {
                0 => true,
                1 => args[0] == null,
                2 => Comparer.Default.Compare(args[0], RValConvert(args[0], args[1])) == 0,
                _ => func()
            };
        }

        /// <summary>
        /// Perform the actual IfLe equality check.
        /// </summary>
        private static bool IfLe(object[] args)
        {
            bool func()
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (Comparer.Default.Compare(args[0], RValConvert(args[0], args[i])) >= 0)
                        return false;
                }

                return true;
            }

            return args.Length switch
            {
                0 => false,
                1 => false,
                _ => func()
            };
        }

        /// <summary>
        /// Perform the actual IfGe equality check.
        /// </summary>
        private static bool IfGe(object[] args)
        {
            bool func()
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (Comparer.Default.Compare(args[0], RValConvert(args[0], args[i])) <= 0)
                        return false;
                }

                return true;
            }

            return args.Length switch
            {
                0 => false,
                1 => false,
                _ => func()
            };
        }

        /// <summary>
        /// Converts the rval to match the lval type.
        /// </summary>
        private static object? RValConvert(object lval, object rval)
        {
            if (lval == null || rval == null)
                return rval;

            var type = lval.GetType();
            if (type == typeof(string))
                return rval;
            else if (type == typeof(bool))
                return bool.Parse(rval.ToString()!);
            else
                return int.Parse(rval.ToString()!, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Check the arguments to validate for correctness.
        /// </summary>
        private static bool CheckArgs(string name, EncodedTextWriter writer, Arguments args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null && args[i].GetType().Name == "UndefinedBindingResult")
                {
                    writer.Write($"!!! {name}: Arg[{i}] == UndefinedBindingResult !!!");
                    return false;
                }
            }

            return true;
        }
    }
}