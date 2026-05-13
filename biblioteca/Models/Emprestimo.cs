namespace biblioteca.Models;

public class Emprestimo
{
    public int Id { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int UsuarioId { get; set; }

    public Livro Livro { get; set; } = null!;

    public int LivroId { get; set; }

    public DateTime DataEmprestimo { get; set; }

    public DateTime DataDevolucaoPrevista { get; set; }

    public DateTime? DataDevolucao { get; set; }

    public StatusEmprestimo Status { get; set; }
}