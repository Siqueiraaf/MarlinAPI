using MarlinIdiomasAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MarlinIdiomasAPI.Data
{
    public class MarlinContext : DbContext
    {
        public MarlinContext(DbContextOptions<MarlinContext> options) : base(options) { }

        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Turma> Turmas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Aluno>()
                        .HasMany(a => a.Turmas)
                        .WithMany(t => t.Alunos)
                        .UsingEntity(j => j.ToTable("AlunoTurma"));
        }
    }
}
