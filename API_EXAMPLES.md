# Ejemplos de Uso de API

## Login Exitoso

### Request
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@baplatform.com",
    "password": "Admin123!",
    "deviceId": "device-uuid-12345",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
    "ipAddress": "192.168.1.100"
  }' \
  --insecure
```

### Response
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI1ZTBl...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI1ZTBl...",
  "expiresIn": 900,
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "tokenType": "Bearer",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "email": "admin@baplatform.com",
    "fullName": "Administrador",
    "role": 1,
    "tenantId": "550e8400-e29b-41d4-a716-446655440002"
  }
}
```

## Error - Credenciales Inválidas

### Request
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@baplatform.com",
    "password": "WrongPassword!",
    "deviceId": "device-uuid-12345"
  }' \
  --insecure
```

### Response (401 Unauthorized)
```json
{
  "message": "Email o contraseña incorrectos."
}
```

## Error - Validación Fallida

### Request
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "invalid-email",
    "password": "123",
    "deviceId": ""
  }' \
  --insecure
```

### Response (400 Bad Request)
```json
{
  "errors": [
    "El formato del email no es válido.",
    "La contraseña debe tener al menos 6 caracteres.",
    "El identificador del dispositivo es requerido."
  ]
}
```

## Health Check

### Request
```bash
curl -X GET "https://localhost:5001/api/auth/health" \
  --insecure
```

### Response
```json
{
  "status": "healthy",
  "timestamp": "2026-03-18T15:30:45.123456Z"
}
```

## Usar Token en Requests Posteriores

### Header de Autorización
```bash
curl -X GET "https://localhost:5001/api/users/profile" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  --insecure
```

El cliente debe extraer el `accessToken` de la respuesta de login e incluirlo en el header `Authorization` como `Bearer <token>`.

## Script de PowerShell para Testing

```powershell
# Variables
$baseUrl = "https://localhost:5001"
$email = "admin@baplatform.com"
$password = "Admin123!"
$deviceId = [guid]::NewGuid().ToString()

# Login
$loginBody = @{
    email = $email
    password = $password
    deviceId = $deviceId
    userAgent = "PowerShell/7.0"
    ipAddress = "127.0.0.1"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $loginBody `
    -SkipCertificateCheck

$loginData = $response.Content | ConvertFrom-Json
$accessToken = $loginData.accessToken
$sessionId = $loginData.sessionId

Write-Host "Login exitoso!"
Write-Host "SessionId: $sessionId"
Write-Host "AccessToken: $($accessToken.Substring(0, 50))..."

# Usar token en requests posteriores
$headers = @{
    "Authorization" = "Bearer $accessToken"
}

# Ejemplo: Get User Profile (cuando esté implementado)
# $profileResponse = Invoke-WebRequest -Uri "$baseUrl/api/users/profile" `
#     -Headers $headers `
#     -SkipCertificateCheck
```

## Usuarios de Prueba

Después de crear el usuario admin vía script SQL, pueden testearse con:

```json
{
  "email": "admin@baplatform.com",
  "password": "Admin123!"  // o la contraseña que hayan generado
}
```

**Generar hash BCrypt en PowerShell**:

```powershell
# Instalar módulo (execute once)
Install-Module -Name BCrypt

# Generar hash
$hash = [BCrypt.Net.BCrypt]::HashPassword("MyPassword123!", 12)
Write-Host $hash

# Verificar
[BCrypt.Net.BCrypt]::Verify("MyPassword123!", $hash)
# Retorna: True
```

## Postman Collection

Pueden importar en Postman:

```json
{
  "info": {
    "name": "BA Backend API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Login",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\"email\": \"admin@baplatform.com\", \"password\": \"Admin123!\", \"deviceId\": \"{{$guid}}\"}"
        },
        "url": {
          "raw": "{{base_url}}/api/auth/login",
          "host": ["{{base_url}}"],
          "path": ["api", "auth", "login"]
        }
      }
    },
    {
      "name": "Health",
      "request": {
        "method": "GET",
        "url": {
          "raw": "{{base_url}}/api/auth/health",
          "host": ["{{base_url}}"],
          "path": ["api", "auth", "health"]
        }
      }
    }
  ],
  "variable": [
    {
      "key": "base_url",
      "value": "https://localhost:5001",
      "type": "string"
    }
  ]
}
```

Importar en Postman y setear variable `base_url` a `https://localhost:5001`.
