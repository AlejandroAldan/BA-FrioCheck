using BA.Backend.Domain.Enums;

namespace BA.Backend.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public string? ActiveSessionId { get; set; }
    public string? CurrentDeviceFingerprint { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool CanLogin()
    {
        return IsActive && !IsLocked;
    }

    public void RegisterSession(string sessionId, string deviceFingerprint)
    {
        ActiveSessionId = sessionId;
        CurrentDeviceFingerprint = deviceFingerprint;
        LastLoginAt = DateTime.UtcNow;
    }
}

