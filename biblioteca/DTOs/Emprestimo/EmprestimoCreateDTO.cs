using System.ComponentModel.DataAnnotations;

namespace biblioteca.DTOS;

public class EmprestimoCreateDTO
{
    [Required(ErrorMessage = "O usuarioId é obrigatório.")]
    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "O livroId é obrigatório.")]
    [Range(1, int.MaxValue)]
    public int LivroId { get; set; }
}