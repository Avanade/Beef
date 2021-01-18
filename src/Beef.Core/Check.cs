// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Beef
{
    /// <summary>
    /// Provides pre-condition checking.
    /// </summary>
    [DebuggerStepThrough]
    public static class Check
    {
        /// <summary>
        /// Checks that the <paramref name="value"/> is not null; otherwise, throws an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="message">An optional message.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is null.</exception>
        public static T NotNull<T>(T? value, string paramName, string? message = null) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName, message);
            else
                return value;
        }

        /// <summary>
        /// Checks that the <paramref name="value"/> is not the default value for the <see cref="Type"/>; otherwise, throws an <see cref="ArgumentException"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="message">An optional message.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is the default value for the <see cref="Type"/>.</exception>
        /// <returns>The value.</returns>
        public static T NotDefault<T>(T value, string paramName, string? message = null)
        {
            if (Comparer<T>.Default.Compare(value, default!) == 0)
                throw new ArgumentException(message ?? "Argument with a default value is considered invalid.", paramName);
            else
                return value;
        }

        /// <summary>
        /// Checks that the <paramref name="value"/> is not null or <see cref="string.Empty"/>; otherwise, throws an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="message">An optional message.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is null or <see cref="string.Empty"/>.</exception>
        /// <returns>The value.</returns>
        public static string NotEmpty(string? value, string paramName, string? message = null)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(paramName, message);
            else
                return value;
        }

        /// <summary>
        /// Checks that the argument <paramref name="predicate"/> is <c>true</c>; otherwise, <c>false</c> will result in an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="message">The exception message.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="predicate"/> is <c>false</c>.</exception>
        public static void IsTrue(bool predicate, string paramName, string? message = null)
        {
            if (!predicate)
                throw new ArgumentException(message ?? "Argument is not valid.", paramName);
        }

        /// <summary>
        /// Checks that the argument <paramref name="predicate"/> is <c>true</c>; otherwise, <c>false</c> will result in an <see cref="ArgumentException"/>.
        /// </summary>
        /// <typeparam name="T">The predicate.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="message">The exception message.</param>
        /// <returns>The value.</returns>
        public static T IsTrue<T>(T value, Predicate<T> predicate, string? message = null)
        {
            if (predicate == null || predicate(value))
                return value;

            throw new ArgumentException(message ?? "Argument is not valid.");
        }

        /// <summary>
        /// Checks that the operation <paramref name="predicate"/> is <c>true</c>; otherwise, <c>false</c> will result in an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="message">The exception message.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="predicate"/> is <c>false</c>.</exception>
        public static void IsTrue(bool predicate, string? message = null)
        {
            if (!predicate)
                throw new InvalidOperationException(message ?? "Operation is not valid.");
        }

        /// <summary>
        /// Checks that the argument <paramref name="predicate"/> is <c>false</c>; otherwise, <c>true</c> will result in an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="message">The exception message.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="predicate"/> is <c>false</c>.</exception>
        public static void IsFalse(bool predicate, string paramName, string? message = null)
        {
            if (predicate)
                throw new ArgumentException(message ?? "Argument is not valid.", paramName);
        }

        /// <summary>
        /// Checks that the argument <paramref name="predicate"/> is <c>false</c>; otherwise, <c>true</c> will result in an <see cref="ArgumentException"/>.
        /// </summary>
        /// <typeparam name="T">The predicate.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="message">The exception message.</param>
        /// <returns>The value.</returns>
        public static T IsFalse<T>(T value, Predicate<T> predicate, string? message = null)
        {
            if (predicate == null || !predicate(value))
                return value;

            throw new ArgumentException(message ?? "Argument is not valid.");
        }

        /// <summary>
        /// Checks that the operation <paramref name="predicate"/> is <c>false</c>; otherwise, <c>true</c> will result in an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="message">The exception message.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="predicate"/> is <c>false</c>.</exception>
        public static void IsFalse(bool predicate, string? message = null)
        {
            if (predicate)
                throw new InvalidOperationException(message ?? "Operation is not valid.");
        }

        /// <summary>
        /// Checks that the operation <paramref name="predicate"/> is <c>true</c>; otherwise, <c>false</c> will result in an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="message">The exception message.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="predicate"/> is <c>false</c>.</exception>
        /// <returns>The value.</returns>
        public static T IsValid<T>(T value, Predicate<T> predicate, string? message = null)
        {
            if (predicate == null || predicate(value))
                return value;

            throw new InvalidOperationException(message ?? "Operation is not valid.");
        }
    }
}
