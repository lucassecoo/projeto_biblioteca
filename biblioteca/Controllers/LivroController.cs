using biblioteca.Data;
using biblioteca.DTOS;
using biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace biblioteca.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LivroController : ControllerBase
{
    private readonly AppDbContext _context;

    public LivroController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/Livro
    // cadastrar
    [HttpPost]
    public async Task<IActionResult> CadastrarLivroAsync(LivroCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var livro = new Livro
        {
            Titulo = dto.Titulo,
            Autor = dto.Autor,
            AnoPublicacao = dto.AnoPublicacao,
            QuantidadeDisponivel = dto.QuantidadeDisponivel
        };

        _context.Livros.Add(livro);
        await _context.SaveChangesAsync();

        return CreatedAtRoute(
            "GetLivroById",
            new { id = livro.Id },
            MapLivroToDTO(livro)
        );
    }

    // GET: api/Livro
    // listar
    [HttpGet]
    public async Task<IActionResult> ListarLivrosAsync()
    {
        var livros = await _context.Livros
            .Include(l => l.Emprestimos)
                .ThenInclude(e => e.Usuario)
            .AsNoTracking()
            .ToListAsync();

        var livrosDTO = livros.Select(MapLivroToDTO).ToList();
        return Ok(livrosDTO);
    }

    // GET: api/Livro/{id}
    // por ID
    [HttpGet("{id:int}", Name = "GetLivroById")]
    public async Task<IActionResult> ObterLivroPorIdAsync(int id)
    {
        var livro = await _context.Livros
            .Include(l => l.Emprestimos)
                .ThenInclude(e => e.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (livro == null)
            return NotFound(new { message = "Livro não encontrado." });

        return Ok(MapLivroToDTO(livro));
    }

    // PUT: api/Livro/{id}
    // atualizar
    [HttpPut("{id:int}")]
    public async Task<IActionResult> AtualizarLivroAsync(int id, LivroUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var livro = await _context.Livros.FindAsync(id);

        if (livro == null)
            return NotFound(new { message = "Livro não encontrado." });

        livro.Titulo = dto.Titulo;
        livro.Autor = dto.Autor;
        livro.AnoPublicacao = dto.AnoPublicacao;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Livro/{id}
    // remover
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoverLivroAsync(int id)
    {
        var livro = await _context.Livros.FindAsync(id);

        if (livro == null)
            return NotFound(new { message = "Livro não encontrado." });

        _context.Livros.Remove(livro);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Como o relacionamento está com DeleteBehavior.Restrict no AppDbContext,
            // se houver empréstimos vinculados, a remoção pode falhar.
            return Conflict(new
            {
                message = "Não foi possível remover o livro, pois existem empréstimos vinculados a ele."
            });
        }

        return NoContent();
    }

    // GET: api/Livro/disponiveis (opcional)
    [HttpGet("disponiveis")]
    public async Task<IActionResult> BuscarLivrosDisponiveisAsync()
    {
        var livros = await _context.Livros
            .Where(l => l.QuantidadeDisponivel > 0)
            .AsNoTracking()
            .ToListAsync();

        var livrosDTO = livros.Select(MapLivroToDTO).ToList();
        return Ok(livrosDTO);
    }

    private static LivroDTO MapLivroToDTO(Livro livro)
    {
        return new LivroDTO
        {
            Id = livro.Id,
            Titulo = livro.Titulo,
            Autor = livro.Autor,
            AnoPublicacao = livro.AnoPublicacao,
            QuantidadeDisponivel = livro.QuantidadeDisponivel,
            Emprestimos = livro.Emprestimos.Select(e => new EmprestimoDTO
            {
                Id = e.Id,
                NomeUsuario = e.Usuario?.Nome ?? string.Empty,
                TituloLivro = livro.Titulo,
                DataEmprestimo = e.DataEmprestimo,
                DataDevolucao = e.DataDevolucao,
                Status = e.Status
            }).ToList()
        };
    }
}