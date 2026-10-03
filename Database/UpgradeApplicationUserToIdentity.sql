-- Upgrade the existing custom table to the ApplicationUserModel : IdentityUser shape.
-- Run against the application's database. Existing values are preserved.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.ApplicationUser', N'U') IS NULL
        THROW 50000, 'dbo.ApplicationUser does not exist.', 1;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ApplicationUser') AND name = N'Id' AND system_type_id = TYPE_ID(N'uniqueidentifier'))
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE referenced_object_id = OBJECT_ID(N'dbo.ApplicationUser') OR parent_object_id = OBJECT_ID(N'dbo.ApplicationUser'))
            THROW 50001, 'Migrate related foreign keys before converting ApplicationUser.Id.', 1;

        ALTER TABLE dbo.ApplicationUser ADD IdentityId nvarchar(450) NULL;
        EXEC(N'UPDATE dbo.ApplicationUser SET IdentityId = CONVERT(nvarchar(450), Id)');
        ALTER TABLE dbo.ApplicationUser ALTER COLUMN IdentityId nvarchar(450) NOT NULL;
        DECLARE @pk sysname = (SELECT name FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.ApplicationUser') AND type = 'PK');
        IF @pk IS NOT NULL
        BEGIN
            DECLARE @dropPk nvarchar(max) = N'ALTER TABLE dbo.ApplicationUser DROP CONSTRAINT ' + QUOTENAME(@pk);
            EXEC(@dropPk);
        END;
        ALTER TABLE dbo.ApplicationUser DROP COLUMN Id;
        EXEC sys.sp_rename N'dbo.ApplicationUser.IdentityId', N'Id', N'COLUMN';
        EXEC(N'ALTER TABLE dbo.ApplicationUser ADD CONSTRAINT ApplicationUser_pkey PRIMARY KEY (Id)');
    END;

    IF COL_LENGTH(N'dbo.ApplicationUser', N'HashedPassword') IS NOT NULL AND COL_LENGTH(N'dbo.ApplicationUser', N'PasswordHash') IS NULL
        EXEC sys.sp_rename N'dbo.ApplicationUser.HashedPassword', N'PasswordHash', N'COLUMN';
    IF COL_LENGTH(N'dbo.ApplicationUser', N'PasswordHash') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD PasswordHash nvarchar(max) NULL;
    EXEC(N'ALTER TABLE dbo.ApplicationUser ALTER COLUMN PasswordHash nvarchar(max) NULL');

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ApplicationUser') AND name COLLATE Latin1_General_100_BIN2 = N'Username')
        EXEC sys.sp_rename N'dbo.ApplicationUser.Username', N'UserName', N'COLUMN';

    IF COL_LENGTH(N'dbo.ApplicationUser', N'AccessFailedCount') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD AccessFailedCount int NOT NULL CONSTRAINT DF_ApplicationUser_AccessFailedCount DEFAULT (0);
    IF COL_LENGTH(N'dbo.ApplicationUser', N'ConcurrencyStamp') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD ConcurrencyStamp nvarchar(max) NULL;
    IF COL_LENGTH(N'dbo.ApplicationUser', N'EmailConfirmed') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD EmailConfirmed bit NOT NULL CONSTRAINT DF_ApplicationUser_EmailConfirmed DEFAULT (0);
    IF COL_LENGTH(N'dbo.ApplicationUser', N'LockoutEnabled') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD LockoutEnabled bit NOT NULL CONSTRAINT DF_ApplicationUser_LockoutEnabled DEFAULT (0);
    IF COL_LENGTH(N'dbo.ApplicationUser', N'LockoutEnd') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD LockoutEnd datetimeoffset NULL;
    IF COL_LENGTH(N'dbo.ApplicationUser', N'NormalizedEmail') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD NormalizedEmail nvarchar(256) NULL;
    IF COL_LENGTH(N'dbo.ApplicationUser', N'NormalizedUserName') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD NormalizedUserName nvarchar(256) NULL;
    IF COL_LENGTH(N'dbo.ApplicationUser', N'PhoneNumber') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD PhoneNumber nvarchar(max) NULL;
    IF COL_LENGTH(N'dbo.ApplicationUser', N'PhoneNumberConfirmed') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD PhoneNumberConfirmed bit NOT NULL CONSTRAINT DF_ApplicationUser_PhoneNumberConfirmed DEFAULT (0);
    IF COL_LENGTH(N'dbo.ApplicationUser', N'SecurityStamp') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD SecurityStamp nvarchar(max) NULL;
    IF COL_LENGTH(N'dbo.ApplicationUser', N'TwoFactorEnabled') IS NULL
        ALTER TABLE dbo.ApplicationUser ADD TwoFactorEnabled bit NOT NULL CONSTRAINT DF_ApplicationUser_TwoFactorEnabled DEFAULT (0);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
