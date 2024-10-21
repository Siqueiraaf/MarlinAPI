using System.ComponentModel.DataAnnotations;

namespace MarlinIdiomasAPI.Models
{
    public class Turma
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O código da turma é obrigatório.")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "O nível da turma é obrigatório.")]
        public string Nivel { get; set; }

        public ICollection<Aluno> Alunos { get; set; } = new List<Aluno>();
    }
}
