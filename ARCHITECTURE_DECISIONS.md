# Decisiones Arquitectónicas - ADR (Architecture Decision Record)

## ADR-001: Usar Clean Architecture

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Necesitamos una arquitectura escalable y mantenible para un SaaS multi-tenant con requisitos de seguridad altoscomo NFC tracking y autenticación segura.

### Decisión
Adoptar Clean Architecture dividida en 4 capas:
1. **Domain**: Entidades y lógica de negocio pura
2. **Application**: Casos de uso (CQRS)
3. **Infrastructure**: Implementación técnica
4. **WebAPI**: Presentación HTTP

### Consecuencias Positivas
- Independencia de tecnología
- Fácil testeo
- Separación clara de responsabilidades
- Escalabilidad futura

### Consecuencias Negativas
- Más capas significa más archivos
- Requiere disciplina en el conocimiento del equipo
- Overhead inicial mayoral crear nuevas features

---

## ADR-002: Implementar CQRS con MediatR

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Necesitamos separar operaciones de lectura (queries) de escritura (commands) para optimizar performance. Las lecturas requieren queries optimizadas en SQL, mientras que las escrituras necesitan transacciones ACID.

### Decisión
Usar patrón CQRS con MediatR como:
- **Commands**: Para escrituras (Login, Logout, CreateUser)
- **Queries**: Para lecturas optimizadas (GetUser, GetSessions)
- **Handlers**: Ejecutan la lógica del caso de uso
- **Behaviors**: Validación y logging cross-cutting

### Consecuencias Positivas
- Queries optimizadas por Dapper
- Comandos con validación centralizada
- Fácil agregar comportamientos (logging, auditoría)
- Escalabildad a event sourcing en el futuro

### Consecuencias Negativas
- Curva de aprendizaje para MediatR
- Más código boilerplate (Command, Handler, Validator)

---

## ADR-003: Entity Framework Core para escrituras, Dapper para lecturas

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Necesitamos balance entre:
- Facilidad de desarrollo (entidades)
- Performance en queries (SQL optimizado)
- Facilidad de migraciones

### Decisión
- **Escrituras (DML)**: Entity Framework Core para INSERT, UPDATE, DELETE
  - Automático en transacciones
  - Fácil cambiar modelos
  
- **Lecturas (SELECT)**: Dapper para queries complejas
  - SQL directo compilado
  - Mejor performance que LINQ to SQL

### Consecuencias Positivas
- Best of both worlds
- Migraciones fáciles
- Performance óptimo en lecturas

### Consecuencias Negativas
- Dos ORMs diferentes
- Requiere cuidado en sincronización de esquemas

---

## ADR-004: BCrypt para Hash de Contraseñas

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Necesitamos almacenar contraseñas de forma segura resistente a ataques de diccionario y force brute.

### Decisión
Usar BCrypt con work factor 12:
- Algoritmo: NIST recomendado
- Salt: Incluido automáticamente
- Costo computacional: Crece exponencialmente por work factor
- Imposible "crackear" en tiempo real

### Código
```csharp
var hash = BCrypt.HashPassword(password, 12);
var isValid = BCrypt.Verify(password, hash);
```

### Consecuencias Positivas
- Seguro contra ataques conocidos
- Industry standard
- Resistente a mejoras de hardware

### Consecuencias Negativas
- Más lento que SHA256 (pero eso es el punto)
- Work factor 12 → ~100ms por login

---

## ADR-005: Sesión Única por Dispositivo con Invalidación Automática

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Requisito de negocio: "Solo una sesión activa por dispositivo". Si el usuario inicia sesión en otro dispositivo, la sesión anterior debe invalidarse.

### Decisión
Implementar lógica en LoginCommand:
1. Buscar sesión activa anterior del usuario
2. Si es en diferente deviceId → invalidar
3. Crear nueva sesión
4. Registrar en BD para auditoría

### Tabla: UserSession
```sql
- UserId + DeviceId = Única sesión activa por dispositivo
- IsActive: Para invalidar sin borrar histórico
- ClosureReason: Para auditoría ("Logout", "New session", etc)
```

### Consecuencias Positivas
- Previene sesiones "fantasmas"
- Auditoría completa de actividad
- Usuario puede tener múltiples sesiones (en diferentes dispositivos)

### Consecuencias Negativas
- Query adicional al login
- Requiere manejo de timezone/datetime consistente (UTC)

---

## ADR-006: JWT de Corta Duración (15 min) + Refresh Token

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Necesitamos tokens de corta duración para minimizar impacto de token robado, pero sin molestar al usuario constantemente pidiendo login.

### Decisión
- **AccessToken**: JWT válido 15 minutos
- **RefreshToken**: JWT válido 7 días (para renovar AccessToken)
- **Validación**: Realizar lookup de sesión en BD para mantenerla "viva"

### Tokens (Claims)
```
AccessToken: NameIdentifier, Email, Role, SessionId, TenantId
RefreshToken: NameIdentifier, SessionId, isRefreshToken
```

### Consecuencias Positivas
- Seguridad: Si robado, solo válido 15 min
- UX: No molesta con login cada 15 min (refresh automático en cliente)
- Auditoría: SessionId en cada token para rastrear

### Consecuencias Negativas
- Lógica más compleja en cliente
- Requiere almacenamiento seguro en cliente (localStorage problema)

---

## ADR-007: Multi-Tenancy a Nivel de Datos

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Sistema SaaS multi-tenant. Necesitamos aislamiento de datos entre tenants sin bases de datos separadas (costo y complejidad).

### Decisión
- **Nivel de Datos**: Agregar TenantId a todas las tablas principais
- **Validación**: Verificar TenantId en cada query
- **Índices**: Compound indices con TenantId

### Indexación
```sql
CREATE INDEX IX_Users_TenantId_IsActive ON Users(TenantId, IsActive);
```

### Consecuencias Positivas
- Una BD = menor costo
- Fácil agregar tenants nuevos
- Datos centralizados para análisis

### Consecuencias Negativas
- Requiere disciplina: Olvidar TenantId filter = data leak
- Más complicado backups/restore por tenant

---

## ADR-008: Inyección de Dependencias Centralizada

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Múltiples capas (Application, Infrastructure) necesitan registrar servicios sin crear acoplamiento.

### Decisión
Usar `ServiceCollectionExtensions` en cada capa:
```csharp
// Application/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddMediatR(...);
    services.AddValidatorsFromAssembly(...);
    return services;
}

// Infrastructure/ServiceCollectionExtensions.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, ...)
{
    services.AddDbContext<ApplicationDbContext>(...);
    services.AddScoped<IUserRepository, UserRepository>();
    return services;
}

// Program.cs
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString, jwtSecret);
```

### Consecuencias Positivas
- Desacoplamiento de capas
- Fácil cambiar implementaciones
- Tests: Mock fácil en constructores

### Consecuencias Negativas
- Más archivos
- Requiere entender el patrón

---

## ADR-009: Validación con FluentValidation

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Necesitamos validación de datos de entrada de forma:
- Reutilizable
- Expresiva
- Testeable

### Decisión
FluentValidation para cada Command/Query:
```csharp
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Password).MinimumLength(6);
    }
}

// Automático en MediatR Pipeline Behavior
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### Consecuencias Positivas
- Validación consistente
- Fácil testear reglas
- Mensajes de error claros

### Consecuencias Negativas
- Overhead: Validación en cada command
- (Pero necesario por seguridad)

---

## ADR-010: SQL Server como Base de Datos

**Estado**: Aceptado  
**Fecha**: 2026-03-18

### Contexto
Cliente requiere SQL Server (enterprise standard en su stack).

### Decisión
Microsoft SQL Server (Express valida para desarrollo):
- Índices optimizados
- Stored procedures (futuro)
- T-SQL para auditoría

### Migraciones
Usar Entity Framework Core Migrations:
```bash
Add-Migration InitialCreate
Update-Database
```

### Consecuencias Positivas
- Cliente requiere SQL Server
- Buena integración con .NET
- Escalabildad a enterprise features

### Consecuencias Negativas
- Costo de licencias en producción
- Sintaxis T-SQL específica

---

## Decisiones Futuras a Evaluar

1. **ADR-011**: Implementar Redis para cache de sesiones
2. **ADR-012**: Event Sourcing para auditoría completa
3. **ADR-013**: API Gateway con rate limiting
4. **ADR-014**: Implementar 2FA (TOTP/SMS)
5. **ADR-015**: Separar bases de datos read/write (CQRS escalado)
