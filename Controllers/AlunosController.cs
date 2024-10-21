using Microsoft.AspNetCore.Mvc;
using MarlinIdiomasAPI.Data;
using MarlinIdiomasAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace MarlinIdiomasAPI.Controllers
{
    [Route("api/alunos")]
    [ApiController]
    public class AlunosController : ControllerBase
    {
        private readonly MarlinContext _context;

        public AlunosController(MarlinContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Aluno>> AddAluno(
            [FromQuery] string nome,
            [FromQuery] string cpf,
            [FromQuery] string email,
            [FromQuery] int turmaId)
        {
            if (!IsValidCPF(cpf))
            {
                return BadRequest(new { message = "CPF inválido." });
            }

            if (!IsValidEmail(email))
            {
                return BadRequest(new { message = "E-mail inválido." });
            }

            var alunoExistente = await _context.Alunos.FirstOrDefaultAsync(a => a.CPF == cpf);
            if (alunoExistente != null)
            {
                return Conflict(new { message = "CPF já cadastrado para outro aluno." });
            }
            var emailExistente = await _context.Alunos.FirstOrDefaultAsync(a => a.Email == email);
            if (emailExistente != null)
            {
                return Conflict(new { message = "Email já cadastrado para outro aluno." });
            }

            var turmaExistente = await _context.Turmas.FirstOrDefaultAsync(t => t.Id == turmaId);
            if (turmaExistente == null)
            {
                return NotFound(new { message = $"Turma com o ID {turmaId} não encontrada." });
            }

            if (await _context.Alunos.CountAsync(a => a.Turmas.Any(t => t.Id == turmaId)) >= 5)
            {
                return Conflict(new { message = $"A turma {turmaId} já atingiu o número máximo de 5 alunos." });
            }

            var novoAluno = new Aluno
            {
                Nome = nome,
                CPF = cpf,
                Email = email,
                Turmas = new List<Turma> { turmaExistente }
            };

            _context.Alunos.Add(novoAluno);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Aluno matriculado com sucesso." });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aluno>>> GetAlunos(
            string nome = null,
            string cpf = null,
            string email = null,
            int? turmaId = null)
        {
            var query = _context.Alunos.Include(a => a.Turmas).AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(a => (a.Nome ?? "").Contains(nome));
            }

            if (!string.IsNullOrEmpty(cpf))
            {
                query = query.Where(a => a.CPF == cpf);
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(a => (a.Email ?? "").Contains(email));
            }

            if (turmaId.HasValue)
            {
                query = query.Where(a => a.Turmas.Any(t => t.Id == turmaId.Value));
            }

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Aluno>> GetAluno(int id)
        {
            var aluno = await _context.Alunos.Include(a => a.Turmas)
                                             .FirstOrDefaultAsync(a => a.Id == id);

            if (aluno == null)
            {
                return NotFound(new { message = "Aluno não encontrado." });
            }

            return Ok(aluno);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAluno(int id, 
            [FromQuery] string nome = null,
            [FromQuery] string cpf = null,
            [FromQuery] string email = null,
            [FromQuery] List<int> turmaIds = null)
        {
            var alunoExistente = await _context.Alunos.Include(a => a.Turmas)
                                                          .FirstOrDefaultAsync(a => a.Id == id);

            if (alunoExistente == null)
            {
                return NotFound(new { message = "Aluno não encontrado." });
            }

            if (!string.IsNullOrEmpty(cpf) && !IsValidCPF(cpf))
            {
                return BadRequest(new { message = "CPF inválido." });
            }

            if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
            {
                return BadRequest(new { message = "E-mail inválido." });
            }

            if (!string.IsNullOrEmpty(nome))
            {
                alunoExistente.Nome = nome;
            }

            if (!string.IsNullOrEmpty(cpf))
            {
                var alunoCpfExistente = await _context.Alunos.FirstOrDefaultAsync(a => a.CPF == cpf && a.Id != id);
                if (alunoCpfExistente != null)
                {
                    return Conflict(new { message = "CPF já cadastrado para outro aluno." });
                }
                alunoExistente.CPF = cpf;
            }

            if (!string.IsNullOrEmpty(email))
            {
                var emailExistente = await _context.Alunos.FirstOrDefaultAsync(a => a.Email == email && a.Id != id);
                if (emailExistente != null)
                {
                    return Conflict(new { message = "E-mail já cadastrado para outro aluno." });
                }
                alunoExistente.Email = email;
            }

            if (turmaIds != null && turmaIds.Count > 0)
            {
                alunoExistente.Turmas.Clear();

                foreach (var turmaId in turmaIds)
                {
                    var turmaExistente = await _context.Turmas.FirstOrDefaultAsync(t => t.Id == turmaId);

                    if (turmaExistente != null)
                    {
                        alunoExistente.Turmas.Add(turmaExistente);
                    }
                    else
                    {
                        return NotFound(new { message = $"Turma com o ID {turmaId} não encontrada." });
                    }
                }
            }

            _context.Entry(alunoExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Aluno atualizado com sucesso.", aluno = alunoExistente });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlunoExists(id))
                {
                    return NotFound(new { message = "Aluno não encontrado para a atualização." });
                }
                else
                {
                    throw;
                }
            }
        }

        private bool AlunoExists(int id)
        {
            return _context.Alunos.Any(a => a.Id == id);
        }

        private bool IsValidCPF(string cpf)
        {
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11 || !Regex.IsMatch(cpf, @"^\d{11}$"))
            {
                return false;
            }

            return true; 
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAluno(int id, [FromQuery] string cpf)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == id);

            if (aluno == null)
            {
                return NotFound(new { message = "Aluno não encontrado." });
            }

            if (aluno.CPF != cpf)
            {
                return BadRequest(new { message = "O CPF fornecido não coincide com o CPF do aluno." });
            }

            _context.Alunos.Remove(aluno);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Aluno com CPF {cpf} excluido com sucesso." });
        }
    }
}
