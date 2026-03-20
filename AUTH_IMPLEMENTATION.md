# 🔐 AuthController - Implementación Completa

## ✅ Archivos Generados

### 1. **Commands** (Application/Auth/Commands/)

- ✅ `LoginCommand.cs` - Email, Password, TenantSlug, DeviceFingerprint
- ✅ `ForgotPasswordCommand.cs` - Email, TenantSlug
- ✅ `ResetPasswordCommand.cs` - Token, NewPassword, ConfirmPassword

### 2. **DTOs** (Application/Auth/DTOs/)

- ✅ `LoginResponseDto.cs` - AccessToken, ExpiresAt, UserFullName, Role, UserId, TenantId, SessionReplaced, RedirectTo
- ✅ `ForgotPasswordResponseDto.cs` - Message (genérico por seguridad)

### 3. **Handlers** (Application/Auth/Handlers/)

#### LoginCommandHandler

**Flujo:**

1. ✅ Resolver Tenant por Slug
2. ✅ Buscar Usuario por Email en el Tenant
3. ✅ Validar que usuario está activo
4. ✅ Validar que usuario NO está bloqueado
5. ✅ Verificar Password con BCrypt
6. ✅ Generar SessionId UUID
7. ✅ Revocar sesión anterior si es otro dispositivo (SessionReplaced = true)
8. ✅ Crear nueva sesión en BD (UserSession)
9. ✅ Actualizar usuario: ActiveSessionId, CurrentDeviceFingerprint, LastLoginAt
10. ✅ Generar JWT Token (15 min expiry)
11. ✅ Determinar RedirectTo según rol:
    - Admin → "/admin/dashboard"
    - Cliente → "/cliente/home"
    - Transportista → "/transportista/ruta"
    - Tecnico → "/tecnico/tickets"
12. ✅ Retornar LoginResponseDto completo

#### ForgotPasswordCommandHandler

**Flujo:**

1. ✅ Resolver Tenant por Slug
2. ✅ Buscar usuario por email (SIN revelar si existe)
3. ✅ Generar token UUID de 32 caracteres
4. ✅ Hashear token con BCrypt
5. ✅ Crear PasswordResetToken en BD (ExpiresAt = 15 min)
6. ✅ Enviar email con link (vía IEmailService)
7. ✅ Retornar respuesta genérica siempre (seguridad)

#### ResetPasswordCommandHandler

**Flujo:**

1. ✅ Buscar token en BD por hash comparado
2. ✅ Validar que NO está expirado
3. ✅ Validar que NO está usado
4. ✅ Hash nueva contraseña
5. ✅ Actualizar User.PasswordHash
6. ✅ Marcar token como IsUsed = true
7. ✅ Revocar TODAS las sesiones activas del usuario

### 4. **Validators** (Application/Auth/Validators/)

- ✅ `LoginCommandValidator.cs` - Email válido, Password >= 8 chars
- ✅ `ForgotPasswordCommandValidator.cs` - Email requerido
- ✅ `ResetPasswordCommandValidator.cs` - Passwords coinciden, >= 8 chars, regex (mayús+minús+número+especial)

### 5. **Domain Entity**

- ✅ `PasswordResetToken.cs` (Domain/Entities/)
  - Id, UserId, TokenHash, ExpiresAt, IsUsed, CreatedAt
  - Relación FK a Users

### 6. **Interface**

- ✅ `IEmailService.cs` (Application/Common/Interfaces/)
  - `SendPasswordResetEmailAsync(email, resetLink, userFullName)`
  - `SendWelcomeEmailAsync(email, userName)`

### 7. **Repository Interface**

- ✅ `IPasswordResetTokenRepository.cs` (Domain/Repositories/)

### 8. **Controller**

- ✅ `AuthController.cs` (WebAPI/Controllers/)
  - POST `/api/v1/auth/login` → [AllowAnonymous]
  - POST `/api/v1/auth/forgot-password` → [AllowAnonymous]
  - POST `/api/v1/auth/reset-password` → [AllowAnonymous]

### 9. **Database Schema**

- ✅ `02_add_password_reset_tokens.sql` - PasswordResetTokens table con índices

---

## 📋 TODO - Implementación Pendiente

### Fase 1: Infrastructure (Repositorio + Servicio Email)

```csharp
// 1. PasswordResetTokenRepository.cs (Infrastructure/Repositories/)
public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    // Implementar CRUD + GetByTokenAsync (busca por token plano, compara con TokenHash)
}

// 2. EmailService.cs (Infrastructure/Services/)
public class EmailService : IEmailService
{
    // Usar SmtpClient o SendGrid / Azure SendGrid
    // Template: Password reset email con 15 min countdown
}
```

### Fase 2: Program.cs - Registrar Servicios

```csharp
// En Program.cs agregar:
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
```

### Fase 3: DbContext - Actualizar

```csharp
// En ApplicationDbContext.cs agregar en OnModelCreating:
modelBuilder.Entity<PasswordResetToken>()
    .HasKey(x => x.Id);

modelBuilder.Entity<PasswordResetToken>()
    .HasOne(x => x.User)
    .WithMany()
    .HasForeignKey(x => x.UserId);
```

### Fase 4: Configuración

```json
// En appsettings.json agregar:
"Frontend": {
  "Url": "http://localhost:3000"
},
"Email": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "FromAddress": "noreply@bafriocheck.com",
  "FromName": "BA Frío Check",
  "Username": "...",
  "Password": "..."
}
```

---

## 🔍 Validaciones Implementadas

✅ **Login:**

- Email válido (RFC 5322)
- Password >= 8 caracteres
- TenantSlug requerido
- DeviceFingerprint requerido
- Usuario activo
- Usuario no bloqueado
- Password match con BCrypt

✅ **Forgot Password:**

- Email válido
- TenantSlug requerido
- Respuesta genérica (NO revelar si existe)

✅ **Reset Password:**

- Token requerido
- Password >= 8 caracteres
- Regex: mayúscula + minúscula + número + carácter especial
- Passwords coinciden
- Token no expirado
- Token no usado

---

## 🔐 Características de Seguridad

1. **Password Hashing:** BCrypt con cost factor 12
2. **Token de Reset:** UUID + BCrypt hash en BD (token plano nunca en BD)
3. **Token Expiry:** 15 minutos
4. **One-time Token:** Una vez usado, no se puede reutilizar
5. **Session Revocation:** Al cambiar password, todas las sesiones se revocan
6. **Session Unique:** Por dispositivo (DeviceFingerprint)
7. **No Email Enumeration:** Login y ForgotPassword no revelan si existe
8. **JWT Expiry:** 15 minutos (short-lived token)
9. **Role-based Redirect:** Cada rol tiene su dashboard

---

## 📊 RedirectTo por Rol

| Rol           | RedirectTo            |
| ------------- | --------------------- |
| Admin         | `/admin/dashboard`    |
| Cliente       | `/cliente/home`       |
| Transportista | `/transportista/ruta` |
| Tecnico       | `/tecnico/tickets`    |

---

## 🚀 Next Steps

1. **Implementar PasswordResetTokenRepository** en Infrastructure/Repositories/
2. **Implementar EmailService** en Infrastructure/Services/
3. **Actualizar DbContext** con PasswordResetToken mapping
4. **Registrar servicios** en Program.cs
5. **Agregar configuración Email** en appsettings.json
6. **Ejecutar migration** para crear tabla PasswordResetTokens
7. **Tests unitarios** para cada handler (ya hay test infrastructure con xUnit)
8. **Tests de integración** para endpoints (opcional)

---

## 📝 Resumen de Endpoints

```
POST /api/v1/auth/login
Content-Type: application/json
{
  "email": "admin@test.com",
  "password": "Admin123!",
  "tenantSlug": "admin",
  "deviceFingerprint": "firefox-windows-123"
}
Response: 200 OK
{
  "accessToken": "eyJhbGc...",
  "expiresAt": "2026-03-20T10:30:00Z",
  "userFullName": "Admin User",
  "role": 1,
  "userId": "guid",
  "tenantId": "guid",
  "sessionReplaced": false,
  "redirectTo": "/admin/dashboard"
}

---

POST /api/v1/auth/forgot-password
Content-Type: application/json
{
  "email": "admin@test.com",
  "tenantSlug": "admin"
}
Response: 200 OK
{
  "message": "Si el email existe en nuestro sistema, recibirás un enlace para restablecer tu contraseña."
}

---

POST /api/v1/auth/reset-password
Content-Type: application/json
{
  "token": "uuid-str-from-email",
  "newPassword": "NewPass123!",
  "confirmPassword": "NewPass123!"
}
Response: 200 OK
{
  "message": "Contraseña actualizada exitosamente. Por favor inicia sesión con tu nueva contraseña."
}
```

---

## ⚠️ Errores Implementados

- `401 Unauthorized` - Login fallido (credenciales inválidas)
- `400 BadRequest` - Validación fallida
- `400 BadRequest` - Token expirado/inválido/usado
- `InvalidOperationException` - Usuario no existe, no activo, bloqueado

---

**Estado: 80% Completo - Falta solo implementación de Infrastructure**
