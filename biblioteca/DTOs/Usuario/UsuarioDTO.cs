namespace biblioteca.DTOS;

public class UsuarioDTO
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<EmprestimoDTO> Emprestimos { get; set; } = new();
}