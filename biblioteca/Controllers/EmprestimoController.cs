using biblioteca.Models;
using biblioteca.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using biblioteca.Data;
using System.Security.Claims;

namespace biblioteca.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmprestimoController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmprestimoController(AppDbContext context)
    {
        _context = context;
    }


    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] EmprestimoCreateDTO dto)
    {   
        var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && usuarioIdToken != dto.UsuarioId.ToString())
        {
            return StatusCode(403, "Você não tem permissão para realizar este empréstimo.");
        }

        var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
        if (usuario == null)
            return NotFound(new { mensagem = "Usuário não encontrado." });

        var livro = await _context.Livros.FindAsync(dto.LivroId);
        if (livro == null)
            return NotFound(new { mensagem = "Livro não encontrado." });

        if (livro.QuantidadeDisponivel <= 0)
            return BadRequest(new { mensagem = "Livro sem estoque disponível." });

        livro.QuantidadeDisponivel--;

        var emprestimo = new Emprestimo
        {
            UsuarioId    = dto.UsuarioId,
            LivroId      = dto.LivroId,
            DataEmprestimo = DateTime.UtcNow,
            DataDevolucaoPrevista  = DateTime.UtcNow.AddDays(14), // prazo automático de 14 dias
            Status         = StatusEmprestimo.Emprestado
        };

        _context.Emprestimos.Add(emprestimo);
        await _context.SaveChangesAsync();

       var emprestimoDTO = new EmprestimoDTO
{
        Id = emprestimo.Id,

        UsuarioId = emprestimo.UsuarioId,
        LivroId = emprestimo.LivroId,

        NomeUsuario = usuario.Nome,
        TituloLivro = livro.Titulo,

        DataEmprestimo = emprestimo.DataEmprestimo,
        DataDevolucaoPrevista = emprestimo.DataDevolucaoPrevista,
        DataDevolucao = emprestimo.DataDevolucao,
        Status = emprestimo.Status
};

        return CreatedAtAction(nameof(GetById), new { id = emprestimo.Id }, emprestimoDTO);
    }

    [HttpGet]
    public async Task<IActionResult> GetByUsuario([FromQuery] int? usuarioId)
    {   
        var usuarioIdToken = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var isAdmin = User.IsInRole("Admin");

        IQueryable<Emprestimo> query = _context.Emprestimos
            .AsNoTracking()
            .Include(e => e.Usuario)
            .Include(e => e.Livro);

        if (isAdmin)
        {
            if (usuarioId.HasValue)
            {
                query = query.Where(e => e.UsuarioId == usuarioId.Value);
            }
        }
        else
        {
            query = query.Where(e => e.UsuarioId == usuarioIdToken);
        }

        var emprestimos = await query
            .Select(e => new EmprestimoDTO
            {
                Id = e.Id,
                UsuarioId = e.UsuarioId,
                LivroId = e.LivroId,
                NomeUsuario = e.Usuario.Nome,
                TituloLivro = e.Livro.Titulo,
                DataEmprestimo = e.DataEmprestimo,
                DataDevolucaoPrevista = e.DataDevolucaoPrevista,
                DataDevolucao = e.DataDevolucao,
                Status = e.Status
            })
            .ToListAsync();

        return Ok(emprestimos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        var emprestimo = await _context.Emprestimos
            .AsNoTracking()
            .Include(e => e.Usuario)
            .Include(e => e.Livro)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (emprestimo == null)
            return NotFound(new
            {
                mensagem = $"Empréstimo com ID {id} não encontrado."
            });

        if (!isAdmin && usuarioIdToken != emprestimo.UsuarioId.ToString())
        {
            return StatusCode(403,
                "Você não tem permissão para acessar este empréstimo.");
        }

        var emprestimoDTO = new EmprestimoDTO
        {
            Id = emprestimo.Id,
            UsuarioId = emprestimo.UsuarioId,
            LivroId = emprestimo.LivroId,
            NomeUsuario = emprestimo.Usuario.Nome,
            TituloLivro = emprestimo.Livro.Titulo,
            DataEmprestimo = emprestimo.DataEmprestimo,
            DataDevolucaoPrevista = emprestimo.DataDevolucaoPrevista,
            DataDevolucao = emprestimo.DataDevolucao,
            Status = emprestimo.Status
        };

        return Ok(emprestimoDTO);
    }

    [HttpPatch("usuarios/{usuarioId}/emprestimos/{emprestimoId}/devolver")]
    public async Task<IActionResult> Devolver(int usuarioId, int emprestimoId)
    {
        var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && usuarioIdToken != usuarioId.ToString())
        {
            return StatusCode(403, "Você não tem permissão para essa devolução.");
        }
        var emprestimo = await _context.Emprestimos
            .Include(e => e.Livro)
            .Include(e => e.Usuario)
            .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId &&
            e.Id == emprestimoId);

        if (emprestimo == null)
            return NotFound(new { mensagem = $"Empréstimo com ID {emprestimoId} não encontrado." });

        if (emprestimo.Status == StatusEmprestimo.Devolvido)
            return BadRequest(new { mensagem = "Este empréstimo já foi devolvido." });

        emprestimo.Livro.QuantidadeDisponivel++;
        emprestimo.Status = StatusEmprestimo.Devolvido;
        emprestimo.DataDevolucao = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var emprestimoDTO = new EmprestimoDTO
        {
            Id = emprestimo.Id,
            UsuarioId = emprestimo.UsuarioId,
            LivroId = emprestimo.LivroId,
            NomeUsuario = emprestimo.Usuario.Nome,
            TituloLivro = emprestimo.Livro.Titulo,
            DataEmprestimo = emprestimo.DataEmprestimo,
            DataDevolucaoPrevista = emprestimo.DataDevolucaoPrevista,
            DataDevolucao = emprestimo.DataDevolucao,
            Status = emprestimo.Status
        };

        return Ok(new { mensagem = "Devolução registrada com sucesso.", emprestimo = emprestimoDTO });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletarEmprestimo(int id)
    {
        var emprestimo = await _context.Emprestimos.FindAsync(id);

        if (emprestimo == null)
            return NotFound(new { mensagem = $"Empréstimo com ID {id} não encontrado." });

        _context.Emprestimos.Remove(emprestimo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

