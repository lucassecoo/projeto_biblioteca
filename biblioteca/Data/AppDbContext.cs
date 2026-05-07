using Microsoft.EntityFrameworkCore;
using biblioteca.Models;


namespace biblioteca.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){ }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Emprestimo> Emprestimos => Set<Emprestimo>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Nome)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(u => u.SenhaHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });

        modelBuilder.Entity<Livro>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Titulo)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(l => l.Autor)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(l => l.AnoPublicacao)
                .IsRequired();

            entity.Property(l => l.QuantidadeDisponivel)
                .IsRequired();
        });

        // Emprestimo
        modelBuilder.Entity<Emprestimo>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DataEmprestimo)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.DataDevolucao)
                .IsRequired(false);

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Emprestimos)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Livro)
                .WithMany(l => l.Emprestimos)
                .HasForeignKey(e => e.LivroId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
