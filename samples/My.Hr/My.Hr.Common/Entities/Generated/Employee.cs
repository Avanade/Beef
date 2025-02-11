/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace My.Hr.Common.Entities;

/// <summary>
/// Represents the Employee entity.
/// </summary>
public partial class Employee : EmployeeBase, IETag, IChangeLog
{
    /// <summary>
    /// Gets or sets the Address.
    /// </summary>
    public Address? Address { get; set; }

    /// <summary>
    /// Gets or sets the Emergency Contacts.
    /// </summary>
    public EmergencyContactCollection? EmergencyContacts { get; set; }

    /// <summary>
    /// Gets or sets the ETag.
    /// </summary>
    [JsonPropertyName("etag")]
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the Change Log.
    /// </summary>
    public ChangeLog? ChangeLog { get; set; }
}
