namespace biblioteca.DTOS;

public class LivroDTO
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Autor { get; set; } = string.Empty;

    public int AnoPublicacao { get; set; }

    public int QuantidadeDisponivel { get; set; }
    public List<EmprestimoDTO> Emprestimos { get; set; }
    = [];
}