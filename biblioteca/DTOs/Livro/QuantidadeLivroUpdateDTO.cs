using System.ComponentModel.DataAnnotations;

namespace biblioteca.DTOS;

public class QuantidadeLivroUpdateDTO
{
    [Range(0, int.MaxValue,
        ErrorMessage = "Quantidade inválida.")]
    public int QuantidadeDisponivel { get; set; }
}