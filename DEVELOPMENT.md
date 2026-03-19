# Guía de Desarrollo - BA Backend

## Estructura de Decisiones Arquitectónicas

### Por qué Clean Architecture?

1. **Independencia de Frameworks**: El negocio no depende de MediatR, EF Core, etc.
2. **Testabilidad**: Las capas están desacopladas, fácil hacer tests
3. **Mantenibilidad**: Cambios en BD no afectan casos de uso
4. **Escalabilidad**: Fácil agregar nuevas características
5. **Claridad**: El código expresa la intención del negocio

### Por qué CQRS?

- **Separación de responsabilidades**: Escritura y lectura separadas
- **Performance**: Queries optimizadas con Dapper
- **Escalabilidad**: Posibilidad de read replicas en el futuro
- **Seguridad**: Control fino sobre qué datos se leen/escriben

### Por qué MediatR?

- **Orquestación centralizada**: Un solo punto de entrada para lógica de negocio
- **Comportamientos cross-cutting**: Validación, logging automático
- **Testabilidad**: Handlers pueden testearse independientemente
- **Extensibilidad**: Fácil agregar comportamientos sin modificar handlers

### Por qué Dapper?

- **Performance**: SQL puro compilado, mejor que EF Core para queries complejas
- **Control**: Exactitud total sobre queries
- **Eficiencia**: Perfecto para lecturas masivas
- **Combinación**: EF Core para escrituras, Dapper para lecturas

## Convenciones de Código

### Nombres

```csharp
// CORRECTO
public class UserRepository : IUserRepository
public async Task<User?> GetByEmailAsync(string email)
private readonly ILogger<AuthController> _logger;

// EVITAR
public class Users_Repository
public async void GetUser()
private ILogger logger;
```

### Métodos Async

```csharp
// Todos los métodos I/O deben ser async
public async Task<User?> GetByIdAsync(Guid id)
{
    return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
}
```

### Null Handling

```csharp
// Usar nullable reference types
public class User
{
    public string Email { get; set; } = null!; // Requerido
    public string? PhoneNumber { get; set; }   // Opcional
}

// En queries
var user = await _context.Users.FirstOrDefaultAsync(...); // User?
```

### Inyección de Dependencias

```csharp
// En constructores - inmutables
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }
}
```

## Flujos Comunes

### Agregar un Nuevo Caso de Uso

#### Ejemplo: Logout de Usuario

1. **Domain Layer** - `src/BA.Backend.Domain/Entities/`
   - Las entidades ya existen (User, UserSession)

2. **Application Layer** - Crear comando
   ```csharp
   // src/BA.Backend.Application/CQRS/Commands/LogoutCommand.cs
   public record LogoutCommand(Guid SessionId) : IRequest<bool>;
   ```

3. **Application Layer** - Crear validador
   ```csharp
   // src/BA.Backend.Application/CQRS/Validators/LogoutCommandValidator.cs
   public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
   {
       public LogoutCommandValidator()
       {
           RuleFor(x => x.SessionId)
               .NotEmpty().WithMessage("SessionId es requerido.");
       }
   }
   ```

4. **Application Layer** - Crear handler
   ```csharp
   // src/BA.Backend.Application/CQRS/Handlers/LogoutCommandHandler.cs
   public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
   {
       private readonly IUserSessionRepository _sessionRepository;

       public LogoutCommandHandler(IUserSessionRepository sessionRepository)
       {
           _sessionRepository = sessionRepository;
       }

       public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
       {
           return await _sessionRepository.InvalidateSessionAsync(
               request.SessionId, 
               "Logout del usuario");
       }
   }
   ```

5. **WebAPI Layer** - Agregar endpoint
   ```csharp
   // En AuthController.cs
   [HttpPost("logout")]
   [Authorize]
   public async Task<IActionResult> Logout()
   {
       var sessionIdClaim = User.FindFirst("sessionId");
       if (sessionIdClaim == null)
           return BadRequest();

       var command = new LogoutCommand(Guid.Parse(sessionIdClaim.Value));
       var result = await _mediator.Send(command);

       return Ok(new { success = result });
   }
   ```

### Agregar una Query CQRS (Lectura)

#### Ejemplo: Get User by ID (lectura)

1. **Application Layer** - Crear query
   ```csharp
   // src/BA.Backend.Application/CQRS/Queries/GetUserByIdQuery.cs
   public record GetUserByIdQuery(Guid UserId) : IRequest<UserInfoDto>;
   ```

2. **Application Layer** - Crear handler
   ```csharp
   // src/BA.Backend.Application/CQRS/Handlers/GetUserByIdQueryHandler.cs
   public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserInfoDto>
   {
       private readonly IUserRepository _userRepository;

       public GetUserByIdQueryHandler(IUserRepository userRepository)
       {
           _userRepository = userRepository;
       }

       public async Task<UserInfoDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
       {
           var user = await _userRepository.GetByIdAsync(request.UserId);
           if (user == null)
               throw new UserNotFoundException(request.UserId.ToString());

           return new UserInfoDto
           {
               Id = user.Id.ToString(),
               Email = user.Email,
               FullName = user.FullName,
               Role = user.Role,
               TenantId = user.TenantId.ToString()
           };
       }
   }
   ```

3. **WebAPI Layer** - Agregar endpoint
   ```csharp
   [HttpGet("{id}")]
   [Authorize]
   public async Task<IActionResult> GetUser(Guid id)
   {
       var query = new GetUserByIdQuery(id);
       var result = await _mediator.Send(query);
       return Ok(result);
   }
   ```

## Testing

### Estructura de Tests (futuro)

```
tests/
├── BA.Backend.Domain.Tests/
├── BA.Backend.Application.Tests/
│   └── CQRS/
│       ├── Commands/
│       │   └── LoginCommandHandlerTests.cs
│       └── Queries/
│           └── GetUserByIdQueryHandlerTests.cs
└── BA.Backend.Infrastructure.Tests/
```

### Ejemplo de Test

```csharp
public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserSessionRepository> _mockSessionRepository;
    private readonly Mock<IJwtTokenService> _mockJwtService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockSessionRepository = new Mock<IUserSessionRepository>();
        _mockJwtService = new Mock<IJwtTokenService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();

        _handler = new LoginCommandHandler(
            _mockUserRepository.Object,
            _mockSessionRepository.Object,
            _mockJwtService.Object,
            _mockPasswordHasher.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", IsActive = true };
        _mockUserRepository.Setup(r => r.GetByEmailAsync("test@test.com"))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var command = new LoginCommand("test@test.com", "password", "device123", null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
    }
}
```

## Performance

### Índices en BD

Los índices están optimizados para operaciones comunes:

```sql
-- Login frecuente
CREATE INDEX [IX_Users_Email] ON [Users]([Email]);

-- Sesión única
CREATE INDEX [IX_UserSession_UserId_DeviceId] ON [UserSessions]([UserId], [DeviceId]);

-- Queries por tenant
CREATE INDEX [IX_Users_TenantId_IsActive] ON [Users]([TenantId], [IsActive]);
```

### Mejoras Futuras

1. **Caching**: Redis para sesiones activas
2. **Batch Operations**: Invalidar múltiples sesiones eficientemente
3. **Event Sourcing**: Auditoría de logins
4. **Read Replicas**: Separate read/write databases

## Debugging

### Log Levels

```csharp
// En Program.cs
"Logging": {
    "LogLevel": {
        "Default": "Information",
        "Microsoft.EntityFrameworkCore": "Debug", // Ver SQL queries
        "Microsoft.AspNetCore": "Warning"
    }
}
```

### Ver SQL Queries

```csharp
// En ApplicationDbContext
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.LogTo(Console.WriteLine);
}
```

## Deployment

### Ambiente de Producción

1. **Cambiar connection string** a servidor SQL real
2. **Generar JWT secret seguro**
3. **Usar HTTPS obligatoriamente**
4. **Configurar CORS restrictivo**
5. **Rate limiting en endpoints**
6. **Auditoría de logins**

## Troubleshooting

### Error: "No JWT SDKs were found"
```bash
# Instalar .NET 8 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### Error: "Connection string not configured"
Verificar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;"  // ← REQUERIDO
  }
}
```

### Error: "User not found" en login
1. Verificar usuario existe en BD
2. Verificar schema SQL está creado
3. Conectar a BD correcta

### Error: "Invalid token"
1. Verificar JWT_SECRET_KEY es consistente
2. Verificar token no expiró
3. Verificar sesión está activa en BD

## Recursos

- [OWASP - Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [Microsoft - ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)
