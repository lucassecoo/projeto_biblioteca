using System.ComponentModel.DataAnnotations;
namespace biblioteca.DTOS;
public class LoginDto {
    
    [Required(ErrorMessage = "Email é obrigatório.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória.")]
    public string Senha { get; set; } = string.Empty;
}