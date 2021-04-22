CREATE DATABASE [XyzLegacy];

GO

USE [XyzLegacy];

GO

CREATE SCHEMA [Legacy];

GO

CREATE TABLE [Legacy].[Person] (
  [PersonId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  [FirstName] NVARCHAR (100) NULL,
  [LastName] NVARCHAR (100) NULL,
  [Phone] NVARCHAR (15) NULL,
  [Email] NVARCHAR (200) NULL,
  [Active] BIT NULL,
  [SapId] INT NULL,
  [RowVersion] ROWVERSION
);

GO

CREATE TABLE [Legacy].[AddressType] (
  [AddressTypeId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  [Code] NVARCHAR(10)
);

GO

CREATE TABLE [Legacy].[PersonAddress] (
  [Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  [PersonId] INT NOT NULL,
  [AddressTypeId] INT NOT NULL,
  [Street1] NVARCHAR(100),
  [Street2] NVARCHAR(100),
  [City] NVARCHAR(50),
  [State] NVARCHAR(50),
  [PostalZipCode] NVARCHAR(20),
  [RowVersion] ROWVERSION,
  CONSTRAINT [FK_PersonAddress_Person] FOREIGN KEY ([PersonId]) REFERENCES [Legacy].[Person] ([PersonId]),
  CONSTRAINT [FK_PersonAddress_AddressType] FOREIGN KEY ([AddressTypeId]) REFERENCES [Legacy].[AddressType] ([AddressTypeId])
);

GO

INSERT INTO [Legacy].[Person] ([FirstName], [LastName], [Phone], [Email], [Active], [SapId]) VALUES ('John', 'Doe', '425 647 1234', 'jd@hotmail.com', 1, 10999)
INSERT INTO [Legacy].[Person] ([FirstName], [LastName], [Phone], [Email], [Active], [SapId]) VALUES ('Sarah', 'Smith', '425 671 9530', 'ss@aol.com', 1, 11888)

INSERT INTO [Legacy].[AddressType] ([Code]) VALUES ('HOME');
INSERT INTO [Legacy].[AddressType] ([Code]) VALUES ('POST');

INSERT INTO [Legacy].[PersonAddress] ([PersonId], [AddressTypeId], [Street1], [City], [State], [PostalZipCode]) VALUES (1, 1, '8000 Main Rd', 'Redmond', '98052', 'WA')
INSERT INTO [Legacy].[PersonAddress] ([PersonId], [AddressTypeId], [Street1], [City], [State], [PostalZipCode]) VALUES (1, 2, '1001 1ST AVE N', 'Seattle', '98109', 'WA')