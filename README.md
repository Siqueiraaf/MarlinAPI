

# MarlinIdiomasAPI - Documentação Técnica

A **MarlinIdiomasAPI** é uma API RESTful criada para uma empresa de cursos de idiomas, permitindo o gerenciamento de alunos e turmas. A API utiliza o Entity Framework Core com mapeamento Code First, e inclui funcionalidades para CRUD de alunos e turmas, além de restrições para impedir duplicações de cadastros e gerenciamento de matrículas.


## Requisitos do Projeto

#### 1. Requisitos Obrigatórios:
- Entity Framework Core com Code First Mapping: A API utiliza o Entity Framework para trabalhar com o banco de dados, criando e mapeando as tabelas com base nos models definidos.

#### 2. Informações Obrigatórias:

- Aluno: Cada aluno tem um nome, CPF e e-mail.
- Turma: Cada turma tem um código e nível.

#### 3. Endpoints Obrigatórios:

**CRUD de Aluno:**
- Cadastro de novos alunos.
- Edição dos dados dos alunos.
- Listagem de todos os alunos ou filtragem com parâmetros opcionais.
- Exclusão de alunos.

**CRUD de Turma:**
- Cadastro de novas turmas.
- Listagem de turmas com opção de filtragem.
- Exclusão de turmas.


# Estrutura do Projeto

## 1. Models
**1.1 Aluno**

A entidade `Aluno` representa os alunos matriculados nas turmas.

#### Propriedades
- `Id`: Identificador único do aluno (chave primária).
- `Nome`: Nome completo do aluno (obrigatório).
- `CPF`: CPF do aluno (obrigatório, único).
- `Email`: Email do aluno (obrigatório, com validação de formato).

A relação com a entidade `Turma` é do tipo Muitos para Muitos.

**1.2. Turma**

A entidade `Turma` representa as turmas da escola de idiomas.

#### Propriedades:

- `Id`: Identificador único da turma (chave primária).
- `Codigo`: Código da turma (obrigatório).
- `Nivel`: Nível da turma (obrigatório).

A relação com a entidade `Aluno` é do tipo Muitos para Muitos.


## 2. Controllers
2.1. **AlunosController**

Responsável pelos endpoints de gerenciamento de alunos:
#### Cadastro de Aluno (`POST /api/aluno`):
- Cria um novo aluno, garantindo que o CPF seja único.
- Valida o CPF e e-mail.
- Matrícula o aluno em uma turma.

#### Edição de Aluno (`PUT /api/aluno/{id}`):
- Permite editar as informações do aluno (nome, CPF e e-mail), garantindo a validação dos dados.

#### Listagem de Alunos (`GET /api/aluno`):
- Retorna todos os alunos cadastrados, com filtros opcionais por nome, CPF, e turma.

#### Exclusão de Aluno (`DELETE /api/aluno/{id}`):
- Remove um aluno do sistema, desde que ele esteja desmatriculado de todas as turmas.

#### 2.2. **TurmasController**
Responsável pelos endpoints de gerenciamento de turmas:

- #### Cadastro de Turma (`POST /api/turma`):
    - Cria uma nova turma com código e nível.
- #### Listagem de Turmas (`GET /api/turma`):
   - Retorna todas as turmas, com filtros opcionais por código, nível ou alunos matriculados.
- #### Exclusão de Turma (`DELETE /api/turma/{id}`):
    - Remove uma turma desde que ela não tenha alunos matriculados.

### 3. Regras de Negócio (Requisitos Bônus)
#### 3.1. Restrições no Cadastro de Aluno
- CPF Único: Não é possível cadastrar dois alunos com o mesmo CPF.
- Matrícula em Turma: No ato do cadastro do aluno, ele deve ser matriculado em pelo menos uma turma.
- Matrícula em Múltiplas Turmas: Um aluno pode ser matriculado em várias turmas diferentes, mas não pode ser matriculado mais de uma vez na mesma turma.

#### 3.2. Limite de Alunos por Turma
Cada turma tem um limite máximo de **5 alunos**. Se uma turma já estiver completa, não será permitido adicionar mais alunos.

#### 3.4. Filtros na Listagem
- **Listagem de Alunos**: Possui filtros por nome, CPF, e o código da turma em que o aluno está matriculado.
- **Listagem de Turmas**: Permite filtrar por código, nível ou pelo número de alunos matriculados.

## 4. Contexto de Dados
O contexto `MarlinContext` gerencia as entidades `Aluno` e `Turma`, e define as relações entre elas. A API utiliza mapeamento Code First para gerar o banco de dados.

#### Relacionamentos
No método `OnModelCreating`, foi configurada a relação muitos-para-muitos entre `Aluno` e `Turma`, utilizando uma tabela intermediária `AlunoTurma` para armazenar as associações.

## Executando o Projeto

A API pode ser executada de duas maneiras: localmente ou utilizando um contêiner Docker.
### 1. Executando com Docker
#### Pré-requisitos:
- [Docker](https://docs.docker.com/get-docker/) instalado.

#### Passos para rodar o projeto com Docker:
#### 1. No diretório raiz do projeto (onde está localizado o Dockerfile), execute o comando abaixo para construir a imagem Docker:

    docker build -t marlinidiomasapi:latest .

#### 2. Após a construção da imagem, execute o contêiner:

    docker run -d -p 8080:80 --name marlin-api marlinidiomasapi:latest
- A aplicação estará acessível localmente na porta `8080`, mapeada para a porta 80 dentro do contêiner.

#### Explicação do Dockerfile:
- Etapa 1 - Build da aplicação: Utiliza a imagem oficial do SDK .NET 8.0 para restaurar dependências e compilar o projeto.
    - O arquivo `.csproj` é copiado e restaurado (`dotnet restore`), depois o código é compilado com `dotnet publish`.
- Etapa 2 - Configuração do ambiente de runtime: Utiliza a imagem `aspnet:8.0` para rodar o aplicativo em produção, expondo a porta `80`.
    - A pasta `out` (onde o build foi gerado) é copiada para o contêiner e a aplicação é iniciada com `dotnet MarlinIdiomasAPI.dll`.


### 2. Executando Localmente (Sem Docker)
#### Pré-requisitos:
- .NET 8.0 SDK
- Banco de dados SQL Server configurado localmente (ou outro de sua escolha).

#### Passos para rodar localmente:
**1. Clone o repositório:**
    
    git clone https://github.com/seu-usuario/MarlinIdiomasAPI.git

    cd MarlinIdiomasAPI
**2. Restaure as dependências:**
    
    dotnet restore
**3. Configure o banco de dados (SQL Server) e atualize a string de conexão no arquivo**
    
    appsettings.json
**4. Execute as migrações do Entity Framework para configurar o banco de dados:**

    dotnet ef database update
**5. Rode a aplicação:**

    dotnet run

**6. Acesse a aplicação em `http://localhost:5000` (ou a porta configurada).**
## Exemplos de Uso da API

#### 1. Cadastro de Aluno
#### Endpoint: `POST /api/aluno`

**Exemplo de Requisição:**
```
POST /api/aluno
{
  "nome": "Joao Silva",
  "cpf": "12345678901",
  "email": "joao@example.com",
  "turmaId": 1
}

```

**Exemplo de Resposta (Sucesso):**
```
{
  "message": "Aluno cadastrado com sucesso.",
  "aluno": {
    "id": 1,
    "nome": "Joao Silva",
    "cpf": "12345678901",
    "email": "joao@example.com",
    "turmas": [
      {
        "id": 1,
        "codigo": "TURMA123",
        "nivel": "Avançado"
      }
    ]
  }
}

```

#### 2. Cadastro de Turma
Endpoint: `POST /api/turma`

**Exemplo de Requisição:**
```
POST /api/turma
{
  "codigo": "TURMA123",
  "nivel": "Avançado"
}

```

**Exemplo de Resposta (Sucesso):**
```
{
  "message": "Turma criada com sucesso.",
  "turma": {
    "id": 1,
    "codigo": "TURMA123",
    "nivel": "Avançado"
  }
}

```

#### 3. Listagem de Turmas
Endpoint: `GET /api/turma`

**Exemplo de Resposta:**
```
[
  {
    "id": 1,
    "codigo": "TURMA123",
    "nivel": "Avançado",
    "alunos": [
      {
        "id": 1,
        "nome": "Joao Silva",
        "cpf": "12345678901",
        "email": "joao@example.com"
      }
    ]
  }
]

```

#### 4. Exclusão de Turma
Endpoint: `DELETE /api/turma/{id}`

**Exemplo de Resposta:**
```
{
  "message": "Turma excluída com sucesso."
}

```

## Tecnologias Utilizadas
- **ASP.NET Core**: Para criação da API Web.
- **Entity Framework Core**: Para mapeamento objeto-relacional com o banco de dados.
- **SQL Server**: Banco de dados relacional para armazenamento dos dados.
- **Swagger**: Para documentação e teste dos endpoints durante o desenvolvimento.
- **Docker**: Para containerização da aplicação.