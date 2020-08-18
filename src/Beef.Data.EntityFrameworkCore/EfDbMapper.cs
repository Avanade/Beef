// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Provides access to the common entity framework mapper capabilities.
    /// </summary>
    public static class EfDbMapper
    {
        /// <summary>
        /// Creates an <see cref="EntityMapper{T, TModel}"/> automatically mapping the properties where they share the same name.
        /// </summary>
        /// <param name="ignoreSrceProperties">An array of source property names to ignore.</param>
        /// <returns>An <see cref="EntityMapper{T, TModel}"/>.</returns>
        public static EfDbMapper<T, TModel> CreateAuto<T, TModel>(params string[] ignoreSrceProperties)
            where T : class, new()
            where TModel : class, new()
        {
            return new EfDbMapper<T, TModel>(true, ignoreSrceProperties);
        }

        /// <summary>
        /// Creates an <see cref="EntityMapper{T, TModel}"/> where properties are added manually.
        /// </summary>
        /// <returns>An <see cref="EntityMapper{T, TModel}"/>.</returns>
        public static EfDbMapper<T, TModel> Create<T, TModel>()
            where T : class, new()
            where TModel : class, new()
        {
            return new EfDbMapper<T, TModel>(false);
        }
    }

    /// <summary>
    /// Provides entity mapping capabilities to and from the <see cref="EfDbBase{TDbContext}">entity framework</see> model.
    /// </summary>
    /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
    /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
    public class EfDbMapper<T, TModel> : EntityMapper<T, TModel>
        where T : class, new()
        where TModel : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbMapper{T, TModel}"/> class.
        /// </summary>
        /// <param name="autoMap">Indicates whether the two entities should automatically map where the properties share the same name.</param>
        /// <param name="ignoreSrceProperties">An array of source property names to ignore.</param>
        public EfDbMapper(bool autoMap = false, params string[] ignoreSrceProperties) : base(autoMap, ignoreSrceProperties) { }

        /// <summary>
        /// Creates a <see cref="EfDbArgs{T, TModel}"/>.
        /// </summary>
        /// <returns>A <see cref="EfDbArgs{T, TModel}"/>.</returns>
        public EfDbArgs<T, TModel> CreateArgs()
        {
            return new EfDbArgs<T, TModel>(this);
        }

        /// <summary>
        /// Creates a <see cref="EfDbArgs{T, TModel}"/> with a <see cref="PagingArgs"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>A <see cref="EfDbArgs{T, TModel}"/>.</returns>
        public EfDbArgs<T, TModel> CreateArgs(PagingArgs paging)
        {
            return new EfDbArgs<T, TModel>(this, paging);
        }

        /// <summary>
        /// Creates a <see cref="EfDbArgs{T, TModel}"/> with a <see cref="PagingResult"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <returns>A <see cref="EfDbArgs{T, TModel}"/>.</returns>
        public EfDbArgs<T, TModel> CreateArgs(PagingResult paging)
        {
            return new EfDbArgs<T, TModel>(this, paging);
        }

        /// <summary>
        /// Adds (or gets) a <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.
        /// </summary>
        /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
        /// <typeparam name="TDestProperty">The destination property <see cref="Type"/>.</typeparam>
        /// <param name="srcePropertyExpression">The <see cref="Expression"/> to reference the source entity property.</param>
        /// <param name="destPropertyExpression">The <see cref="Expression"/> to reference the destination entity property.</param>
        /// <param name="property">An <see cref="Action"/> enabling access to the created <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.</param>
        /// <returns>The <see cref="EntityMapper{TSrce, TDest}"/>.</returns>
        public new EfDbMapper<T, TModel> HasProperty<TSrceProperty, TDestProperty>(Expression<Func<T, TSrceProperty>> srcePropertyExpression, Expression<Func<TModel, TDestProperty>> destPropertyExpression, Action<PropertyMapper<T, TSrceProperty, TModel, TDestProperty>>? property = null)
        {
            base.HasProperty(srcePropertyExpression, destPropertyExpression, property);
            return this;
        }

        /// <summary>
        /// Adds the standard properties for <see cref="IETag"/> and <see cref="IChangeLog"/>.
        /// </summary>
        /// <returns>The <see cref="EfDbMapper{T, TModel}"/>.</returns>
        public EfDbMapper<T, TModel> AddStandardProperties()
        {
            if (typeof(IETag).IsAssignableFrom(typeof(T)) && GetBySrcePropertyName(nameof(IETag.ETag)) == null)
            {
                var pi = typeof(TModel).GetProperty(Database.DatabaseColumns.RowVersionName);
                if (pi != null && pi.PropertyType == typeof(byte[]))
                {
                    // Create the lambda expressions for the property and add to the mapper.
                    var spe = Expression.Parameter(SrceType, "x");
                    var sex = Expression.Lambda(Expression.Property(spe, nameof(IETag.ETag)), spe);
                    var dpe = Expression.Parameter(DestType, "x");
                    var dex = Expression.Lambda(Expression.Property(dpe, Database.DatabaseColumns.RowVersionName), dpe);
                    var pmap = (IPropertyMapper<T, TModel>?)typeof(EntityMapper<T, TModel>)
                        .GetMethod("PropertySrceAndDest", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.MakeGenericMethod(new Type[] { typeof(string), typeof(byte[]) })
                        .Invoke(this, new object[] { sex, dex });

                    pmap?.SetConverter(StringToBase64Converter.Default);
                }
            }

            if (typeof(IChangeLog).IsAssignableFrom(typeof(T)) && GetBySrcePropertyName(nameof(IChangeLog.ChangeLog)) == null)
            {
                var spe = Expression.Parameter(SrceType, "x");
                var sex = Expression.Lambda(Expression.Property(spe, nameof(IChangeLog.ChangeLog)), spe);
                var pmap = (IPropertyMapper<T, TModel>?)typeof(EntityMapper<T, TModel>)
                    .GetMethod("SrceProperty")
                    ?.MakeGenericMethod(new Type[] { typeof(ChangeLog) })
                    .Invoke(this, new object[] { sex });

                pmap?.SetMapper(new ChangeLogMapper<TModel>());
            }

            return this;
        }
    }

    /// <summary>
    /// Provides entity mapping capabilities to and from the <see cref="EfDbBase{TDbContext}">entity framework</see> model with a singleton <see cref="Default"/>.
    /// </summary>
    /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
    /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
    /// <typeparam name="TMapper">The mapper <see cref="Type"/>.</typeparam>
    public class EfDbMapper<T, TModel, TMapper> : EfDbMapper<T, TModel>
        where T : class, new()
        where TModel : class, new()
        where TMapper : EfDbMapper<T, TModel, TMapper>, new()
    {
        private static readonly TMapper _default = new TMapper();

        /// <summary>
        /// Gets the current instance of the mapper.
        /// </summary>
#pragma warning disable CA1000 // Do not declare static members on generic types; by-design, is ok.
        public static TMapper Default
#pragma warning restore CA1000 
        {
            get
            {
                if (_default == null)
                    throw new InvalidOperationException("An instance of this Mapper cannot be referenced as it is still being constructed; beware that you may have a circular reference within the constructor.");

                return _default;
            }
        }
    }
}