using Microsoft.AspNetCore.Mvc;
using MarlinIdiomasAPI.Data;
using MarlinIdiomasAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MarlinIdiomasAPI.Controllers
{
    [Route("api/turma")]
    [ApiController]
    public class TurmasController : ControllerBase
    {
        private readonly MarlinContext _context;

        public TurmasController(MarlinContext context)
        {
            _context = context;
        }
        
        [HttpPost]
        public async Task<ActionResult<Turma>> PostTurma(
            [FromQuery] string codigo,
            [FromQuery] string nivel)
        {
            if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(nivel))
            {
                return BadRequest(new { message = "Os parâmetros 'codigo' e 'nivel' são obrigatórios." });
            }

            var turmaExistente = await _context.Turmas.FirstOrDefaultAsync(t => t.Codigo == codigo);
            if (turmaExistente != null)
            {
                return Conflict(new { message = "Já existe uma turma com este código." });
            }

            var turma = new Turma
            {
                Codigo = codigo,
                Nivel = nivel
            };

            _context.Turmas.Add(turma);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Erro ao salvar a turma: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro inesperado: " + ex.Message });
            }

            return CreatedAtAction(nameof(GetTurma), new { id = turma.Id }, new 
            { 
                message = "Turma criada com sucesso.", 
                turma 
            });
        }

        [HttpPost("{turmaId}/alunos")]
        public async Task<ActionResult> AddAlunoToTurma(int turmaId, 
            [FromQuery] string nome, 
            [FromQuery] string cpf, 
            [FromQuery] string email)
        {
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(cpf))
            {
                return BadRequest(new { message = "Nome e CPF do aluno são obrigatórios." });
            }
        
            var turma = await _context.Turmas.Include(t => t.Alunos).FirstOrDefaultAsync(t => t.Id == turmaId);
        
            if (turma == null)
            {
                return NotFound(new { message = "Turma não encontrada." });
            }
        
            if (turma.Alunos.Any(a => a.CPF == cpf))
            {
                return Conflict(new { message = "Aluno já está matriculado na turma." });
            }
        
            var alunoExistente = await _context.Alunos.FirstOrDefaultAsync(a => a.CPF == cpf);
            if (alunoExistente != null)
            {
                turma.Alunos.Add(alunoExistente);
            }
            else
            {
                var novoAluno = new Aluno
                {
                    Nome = nome,
                    CPF = cpf,
                    Email = email
                };
                turma.Alunos.Add(novoAluno);
            }
        
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Aluno adicionado à turma com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Erro ao adicionar o aluno: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Turma>>> GetTurmas(
            [FromQuery] string codigo = null, 
            [FromQuery] string nivel = null, 
            [FromQuery] int? alunoId = null)
        {
            var query = _context.Turmas.Include(t => t.Alunos).AsQueryable();

            if (!string.IsNullOrEmpty(codigo))
            {
                query = query.Where(t => t.Codigo.Contains(codigo));
            }

            if (!string.IsNullOrEmpty(nivel))
            {
                query = query.Where(t => t.Nivel.Contains(nivel));
            }

            if (alunoId.HasValue)
            {
                query = query.Where(t => t.Alunos.Any(a => a.Id == alunoId.Value));
            }

            var turmas = await query.ToListAsync();
            return Ok(turmas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Turma>> GetTurma(int id)
        {
            var turma = await _context.Turmas.Include(t => t.Alunos).FirstOrDefaultAsync(t => t.Id == id);
            if (turma == null)
            {
                return NotFound(new { message = "Turma não encontrada." });
            }
            return turma;
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTurma(int id, [FromQuery] string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                return BadRequest(new { message = "O código da turma é obrigatório." });
            }
        
            var turma = await _context.Turmas.Include(t => t.Alunos).FirstOrDefaultAsync(t => t.Id == id);
        
            if (turma == null)
            {
                return NotFound(new { message = "Turma não encontrada." });
            }
        
            if (turma.Codigo != codigo)
            {
                return Conflict(new { message = "O código da turma não corresponde ao código registrado." });
            }
        
            if (turma.Alunos.Count != 0)
            {
                return Conflict(new { message = "Não é possível excluir a turma, pois ela possui alunos matriculados." });
            }
        
            _context.Turmas.Remove(turma);
            await _context.SaveChangesAsync();
        
            return NoContent();
        }

        private bool TurmaExists(int id)
        {
            return _context.Turmas.Any(e => e.Id == id);
        }
    }
}
