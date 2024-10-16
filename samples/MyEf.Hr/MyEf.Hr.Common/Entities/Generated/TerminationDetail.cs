/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace MyEf.Hr.Common.Entities;

/// <summary>
/// Represents the Termination Detail entity.
/// </summary>
public partial class TerminationDetail
{
    /// <summary>
    /// Gets or sets the Date.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets the corresponding <c>Reason</c> text (read-only where selected).
    /// </summary>
    public string? ReasonText { get; set; }

    /// <summary>
    /// Gets or sets the Reason code.
    /// </summary>
    public string? Reason { get; set; }
}
