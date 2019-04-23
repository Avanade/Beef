// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.Database;
using System;
using System.Xml.Linq;

namespace Beef.CodeGen.Entities
{
    /// <summary>
    /// Represents the SQL Server Database <b>Column</b> schema definition.
    /// </summary>
    public class Column
    {
        /// <summary>
        /// Indicates whether the database type maps to a <see cref="string"/>.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsString(string dbType)
        {
            switch (dbType.ToLower())
            {
                case "nchar":
                case "char":
                case "nvarchar":
                case "varchar":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Indicates whether the database type maps to a <see cref="decimal"/>.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsDecimal(string dbType)
        {
            switch (dbType.ToLower())
            {
                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Indicates whether the database type maps to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsDateTime(string dbType)
        {
            switch (dbType.ToLower())
            {
                case "date":
                case "datetime":
                case "datetime2":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Indicates whether the database type maps to an integer.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsInteger(string dbType)
        {
            switch (dbType.ToLower())
            {
                case "int":
                case "bigint":
                case "smallint":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> name for the database type.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        /// <returns>The .NET <see cref="System.Type"/> name.</returns>
        public static string GetDotNetTypeName(string dbType)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings

            if (string.IsNullOrEmpty(dbType))
                return null;

            if (TypeIsString(dbType))
                return "string";
            else if (TypeIsDecimal(dbType))
                return "decimal";
            else if (TypeIsDateTime(dbType))
                return "DateTime";

            switch (dbType.ToLower())
            {
                case "rowversion":
                case "timestamp":
                case "varbinary": return "byte[]";
                case "bit": return "bool";
                case "datetimeoffset": return "DateTimeOffset";
                case "float": return "double";
                case "int": return "int";
                case "bigint": return "long";
                case "smallint": return "short";
                case "tinyint": return "byte";
                case "real": return "float";
                case "time": return "TimeSpan";
                case "uniqueidentifier": return "Guid";

                default:
                    throw new InvalidOperationException($"Database data type '{dbType}' does not have corresponding .NET type mapping defined.");
            }
        }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> for the database type.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        /// <returns>The .NET <see cref="System.Type"/> name.</returns>
        public static Type GetDotNetType(string dbType)
        {
            switch (GetDotNetTypeName(dbType))
            {
                case "string": return typeof(string);
                case "decimal": return typeof(decimal);
                case "DateTime": return typeof(DateTime);
                case "byte[]": return typeof(byte[]);
                case "bool": return typeof(bool);
                case "DateTimeOffset": return typeof(DateTimeOffset);
                case "double": return typeof(double);
                case "int": return typeof(int);
                case "long": return typeof(long);
                case "short": return typeof(short);
                case "byte": return typeof(byte);
                case "float": return typeof(float);
                case "TimeSpan": return typeof(TimeSpan);
                case "Guid": return typeof(Guid);

                default:
                    throw new InvalidOperationException($"Database data type '{dbType}' does not have corresponding .NET type mapping defined.");
            }
        }

        /// <summary>
        /// Gets the column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the SQL Server data type.
        /// </summary>
        public string Type { get; set; }

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
        public string DefaultValue { get; set; }

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
        public string ForeignTable { get; set; }

        /// <summary>
        /// Gets or sets the foreign key schema.
        /// </summary>
        public string ForeignSchema { get; set; }

        /// <summary>
        /// Gets or sets the foreign key column name.
        /// </summary>
        public string ForeignColumn { get; set; }

        /// <summary>
        /// Indicates whether the foreign key is references a reference data table/entity.
        /// </summary>
        public bool IsForeignRefData { get; set; }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> name.
        /// </summary>
        public string DotNetType
        {
            get => GetDotNetTypeName(Type);
        }

        /// <summary>
        /// Creates (and adds) the <see cref="Table"/> element for code generation.
        /// </summary>
        /// <param name="xml">The <see cref="XElement"/> to add to.</param>
        public void CreateXml(XElement xml)
        {
            var xc = new XElement("Column",
                new XAttribute("Name", Name),
                new XAttribute("Type", Type),
                new XAttribute("DotNetType", DotNetType),
                new XAttribute("IsNullable", IsNullable));

            if (Length.HasValue)
                xc.Add(new XAttribute("Length", Length.Value));

            if (Precision.HasValue)
                xc.Add(new XAttribute("Precision", Precision.Value));

            if (Scale.HasValue)
                xc.Add(new XAttribute("Scale", Scale.Value));

            if (IsIdentity)
            {
                xc.Add(new XAttribute("IsIdentity", true));
                if (!IsComputed && DefaultValue == null)
                {
                    xc.Add(new XAttribute("IdentitySeed", IdentitySeed ?? 1));
                    xc.Add(new XAttribute("IdentityIncrement", IdentityIncrement ?? 1));
                }
            }

            if (IsComputed)
                xc.Add(new XAttribute("IsComputed", true));

            if (!string.IsNullOrEmpty(DefaultValue))
                xc.Add(new XAttribute("DefaultValue", DefaultValue));

            if (IsPrimaryKey)
                xc.Add(new XAttribute("IsPrimaryKey", true));

            if (IsUnique)
                xc.Add(new XAttribute("IsUnique", true));

            if (ForeignTable != null)
            {
                xc.Add(new XAttribute("ForeignTable", ForeignTable));
                xc.Add(new XAttribute("ForeignSchema", ForeignSchema));
                xc.Add(new XAttribute("ForeignColumn", ForeignColumn));
                if (IsForeignRefData)
                    xc.Add(new XAttribute("IsForeignRefData", true));
            }

            xml.Add(xc);
        }
    }

    /// <summary>
    /// Represents the <see cref="Column"/> database mapper.
    /// </summary>
    internal class ColumnMapper : DatabaseMapper<Column, ColumnMapper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnMapper"/> class.
        /// </summary>
        public ColumnMapper()
        {
            Property(x => x.Name, "COLUMN_NAME");
            Property(x => x.Type, "DATA_TYPE");
            Property(x => x.IsNullable).MapFromDb((dr, c, ot) => dr.GetValue<string>("IS_NULLABLE").ToUpper() == "YES");
            Property(x => x.Length, "CHARACTER_MAXIMUM_LENGTH");
            Property(x => x.Precision).MapFromDb((dr, c, ot) => dr.GetValue<int?>("NUMERIC_PRECISION") ?? dr.GetValue<int?>("DATETIME_PRECISION"));
            Property(x => x.Scale, "NUMERIC_SCALE");
            Property(x => x.DefaultValue, "COLUMN_DEFAULT");
        }
    }
}
