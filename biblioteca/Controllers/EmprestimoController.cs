using biblioteca.Models;
using biblioteca.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using biblioteca.Data;

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
        // 1. Valida se o usuário existe → 404 se não achar
        var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
        if (usuario == null)
            return NotFound(new { mensagem = "Usuário não encontrado." });

        // 2. Valida se o livro existe → 404 se não achar
        var livro = await _context.Livros.FindAsync(dto.LivroId);
        if (livro == null)
            return NotFound(new { mensagem = "Livro não encontrado." });

        // 3. Verifica estoque disponível → 400 se não tiver
        if (livro.QuantidadeDisponivel <= 0)
            return BadRequest(new { mensagem = "Livro sem estoque disponível." });

        // 4. Diminui a quantidade (emprestando)
        livro.QuantidadeDisponivel--;

        // 5. Cria o empréstimo com datas automáticas e status inicial
        var emprestimo = new Emprestimo
        {
            UsuarioId    = dto.UsuarioId,
            LivroId      = dto.LivroId,
            DataEmprestimo = DateTime.Now,
            DataDevolucao  = DateTime.Now.AddDays(14), // prazo automático de 14 dias
            Status         = StatusEmprestimo.Emprestado
        };

        _context.Emprestimos.Add(emprestimo);
        await _context.SaveChangesAsync();

        // 201 Created com a localização do novo recurso no header
        return CreatedAtAction(nameof(GetById), new { id = emprestimo.Id }, emprestimo);
    }

    
    // GET /api/emprestimo  →  Listar todos
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Include traz os dados relacionados (usuario e livro junto)
        var emprestimos = await _context.Emprestimos
            .Include(e => e.Usuario)
            .Include(e => e.Livro)
            .ToListAsync();

        return Ok(emprestimos); // 200 com a lista
    }

    
    // GET /api/emprestimo/{id}  →  Buscar por ID

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var emprestimo = await _context.Emprestimos
            .Include(e => e.Usuario)
            .Include(e => e.Livro)
            .FirstOrDefaultAsync(e => e.Id == id);

        // 404 se não existir
        if (emprestimo == null)
            return NotFound(new { mensagem = $"Empréstimo com ID {id} não encontrado." });

        return Ok(emprestimo); // 200 com os dados
    }


    // PATCH /api/emprestimo/{id}/devolver  →  Registrar devolução

    [HttpPatch("{id}/devolver")]
    public async Task<IActionResult> Devolver(int id)
    {
        var emprestimo = await _context.Emprestimos
            .Include(e => e.Livro)
            .FirstOrDefaultAsync(e => e.Id == id);

        // 404 se não existir
        if (emprestimo == null)
            return NotFound(new { mensagem = $"Empréstimo com ID {id} não encontrado." });

        // 400 se já foi devolvido (evita devolução dupla)
        if (emprestimo.Status == StatusEmprestimo.Devolvido)
            return BadRequest(new { mensagem = "Este empréstimo já foi devolvido." });

        // Aumenta o estoque de volta e atualiza o status
        emprestimo.Livro.QuantidadeDisponivel++;
        emprestimo.Status        = StatusEmprestimo.Devolvido;
        emprestimo.DataDevolucao = DateTime.Now; // registra a data real da devolução

        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Devolução registrada com sucesso.", emprestimo });
    }
}

