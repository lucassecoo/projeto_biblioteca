using biblioteca.Data;
using biblioteca.DTOS;
using biblioteca.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BCrypt.Net.BCrypt;

namespace biblioteca.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService) {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto) {

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (usuario == null)
        {
            return NotFound("Usuário não encontrado");
        }
        if (!Verify(dto.Senha, usuario.SenhaHash))
            return Unauthorized(new { message = "Email ou senha inválidos" });

        var token = _tokenService.GenerateToken(usuario.Email);

        return Ok(new {
            token,
            nome = usuario.Nome,
            usuario = usuario.Email
        });
    }    
}