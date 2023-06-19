/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace My.Hr.Business.Data.EfModel;

/// <summary>
/// Represents extension methods for the <see cref="ModelBuilder"/>.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Adds all the generated models to the <see cref="ModelBuilder"/>.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/>.</param>
    public static void AddGeneratedModels(this ModelBuilder modelBuilder)
    {
        Gender.AddToModel(modelBuilder.ThrowIfNull());
        TerminationReason.AddToModel(modelBuilder);
        RelationshipType.AddToModel(modelBuilder);
        USState.AddToModel(modelBuilder);
        Employee.AddToModel(modelBuilder);
        PerformanceReview.AddToModel(modelBuilder);
        PerformanceOutcome.AddToModel(modelBuilder);
    }
}

#pragma warning restore
#nullable restore