// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.Database.Mapper;
using Beef.Mapper;
using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

namespace Beef.Data.Database
{
    /// <summary>
    /// Enables core <see cref="DatabaseBase">database</see> property mapping capabilities.
    /// </summary>
    public interface IDatabasePropertyMapper : IPropertyMapperBase
    {
        /// <summary>
        /// Gets or sets the destination <see cref="DatabaseParameters"/> name.
        /// </summary>
        string DestParameterName { get; set; }

        /// <summary>
        /// Gets or sets the destination <see cref="DbType"/> (overriding inferred).
        /// </summary>
        /// <remarks>A <c>null</c> value indicates that the <see cref="DbType"/> is to be inferred.</remarks>
        DbType? DestDbType { get; set; }

        /// <summary>
        /// Sets the source property value from <see cref="DatabaseRecord"/>.
        /// </summary>
        /// <param name="entity">The source entity.</param>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void SetSrceValue(object entity, DatabaseRecord dr, OperationTypes operationType);

        /// <summary>
        /// Sets the destination <see cref="DatabaseParameters"/> from the source property value.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="parameters">The <see cref="DatabaseParameters"/>.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void SetDestValue(object value, DatabaseParameters parameters, OperationTypes operationType);
    }

    /// <summary>
    /// Enables <see cref="DatabaseBase">database</see> property mapping capabilities.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    public interface IDatabasePropertyMapper<TSrce> : IDatabasePropertyMapper, IPropertySrceMapper<TSrce> where TSrce : class, new()
    {
        /// <summary>
        /// Invokes the <b>when</b> clauses to determine whether the destination to source mapping should occur.
        /// </summary>
        /// <param name="dr">The <see cref="DbDataReader"/>.</param>
        /// <returns><c>true</c> indicates that the mapping should occur; otherwise, <c>false</c>.</returns>
        bool MapDestToSrceWhen(DatabaseRecord dr);

        /// <summary>
        /// Sets the source property value from <see cref="DatabaseRecord"/>.
        /// </summary>
        /// <param name="entity">The source entity.</param>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void SetSrceValue(TSrce entity, DatabaseRecord dr, OperationTypes operationType);

        /// <summary>
        /// Sets the destination <see cref="DatabaseParameters"/> from the source property value.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="parameters">The <see cref="DatabaseParameters"/>.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void SetDestValue(TSrce value, DatabaseParameters parameters, OperationTypes operationType);
    }

    /// <summary>
    /// Provides property/parameter mapping capabilities to the <see cref="DatabaseBase">database</see>.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public class DatabasePropertyMapper<TSrce, TSrceProperty> : PropertyMapperCustomBase<TSrce, TSrceProperty>, IDatabasePropertyMapper<TSrce>
        where TSrce : class, new()
    {
        private Func<DatabaseRecord, bool>? _mapDestToSrceWhen;
        private Action<TSrce, DatabaseParameters, OperationTypes>? _mapToDbOverride;
        private Func<DatabaseRecord, TSrce, OperationTypes, TSrceProperty>? _mapFromDbOverride;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabasePropertyMapper{TSrce, TSrceProperty}"/> class.
        /// </summary>
        /// <param name="srcePropertyExpression">The <see cref="LambdaExpression"/> to reference the source entity property.</param>
        /// <param name="destColumnName">The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}.DestPropertyName">name</see> of the destination database column (auto-generated from the source where not specified).</param>
        /// <param name="operationTypes">The <see cref="Beef.Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property (default to <see cref="OperationTypes.Any"/>).</param>
        public DatabasePropertyMapper(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression, string? destColumnName = null, OperationTypes operationTypes = OperationTypes.Any) 
            : base(srcePropertyExpression, destColumnName, operationTypes) => DestParameterName = "@" + DestPropertyName;

        /// <summary>
        /// Gets or sets the destination <see cref="DatabaseParameters"/> name.
        /// </summary>
        /// <remarks>Defaults to the <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}.SrcePropertyName"/> prefixed with '@' (as per the standard stored procedure convention.</remarks>
        public string DestParameterName { get; set; }

        /// <summary>
        /// Gets or sets the destination <see cref="DbType"/> (overriding inferred).
        /// </summary>
        /// <remarks>A <c>null</c> value indicates that the <see cref="DbType"/> is to be inferred.</remarks>
        public DbType? DestDbType { get; set; }

        /// <summary>
        /// Sets the <see cref="DestParameterName"/> (enables fluent-style).
        /// </summary>
        /// <param name="name">The <see cref="DbParameter"/> <see cref="DbParameter.ParameterName"/>.</param>
        /// <returns>The <see cref="DatabasePropertyMapper{TSrce, TSrceProperty}"/>.</returns>
        public DatabasePropertyMapper<TSrce, TSrceProperty> ParameterName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            DestParameterName = name;
            return this;
        }

        /// <summary>
        /// Sets the unique key (enables fluent-style).
        /// </summary>
        /// <param name="autoGeneratedOnCreate">Indicates whether the destination property value is auto-generated on create (defaults to <c>true</c>).</param>
        /// <returns>The <see cref="DatabasePropertyMapper{TSrce, TSrceProperty}"/>.</returns>
        public new DatabasePropertyMapper<TSrce, TSrceProperty> SetUniqueKey(bool autoGeneratedOnCreate = true)
        {
            base.SetUniqueKey(autoGeneratedOnCreate);
            return this;
        }

        /// <summary>
        /// Sets the <see cref="DestDbType"/> (enables fluent-style).
        /// </summary>
        /// <param name="dbType">The <see cref="DbParameter"/> <see cref="DbParameter.DbType"/>.</param>
        /// <returns>The <see cref="DatabasePropertyMapper{TSrce, TSrceProperty}"/>.</returns>
        public DatabasePropertyMapper<TSrce, TSrceProperty> SetDbType(DbType dbType)
        {
            DestDbType = dbType;
            return this;
        }

        /// <summary>
        /// Adds a conditional clause to this <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/> which must be <c>true</c> when mapping from the source to the destination.
        /// </summary>
        /// <param name="predicate">A function to determine whether the property is to be mapped.</param>
        /// <returns>The <see cref="DatabasePropertyMapper{TEntity, TProperty}"/>.</returns>
        public new DatabasePropertyMapper<TSrce, TSrceProperty> MapSrceToDestWhen(Func<TSrce, bool> predicate) => (DatabasePropertyMapper<TSrce, TSrceProperty>)base.MapSrceToDestWhen(predicate);

        /// <summary>
        /// Defines a conditional clause which must be <c>true</c> when mapping from the destination to the source.
        /// </summary>
        /// <param name="predicate">A function to determine whether the property is to be mapped.</param>
        /// <returns>The <see cref="DatabasePropertyMapper{TEntity, TProperty}"/>.</returns>
        public DatabasePropertyMapper<TSrce, TSrceProperty> MapDestToSrceWhen(Func<DatabaseRecord, bool> predicate)
        {
            _mapDestToSrceWhen = predicate;
            return this;
        }

        /// <summary>
        /// Invokes the <see cref="MapDestToSrceWhen(Func{DatabaseRecord, bool})"/> clauses to determine whether mapping from the destination should occur.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        /// <returns><c>true</c> indicates that the mapping should occur; otherwise, <c>false</c>.</returns>
        public bool MapDestToSrceWhen(DatabaseRecord dr) => (_mapDestToSrceWhen == null) || _mapDestToSrceWhen.Invoke(dr);

        /// <summary>
        /// Overrides the entity to database mapping for this <see cref="DatabasePropertyMapper{TSrce, TSrceProperty}"/>.
        /// </summary>
        /// <param name="action">An action to override the mapping.</param>
        /// <returns>The <see cref="DatabasePropertyMapper{TEntity, TProperty}"/>.</returns>
        public DatabasePropertyMapper<TSrce, TSrceProperty> MapToDb(Action<TSrce, DatabaseParameters, OperationTypes> action)
        {
            _mapToDbOverride = action;
            return this;
        }

        /// <summary>
        /// Overrides the database to entity mapping for this <see cref="DatabasePropertyMapper{TSrce, TSrceProperty}"/>.
        /// </summary>
        /// <param name="func">A function to override the mapping.</param>
        /// <returns>The <see cref="DatabasePropertyMapper{TEntity, TProperty}"/>.</returns>
        public DatabasePropertyMapper<TSrce, TSrceProperty> MapFromDb(Func<DatabaseRecord, TSrce, OperationTypes, TSrceProperty> func)
        {
            _mapFromDbOverride = func;
            return this;
        }

        /// <summary>
        /// Sets the source property value from <see cref="DatabaseRecord"/>.
        /// </summary>
        /// <param name="entity">The source entity.</param>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IDatabasePropertyMapper.SetSrceValue(object entity, DatabaseRecord dr, OperationTypes operationType) => SetSrceValue((TSrce)entity, dr, operationType);

        /// <summary>
        /// Sets the destination <see cref="DatabaseParameters"/> from the source property value.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="parameters">The <see cref="DatabaseParameters"/>.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IDatabasePropertyMapper.SetDestValue(object value, DatabaseParameters parameters, OperationTypes operationType) => SetDestValue((TSrce)value, parameters, operationType);

        /// <summary>
        /// Sets the source property value from <see cref="DatabaseRecord"/>.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        public void SetSrceValue(TSrce value, DatabaseRecord dr, OperationTypes operationType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (dr == null)
                throw new ArgumentNullException(nameof(dr));

            if (!OperationTypes.HasFlag(operationType))
                return;

            if (_mapFromDbOverride != null)
            {
                typeof(TSrce).GetProperty(SrcePropertyName).SetValue(value, _mapFromDbOverride(dr, value, operationType));
                return;
            }

            TSrceProperty val = default!;
            if (Mapper != null)
            {
                var em = (IDatabaseMapper)Mapper;
                val = (TSrceProperty)em.MapFromDb(dr, operationType, this)!;
            }
            else
            {
                int index = dr.GetOrdinal(DestPropertyName);
                if (!dr.IsDBNull(index))
                {
                    if (Converter != null)
                        val = (TSrceProperty)Converter.ConvertToSrce(Convert.ChangeType(dr.DataRecord.GetValue(index), Converter.DestUnderlyingType, System.Globalization.CultureInfo.InvariantCulture))!;
                    else
                        val = dr.GetValue<TSrceProperty>(index);
                }
            }

            SetSrceValue(value, val, operationType);
        }

        /// <summary>
        /// Sets the destination <see cref="DatabaseParameters"/> from the source property value.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="parameters">The <see cref="DatabaseParameters"/>.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        public void SetDestValue(TSrce value, DatabaseParameters parameters, OperationTypes operationType)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (!OperationTypes.HasFlag(operationType))
                return;

            if (parameters.Contains(DestParameterName))
                return;

            if (_mapToDbOverride != null)
            {
                _mapToDbOverride(value, parameters, operationType);
                return;
            }

            var val = GetSrceValue(value, operationType)!;
            if (Mapper != null)
            {
                if (val != null)
                {
                    var em = (IDatabaseMapper)Mapper;
                    em.MapToDb(val, parameters, operationType, this);
                }
            }
            else
            {
                if (DestDbType.HasValue)
                    parameters.AddParameter(DestParameterName, Converter == null ? val : Converter.ConvertToDest(val), dbType: DestDbType.Value);
                else
                    parameters.AddParameter(DestParameterName, Converter == null ? val : Converter.ConvertToDest(val));
            }
        }
    }
}