# Estructura del Proyecto - BA Backend

## Visión General

```
BA-Backend (Solución)
│
├── 📄 BA.Backend.sln                    # Solución principal de .NET
│
├── 📂 src/                              # Código fuente
│   │
│   ├── 📂 BA.Backend.Domain/            # CAPA 1: Lógica de Negocio Pura
│   │   ├── 📄 BA.Backend.Domain.csproj
│   │   ├── 📂 Entities/
│   │   │   ├── 📄 User.cs              # Entidad: Usuario del sistema
│   │   │   ├── 📄 UserRole.cs          # Enum: Roles (Admin, Cliente, etc)
│   │   │   └── 📄 UserSession.cs       # Entidad: Sesiones activas
│   │   └── 📂 Interfaces/
│   │       ├── 📄 IUserRepository.cs
│   │       ├── 📄 IUserSessionRepository.cs
│   │       └── 📄 IJwtTokenService.cs
│   │
│   ├── 📂 BA.Backend.Application/       # CAPA 2: Casos de Uso (CQRS)
│   │   ├── 📄 BA.Backend.Application.csproj
│   │   ├── 📄 ServiceCollectionExtensions.cs  # DI: MediatR + Validadores
│   │   ├── 📄 ValidationBehavior.cs    # CQRS: Behavior de validación
│   │   ├── 📂 CQRS/
│   │   │   ├── 📂 Commands/
│   │   │   │   └── 📄 LoginCommand.cs  # Comando: Input para login
│   │   │   ├── 📂 Handlers/
│   │   │   │   └── 📄 LoginCommandHandler.cs  # Lógica del caso de uso
│   │   │   └── 📂 Validators/
│   │   │       └── 📄 LoginCommandValidator.cs # Validación: FluentValidation
│   │   ├── 📂 DTOs/
│   │   │   ├── 📄 LoginRequestDto.cs   # DTO: Input de login
│   │   │   └── 📄 LoginResponseDto.cs  # DTO: Output + token JWT
│   │   ├── 📂 Exceptions/
│   │   │   └── 📄 ApplicationException.cs  # Excepciones personalizadas
│   │   └── 📂 Interfaces/
│   │       └── 📄 IPasswordHasher.cs   # Contrato: Hash de contraseñas
│   │
│   ├── 📂 BA.Backend.Infrastructure/    # CAPA 3: Implementación Técnica
│   │   ├── 📄 BA.Backend.Infrastructure.csproj
│   │   ├── 📄 ServiceCollectionExtensions.cs  # DI: DbContext + Repositories + Services
│   │   ├── 📂 Data/
│   │   │   └── 📄 ApplicationDbContext.cs  # Entity Framework Core (SQL Server)
│   │   ├── 📂 Repositories/
│   │   │   ├── 📄 UserRepository.cs    # Implementación: CRUD de usuarios
│   │   │   └── 📄 UserSessionRepository.cs  # Implementación: Sesiones
│   │   └── 📂 Services/
│   │       ├── 📄 JwtTokenService.cs   # Implementación: JWT + Validación
│   │       └── 📄 PasswordHasher.cs    # Implementación: BCrypt
│   │
│   └── 📂 BA.Backend.WebAPI/            # CAPA 4: Presentación HTTP
│       ├── 📄 BA.Backend.WebAPI.csproj
│       ├── 📄 Program.cs                # Configuración de ASP.NET Core
│       ├── 📄 appsettings.json          # Configuración: BD, JWT, CORS
│       ├── 📄 appsettings.Development.json
│       └── 📂 Controllers/
│           └── 📄 AuthController.cs    # Endpoint: POST /api/auth/login
│
├── 📂 sql/                              # Scripts de Base de Datos
│   └── 📄 01_initial_schema.sql        # Script de inicialización (DDL)
│
├── 📂 .env                              # Variables de ambiente (NO versionar)
├── 📄 .env.example                      # Plantilla de variables
├── 📄 .gitignore                        # Archivos a ignorar en Git
│
└── 📚 Documentación/
    ├── 📄 README.md                     # Guía completa del proyecto
    ├── 📄 QUICK_START.md                # Inicio rápido (5 pasos)
    ├── 📄 DEVELOPMENT.md                # Guía para desarrolladores
    ├── 📄 ARCHITECTURE_DECISIONS.md     # ADR (Decisiones arquitectónicas)
    └── 📄 API_EXAMPLES.md               # Ejemplos de requests/responses
```

## Detalles por Capa

### 🔷 DOMAIN LAYER (BA.Backend.Domain)

**Propósito**: Expresa las reglas de negocio mediante entidades y contratos

**Contenido**:
- **Entities**: Modelos de datos (User, UserSession, UserRole)
- **Interfaces**: Contratos que otras capas implementarán

**Dependencias**: ❌ Ninguna (evita acoplamiento)

**Ejemplo**:
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    // ...
}
```

---

### 🔶 APPLICATION LAYER (BA.Backend.Application)

**Propósito**: Implementa casos de uso usando CQRS y validación

**Contenido**:
- **Commands**: Operaciones de escritura (LoginCommand)
- **Handlers**: Lógica del caso de uso
- **Validators**: Validación de entrada (FluentValidation)
- **DTOs**: Modelos transferencia entre capas
- **Exceptions**: Excepciones del negocio

**Patrón**: CQRS (Command Query Responsibility Segregation)

**Flujo CQRS**:
```
Request
  ↓
[ValidationBehavior] → Validar con FluentValidation
  ↓
[LoginCommandHandler] → Ejecutar lógica del caso de uso
  ↓
Response
```

---

### 🔴 INFRASTRUCTURE LAYER (BA.Backend.Infrastructure)

**Propósito**: Implementa los servicios técnicos requeridos por Application

**Contenido**:
- **DbContext**: Mapeo de entidades a BD (Entity Framework Core)
- **Repositories**: Acceso a datos (EF Core para escritura, Dapper para lectura)
- **Services**: JWT, password hashing, etc.

**Patrón**: Repository Pattern

**Ejemplo - Flujo de Login**:
```
1. UserRepository.GetByEmailAsync("user@example.com")  ← Lectura (Dapper)
2. PasswordHasher.VerifyPassword(password, hash)        ← Verificación
3. JwtTokenService.GenerateToken(user, sessionId)       ← Generación JWT
4. UserSessionRepository.CreateSessionAsync(session)    ← Escritura (EF Core)
```

---

### 🟢 WEBAPI LAYER (BA.Backend.WebAPI)

**Propósito**: Expone casos de uso como endpoints HTTP REST

**Contenido**:
- **Controllers**: Mapeo de HTTP → CQRS
- **Program.cs**: Configuración de DI y middleware
- **appsettings.json**: Configuración de ambiente

**Ejemplo - Endpoint**:
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
{
    var command = new LoginCommand(...);
    var response = await _mediator.Send(command);
    return Ok(response);
}
```

---

## Flujo de Datos - Login

```
Cliente HTTP Request
  │
  ├→ [AuthController.Login]
  │    ├→ Crear LoginCommand
  │    └→ Enviar a MediatR
  │
  ├→ [ValidationBehavior]
  │    ├→ Validar email (formato)
  │    ├→ Validar password (mínimo 6 caracteres)
  │    └→ Validar deviceId (no vacío)
  │
  ├→ [LoginCommandHandler]
  │    ├→ UserRepository.GetByEmailAsync()
  │    ├→ PasswordHasher.VerifyPassword()
  │    ├→ UserSessionRepository.InvalidatePreviousSessionAsync()
  │    ├→ JwtTokenService.GenerateToken()
  │    ├→ UserSessionRepository.CreateSessionAsync()
  │    └→ UserRepository.UpdateAsync()
  │
  └→ [AuthController] Retorna LoginResponseDto
        │
        └→ Cliente recibe AccessToken + RefreshToken
```

---

## Índices de Base de Datos

```sql
-- Búsqueda rápida por email (login)
CREATE INDEX IX_Users_Email ON Users([Email]);

-- Sesión única por dispositivo
CREATE INDEX IX_UserSession_UserId_DeviceId ON UserSessions([UserId], [DeviceId]);

-- Queries por tenant
CREATE INDEX IX_Users_TenantId_IsActive ON Users([TenantId], [IsActive]);

-- Búsqueda de sesiones activas
CREATE INDEX IX_UserSession_UserId_IsActive ON UserSessions([UserId], [IsActive]);
```

---

## Dependencias Externas

| Paquete | Versión | Propósito |
|---------|---------|----------|
| Entity Framework Core | 8.0.0 | ORM para SQL Server |
| MediatR | 12.1.1 | CQRS / Orquestación |
| FluentValidation | 11.8.1 | Validación de datos |
| System.IdentityModel.Tokens.Jwt | 7.1.0 | JWT |
| Dapper | 2.0.123 | Queries optimizadas |
| BCrypt.Net-Core | 1.6.0 | Hash de contraseñas |

---

## Convención de Nombrado

```csharp
// Proyectos
BA.Backend.Domain
BA.Backend.Application
BA.Backend.Infrastructure
BA.Backend.WebAPI

// Carpetas
Entities/      (Modelos de datos)
Interfaces/    (Contratos)
CQRS/          (Handlers, Validators, etc)
DTOs/          (Data Transfer Objects)
Services/      (Servicios técnicos)
Repositories/  (Acceso a datos)

// Clases
Entity → User
DTO → UserInfoDto
Repository → UserRepository
Service → JwtTokenService
Command → LoginCommand
Handler → LoginCommandHandler
Validator → LoginCommandValidator

// Métodos
Async → GetUserAsync(), CreateSessionAsync()
``` 

---

## Próximas Capas a Implementar

### Phase 2: Queries CQRS
```
Application/CQRS/Queries/
├── GetUserByIdQuery
├── GetUserSessionsQuery
└── GetActiveSessionQuery
```

### Phase 3: Más Comandos
```
Application/CQRS/Commands/
├── LogoutCommand
├── ChangePasswordCommand
├── InvalidateTokenCommand    (Admin button)
└── RefreshTokenCommand
```

### Phase 4: Seguridad Avanzada
- Rate limiting
- 2FA (TOTP)
- OAuth2 / OpenID Connect
- Event sourcing para auditoría

---

## Recursos de Referencia

- 📌 [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- 📌 [CQRS Pattern - Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- 📌 [MediatR Documentation](https://github.com/jbogard/MediatR)
- 📌 [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- 📌 [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- 📌 [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

---

**Última actualización**: 2026-03-18  
**Versión**: 1.0.0
