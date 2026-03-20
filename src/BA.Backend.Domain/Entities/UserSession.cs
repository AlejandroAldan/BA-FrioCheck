namespace BA.Backend.Domain.Entities
{
    public class UserSession
    {
        public Guid Id { get; set; } /*ID unico de la sesion (Guid = identificador global unico)*/
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; } /*ID del tenant al que pertenece el usuario(empresa o organizacion)*/

        public string DeviceId { get; set; } = null!; /*ID unico del dispositivo (movil, laptop, etc) para identificar la sesion desde ese dispositivo*/
        public string DeviceFingerprint { get; set; } = null!;

        public string AccessToken { get; set; } = null!; /*El token JWT (la cadena incriptada)*/
        public string? JwtToken { get; set; }
        public DateTime IssuedAt { get; set; } /*cuando creo el token (fecha/hora)*/
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }

        public bool IsActive { get; set; } = true; /*indica si la sesion esta activa o no (si se cerro la sesion, se invalido el token, etc)*/
        public string? InvalidationReason { get; set; }
        public string? ClosureReason { get; set; }
        public DateTime? InvalidatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        
        //foreign key para relacionar con la entidad User
        public User user { get; set; } = null!; /* referencia a la entidad User para poder acceder a los datos del usuario desde la sesion
                                            Entity Framework Core lo usara para cargar el User autmaticamente*/
    }
}