using System.ComponentModel.DataAnnotations;

namespace biblioteca.DTOS;

public class LivroCreateDTO
{   
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(120, ErrorMessage = "O nome pode ter no máximo 120 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O autor é obrigatório.")]
    [MaxLength(120, ErrorMessage = "O autor pode ter no máximo 120 caracteres.")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ano de publicação é obrigatório.")]
    public int AnoPublicacao { get; set; }

    [Required(ErrorMessage = "A quantidade disponível é obrigatória.")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantidade inválida.")]
    public int QuantidadeDisponivel { get; set; }
}