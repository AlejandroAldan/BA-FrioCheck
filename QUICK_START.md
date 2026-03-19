# GUÍA DE INICIO RÁPIDO

## ¿Qué se ha creado?

Una estructura completa de Clean Architecture con CQRS para un sistema de autenticación y gestión de sesiones NFC.

```
BA-Backend/
├── src/
│   ├── BA.Backend.Domain/              (Entidades y contratos)
│   ├── BA.Backend.Application/         (Casos de uso - CQRS)
│   ├── BA.Backend.Infrastructure/      (Implementación técnica)
│   └── BA.Backend.WebAPI/              (Controladores HTTP)
├── sql/                                 (Scripts de BD)
├── BA.Backend.sln                      (Solución principal)
├── Dockerfile                          (Para containerizar)
├── docker-compose.yml                  (Para desarrollo local)
└── README.md                           (Documentación completa)
```

## Pasos para Comenzar

### 1️⃣ Restaurar dependencias
```bash
cd "c:\Users\aleja\OneDrive\Escritorio\BA-Backend"
dotnet restore
```

### 2️⃣ Crear base de datos

```bash
# Crear la BD y ejecutar script SQL
sqlcmd -S localhost -U sa -P YourPassword123! -i sql/01_initial_schema.sql
```

O usar SQL Server Management Studio para crear `BA_Backend_DB` y ejecutar el script manualmente.

### 3️⃣ Configurar JWT Secret
Editar `src/BA.Backend.WebAPI/appsettings.json` y reemplazar:
```json
{
  "Jwt": {
    "SecretKey": "GENERAR-NUEVA-CLAVE-DE-32-CARACTERES"
  }
}
```

### 4️⃣ Compilar solución
```bash
dotnet build
```

### 5️⃣ Ejecutar WebAPI
```bash
cd src/BA.Backend.WebAPI
dotnet run
```

Acceder a: `https://localhost:5001/swagger`

### ✅ Verificar que funciona
Ejecutar test de login (ver API_EXAMPLES.md):
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@baplatform.com",
    "password": "Admin123!",
    "deviceId": "device-12345"
  }' \
  --insecure
```

## Archivos Importantes

| Archivo | Descripción |
|---------|-------------|
| [README.md](README.md) | Documentación técnica completa |
| [DEVELOPMENT.md](DEVELOPMENT.md) | Guía para desarrolladores |
| [ARCHITECTURE_DECISIONS.md](ARCHITECTURE_DECISIONS.md) | Decisiones arquitectónicas (ADR) |
| [API_EXAMPLES.md](API_EXAMPLES.md) | Ejemplos de requests/responses |
| [.env.example](.env.example) | Variables de ambiente |

## Estructura de Carpetas

### Domain Layer
```
src/BA.Backend.Domain/
├── Entities/
│   ├── User.cs              # Usuario del sistema
│   ├── UserRole.cs          # Enum de roles (Admin, Cliente, etc)
│   └── UserSession.cs       # Sesiones activas (sesión única)
└── Interfaces/
    ├── IUserRepository.cs
    ├── IUserSessionRepository.cs
    └── IJwtTokenService.cs
```

**Responsabilidad**: Define QUÉ hace el sistema (entidades de negocio y contratos)

### Application Layer (CQRS)
```
src/BA.Backend.Application/
├── CQRS/
│   ├── Commands/            # Operaciones de escritura
│   │   └── LoginCommand.cs
│   ├── Handlers/            # Lógica de casos de uso
│   │   └── LoginCommandHandler.cs
│   └── Validators/          # Validaciones de entrada
│       └── LoginCommandValidator.cs
├── DTOs/                    # Modelos de comunicación
│   ├── LoginRequestDto.cs
│   └── LoginResponseDto.cs
├── Interfaces/              # Contratos para Infrastructure
│   └── IPasswordHasher.cs
└── Exceptions/              # Excepciones personalizadas
```

**Responsabilidad**: Define CÓMO se hacen las cosas (lógica de casos de uso)

### Infrastructure Layer
```
src/BA.Backend.Infrastructure/
├── Data/
│   └── ApplicationDbContext.cs  # Entity Framework Core
├── Repositories/
│   ├── UserRepository.cs
│   └── UserSessionRepository.cs
└── Services/
    ├── JwtTokenService.cs       # Generación/validación JWT
    └── PasswordHasher.cs        # Hashing BCrypt
```

**Responsabilidad**: Implementa los servicios técnicos (BD, JWT, Hash)

### WebAPI Layer
```
src/BA.Backend.WebAPI/
├── Controllers/
│   └── AuthController.cs        # Endpoint POST /api/auth/login
├── Program.cs                   # Configuración de DI y middleware
├── appsettings.json             # Configuración
└── appsettings.Development.json
```

**Responsabilidad**: Expone casos de uso como endpoints HTTP

## Próximos Pasos

1. **Testear Login**: Ver ejemplos en [API_EXAMPLES.md](API_EXAMPLES.md)
2. **Crear más commands**: Logout, ChangePassword, etc
3. **Implementar queries**: GetUser, GetSessions, etc
4. **Agregar auditoría**: Registrar intentos de login
5. **Implementar 2FA**: Autenticación de dos factores
6. **Agregar roles**: Validar permisos por endpoint

## Troubleshooting

### ❌ "dotnet: El comando no se reconoce"
Instalar .NET 8 SDK desde: https://dotnet.microsoft.com/download/dotnet/8.0

### ❌ "Connection string not configured"
Verificar `appsettings.json` tiene `ConnectionStrings.DefaultConnection`

### ❌ "Unable to connect to SQL Server"
Verificar SQL Server está corriendo y accesible en puerto 1433

### ❌ "JWT token is invalid"
- Verificar `JWT_SECRET_KEY` es consistente entre ejecuciones
- Verificar token no expiró (válido 15 minutos)
- Verificar sesión existe en BD

## Soporte

Ver guía completa en [README.md](README.md)

---

**Última actualización**: 2026-03-18
