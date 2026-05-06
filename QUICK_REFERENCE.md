# 🚀 Quick Deployment Reference Card

## 🔑 Required Environment Variables

### Azure Web App Configuration
```bash
# Database Connection
MYSQL_CONNECTION_STRING="Server=<server>.mysql.database.azure.com;Database=financesap_db;User=<user>;Password=<password>;SslMode=Required;"

# JWT Configuration
Jwt__Key="<YourSecureJwtKey-MinimumLength32Characters>"
Jwt__Issuer="FinanceSap"
Jwt__Audience="FinanceSap"

# Environment
ASPNETCORE_ENVIRONMENT="Production"
```

## 📝 Quick Commands

### Deploy Database Schema
```bash
mysql -h <server>.mysql.database.azure.com \
      -u <user> \
      -p \
      financesap_db < database-schema.sql
```

### Set Environment Variables
```bash
az webapp config appsettings set \
  --resource-group rg-financesap \
  --name financesap-api \
  --settings \
    MYSQL_CONNECTION_STRING="<connection-string>" \
    Jwt__Key="<jwt-key>" \
    Jwt__Issuer="FinanceSap" \
    Jwt__Audience="FinanceSap" \
    ASPNETCORE_ENVIRONMENT="Production"
```

### View Logs
```bash
az webapp log tail --resource-group rg-financesap --name financesap-api
```

### Restart App
```bash
az webapp restart --resource-group rg-financesap --name financesap-api
```

## 🔗 Important URLs

- **API**: `https://financesap-api.azurewebsites.net`
- **Scalar Docs**: `https://financesap-api.azurewebsites.net/scalar/v1`
- **Azure Portal**: `https://portal.azure.com`
- **GitHub Actions**: `https://github.com/<your-repo>/actions`

## 📊 API Endpoints

### Customers
- `POST /api/customers` - Create customer
- `GET /api/customers/{id}` - Get customer

### Accounts
- `POST /api/accounts` - Create account
- `GET /api/accounts/{id}/balance` - Get balance

### Loans
- `POST /api/loans` - Request loan
- `GET /api/loans/{id}` - Get loan details

### Authentication
- `POST /api/auth/register` - Register user
- `POST /api/auth/login` - Login

## 🧪 Test Request Example

```bash
# Create a loan request
curl -X POST https://financesap-api.azurewebsites.net/api/loans \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "00000000-0000-0000-0000-000000000000",
    "amount": 10000,
    "installments": 12,
    "annualRate": 0.12
  }'
```

## ⚠️ Important Notes

1. **Interest Rate Format**: Use decimal (0.12 for 12%)
2. **Connection String**: Must include `SslMode=Required` for Azure MySQL
3. **JWT Key**: Minimum 32 characters for security
4. **GitHub Secret**: Must be named `AZURE_WEBAPP_PUBLISH_PROFILE`
5. **Database**: Run `database-schema.sql` before first deployment

## 🆘 Quick Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection failed | Check firewall rules |
| 500 Error | Check application logs |
| Tests failing | Verify environment variables |
| Deployment failed | Check GitHub Actions logs |

## 📞 Support Commands

```bash
# Check app status
az webapp show --resource-group rg-financesap --name financesap-api --query state

# Get app URL
az webapp show --resource-group rg-financesap --name financesap-api --query defaultHostName -o tsv

# List environment variables
az webapp config appsettings list --resource-group rg-financesap --name financesap-api

# Download logs
az webapp log download --resource-group rg-financesap --name financesap-api --log-file logs.zip
```
