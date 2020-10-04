CREATE PROCEDURE [Hr].[spEmployeeGet]
  @EmployeeId AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Execute the primary select query.
  SELECT
    [e].[EmployeeId],
    [e].[Email],
    [e].[FirstName],
    [e].[LastName],
    [e].[GenderCode],
    [e].[Birthday],
    [e].[StartDate],
    [e].[TerminationDate],
    [e].[TerminationReasonCode],
    [e].[PhoneNo],
    [e].[AddressJson],
    [e].[RowVersion],
    [e].[CreatedBy],
    [e].[CreatedDate],
    [e].[UpdatedBy],
    [e].[UpdatedDate]
    FROM [Hr].[Employee] AS [e]
      WHERE [e].[EmployeeId] = @EmployeeId

  -- Execute additional statements.
  EXEC [Hr].[spEmergencyContactGetByEmployeeId] @EmployeeId
END