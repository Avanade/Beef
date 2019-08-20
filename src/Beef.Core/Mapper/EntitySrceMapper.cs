using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Mapper
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
        new IPropertySrceMapper<TSrce> GetBySrcePropertyName(string name);

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        new IPropertySrceMapper<TSrce> GetByDestPropertyName(string name);
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
        private IPropertyMapperBase[] _uniqueKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySrceMapper{TSrce}"/> class.
        /// </summary>
        public EntitySrceMapper()
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
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            if (_srceMappings.ContainsKey(mapping.SrcePropertyName))
                throw new ArgumentException(string.Format("Source property '{0}' mapping can not be specified more than once.", mapping.SrcePropertyName), "SourcePropertyName");

            if (_destMappings.ContainsKey(mapping.DestPropertyName))
                throw new ArgumentException(string.Format("Destination property '{0}' mapping can not be specified more than once.", mapping.DestPropertyName), "DestinationPropertyName");

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
        public IPropertySrceMapper<TSrce>[] Mappings
        {
            get { return _mappings.ToArray(); }
        }

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by source property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        public IPropertySrceMapper<TSrce> GetBySrcePropertyName(string name)
        {
            return _srceMappings.TryGetValue(name, out IPropertySrceMapper<TSrce> map) ? map : null;
        }

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        public IPropertySrceMapper<TSrce> GetByDestPropertyName(string name)
        {
            return _destMappings.TryGetValue(name, out IPropertySrceMapper<TSrce> map) ? map : null;
        }

        /// <summary>
        /// Gets the <see cref="IPropertyMapperBase"/> mapping by source property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertyMapperBase"/> where found; otherwise, <c>null</c>.</returns>
        IPropertyMapperBase IEntityMapperBase.GetBySrcePropertyName(string name)
        {
            return GetBySrcePropertyName(name);
        }

        /// <summary>
        /// Gets the <see cref="IPropertyMapperBase"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertyMapperBase"/> where found; otherwise, <c>null</c>.</returns>
        IPropertyMapperBase IEntityMapperBase.GetByDestPropertyName(string name)
        {
            return GetByDestPropertyName(name);
        }

        /// <summary>
        /// Gets the properties that form the unique key.
        /// </summary>
        protected IPropertyMapperBase[] UniqueKey
        {
            get
            {
                if (_uniqueKey != null)
                    return _uniqueKey;

                _uniqueKey = Mappings.Where(x => x.IsUniqueKey).ToArray();
                return _uniqueKey;
            }
        }
    }
}