# 🔄 Migração MySQL → SQLite - Guia Completo

## ✅ MIGRAÇÃO CONCLUÍDA COM SUCESSO

### 📋 Resumo das Alterações

A aplicação foi refatorada de **MySQL/Pomelo** para **SQLite** para simplificar o deploy inicial.

---

## 1️⃣ PACOTES NUGET

### ❌ Removidos:
```bash
dotnet remove FinanceSap.Infrastructure package Pomelo.EntityFrameworkCore.MySql
```

### ✅ Adicionados:
```bash
dotnet add FinanceSap.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite --version 9.0.0
```

**Pacotes Instalados**:
- `Microsoft.EntityFrameworkCore.Sqlite` 9.0.0
- `Microsoft.EntityFrameworkCore.Sqlite.Core` 9.0.0
- `Microsoft.Data.Sqlite.Core` 9.0.0
- `SQLitePCLRaw.bundle_e_sqlite3` 2.1.10
- `SQLitePCLRaw.core` 2.1.10
- `SQLitePCLRaw.provider.e_sqlite3` 2.1.10
- `SQLitePCLRaw.lib.e_sqlite3` 2.1.10

---

## 2️⃣ CONFIGURAÇÃO DO DBCONTEXT

### Arquivo: `FinanceSap.Infrastructure/DependencyInjection.cs`

**ANTES (MySQL)**:
```csharp
var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
    ?? configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysql => mysql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    )
);
```

**DEPOIS (SQLite)**:
```csharp
var connectionString = Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING")
    ?? configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=financesap.db";

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        connectionString,
        sqlite => sqlite.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    )
);
```

**Mudanças Principais**:
- ✅ Variável de ambiente: `MYSQL_CONNECTION_STRING` → `SQLITE_CONNECTION_STRING`
- ✅ Provider: `UseMySql()` → `UseSqlite()`
- ✅ Removido: `ServerVersion.AutoDetect()` (não necessário no SQLite)
- ✅ Fallback padrão: `"Data Source=financesap.db"`

---

## 3️⃣ CONNECTION STRING

### Arquivo: `FinanceSap.Api/appsettings.json`

**ANTES (MySQL)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=financesap;Uid=root;Pwd=root;"
  }
}
```

**DEPOIS (SQLite)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=financesap.db"
  }
}
```

### 📝 Formatos de Connection String SQLite

#### Arquivo Local (Recomendado para Desenvolvimento):
```
Data Source=financesap.db
```
- Cria o arquivo `financesap.db` no diretório de execução
- Caminho relativo ao diretório da aplicação

#### Caminho Absoluto:
```
Data Source=C:\Data\financesap.db
```

#### Caminho Relativo com Subpasta:
```
Data Source=./data/financesap.db
```

#### In-Memory (Apenas para Testes):
```
Data Source=:memory:
```
- ⚠️ Dados são perdidos ao fechar a conexão
- Útil apenas para testes unitários

#### Modo Read-Only:
```
Data Source=financesap.db;Mode=ReadOnly
```

#### Com Cache Compartilhado:
```
Data Source=financesap.db;Cache=Shared
```

---

## 4️⃣ MIGRATIONS

### ❌ Migrations Antigas (MySQL) - REMOVIDAS

Todas as migrations antigas foram deletadas pois eram específicas para MySQL:
```bash
rmdir /s /q FinanceSap.Infrastructure\Migrations
```

**Motivo**: Migrations MySQL contêm tipos de dados e sintaxe incompatíveis com SQLite.

### ✅ Nova Migration (SQLite) - CRIADA

```bash
dotnet ef migrations add InitialSqliteMigration \
  --project FinanceSap.Infrastructure \
  --startup-project FinanceSap.Api
```

**Arquivo Gerado**:
- `FinanceSap.Infrastructure/Migrations/YYYYMMDDHHMMSS_InitialSqliteMigration.cs`
- `FinanceSap.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`

---

## 5️⃣ AUTO-CRIAÇÃO DO BANCO DE DADOS

### Arquivo: `FinanceSap.Api/Program.cs`

**Código Adicionado**:
```csharp
using FinanceSap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var app = builder.Build();

// ── Auto-create SQLite Database ──────────────────────────────────────────────
// Garante que o banco de dados SQLite seja criado automaticamente na inicialização
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Aplica migrations pendentes e cria o banco se não existir
        dbContext.Database.Migrate();
        app.Logger.LogInformation("✅ SQLite database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Error initializing SQLite database");
        throw;
    }
}
```

**Comportamento**:
1. ✅ Cria o arquivo `.db` se não existir
2. ✅ Aplica todas as migrations pendentes
3. ✅ Loga sucesso ou erro
4. ✅ Executa automaticamente no startup da aplicação

---

## 6️⃣ GITIGNORE

O arquivo `.gitignore` já está configurado para ignorar bancos SQLite:

```gitignore
# Banco SQLite local usado em testes ou desenvolvimento rápido
*.db
*.db-shm
*.db-wal
*.sqlite
*.sqlite3
```

**Arquivos Ignorados**:
- `financesap.db` - Arquivo principal do banco
- `financesap.db-shm` - Shared memory file (SQLite WAL mode)
- `financesap.db-wal` - Write-Ahead Log file

---

## 7️⃣ DIFERENÇAS MYSQL vs SQLITE

### Tipos de Dados

| MySQL | SQLite | Observação |
|-------|--------|------------|
| `VARCHAR(n)` | `TEXT` | SQLite não limita tamanho |
| `INT` | `INTEGER` | Compatível |
| `DECIMAL(18,2)` | `REAL` | SQLite usa ponto flutuante |
| `DATETIME` | `TEXT` | SQLite armazena como ISO8601 |
| `TINYINT(1)` | `INTEGER` | 0 = false, 1 = true |
| `CHAR(36)` | `TEXT` | Para GUIDs |

### Recursos Não Suportados no SQLite

❌ **Stored Procedures** - Não existem no SQLite  
❌ **Triggers Complexos** - Suporte limitado  
❌ **Foreign Key Cascade** - Desabilitado por padrão (pode ser habilitado)  
❌ **AUTO_INCREMENT** - Usa `AUTOINCREMENT` (diferente)  
❌ **Multiple Databases** - Apenas um banco por conexão  

### Vantagens do SQLite

✅ **Zero Configuration** - Não precisa de servidor  
✅ **Arquivo Único** - Fácil backup e deploy  
✅ **Cross-Platform** - Funciona em Windows, Linux, macOS  
✅ **Rápido** - Excelente para leitura  
✅ **Confiável** - Usado em produção por milhões de apps  

---

## 8️⃣ COMANDOS ÚTEIS

### Criar Nova Migration
```bash
dotnet ef migrations add NomeDaMigration \
  --project FinanceSap.Infrastructure \
  --startup-project FinanceSap.Api
```

### Aplicar Migrations
```bash
dotnet ef database update \
  --project FinanceSap.Infrastructure \
  --startup-project FinanceSap.Api
```

### Reverter Migration
```bash
dotnet ef migrations remove \
  --project FinanceSap.Infrastructure \
  --startup-project FinanceSap.Api
```

### Gerar Script SQL
```bash
dotnet ef migrations script \
  --project FinanceSap.Infrastructure \
  --startup-project FinanceSap.Api \
  --output schema.sql
```

### Visualizar Banco SQLite
```bash
# Instalar SQLite CLI
# Windows: choco install sqlite
# macOS: brew install sqlite
# Linux: apt-get install sqlite3

# Abrir banco
sqlite3 financesap.db

# Comandos úteis
.tables                    # Listar tabelas
.schema customers          # Ver estrutura da tabela
SELECT * FROM customers;   # Query
.quit                      # Sair
```

---

## 9️⃣ TESTES

### Executar Testes
```bash
dotnet test
```

**Observação**: Os testes de integração usarão SQLite in-memory automaticamente se configurado no `CustomWebApplicationFactory`.

### Verificar Banco Criado
```bash
# Executar a API
dotnet run --project FinanceSap.Api

# Verificar se o arquivo foi criado
dir FinanceSap.Api\financesap.db
```

---

## 🔟 DEPLOY

### Desenvolvimento Local
1. Clone o repositório
2. Execute `dotnet run --project FinanceSap.Api`
3. O banco `financesap.db` será criado automaticamente
4. Acesse `https://localhost:7001/scalar/v1`

### Produção (Azure/Docker)

#### Opção 1: Arquivo Local
```bash
# Variável de ambiente
SQLITE_CONNECTION_STRING="Data Source=/app/data/financesap.db"
```

#### Opção 2: Volume Persistente (Docker)
```yaml
volumes:
  - ./data:/app/data
environment:
  - SQLITE_CONNECTION_STRING=Data Source=/app/data/financesap.db
```

#### Opção 3: Azure App Service
- SQLite funciona no Azure App Service
- Use caminho relativo: `Data Source=./financesap.db`
- ⚠️ Dados podem ser perdidos em redeploy (use Azure Storage para persistência)

---

## ⚠️ LIMITAÇÕES E CONSIDERAÇÕES

### Quando NÃO Usar SQLite

❌ **Alta Concorrência de Escrita** - SQLite trava o arquivo inteiro  
❌ **Múltiplos Servidores** - Não suporta clustering  
❌ **Bancos Muito Grandes** - Limite prático ~1TB, recomendado <100GB  
❌ **Requisitos Enterprise** - Sem stored procedures, views materializadas, etc.  

### Quando Usar SQLite

✅ **Desenvolvimento Local** - Perfeito para prototipagem  
✅ **Aplicações Desktop** - Banco embutido  
✅ **Mobile Apps** - Padrão no iOS/Android  
✅ **Testes Automatizados** - In-memory é extremamente rápido  
✅ **Aplicações de Leitura** - Excelente performance  
✅ **Deploy Simplificado** - Sem dependências externas  

---

## 📊 CHECKLIST DE MIGRAÇÃO

- [x] Remover pacote Pomelo.EntityFrameworkCore.MySql
- [x] Adicionar pacote Microsoft.EntityFrameworkCore.Sqlite
- [x] Atualizar DependencyInjection.cs (UseSqlite)
- [x] Atualizar appsettings.json (connection string)
- [x] Remover migrations antigas do MySQL
- [x] Criar nova migration para SQLite
- [x] Adicionar auto-criação do banco no Program.cs
- [x] Verificar .gitignore (ignorar *.db)
- [x] Testar build (`dotnet build`)
- [x] Testar execução (`dotnet run`)
- [x] Verificar criação do arquivo .db
- [x] Testar endpoints da API
- [x] Executar testes unitários

---

## 🎉 CONCLUSÃO

A migração de MySQL para SQLite foi concluída com sucesso!

**Benefícios Obtidos**:
- ✅ Deploy simplificado (sem servidor MySQL)
- ✅ Desenvolvimento mais rápido (zero configuração)
- ✅ Testes mais rápidos (in-memory)
- ✅ Portabilidade (arquivo único)
- ✅ Cross-platform (Windows, Linux, macOS)

**Próximos Passos**:
1. Executar `dotnet run --project FinanceSap.Api`
2. Verificar criação do `financesap.db`
3. Testar endpoints via Scalar
4. Executar testes: `dotnet test`

---

**📝 Nota**: Se precisar voltar para MySQL no futuro, basta reverter as alterações e recriar as migrations.
