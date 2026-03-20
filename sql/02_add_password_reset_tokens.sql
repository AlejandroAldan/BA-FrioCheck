-- Agregar esta sección al archivo 01_initial_schema.sql

-- Tabla para tokens de reset de contraseña
CREATE TABLE dbo.PasswordResetTokens (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(512) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);

-- Indice para búsqueda rápida por usuario
CREATE INDEX IX_PasswordResetTokens_UserId ON dbo.PasswordResetTokens(UserId);

-- Indice para búsqueda de tokens no usados
CREATE INDEX IX_PasswordResetTokens_IsUsed ON dbo.PasswordResetTokens(IsUsed, ExpiresAt);
GO
