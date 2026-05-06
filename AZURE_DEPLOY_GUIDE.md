# 🚀 Guia de Deploy - FinanceSap.Enterprise no Azure

## ✅ Arquivos Atualizados

### 1. **Program.cs** 
- ✅ Prioriza `MYSQL_CONNECTION_STRING` do Azure
- ✅ Sobrescreve configuração do appsettings.json automaticamente

### 2. **.github/workflows/main_financesap.yml**
- ✅ Pipeline CI/CD completo
- ✅ Build, Test e Deploy automatizados
- ✅ Publicação de resultados de testes

### 3. **azure-database-schema.sql**
- ✅ Script consolidado com TODAS as tabelas:
  - `customers` (Clientes)
  - `accounts` (Contas)
  - `loan_applications` (Solicitações de Empréstimo)
  - `loans` (Empréstimos Aprovados) ⭐ **NOVO**
  - Tabelas do ASP.NET Core Identity (8 tabelas)
  - `__EFMigrationsHistory` (Controle de migrations)

---

## 📋 Checklist de Deploy no Azure

### **Passo 1: Criar Recursos no Azure**

```bash
# 1. Criar Resource Group
az group create --name rg-financesap --location brazilsouth

# 2. Criar Azure MySQL Flexible Server
az mysql flexible-server create \
  --resource-group rg-financesap \
  --name financesap-mysql \
  --location brazilsouth \
  --admin-user adminuser \
  --admin-password <SUA_SENHA_FORTE> \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 8.0

# 3. Criar banco de dados
az mysql flexible-server db create \
  --resource-group rg-financesap \
  --server-name financesap-mysql \
  --database-name financesap

# 4. Configurar firewall (permitir Azure Services)
az mysql flexible-server firewall-rule create \
  --resource-group rg-financesap \
  --name financesap-mysql \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# 5. Criar App Service Plan
az appservice plan create \
  --name plan-financesap \
  --resource-group rg-financesap \
  --location brazilsouth \
  --sku B1 \
  --is-linux

# 6. Criar Web App
az webapp create \
  --resource-group rg-financesap \
  --plan plan-financesap \
  --name financesap \
  --runtime "DOTNET|9.0"
```

---

### **Passo 2: Executar Script SQL no Azure MySQL**

```bash
# Conectar ao MySQL e executar o script
mysql -h financesap-mysql.mysql.database.azure.com \
      -u adminuser \
      -p \
      financesap < azure-database-schema.sql
```

**Ou via Azure Portal:**
1. Acesse o Azure MySQL no portal
2. Vá em **Query Editor**
3. Cole o conteúdo de `azure-database-schema.sql`
4. Execute

---

### **Passo 3: Configurar Variáveis de Ambiente no Azure App Service**

```bash
# Connection String do MySQL
az webapp config appsettings set \
  --resource-group rg-financesap \
  --name financesap \
  --settings MYSQL_CONNECTION_STRING="Server=financesap-mysql.mysql.database.azure.com;Port=3306;Database=financesap;Uid=adminuser;Pwd=<SUA_SENHA>;SslMode=Required;"

# JWT Secret (CRÍTICO: Use uma chave forte de 32+ caracteres)
az webapp config appsettings set \
  --resource-group rg-financesap \
  --name financesap \
  --settings Jwt__Key="<GERE_UMA_CHAVE_SEGURA_DE_32_CARACTERES>"

# JWT Issuer e Audience
az webapp config appsettings set \
  --resource-group rg-financesap \
  --name financesap \
  --settings Jwt__Issuer="FinanceSap" Jwt__Audience="FinanceSap"
```

**⚠️ IMPORTANTE:** Gere uma chave JWT segura:
```bash
# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | % {[char]$_})

# Linux/Mac
openssl rand -base64 32
```

---

### **Passo 4: Configurar GitHub Actions**

1. **Obter Publish Profile do Azure:**
   ```bash
   az webapp deployment list-publishing-profiles \
     --resource-group rg-financesap \
     --name financesap \
     --xml
   ```

2. **Adicionar Secret no GitHub:**
   - Vá em: `Settings` → `Secrets and variables` → `Actions`
   - Clique em `New repository secret`
   - Nome: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Valor: Cole o XML do comando anterior

3. **Fazer Push para `main`:**
   ```bash
   git add .
   git commit -m "feat: Azure deployment ready"
   git push origin main
   ```

4. **Acompanhar Deploy:**
   - Acesse: `Actions` no GitHub
   - Veja o workflow `Build and Deploy to Azure` executando

---

## 🔍 Verificação Pós-Deploy

### **1. Verificar Saúde da API**
```bash
curl https://financesap.azurewebsites.net/health
```

### **2. Testar Endpoint de Registro**
```bash
curl -X POST https://financesap.azurewebsites.net/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "teste@financesap.com",
    "password": "Teste@123",
    "fullName": "Usuário Teste",
    "document": "12345678901"
  }'
```

### **3. Verificar Logs no Azure**
```bash
az webapp log tail \
  --resource-group rg-financesap \
  --name financesap
```

---

## 🛡️ Segurança - Checklist Final

- [ ] ✅ JWT Key com 32+ caracteres aleatórios
- [ ] ✅ Connection String com `SslMode=Required`
- [ ] ✅ Firewall do MySQL configurado (apenas Azure Services)
- [ ] ✅ HTTPS Redirect habilitado (já está no Program.cs)
- [ ] ✅ Rate Limiting configurado (já está no código)
- [ ] ✅ Security Headers aplicados (já está no código)
- [ ] ✅ CORS configurado para produção (ajustar origens permitidas)

---

## 📊 Monitoramento

### **Application Insights (Recomendado)**
```bash
# Criar Application Insights
az monitor app-insights component create \
  --app financesap-insights \
  --location brazilsouth \
  --resource-group rg-financesap \
  --application-type web

# Obter Instrumentation Key
az monitor app-insights component show \
  --app financesap-insights \
  --resource-group rg-financesap \
  --query instrumentationKey

# Configurar no App Service
az webapp config appsettings set \
  --resource-group rg-financesap \
  --name financesap \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="<CONNECTION_STRING>"
```

---

## 🆘 Troubleshooting

### **Erro: "Connection refused" no MySQL**
- Verifique se o firewall permite conexões do Azure
- Confirme que `SslMode=Required` está na connection string

### **Erro: "JWT Key not configured"**
- Verifique se `Jwt__Key` está configurado no App Service
- Use `__` (dois underscores) para separar seções no Azure

### **Erro: "Migration not applied"**
- Execute o script SQL manualmente no Azure MySQL
- Verifique se a tabela `__EFMigrationsHistory` existe

### **Erro 500 na API**
- Verifique logs: `az webapp log tail`
- Confirme que todas as variáveis de ambiente estão configuradas

---

## 📞 Suporte

- **Documentação Azure:** https://docs.microsoft.com/azure
- **Pricing Calculator:** https://calculator.aws (para AWS) / https://azure.microsoft.com/pricing/calculator (para Azure)
- **GitHub Actions:** https://docs.github.com/actions

---

## ✅ Status do Projeto

- ✅ Módulo de Clientes (100% testado)
- ✅ Módulo de Contas (100% testado)
- ✅ Módulo de Empréstimos (100% testado) ⭐
- ✅ Autenticação JWT (implementado)
- ✅ Rate Limiting (implementado)
- ✅ Security Headers (implementado)
- ✅ CI/CD Pipeline (configurado)
- ✅ Azure Deploy Ready (pronto para produção)

---

**🎉 Parabéns! Seu sistema está pronto para produção no Azure!**
