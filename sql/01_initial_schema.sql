CREATE DATABASE BA_Backend_DB;
GO

USE BA_Backend_DB;
GO

CREATE TABLE dbo.Tenants (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Slug NVARCHAR(100) NOT NULL UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE TABLE dbo.Users (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Tenants(Id) ON DELETE CASCADE,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    Role TINYINT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsLocked BIT NOT NULL DEFAULT 0,
    ActiveSessionId NVARCHAR(32) NULL,
    CurrentDeviceFingerprint NVARCHAR(64) NULL,
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Users_Email_Tenant UNIQUE (Email, TenantId),
    CONSTRAINT FK_Users_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
);
GO

CREATE INDEX IX_Users_TenantId ON dbo.Users(TenantId);
CREATE INDEX IX_Users_Email ON dbo.Users(Email);
GO

CREATE TABLE dbo.ActiveSessions (
    SessionId NVARCHAR(32) NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    IsRevoked BIT NOT NULL DEFAULT 0,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX IX_ActiveSessions_Validation ON dbo.ActiveSessions(SessionId, IsRevoked, ExpiresAt);
CREATE INDEX IX_ActiveSessions_UserId ON dbo.ActiveSessions(UserId);
GO

INSERT INTO dbo.Tenants (Name, Slug, IsActive) VALUES (N'Admin Tenant', 'admin', 1);
GO

DECLARE @AdminTenantId UNIQUEIDENTIFIER;
SELECT @AdminTenantId = Id FROM dbo.Tenants WHERE Slug = 'admin';

INSERT INTO dbo.Users (TenantId, Email, PasswordHash, FullName, Role, IsActive, IsLocked)
VALUES (
    @AdminTenantId,
    'admin@test.com',
    '$2a$12$ufgNR3HGmE7BZXXS7TvnIe6i2B.k7pCvG7VbFxJJSPNjGqI0l1R7u',
    'Admin User',
    1,
    1,
    0
);
GO
