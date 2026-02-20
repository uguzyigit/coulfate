# 🚀 Deployment Guide

**Production deployment guide for NopCommerce 4.90 Marketplace Platform**

---

## 📋 Pre-Deployment Checklist

### Code Quality
- [ ] All tests passing
- [ ] No console errors in browser
- [ ] No warnings in build output
- [ ] Code reviewed
- [ ] Documentation updated

### Database
- [ ] Database backup taken
- [ ] Migration scripts tested
- [ ] Indexes optimized
- [ ] Connection strings configured

### Configuration
- [ ] appsettings.json configured for production
- [ ] HTTPS enforced
- [ ] Error pages configured
- [ ] Logging configured

### Security
- [ ] Strong passwords set
- [ ] Database credentials secured
- [ ] API keys in environment variables
- [ ] CORS configured properly

---

## 🗄️ Database Deployment

### 1. Backup Existing Database
```bash
# Full backup
mysqldump -u nopuser -p nopcommerce490 > backup_$(date +%Y%m%d_%H%M).sql

# Marketplace tables only
mysqldump -u nopuser -p nopcommerce490 \
    MarketplaceVendor \
    MarketplaceVendorProduct \
    MarketplaceVendorOrderLine \
    MarketplaceVendorBalance \
    MarketplaceVendorShippingAccount \
    MarketplaceVendorPayout \
    MarketplaceCategoryCommission \
    MarketplaceProductCommission \
    MarketplaceOrderCommission \
    MarketplaceVendorSettings \
    MarketplaceVendorCurrentAccount \
    MarketplaceVendorTransaction \
    > marketplace_backup_$(date +%Y%m%d_%H%M).sql
```

### 2. Deploy Core Plugin First
```bash
# On production server
cd /var/www/nopcommerce

# 1. Install Core plugin via admin panel
# Admin → Configuration → Local Plugins → Marketplace.Core → Install

# 2. Verify tables created
mysql -u nopuser -p nopcommerce490 -e "SHOW TABLES LIKE 'Marketplace%';"
```

### 3. Deploy Other Plugins

**Installation Order:**
1. Marketplace.Core ✅
2. Marketplace.Commission
3. Marketplace.VendorExtensions  
4. Payments.Iyzico
5. Accounting.Parasut

---

## 📦 Application Deployment

### Method 1: Manual Deployment (Recommended for First Time)
```bash
# On development machine
cd /Users/piqmacbook/Projects/nopcommerce-4.90

# 1. Build in Release mode
dotnet build -c Release

# 2. Publish
dotnet publish src/Presentation/Nop.Web/Nop.Web.csproj \
    -c Release \
    -o /tmp/nopcommerce-publish

# 3. Create deployment package
cd /tmp/nopcommerce-publish
tar -czf nopcommerce-$(date +%Y%m%d).tar.gz .

# 4. Transfer to server
scp nopcommerce-20260113.tar.gz user@server:/var/www/

# On production server
cd /var/www
tar -xzf nopcommerce-20260113.tar.gz -C nopcommerce/

# 5. Set permissions
chown -R www-data:www-data nopcommerce/
chmod -R 755 nopcommerce/

# 6. Restart web server
systemctl restart nginx  # or apache2
```

### Method 2: CI/CD Pipeline (GitHub Actions Example)
```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 9.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build -c Release --no-restore
    
    - name: Publish
      run: dotnet publish src/Presentation/Nop.Web/Nop.Web.csproj \
           -c Release -o ./publish
    
    - name: Deploy to server
      uses: easingthemes/ssh-deploy@main
      env:
        SSH_PRIVATE_KEY: ${{ secrets.SSH_PRIVATE_KEY }}
        REMOTE_HOST: ${{ secrets.REMOTE_HOST }}
        REMOTE_USER: ${{ secrets.REMOTE_USER }}
        TARGET: /var/www/nopcommerce
```

---

## ⚙️ Configuration

### appsettings.json (Production)
```json
{
  "ConnectionStrings": {
    "ConnectionString": "Server=production-db-server;Port=3306;Database=nopcommerce490;Uid=nopuser;Pwd=STRONG_PASSWORD_HERE;",
    "DataProvider": "mysql"
  },
  "Hosting": {
    "UseHttpClusterHttps": false,
    "UseHttpXForwardedProto": false,
    "ForwardedHttpHeader": "",
    "UseDetailedErrors": false
  },
  "CachingSettings": {
    "CacheType": "Redis",
    "RedisConnectionString": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  }
}
```

### Environment Variables
```bash
# /etc/environment or .env
NOPCOMMERCE_DB_PASSWORD=strong_password_here
IYZICO_API_KEY=your_api_key
IYZICO_SECRET_KEY=your_secret_key
PARASUT_CLIENT_ID=your_client_id
PARASUT_CLIENT_SECRET=your_client_secret
```

---

## 🌐 Web Server Configuration

### Nginx Configuration
```nginx
# /etc/nginx/sites-available/nopcommerce
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    client_max_body_size 100M;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Systemd Service
```ini
# /etc/systemd/system/nopcommerce.service
[Unit]
Description=NopCommerce Marketplace
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/nopcommerce
ExecStart=/usr/bin/dotnet /var/www/nopcommerce/Nop.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=nopcommerce
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```
```bash
# Enable and start service
systemctl enable nopcommerce
systemctl start nopcommerce
systemctl status nopcommerce
```

---

## 🔐 SSL Certificate (Let's Encrypt)
```bash
# Install certbot
apt-get update
apt-get install certbot python3-certbot-nginx

# Obtain certificate
certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Auto-renewal (runs twice daily)
certbot renew --dry-run
```

---

## 📊 Monitoring

### Application Logging
```csharp
// Configure in Startup
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddFile("/var/log/nopcommerce/app-{Date}.log");
});
```

### Health Checks
```bash
# Add to crontab
*/5 * * * * curl -f http://localhost:5000/health || systemctl restart nopcommerce
```

### Database Monitoring
```sql
-- Check table sizes
SELECT 
    TABLE_NAME,
    ROUND((DATA_LENGTH + INDEX_LENGTH) / 1024 / 1024, 2) as SizeMB
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = 'nopcommerce490'
ORDER BY (DATA_LENGTH + INDEX_LENGTH) DESC;

-- Check slow queries (enable slow query log)
-- /etc/mysql/mysql.conf.d/mysqld.cnf
slow_query_log = 1
slow_query_log_file = /var/log/mysql/slow-queries.log
long_query_time = 2
```

---

## 🔄 Rollback Procedure

### If Deployment Fails
```bash
# 1. Stop service
systemctl stop nopcommerce

# 2. Restore previous version
cd /var/www
rm -rf nopcommerce
tar -xzf backups/nopcommerce-previous.tar.gz -C nopcommerce/

# 3. Restore database (if needed)
mysql -u nopuser -p nopcommerce490 < backups/backup_before_deployment.sql

# 4. Restart service
systemctl start nopcommerce

# 5. Verify
curl -I http://localhost:5000
```

---

## ✅ Post-Deployment Verification

### 1. Smoke Tests
```bash
# Homepage loads
curl -I https://yourdomain.com

# Admin panel loads
curl -I https://yourdomain.com/Admin

# API health check
curl https://yourdomain.com/health
```

### 2. Admin Panel Checks

- [ ] Login works
- [ ] Plugins are installed
- [ ] Configuration pages load
- [ ] Reports generate
- [ ] No errors in logs

### 3. Functional Tests

- [ ] Customer can browse products
- [ ] Customer can add to cart
- [ ] Customer can checkout
- [ ] Payment processes (test mode)
- [ ] Order confirmation received
- [ ] Commission calculated
- [ ] Vendor transaction created

### 4. Performance Tests
```bash
# Load test with ab (Apache Bench)
ab -n 1000 -c 10 https://yourdomain.com/

# Response time should be < 500ms
```

---

## 📋 Deployment Checklist

### Pre-Deployment
- [ ] Code freeze announced
- [ ] Team notified
- [ ] Maintenance page prepared
- [ ] Database backup completed
- [ ] File system backup completed

### During Deployment
- [ ] Maintenance mode enabled
- [ ] Application stopped
- [ ] Files deployed
- [ ] Plugins installed
- [ ] Configuration updated
- [ ] Database migrated
- [ ] Application started
- [ ] Smoke tests passed

### Post-Deployment
- [ ] Maintenance mode disabled
- [ ] Monitoring active
- [ ] Logs checked
- [ ] Team notified (success/failure)
- [ ] Documentation updated

---

## 🆘 Emergency Contacts

- **DevOps Team:** devops@yourcompany.com
- **Database Admin:** dba@yourcompany.com
- **On-Call Developer:** +90 xxx xxx xxxx
- **Hosting Provider:** support@hostingprovider.com

---

**Last Updated:** January 13, 2026
