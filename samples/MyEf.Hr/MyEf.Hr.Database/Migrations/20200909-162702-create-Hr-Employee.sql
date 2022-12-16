-- Migration Script

BEGIN TRANSACTION

CREATE TABLE [Hr].[Employee] (
  [EmployeeId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,  -- This is the primary key
  [Email] NVARCHAR(250) NULL UNIQUE,                                               -- This is the employee's unique email address
  [FirstName] NVARCHAR(100) NULL,
  [LastName] NVARCHAR(100) NULL,
  [GenderCode] NVARCHAR(50) NULL,                                                  -- This is the related Gender code; see Ref.Gender table
  [Birthday] DATE NULL,    
  [StartDate] DATE NULL,
  [TerminationDate] DATE NULL,
  [TerminationReasonCode] NVARCHAR(50) NULL,                                       -- This is the related Termination Reason code; see Ref.TerminationReason table
  [PhoneNo] NVARCHAR(50) NULL,
  [AddressJson] NVARCHAR(500) NULL,                                                -- This is the full address persisted as JSON.
  [RowVersion] TIMESTAMP NOT NULL,                                                 -- This is used for concurrency version checking. 
  [CreatedBy] NVARCHAR(250) NULL,                                                  -- The following are standard audit columns.
  [CreatedDate] DATETIME2 NULL,
  [UpdatedBy] NVARCHAR(250) NULL,
  [UpdatedDate] DATETIME2 NULL
);
	
COMMIT TRANSACTION