// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Text;

namespace Beef.CodeGen.Database
{
    /// <summary>
    /// Represents the Database <b>Column</b> schema definition.
    /// </summary>
    public class DbColumn
    {
        /// <summary>
        /// Gets the owning (parent) <see cref="DbTable"/>.
        /// </summary>
        public DbTable? DbTable { get; set; }

        /// <summary>
        /// Gets the column name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets the SQL Server data type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Indicates whether the column is nullable.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// Indicates whether the column is an auto-generated identity.
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Gets or sets the identity seed value.
        /// </summary>
        public int? IdentitySeed { get; set; }

        /// <summary>
        /// Gets or sets the identity increment value;
        /// </summary>
        public int? IdentityIncrement { get; set; }

        /// <summary>
        /// Indicates whether the column is computed.
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Indicates whether the column is the primary key.
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Indicates whether the column has a unique constraint.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets the foreign key table.
        /// </summary>
        public string? ForeignTable { get; set; }

        /// <summary>
        /// Gets or sets the foreign key schema.
        /// </summary>
        public string? ForeignSchema { get; set; }

        /// <summary>
        /// Gets or sets the foreign key column name.
        /// </summary>
        public string? ForeignColumn { get; set; }

        /// <summary>
        /// Indicates whether the foreign key is references a reference data table/entity.
        /// </summary>
        public bool IsForeignRefData { get; set; }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> name.
        /// </summary>
        public string DotNetType => string.IsNullOrEmpty(Type) ? "string" : DbTypeHelper.GetDotNetTypeName(Type);

        /// <summary>
        /// Gets the fully defined SQL type.
        /// </summary>
        public string SqlType
        {
            get
            {
                var sb = new StringBuilder(Type!.ToUpperInvariant());
                if (DbTypeHelper.TypeIsString(Type))
                    sb.Append(Length.HasValue && Length.Value > 0 ? $"({Length.Value})" : "(MAX)");

                sb.Append(Type.ToUpperInvariant() switch
                {
                    "DECIMAL" => $"({Precision}, {Scale})",
                    "NUMERIC" => $"({Precision}, {Scale})",
                    "TIME" => Scale.HasValue && Scale.Value > 0 ? $"({Scale})" : string.Empty,
                    _ => string.Empty
                });

                if (IsNullable)
                    sb.Append(" NULL");

                return sb.ToString();
            }
        }

        /// <summary>
        /// Clones the <see cref="DbColumn"/> creating a new instance.
        /// </summary>
        /// <returns></returns>
        public DbColumn Clone()
        {
            return new DbColumn
            {
                DbTable = DbTable,
                Name = Name,
                Type = Type,
                IsNullable = IsNullable,
                Length = Length,
                Precision = Precision,
                Scale = Scale,
                IsIdentity = IsIdentity,
                IdentityIncrement = IdentityIncrement,
                IdentitySeed = IdentitySeed,
                IsComputed = IsComputed,
                DefaultValue = DefaultValue,
                IsPrimaryKey = IsPrimaryKey,
                IsUnique = IsUnique,
                ForeignTable = ForeignTable,
                ForeignSchema = ForeignSchema,
                ForeignColumn = ForeignColumn,
                IsForeignRefData = IsForeignRefData
            };
        }
    }
}