# ✅ Azure Deployment Checklist

Use this checklist to ensure a smooth deployment to Azure.

## 📋 Pre-Deployment

### Local Verification
- [ ] All tests passing (`dotnet test`)
- [ ] Application builds successfully (`dotnet build`)
- [ ] API runs locally (`dotnet run --project FinanceSap.Api`)
- [ ] Scalar documentation accessible at `/scalar/v1`
- [ ] All endpoints tested locally

### Code Repository
- [ ] Code committed to Git
- [ ] Repository pushed to GitHub
- [ ] `.github/workflows/main_financesap.yml` file present
- [ ] `database-schema.sql` file present
- [ ] Documentation files present

---

## 🔧 Azure Resources Setup

### Resource Group
- [ ] Created resource group: `rg-financesap`
- [ ] Location selected (e.g., `eastus`)

### MySQL Database
- [ ] Created Azure Database for MySQL Flexible Server
- [ ] Server name: `financesap-mysql` (or your choice)
- [ ] Admin username configured
- [ ] Admin password saved securely
- [ ] Database created: `financesap_db`
- [ ] Firewall rule: Allow Azure Services (0.0.0.0)
- [ ] Firewall rule: Allow your IP (for management)

### App Service
- [ ] Created App Service Plan: `asp-financesap`
- [ ] Plan tier: B1 or higher
- [ ] Created Web App: `financesap-api` (or your choice)
- [ ] Runtime: .NET 9.0
- [ ] Platform: Linux

---

## 💾 Database Setup

### Schema Deployment
- [ ] Connected to Azure MySQL server
- [ ] Database `financesap_db` selected
- [ ] Executed `database-schema.sql` script
- [ ] Verified all tables created:
  - [ ] `customers`
  - [ ] `accounts`
  - [ ] `loan_applications`
  - [ ] `loans`
  - [ ] `AspNetUsers`
  - [ ] `AspNetRoles`
  - [ ] Other Identity tables
  - [ ] `__EFMigrationsHistory`

### Verification Queries
```sql
-- Run these to verify
SHOW TABLES;
SELECT * FROM __EFMigrationsHistory;
DESCRIBE customers;
DESCRIBE loans;
```

---

## 🔐 Environment Variables

### Connection String
- [ ] Built connection string with format:
  ```
  Server=<server>.mysql.database.azure.com;
  Database=financesap_db;
  User=<user>;
  Password=<password>;
  SslMode=Required;
  ```
- [ ] Set `MYSQL_CONNECTION_STRING` in Azure Web App
- [ ] Verified connection string (no extra spaces/line breaks)

### JWT Configuration
- [ ] Generated secure JWT key (32+ characters)
- [ ] Set `Jwt__Key` in Azure Web App
- [ ] Set `Jwt__Issuer` = "FinanceSap"
- [ ] Set `Jwt__Audience` = "FinanceSap"

### Environment
- [ ] Set `ASPNETCORE_ENVIRONMENT` = "Production"

### Verification
```bash
# List all settings
az webapp config appsettings list \
  --resource-group rg-financesap \
  --name financesap-api
```

---

## 🔒 Security Configuration

### Web App Security
- [ ] HTTPS only enabled
- [ ] Managed Identity assigned (optional)
- [ ] CORS configured (if needed)
- [ ] Diagnostic logging enabled

### Database Security
- [ ] SSL/TLS enforced
- [ ] Firewall rules configured
- [ ] Admin password is strong
- [ ] Connection string uses `SslMode=Required`

---

## 🐙 GitHub Configuration

### Repository Secrets
- [ ] Downloaded publish profile from Azure Portal:
  ```bash
  az webapp deployment list-publishing-profiles \
    --resource-group rg-financesap \
    --name financesap-api \
    --xml
  ```
- [ ] Added secret to GitHub:
  - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
  - Value: Full XML content
- [ ] Secret verified in GitHub Settings → Secrets

### Workflow File
- [ ] File exists: `.github/workflows/main_financesap.yml`
- [ ] `AZURE_WEBAPP_NAME` matches your Web App name
- [ ] Workflow syntax is valid

---

## 🚀 Deployment

### Manual Deployment (Optional First Time)
- [ ] Built application locally
- [ ] Published to `./publish` folder
- [ ] Created zip file
- [ ] Deployed via Azure CLI or Portal

### Automatic Deployment
- [ ] Pushed code to `main` branch
- [ ] GitHub Actions workflow triggered
- [ ] Build step completed successfully
- [ ] Tests passed
- [ ] Deployment completed
- [ ] No errors in GitHub Actions logs

---

## ✅ Post-Deployment Verification

### Application Status
- [ ] Web App status is "Running"
  ```bash
  az webapp show \
    --resource-group rg-financesap \
    --name financesap-api \
    --query state
  ```

### API Endpoints
- [ ] Base URL accessible: `https://<app-name>.azurewebsites.net`
- [ ] Scalar documentation: `https://<app-name>.azurewebsites.net/scalar/v1`
- [ ] Test endpoint responds correctly

### Test API Calls
```bash
# Get app URL
APP_URL="https://financesap-api.azurewebsites.net"

# Test Scalar
curl $APP_URL/scalar/v1

# Test API endpoint (should return 404 for non-existent customer)
curl -X POST $APP_URL/api/loans \
  -H "Content-Type: application/json" \
  -d '{"customerId":"00000000-0000-0000-0000-000000000000","amount":10000,"installments":12,"annualRate":0.12}'
```

### Logs Review
- [ ] Application logs show no errors
  ```bash
  az webapp log tail \
    --resource-group rg-financesap \
    --name financesap-api
  ```
- [ ] Database connections successful
- [ ] No authentication errors

---

## 📊 Monitoring Setup (Optional but Recommended)

### Application Insights
- [ ] Application Insights resource created
- [ ] Instrumentation key configured
- [ ] Telemetry data flowing

### Alerts
- [ ] HTTP 5xx errors alert configured
- [ ] High response time alert configured
- [ ] Database connection failure alert configured

### Diagnostic Logs
- [ ] Application logging enabled
- [ ] Web server logging enabled
- [ ] Detailed error messages enabled
- [ ] Failed request tracing enabled

---

## 🧪 Functional Testing

### Create Test Customer
```bash
curl -X POST $APP_URL/api/customers \
  -H "Content-Type: application/json" \
  -d '{"document":"12345678901","fullName":"Test Customer"}'
```

### Create Test Account
```bash
curl -X POST $APP_URL/api/accounts \
  -H "Content-Type: application/json" \
  -d '{"customerId":"<customer-id>"}'
```

### Request Test Loan
```bash
curl -X POST $APP_URL/api/loans \
  -H "Content-Type: application/json" \
  -d '{"customerId":"<customer-id>","amount":10000,"installments":12,"annualRate":0.12}'
```

### Verify Results
- [ ] Customer created successfully
- [ ] Account created successfully
- [ ] Loan request processed correctly
- [ ] Data persisted in database

---

## 📝 Documentation

### Update Documentation
- [ ] Update README with live URL
- [ ] Document any environment-specific configurations
- [ ] Update API documentation if needed
- [ ] Add deployment date and version

### Team Communication
- [ ] Notify team of deployment
- [ ] Share live URL
- [ ] Share Scalar documentation URL
- [ ] Provide access credentials (if needed)

---

## 🔄 Continuous Deployment

### Verify CI/CD Pipeline
- [ ] Push to `main` triggers deployment
- [ ] Tests run before deployment
- [ ] Failed tests block deployment
- [ ] Deployment notifications working

### Rollback Plan
- [ ] Know how to access previous deployment
- [ ] Understand rollback procedure
- [ ] Have database backup strategy

---

## 💰 Cost Management

### Monitor Costs
- [ ] Set up cost alerts
- [ ] Review Azure Cost Management
- [ ] Understand pricing tier costs
- [ ] Plan for scaling

### Optimization
- [ ] Review resource utilization
- [ ] Consider reserved instances for production
- [ ] Scale down non-production environments

---

## 🎉 Final Checks

- [ ] Application is live and accessible
- [ ] All endpoints working correctly
- [ ] Database operations successful
- [ ] Authentication working
- [ ] Logs are clean (no errors)
- [ ] Performance is acceptable
- [ ] Security headers present
- [ ] HTTPS enforced
- [ ] Documentation updated
- [ ] Team notified

---

## 📞 Support Information

### Azure Resources
- Resource Group: `rg-financesap`
- MySQL Server: `financesap-mysql.mysql.database.azure.com`
- Web App: `financesap-api.azurewebsites.net`
- Region: `eastus` (or your choice)

### Important URLs
- Azure Portal: https://portal.azure.com
- GitHub Repository: https://github.com/<your-username>/<your-repo>
- GitHub Actions: https://github.com/<your-username>/<your-repo>/actions
- Live API: https://financesap-api.azurewebsites.net
- API Docs: https://financesap-api.azurewebsites.net/scalar/v1

### Troubleshooting Resources
- [AZURE_DEPLOYMENT.md](./AZURE_DEPLOYMENT.md) - Full deployment guide
- [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) - Quick commands
- Azure Documentation: https://docs.microsoft.com/azure
- GitHub Actions Docs: https://docs.github.com/actions

---

## ✅ Deployment Complete!

**Date**: _______________
**Deployed By**: _______________
**Version**: _______________
**Status**: _______________

**Notes**:
_______________________________________
_______________________________________
_______________________________________

---

**🎊 Congratulations on your successful Azure deployment!**
