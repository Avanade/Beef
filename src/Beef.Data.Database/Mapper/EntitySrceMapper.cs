// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper;
using Beef.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Beef.Data.Database.Mapper
{
    /// <summary>
    /// Enables the base source entity <see cref="Type"/> mapping capabilities. 
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    public interface IEntitySrceMapper<TSrce> : IEntityMapperBase
    {
        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by source property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        new IPropertySrceMapper<TSrce>? GetBySrcePropertyName(string name);

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        new IPropertySrceMapper<TSrce>? GetByDestPropertyName(string name);
    }

    /// <summary>
    /// Provides the base source entity mapping capabilities.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    public abstract class EntitySrceMapper<TSrce> : IEntitySrceMapper<TSrce> where TSrce : class
    {
        private readonly List<IPropertySrceMapper<TSrce>> _mappings = new List<IPropertySrceMapper<TSrce>>();
        private readonly Dictionary<string, IPropertySrceMapper<TSrce>> _srceMappings = new Dictionary<string, IPropertySrceMapper<TSrce>>();
        private readonly Dictionary<string, IPropertySrceMapper<TSrce>> _destMappings = new Dictionary<string, IPropertySrceMapper<TSrce>>();
        private IPropertyMapperBase[]? _uniqueKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySrceMapper{TSrce}"/> class.
        /// </summary>
        protected EntitySrceMapper()
        {
            if (typeof(TSrce) == typeof(string))
                throw new InvalidOperationException("SrceType must not be a String.");
        }

        /// <summary>
        /// Gets the source <see cref="Type"/>.
        /// </summary>
        public Type SrceType { get => typeof(TSrce); }

        /// <summary>
        /// Adds the <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/> to the underlying <see cref="Mappings"/> collection.
        /// </summary>
        /// <typeparam name="TSrceProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="mapping">The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/>.</param>
        protected void AddPropertyMapper<TSrceProperty>(PropertyMapperCustomBase<TSrce, TSrceProperty> mapping)
        {
            Check.NotNull(mapping, nameof(mapping));

            if (_srceMappings.ContainsKey(mapping.SrcePropertyName))
                throw new ArgumentException($"Source property '{mapping.SrcePropertyName}' mapping can not be specified more than once.", nameof(mapping));

            if (_destMappings.ContainsKey(mapping.DestPropertyName))
                throw new ArgumentException($"Destination property '{mapping.DestPropertyName}' mapping can not be specified more than once.", nameof(mapping));

            _srceMappings.Add(mapping.SrcePropertyName, mapping);
            _destMappings.Add(mapping.DestPropertyName, mapping);
            _mappings.Add(mapping);
        }


        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mappings.
        /// </summary>
        IEnumerable<IPropertyMapperBase> IEntityMapperBase.Mappings => Mappings;

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mappings.
        /// </summary>
        public IReadOnlyCollection<IPropertySrceMapper<TSrce>> Mappings => new ReadOnlyCollection<IPropertySrceMapper<TSrce>>(_mappings.ToArray()); 

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by source property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        public IPropertySrceMapper<TSrce>? GetBySrcePropertyName(string name) => _srceMappings.TryGetValue(name, out IPropertySrceMapper<TSrce> map) ? map : null;

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by source property expression.
        /// </summary>
        /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
        /// <param name="srcePropertyExpression">The <see cref="Expression"/> to reference the source entity property.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        public IPropertySrceMapper<TSrce>? GetBySrceProperty<TSrceProperty>(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression)
        {
            if (srcePropertyExpression == null)
                throw new ArgumentNullException(nameof(srcePropertyExpression));

            var spe = PropertyExpression.Create(srcePropertyExpression);
            return GetBySrcePropertyName(spe.Name);
        }

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        public IPropertySrceMapper<TSrce>? GetByDestPropertyName(string name) => _destMappings.TryGetValue(name, out IPropertySrceMapper<TSrce> map) ? map : null;

        /// <summary>
        /// Gets the <see cref="IPropertyMapperBase"/> mapping by source property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertyMapperBase"/> where found; otherwise, <c>null</c>.</returns>
        IPropertyMapperBase? IEntityMapperBase.GetBySrcePropertyName(string name) => GetBySrcePropertyName(name);

        /// <summary>
        /// Gets the <see cref="IPropertyMapperBase"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertyMapperBase"/> where found; otherwise, <c>null</c>.</returns>
        IPropertyMapperBase? IEntityMapperBase.GetByDestPropertyName(string name) => GetByDestPropertyName(name);

        /// <summary>
        /// Gets the properties that form the unique key.
        /// </summary>
        protected IReadOnlyList<IPropertyMapperBase> UniqueKey
        {
            get
            {
                if (_uniqueKey != null)
                    return new ReadOnlyCollection<IPropertyMapperBase>(_uniqueKey);

                _uniqueKey = Mappings.Where(x => x.IsUniqueKey).ToArray();
                return new ReadOnlyCollection<IPropertyMapperBase>(_uniqueKey);
            }
        }
    }
}