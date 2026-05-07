using System.Security.Claims;
using biblioteca.Data;
using biblioteca.DTOS;
using biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BCrypt.Net.BCrypt;

namespace biblioteca.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context) {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        var usuariosDTO = usuarios.Select(u => new UsuarioDTO
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email
        }).ToList();

        return Ok(usuariosDTO);
    }

    [HttpGet("{id:int}", Name = "GetById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (usuarioIdToken != id.ToString())
        {
            return StatusCode(403, "Token inválido para este usuário.");
        }
        var usuario = await _context.Usuarios.FindAsync(id);

        if(usuario == null)
        {
            return NotFound();
        }

        return Ok(new UsuarioDTO
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email
        });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> CreateUsuarioAsync(UsuarioCreateDTO dto) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.Senha != dto.ConfirmarSenha)
            return BadRequest(new { message = "As senhas não conferem." });

        var existe = await _context.Usuarios
            .AnyAsync(u => u.Email == dto.Email);

        if (existe)
            return BadRequest(new { message = "Este email já está em uso." });

        string senhaHash = HashPassword(dto.Senha);

        var usuario = new Usuario {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = senhaHash 
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        
        return CreatedAtRoute("GetById",
            new { id = usuario.Id },
            new UsuarioDTO {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            }
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUsuarioAsync(int id, UsuarioUpdateDTO dto)
    {   
        var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (usuarioIdToken != id.ToString())
        {
            return StatusCode(403, "Token inválido para este usuário.");

        }
        
        var usuario = await _context.Usuarios.FindAsync(id);

        if(usuario == null)
        {
            return NotFound();
        }

        var existe = await _context.Usuarios
            .AnyAsync(u => u.Email == dto.Email);

        if (existe)
            return BadRequest(new { message = "Este email já está em uso." });

        usuario.Nome = dto.Nome;
        usuario.Email = dto.Email;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUsuarioAsync(int id)
    {
        var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (usuarioIdToken != id.ToString())
        {
            return StatusCode(403, "Token inválido para este usuário.");
        }
        var usuario = await _context.Usuarios.FindAsync(id);
        
        if(usuario == null)
        {
            return NotFound();
        }

       _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent(); 
    }
}