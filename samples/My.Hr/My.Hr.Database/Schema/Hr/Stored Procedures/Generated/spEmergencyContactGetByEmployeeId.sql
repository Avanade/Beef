CREATE OR ALTER PROCEDURE [Hr].[spEmergencyContactGetByEmployeeId]
  @EmployeeId AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Select the requested data.
  SELECT
      [ec].[EmergencyContactId],
      [ec].[EmployeeId],
      [ec].[FirstName],
      [ec].[LastName],
      [ec].[PhoneNo],
      [ec].[RelationshipTypeCode]
    FROM [Hr].[EmergencyContact] AS [ec]
    WHERE [ec].[EmployeeId] = @EmployeeId
END