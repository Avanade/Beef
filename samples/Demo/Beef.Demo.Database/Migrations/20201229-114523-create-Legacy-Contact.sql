-- Migration Script

BEGIN TRANSACTION

CREATE TABLE [Legacy].[Contact] (
  [ContactId]    INT            NOT NULL IDENTITY(1,1),
  [Name]         NVARCHAR (200) NULL,
  [Phone]        VARCHAR (15)   NULL,
  [Email]        VARCHAR (200)  NULL,
  [Active]       BIT            NULL,
  [DontCallList] BIT            NULL,
  [AddressId]    INT            NULL,
  CONSTRAINT [PK_Contact] PRIMARY KEY CLUSTERED ([ContactId] ASC),
  CONSTRAINT [FK_Contact_Address] FOREIGN KEY ([AddressId]) REFERENCES [Legacy].[Address] ([Id])
);

COMMIT TRANSACTION