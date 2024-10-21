using System.ComponentModel.DataAnnotations;

namespace MarlinIdiomasAPI.Models
{
    public class Aluno
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O Nome do aluno é obrigatório.")]
        public string Nome { get; set; }
        
        [Required(ErrorMessage = "O CPF do aluno é obrigatório.")]
        public string CPF { get; set; }
        
        [Required(ErrorMessage = "O Email do aluno é obrigatório.")]
        [EmailAddress(ErrorMessage = "O Email informado não é válido.")]
        public string Email { get; set; }

        public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
    }
}
