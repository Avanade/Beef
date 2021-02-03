CREATE TYPE [Hr].[udtEmergencyContactList] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost. 
   */

  [EmergencyContactId] UNIQUEIDENTIFIER,
  [FirstName] NVARCHAR(100) NULL,
  [LastName] NVARCHAR(100) NULL,
  [PhoneNo] NVARCHAR(50) NULL,
  [RelationshipTypeCode] NVARCHAR(50) NULL
)