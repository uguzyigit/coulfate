# 🔧 Troubleshooting Guide

**Common issues and solutions for NopCommerce 4.90 Marketplace Platform**

---

## 📋 Table of Contents

1. [Plugin Issues](#plugin-issues)
2. [Database Issues](#database-issues)
3. [Build & Deployment Issues](#build--deployment-issues)
4. [Runtime Errors](#runtime-errors)
5. [Performance Issues](#performance-issues)
6. [Payment Gateway Issues](#payment-gateway-issues)

---

## 🔌 Plugin Issues

### Plugin Not Appearing in Admin Panel

**Symptoms:**
- Plugin installed but not visible in Local Plugins list

**Solutions:**

1. **Check plugin.json exists:**
```bash
ls -la src/Presentation/Nop.Web/Plugins/YourGroup.YourPlugin/plugin.json
```

2. **Check DLL exists:**
```bash
ls -la src/Presentation/Nop.Web/Plugins/YourGroup.YourPlugin/*.dll
```

3. **Clear plugin cache:**
```bash
rm -f src/Presentation/Nop.Web/App_Data/plugins.json
```

4. **Restart application**

5. **Check plugin.json format:**
```json
{
  "SystemName": "YourGroup.YourPlugin",
  "FileName": "Nop.Plugin.YourGroup.YourPlugin.dll"
}
```

---

### Cannot Uninstall Core Plugin

**Error:** "These plugins depend on this: Commission, VendorExtensions, İyzico"

**This is CORRECT behavior!** ✅

Core cannot be uninstalled while other plugins depend on it.

**Solution:**
1. Uninstall dependent plugins first (İyzico, Commission, VendorExtensions)
2. Then uninstall Core

---

### Plugin Uninstall Deletes Data

**Old Behavior (Before Core Migration):**
- Uninstalling Commission deleted all commission records ❌
- Uninstalling VendorExtensions deleted all transactions ❌

**New Behavior (After Core Migration):**
- Uninstalling Commission preserves commission records ✅
- Uninstalling VendorExtensions preserves transactions ✅
- Data stays in Core plugin

**If you still lose data:**
Check plugin's UninstallAsync() - it should NOT have DROP TABLE commands:
```csharp
// ❌ WRONG
public override async Task UninstallAsync()
{
    await _dataProvider.ExecuteNonQueryAsync(DropTablesScript);
    await base.UninstallAsync();
}

// ✅ CORRECT
public override async Task UninstallAsync()
{
    await _settingService.DeleteSettingAsync<YourSettings>();
    await _localizationService.DeleteLocaleResourcesAsync("...");
    await base.UninstallAsync();
}
```

---

### DI Not Working - "Cannot resolve parameter"

**Error:**
```
DependencyResolutionException: Unable to resolve service for type 'IYourService'
```

**Solutions:**

1. **Check NopStartup.cs exists:**
```bash
ls -la src/Plugins/Nop.Plugin.YourGroup.YourPlugin/Infrastructure/NopStartup.cs
```

2. **Check service registration:**
```csharp
// Infrastructure/NopStartup.cs
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<IYourService, YourService>();  // ← This line must exist
}
```

3. **Rebuild plugin:**
```bash
dotnet build src/Plugins/Nop.Plugin.YourGroup.YourPlugin/
```

4. **Clear cache and restart**

---

### Widget Not Loading

**Symptoms:**
- ViewComponent not executing
- JavaScript not injecting

**Solutions:**

1. **Check widget zones:**
```csharp
public Task<IList<string>> GetWidgetZonesAsync()
{
    return Task.FromResult<IList<string>>(new List<string>
    {
        AdminWidgetZones.HeaderAfter  // ✅ For admin
        // NOT PublicWidgetZones.HeadHtmlTag for admin!
    });
}
```

2. **Check ViewComponent registration:**
```csharp
public Type GetWidgetViewComponent(string widgetZone)
{
    return typeof(YourViewComponent);  // ← Must return correct type
}
```

3. **Check ViewComponent view path:**
```csharp
return View("~/Plugins/YourGroup.YourPlugin/Views/Components/YourViewComponent/Default.cshtml");
```

---

## 🗄️ Database Issues

### Table Not Found After Plugin Install

**Error:**
```
Table 'nopcommerce490.marketplaceordercreatecommission' doesn't exist
```

**Solutions:**

1. **Check if InstallAsync executed:**
Add logging to plugin's InstallAsync:
```csharp
public override async Task InstallAsync()
{
    try
    {
        _logger.Information("Creating tables...");
        await _dataProvider.ExecuteNonQueryAsync(InstallationData.CreateTablesScript);
        _logger.Information("Tables created successfully");
    }
    catch (Exception ex)
    {
        _logger.Error($"Failed: {ex.Message}");
        throw;
    }
    
    await base.InstallAsync();
}
```

2. **Manual table creation:**
```bash
# Execute SQL manually in TablePlus or MySQL Workbench
# Copy SQL from Data/InstallationData.cs
```

3. **Check database connection:**
```bash
mysql -u nopuser -p'Nop123456!' nopcommerce490 -e "SHOW TABLES;"
```

---

### MySQL Case Sensitivity Error

**Error:**
```
Table 'nopcommerce490.marketplaceordercreatecommission' doesn't exist
(lowercase instead of MarketplaceOrderCommission)
```

**Solution:** Use backticks in queries:
```csharp
// ❌ WRONG
var sql = "SELECT * FROM MarketplaceOrderCommission";

// ✅ CORRECT
var sql = "SELECT * FROM `MarketplaceOrderCommission`";
```

**Find all queries missing backticks:**
```bash
grep -r "FROM Marketplace" --include="*.cs" | grep -v '`'
```

---

### ISNULL vs IFNULL Error

**Error:**
```
Incorrect parameter count in the call to native function 'ISNULL'
```

**Solution:** MySQL uses IFNULL, not ISNULL:
```sql
-- ❌ SQL Server
SELECT ISNULL(SUM(Amount), 0) FROM Table

-- ✅ MySQL
SELECT IFNULL(SUM(Amount), 0) FROM `Table`
```

---

### TOP vs LIMIT Error

**Error:**
```
You have an error in your SQL syntax near 'TOP 100'
```

**Solution:**
```sql
-- ❌ SQL Server
SELECT TOP 100 * FROM Table ORDER BY Id DESC

-- ✅ MySQL
SELECT * FROM `Table` ORDER BY Id DESC LIMIT 100
```

---

## 🔨 Build & Deployment Issues

### Build Fails - Project Not Found

**Error:**
```
error MSB4025: The project file could not be loaded
```

**Solutions:**

1. **Check .csproj file exists:**
```bash
ls -la src/Plugins/Nop.Plugin.YourGroup.YourPlugin/*.csproj
```

2. **Check .csproj is valid XML:**
```bash
xmllint --noout src/Plugins/Nop.Plugin.YourGroup.YourPlugin/*.csproj
```

3. **Restore packages:**
```bash
dotnet restore
```

---

### DLL Not Copied to Runtime

**Symptoms:**
- Build succeeds
- Plugin not updating in admin panel

**Solutions:**

1. **Manual copy:**
```bash
cp src/Plugins/Nop.Plugin.YourGroup.YourPlugin/obj/Debug/net9.0/*.dll \
   src/Presentation/Nop.Web/Plugins/YourGroup.YourPlugin/
```

2. **Check OutputPath in .csproj:**
```xml
<OutputPath>..\..\Presentation\Nop.Web\Plugins\YourGroup.YourPlugin</OutputPath>
```

3. **Clean and rebuild:**
```bash
dotnet clean
dotnet build
```

---

### Views Not Updating

**Symptoms:**
- View changes not reflecting
- Old HTML still showing

**Solutions:**

1. **Check CopyToOutputDirectory:**
```xml
<Content Include="Views\**\*.*">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</Content>
```

2. **Manual copy:**
```bash
cp -r src/Plugins/Nop.Plugin.YourGroup.YourPlugin/Views \
   src/Presentation/Nop.Web/Plugins/YourGroup.YourPlugin/
```

3. **Clear browser cache (Ctrl+Shift+R)**

---

## ⚡ Runtime Errors

### AJAX Requests Failing (404)

**Error:** ERR_CONNECTION_REFUSED or 404

**Solutions:**

1. **Include area parameter:**
```javascript
// ❌ WRONG
$.post('@Url.Action("Action", "Controller")', { ... })

// ✅ CORRECT
$.post('@Url.Action("Action", "Controller", new { area = "Admin" })', { ... })
```

2. **Include anti-forgery token:**
```javascript
$.post(url, {
    data: value,
    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
})
```

3. **Check RouteProvider:**
```csharp
endpointRouteBuilder.MapControllerRoute(
    name: "Plugin.YourGroup.YourPlugin.Action",
    pattern: "Admin/YourController/YourAction",
    defaults: new { controller = "YourController", action = "YourAction", area = "Admin" });
```

---

### ViewComponent Model Error

**Error:**
```
RuntimeBinderException: 'object' does not contain a definition for 'PropertyName'
```

**Solution:** Use ViewData, not anonymous types:
```csharp
// ❌ WRONG
var model = new { Property1 = value1 };
return View("~/Path/View.cshtml", model);

// ✅ CORRECT
ViewData["Property1"] = value1;
return View("~/Path/View.cshtml");
```

In view:
```cshtml
@{
    var property1 = ViewData["Property1"] as YourType;
}
```

---

### Permission Denied Error

**Error:** "You do not have permission to perform the selected operation"

**Solution:** Remove manual permission check:
```csharp
// ❌ WRONG
public async Task<IActionResult> Configure()
{
    if (!await _permissionService.AuthorizeAsync("ManagePlugins"))
        return AccessDeniedView();
    // ...
}

// ✅ CORRECT
[AuthorizeAdmin]  // ← This is sufficient
public class YourController : BasePluginController
{
    public async Task<IActionResult> Configure()
    {
        // No manual check needed
    }
}
```

---

## 🐌 Performance Issues

### Slow Commission Reports Page

**Symptoms:**
- Page takes > 5 seconds to load
- Database CPU high

**Solutions:**

1. **Add indexes:**
```sql
CREATE INDEX IX_OrderCommission_VendorId_Created 
ON `MarketplaceOrderCommission`(`VendorId`, `CreatedOnUtc`);
```

2. **Optimize query:**
```csharp
// ❌ SLOW - Loading all data
var all = await _repository.Table.ToListAsync();
var filtered = all.Where(x => x.VendorId == vendorId).ToList();

// ✅ FAST - Filter in database
var filtered = await _repository.Table
    .Where(x => x.VendorId == vendorId)
    .ToListAsync();
```

3. **Add pagination:**
```csharp
var pageIndex = 0;
var pageSize = 100;
var query = _repository.Table
    .Where(x => x.VendorId == vendorId)
    .OrderByDescending(x => x.CreatedOnUtc)
    .Skip(pageIndex * pageSize)
    .Take(pageSize);
```

---

### High Memory Usage

**Symptoms:**
- Application uses > 2GB RAM
- Server slow

**Solutions:**

1. **Enable response compression:**
```csharp
services.AddResponseCompression();
```

2. **Use caching:**
```csharp
var key = $"commission.vendor.{vendorId}";
var data = await _cacheManager.GetAsync(key, async () => 
{
    return await _repository.GetDataAsync(vendorId);
});
```

3. **Dispose resources:**
```csharp
using var httpClient = new HttpClient();
// Use httpClient
// Automatically disposed
```

---

## 💳 Payment Gateway Issues

### İyzico 3DS Payment Fails

**Error:** Payment status stuck at "Pending"

**Solutions:**

1. **Check callback URL:**
```csharp
var callbackUrl = $"{_webHelper.GetStoreLocation()}PaymentIyzico/Callback";
// Must be accessible from internet
```

2. **Check API credentials:**
```csharp
var settings = await _settingService.LoadSettingAsync<IyzicoPaymentSettings>();
// ApiKey and SecretKey must be correct
```

3. **Check logs:**
```bash
tail -f src/Presentation/Nop.Web/App_Data/Logs/nopcommerce-*.txt | grep -i iyzico
```

4. **Test mode:**
Enable test mode in plugin settings and use test cards

---

## 📞 Getting Help

If none of these solutions work:

1. **Check logs:**
```bash
tail -100 src/Presentation/Nop.Web/App_Data/Logs/nopcommerce-*.txt
```

2. **Enable detailed errors:**
```json
// appsettings.json
{
  "Hosting": {
    "UseDetailedErrors": true
  }
}
```

3. **Check NopCommerce forums:**
https://www.nopcommerce.com/boards

4. **Contact support:**
support@yourcompany.com

---

**Last Updated:** January 13, 2026
