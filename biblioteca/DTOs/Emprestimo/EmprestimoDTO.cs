using biblioteca.Models;

namespace biblioteca.DTOS;

public class EmprestimoDTO
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int LivroId { get; set; }

    public string NomeUsuario { get; set; } = string.Empty;

    public string TituloLivro { get; set; } = string.Empty;

    public DateTime DataEmprestimo { get; set; }

    public DateTime DataDevolucaoPrevista { get; set; }

    public DateTime? DataDevolucao { get; set; }

    public StatusEmprestimo Status { get; set; }
}