/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using Beef.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beef.Demo.Business.Data.EfModel
{
    /// <summary>
    /// Represents the Entity Framework (EF) model for database object 'Demo.Contact'.
    /// </summary>
    public partial class Contact : ILogicallyDeleted
    {
        /// <summary>
        /// Gets or sets the 'ContactId' column value.
        /// </summary>
        public Guid ContactId { get; set; }

        /// <summary>
        /// Gets or sets the 'FirstName' column value.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the 'LastName' column value.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the 'StatusCode' column value.
        /// </summary>
        public string? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the 'IsDeleted' column value.
        /// </summary>
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// Adds the table/model configuration to the <see cref="ModelBuilder"/>.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/>.</param>
        public static void AddToModel(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.ToTable("Contact", "Demo");
                entity.HasKey("ContactId");
                entity.Property(p => p.ContactId).HasColumnType("UNIQUEIDENTIFIER");
                entity.Property(p => p.FirstName).HasColumnType("NVARCHAR(50)");
                entity.Property(p => p.LastName).HasColumnType("NVARCHAR(50)");
                entity.Property(p => p.StatusCode).HasColumnType("NVARCHAR(50)");
                entity.Property(p => p.IsDeleted).HasColumnType("BIT");
                entity.HasQueryFilter(v => v.IsDeleted != true);
                AddToModel(entity);
            });
        }
        
        /// <summary>
        /// Enables further configuration of the underlying <see cref="EntityTypeBuilder"/> when configuring the <see cref="ModelBuilder"/>.
        /// </summary>
        static partial void AddToModel(EntityTypeBuilder<Contact> entity);
    }
}

#pragma warning restore
#nullable restore