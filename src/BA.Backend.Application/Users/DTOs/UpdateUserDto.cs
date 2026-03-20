using System.ComponentModel.DataAnnotations;

namespace BA.Backend.Application.Users.DTOs;

public class UpdateUserDto
{
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Range(0, 3)]
    public int Role { get; set; }

    public bool IsActive { get; set; } = true;
}
