using System.ComponentModel.DataAnnotations;

namespace BA.Backend.Application.Users.DTOs;

public class CreateUserDto
{
    [Required(ErrorMessage = "Email es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "FullName es requerido")]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password es requerido")]
    [MinLength(8, ErrorMessage = "Password mínimo 8 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role es requerido")]
    [Range(0, 3)]
    public int Role { get; set; }
}
