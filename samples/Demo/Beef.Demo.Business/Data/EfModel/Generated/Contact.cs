/*
 * This file is automatically generated; any changes will be lost. 
 */

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beef.Demo.Business.Data.EfModel;

/// <summary>
/// Represents the Entity Framework (EF) model for SqlServer database object [Demo].[Contact].
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
    /// Gets or sets the 'Comms' column value.
    /// </summary>
    public string? Comms { get; set; }

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
        modelBuilder.ThrowIfNull().Entity<Contact>(entity =>
        {
            entity.ToTable("Contact", "Demo");
            entity.HasKey(nameof(ContactId));
            entity.Property(p => p.ContactId).HasColumnName("ContactId").HasColumnType("UNIQUEIDENTIFIER");
            entity.Property(p => p.FirstName).HasColumnName("FirstName").HasColumnType("NVARCHAR(50)");
            entity.Property(p => p.LastName).HasColumnName("LastName").HasColumnType("NVARCHAR(50)");
            entity.Property(p => p.StatusCode).HasColumnName("StatusCode").HasColumnType("NVARCHAR(50)");
            entity.Property(p => p.Comms).HasColumnName("Comms").HasColumnType("NVARCHAR(MAX)");
            entity.Property(p => p.IsDeleted).HasColumnName("IsDeleted").HasColumnType("BIT");
            entity.HasQueryFilter(v => v.IsDeleted != true);
            AddToModel(entity);
        });
    }
        
    /// <summary>
    /// Enables further configuration of the underlying <see cref="EntityTypeBuilder"/> when configuring the <see cref="ModelBuilder"/>.
    /// </summary>
    static partial void AddToModel(EntityTypeBuilder<Contact> entity);
}