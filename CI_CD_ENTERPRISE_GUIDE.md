# FinanceSap CI/CD Pipeline — Enterprise-Grade Configuration

## 🎯 Objetivo

Pipeline de CI/CD de nível production-ready com:
- ✅ **Environment Parity** — CI espelha exatamente o ambiente de produção
- ✅ **Native Docker Healthchecks** — dependência estrita no MySQL estar healthy
- ✅ **Secret Masking** — JWT secrets nunca aparecem nos logs
- ✅ **Observability** — GitHub Summary com detalhes de falhas
- ✅ **12-Factor App** — configuração via variáveis de ambiente

---

## 📋 Checklist de Configuração

### 1. GitHub Secrets (OBRIGATÓRIO)

Acesse: **GitHub → Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Valor | Descrição |
|-------------|-------|-----------|
| `JWT_KEY` | `sua-chave-secreta-minimo-32-caracteres` | Chave de assinatura JWT (NUNCA commitar) |
| `JWT_ISSUER` | `FinanceSap` | Emissor do token |
| `JWT_AUDIENCE` | `FinanceSap` | Audiência do token |

**⚠️ CRÍTICO:** Se esses secrets não existirem, o job de integração **falhará** ao inicializar a aplicação.

---

## 🏗️ Arquitetura do Pipeline

```
┌─────────────────────────────────────────────────────────────┐
│  STAGE 1: Build & Unit Tests (~2 min)                      │
│  ✓ Compila a solution                                       │
│  ✓ Roda testes unitários (CpfTests, etc.)                   │
│  ✓ Gera relatório de cobertura                              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  STAGE 2: Integration Tests (~5 min)                        │
│  ✓ Sobe MySQL 8.0 com healthcheck nativo                    │
│  ✓ Aguarda MySQL estar 100% pronto                          │
│  ✓ Roda testes de integração (CustomerIntegrationTests)     │
│  ✓ Publica resultados no GitHub Summary                     │
│  ✓ Upload de artefatos (TRX, HTML, logs)                    │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔐 Secret Masking & Environment Variables

### Convenção de Nomenclatura (.NET)

O .NET usa **double underscore (`__`)** para mapear hierarquia de configuração:

```yaml
# GitHub Actions Workflow
env:
  Jwt__Key: ${{ secrets.JWT_KEY }}
  Jwt__Issuer: ${{ secrets.JWT_ISSUER }}
  ConnectionStrings__DefaultConnection: "Server=127.0.0.1;..."
```

**Mapeia automaticamente para:**

```json
{
  "Jwt": {
    "Key": "valor-do-secret",
    "Issuer": "FinanceSap"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;..."
  }
}
```

### Como o DependencyInjection.cs Lê

```csharp
var jwtKey = configuration["Jwt:Key"];  // ← Resolve Jwt__Key automaticamente
```

**Nenhuma mudança de código necessária!** O `IConfiguration` do .NET resolve automaticamente.

---

## 🐳 MySQL Service Container — Native Healthcheck

### Configuração Atual

```yaml
services:
  mysql:
    image: mysql:8.0
    options: >-
      --name mysql-test-container
      --health-cmd="mysqladmin ping --silent -h localhost -uroot -proot"
      --health-interval=5s
      --health-timeout=3s
      --health-retries=20
      --health-start-period=30s
```

### Como Funciona

1. **GitHub Actions inicia o container** antes de qualquer step
2. **Docker executa o healthcheck** a cada 5 segundos
3. **Após 30 segundos** (start-period), começa a contar retries
4. **Máximo 20 tentativas** (100 segundos total)
5. **Status `healthy`** → steps começam a executar

### Verificação Explícita (Defense in Depth)

Mesmo com o healthcheck nativo, adicionamos um step de verificação:

```bash
for i in {1..30}; do
  if mysqladmin ping -h 127.0.0.1 -P 3306 -uroot -proot --silent 2>/dev/null; then
    echo "✅ MySQL is ready"
    exit 0
  fi
  sleep 2
done
```

**Por quê?** O healthcheck valida que o **processo** está de pé, mas não garante que o usuário `root` aceita conexões TCP externas. Essa verificação adicional elimina race conditions.

---

## 📊 Observability — GitHub Summary

### O Que É Gerado

Quando os testes falham, o workflow gera automaticamente um relatório no **GitHub Summary**:

```markdown
## 🧪 Integration Test Results

| Metric | Count |
|--------|-------|
| Total  | 15    |
| ✅ Passed | 12    |
| ❌ Failed | 3     |

### ❌ Failed Tests
```
Test: Create_WithDuplicateCpf_Returns409Conflict
Error: Expected status code 409, but got 500
Stack: at FinanceSap.Tests.Integration.CustomerIntegrationTests...
```
```

### Como Acessar

1. Acesse a aba **Actions** no GitHub
2. Clique no workflow run que falhou
3. Role até o final da página → **Summary**

---

## 🔍 Debugging de Falhas

### Logs Disponíveis

| Artefato | Localização | Retenção |
|----------|-------------|----------|
| **TRX Report** | Actions → Artifacts → `integration-test-results` | 30 dias |
| **HTML Report** | Actions → Artifacts → `integration-test-results` | 30 dias |
| **MySQL Logs** | GitHub Summary (se falhar) | Permanente |
| **Coverage Report** | Actions → Artifacts → `unit-test-coverage` | 7 dias |

### Comandos de Diagnóstico

**Ver logs do MySQL container:**
```bash
docker logs mysql-test-container
```

**Testar conexão manualmente:**
```bash
mysqladmin ping -h 127.0.0.1 -P 3306 -uroot -proot
mysql -h 127.0.0.1 -P 3306 -uroot -proot -e "SELECT VERSION();"
```

---

## 🚀 Diferenças vs. Versão Anterior

| Aspecto | Antes | Agora (Enterprise) |
|---------|-------|-------------------|
| **Healthcheck** | Manual loop com `sleep` | Native Docker healthcheck + verificação explícita |
| **Secrets** | `TEST_DB_CONNECTION` hardcoded | `ConnectionStrings__DefaultConnection` via env |
| **JWT Config** | `Jwt__Key` sem fallback | `Jwt__Key` com fallback para `FinanceSap` |
| **Environment** | Não definido | `ASPNETCORE_ENVIRONMENT=Testing` |
| **Logging** | Console básico | GitHub Summary + TRX + HTML + MySQL logs |
| **Timeout** | Sem limite | 10 min (unit) / 15 min (integration) |
| **Test Reporter** | Upload manual | `dorny/test-reporter` com anotações |
| **Emojis** | ❌ | ✅ (melhor UX no GitHub) |

---

## 📝 Validação Local

### Simular o CI Localmente

```bash
# 1. Exportar as variáveis de ambiente
export ASPNETCORE_ENVIRONMENT=Testing
export ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3306;Database=financesap_tests;Uid=root;Pwd=root;"
export Jwt__Key="CHANGE-THIS-TO-A-SECURE-KEY-AT-LEAST-32-CHARACTERS-LONG"
export Jwt__Issuer="FinanceSap"
export Jwt__Audience="FinanceSap"

# 2. Subir o MySQL (Docker)
docker run -d \
  --name mysql-local-test \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=financesap_tests \
  -p 3306:3306 \
  mysql:8.0

# 3. Aguardar MySQL estar pronto
for i in {1..30}; do
  if mysqladmin ping -h 127.0.0.1 -P 3306 -uroot -proot --silent 2>/dev/null; then
    echo "MySQL ready"
    break
  fi
  sleep 2
done

# 4. Rodar os testes
dotnet test FinanceSap.Tests/FinanceSap.Tests.csproj \
  --configuration Release \
  --filter "FullyQualifiedName~Integration" \
  --logger "console;verbosity=detailed"

# 5. Limpar
docker stop mysql-local-test && docker rm mysql-local-test
```

---

## 🛡️ Security Best Practices

### ✅ Implementado

- **Secret Masking**: GitHub Actions oculta automaticamente valores de `${{ secrets.* }}` nos logs
- **Least Privilege**: MySQL usa `root` apenas no CI (em produção, use usuário dedicado)
- **Network Isolation**: Service containers rodam em rede isolada do runner
- **Immutable Infrastructure**: Cada job é uma VM nova e limpa
- **Audit Trail**: Todos os runs ficam registrados por 90 dias (GitHub Free)

### ⚠️ Próximos Passos (Opcional)

- [ ] Adicionar **Dependabot** para atualizar actions automaticamente
- [ ] Configurar **CODEOWNERS** para exigir review em `.github/workflows/`
- [ ] Adicionar **branch protection rules** (require status checks)
- [ ] Integrar com **SonarCloud** para análise de código
- [ ] Adicionar **SAST** (Snyk, Trivy) para scan de vulnerabilidades

---

## 📚 Referências

- [GitHub Actions: Service Containers](https://docs.github.com/en/actions/using-containerized-services/about-service-containers)
- [.NET Configuration: Environment Variables](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-9.0#environment-variables)
- [Docker Healthcheck Reference](https://docs.docker.com/engine/reference/builder/#healthcheck)
- [12-Factor App: Config](https://12factor.net/config)

---

**Pipeline pronto para produção!** 🚀
