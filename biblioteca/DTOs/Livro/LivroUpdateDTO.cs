using System.ComponentModel.DataAnnotations;
namespace biblioteca.DTOS;

public class LivroUpdateDTO
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O título deve ter entre 2 e 120 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O autor é obrigatório.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O autor deve ter entre 2 e 120 caracteres.")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ano de publicação é obrigatório.")]
    [Range(1, 2100, ErrorMessage = "Ano de publicação inválido.")]
    public int AnoPublicacao { get; set; }

    [Required(ErrorMessage = "A quantidade disponível é obrigatória.")]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser um número não negativo.")]
    public int QuantidadeDisponivel { get; set; }
}