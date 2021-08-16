// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using System;
using System.Linq.Expressions;

namespace Beef.Mapper
{
    /// <summary>
    /// Adds additional extension methods for <c>AutoMapper</c>.
    /// </summary>
    public static class AutoMapperExtensions
    {
        /// <summary>
        /// Gets the <see cref="Beef.Mapper.OperationTypes"/> name used for indexing <see cref="IMappingOperationOptions.Items"/>.
        /// </summary>
        public const string OperationTypesName = nameof(OperationTypes);

        /// <summary>
        /// Conditionally map this member with the specified the <see cref="Beef.Mapper.OperationTypes"/> against the <see cref="IMappingOperationOptions.Items"/> <see cref="OperationTypesName"/> value, evaluated before accessing the source value.
        /// </summary>
        /// <typeparam name="TSource">The source entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TDestination">The destination entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TSourceMember">The source entity member <see cref="Type"/>.</typeparam>
        /// <param name="mce">The <see cref="IMemberConfigurationExpression{TSource, TDestination, TMember}"/>.</param>
        /// <param name="operationTypes">The <see cref="Beef.Mapper.OperationTypes"/>.</param>
        /// <remarks>Uses the <see cref="IMemberConfigurationExpression{TSource, TDestination, TMember}.PreCondition(Func{ResolutionContext, bool})"/>.</remarks>
        public static IMemberConfigurationExpression<TSource, TDestination, TSourceMember> OperationTypes<TSource, TDestination, TSourceMember>(this IMemberConfigurationExpression<TSource, TDestination, TSourceMember> mce, OperationTypes operationTypes)
        {
            if (mce == null)
                throw new ArgumentNullException(nameof(mce));

            mce.PreCondition((ResolutionContext rc) => !rc.Options.Items.TryGetValue(OperationTypesName, out var ot) || operationTypes.HasFlag((OperationTypes)ot));
            return mce;
        }

        /// <summary>
        /// Conditionally map this path with the specified the <see cref="Beef.Mapper.OperationTypes"/> against the <see cref="IMappingOperationOptions.Items"/> <see cref="OperationTypesName"/> value, evaluated before accessing the source value.
        /// </summary>
        /// <typeparam name="TSource">The source entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TDestination">The destination entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TMember">The member <see cref="Type"/>.</typeparam>
        /// <param name="pce">The <see cref="IPathConfigurationExpression{TSource, TDestination, TMember}"/>.</param>
        /// <param name="operationTypes">The <see cref="Beef.Mapper.OperationTypes"/>.</param>
        /// <remarks>Uses the <see cref="IPathConfigurationExpression{TSource, TDestination, TMember}.Condition(Func{ConditionParameters{TSource, TDestination, TMember}, bool})"/>.</remarks>
        public static IPathConfigurationExpression<TSource, TDestination, TMember> OperationTypes<TSource, TDestination, TMember>(this IPathConfigurationExpression<TSource, TDestination, TMember> pce, OperationTypes operationTypes)
        {
            if (pce == null)
                throw new ArgumentNullException(nameof(pce));

            pce.Condition(cp => !cp.Context.Options.Items.TryGetValue(OperationTypesName, out var ot) || operationTypes.HasFlag((OperationTypes)ot));
            return pce;
        }

        /// <summary>
        /// Flattens the complex typed source member into the same named destination members.
        /// </summary>
        /// <typeparam name="TDestination">The destination entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TSource">The source entity <see cref="Type"/>.</typeparam>
        /// <param name="me">The <see cref="IMappingExpression{TSource, TDestination}"/>.</param>
        /// <param name="memberExpressions">One or more members to flatten.</param>
        /// <remarks>Uses the <see cref="IMappingExpression{TSource, TDestination}.IncludeMembers(Expression{Func{TSource, object}}[])"/>.</remarks>
        public static IMappingExpression<TDestination, TSource> Flatten<TDestination, TSource>(this IMappingExpression<TDestination, TSource> me, params Expression<Func<TDestination, object>>[] memberExpressions)
        {
            if (me == null)
                throw new ArgumentNullException(nameof(me));

            me.IncludeMembers(memberExpressions);
            return me;
        }

        /// <summary>
        /// Flattens the complex typed source member from the same named destination members.
        /// </summary>
        /// <typeparam name="TDestination">The destination entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TSource">The source entity <see cref="Type"/>.</typeparam>
        /// <param name="me">The <see cref="IMappingExpression{TSource, TDestination}"/>.</param>
        /// <param name="memberExpressions">One or more members to flatten.</param>
        /// <remarks>Executes similar to: <c>d2s.ForMember(expression, o => o.MapFrom(d => d))</c></remarks>
        public static IMappingExpression<TDestination, TSource> Unflatten<TDestination, TSource>(this IMappingExpression<TDestination, TSource> me, params Expression<Func<TSource, object>>[] memberExpressions)
        {
            if (me == null)
                throw new ArgumentNullException(nameof(me));

            if (memberExpressions != null)
                memberExpressions.ForEach(exp => me.ForMember(exp, o => o.MapFrom(d => d)));

            return me;
        }

        /// <summary>
        /// Maps the source to the destination.
        /// </summary>
        /// <typeparam name="TSource">The source entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TDestination">The destination entity <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IMapper"/>.</param>
        /// <param name="source">The source value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination value.</returns>
        public static TDestination Map<TSource, TDestination>(this IMapper mapper, TSource source, OperationTypes operationType)
            => Check.NotNull(mapper, nameof(mapper)).Map<TSource, TDestination>(source, o => o.Items.Add(OperationTypesName, operationType));

        /// <summary>
        /// Maps the source to the destination updating an existing object.
        /// </summary>
        /// <typeparam name="TSource">The source entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TDestination">The destination entity <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IMapper"/>.</param>
        /// <param name="source">The source entity.</param>
        /// <param name="destination">The destination entity</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination value.</returns>
        public static TDestination Map<TSource, TDestination>(this IMapper mapper, TSource source, TDestination destination, OperationTypes operationType)
            => Check.NotNull(mapper, nameof(mapper)).Map(source, destination, o => o.Items.Add(OperationTypesName, operationType));
    }
}