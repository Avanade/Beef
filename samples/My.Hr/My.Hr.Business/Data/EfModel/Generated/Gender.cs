/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options
#pragma warning disable CA2227, CA1819 // Collection/Array properties should be read only; ignored, as acceptable for an EF Model.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace My.Hr.Business.Data.EfModel
{
    /// <summary>
    /// Represents the Entity Framework (EF) model for database object 'Ref.Gender'.
    /// </summary>
    public partial class Gender
    {
        /// <summary>
        /// Gets or sets the 'GenderId' column value.
        /// </summary>
        public Guid GenderId { get; set; }

        /// <summary>
        /// Gets or sets the 'Code' column value.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the 'Text' column value.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the 'IsActive' column value.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the 'SortOrder' column value.
        /// </summary>
        public int? SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the 'RowVersion' column value.
        /// </summary>
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Gets or sets the 'CreatedBy' column value.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the 'CreatedDate' column value.
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the 'UpdatedBy' column value.
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the 'UpdatedDate' column value.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Adds the table/model configuration to the <see cref="ModelBuilder"/>.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/>.</param>
        public static void AddToModel(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<Gender>(entity =>
            {
                entity.ToTable("Gender", "Ref");
                entity.HasKey("GenderId");
                entity.Property(p => p.GenderId).HasColumnType("UNIQUEIDENTIFIER");
                entity.Property(p => p.Code).HasColumnType("NVARCHAR(50)");
                entity.Property(p => p.Text).HasColumnType("NVARCHAR(250)");
                entity.Property(p => p.IsActive).HasColumnType("BIT");
                entity.Property(p => p.SortOrder).HasColumnType("INT");
                entity.Property(p => p.RowVersion).HasColumnType("TIMESTAMP").IsRowVersion();
                entity.Property(p => p.CreatedBy).HasColumnType("NVARCHAR(250)").ValueGeneratedOnUpdate();
                entity.Property(p => p.CreatedDate).HasColumnType("DATETIME2").ValueGeneratedOnUpdate();
                entity.Property(p => p.UpdatedBy).HasColumnType("NVARCHAR(250)").ValueGeneratedOnAdd();
                entity.Property(p => p.UpdatedDate).HasColumnType("DATETIME2").ValueGeneratedOnAdd();
                AddToModel(entity);
            });
        }
        
        /// <summary>
        /// Enables further configuration of the underlying <see cref="EntityTypeBuilder"/> when configuring the <see cref="ModelBuilder"/>.
        /// </summary>
        static partial void AddToModel(EntityTypeBuilder<Gender> entity);
    }
}

#pragma warning restore CA2227, CA1819
#pragma warning restore IDE0005
#nullable restore