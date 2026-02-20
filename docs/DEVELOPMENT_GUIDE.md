# 💻 Development Guide

**Complete guide for developing plugins on NopCommerce 4.90 Marketplace Platform**

---

## 📋 Table of Contents

1. [Development Environment Setup](#development-environment-setup)
2. [Plugin Development Workflow](#plugin-development-workflow)
3. [Creating a New Plugin](#creating-a-new-plugin)
4. [Working with Core](#working-with-core)
5. [Database Operations](#database-operations)
6. [Event System](#event-system)
7. [Best Practices](#best-practices)
8. [Common Pitfalls](#common-pitfalls)
9. [Testing](#testing)
10. [Debugging](#debugging)

---

## 🛠️ Development Environment Setup

### Prerequisites

**Required:**
- ✅ .NET 9.0 SDK ([Download](https://dotnet.microsoft.com/download))
- ✅ MySQL 8.0+ ([Download](https://dev.mysql.com/downloads/))
- ✅ Git

**Recommended:**
- ✅ Visual Studio Code with C# Dev Kit
- ✅ MySQL Workbench or TablePlus
- ✅ Postman (API testing)

### Initial Setup
```bash
# 1. Clone repository
git clone <repository-url>
cd nopcommerce-4.90

# 2. Restore packages
dotnet restore

# 3. Configure database
nano src/Presentation/Nop.Web/App_Data/appsettings.json
# Update connection string

# 4. Build solution
dotnet build

# 5. Run NopCommerce
cd src/Presentation/Nop.Web
dotnet run

# Access: http://localhost:5000
```

### Database Setup
```sql
-- Create database
CREATE DATABASE nopcommerce490 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_general_ci;

-- Create user
CREATE USER 'nopuser'@'localhost' IDENTIFIED BY 'Nop123456!';
GRANT ALL PRIVILEGES ON nopcommerce490.* TO 'nopuser'@'localhost';
FLUSH PRIVILEGES;
```

---

## 🔄 Plugin Development Workflow

### Recommended Development Flow
```
1. Plan (Define requirements)
   ↓
2. Design (Entity, Service, Controller structure)
   ↓
3. Implement (Code in VS Code)
   ↓
4. Build & Deploy (Claude Code CLI)
   ↓
5. Test (Browser + Postman)
   ↓
6. Debug (VS Code breakpoints if needed)
   ↓
7. Iterate (Repeat 3-6)
   ↓
8. Document (README, comments)
   ↓
9. Production Deploy (Claude Code CLI)
```

### Tools for Each Phase

| Phase | Tool | Why |
|-------|------|-----|
| Code Writing | VS Code | IntelliSense, syntax highlight |
| Build & Deploy | Claude Code (Terminal) | Automation, speed |
| Testing | Browser + Postman | Manual testing |
| Debugging | VS Code | Breakpoints, step-through |
| Database | TablePlus/MySQL Workbench | Visual queries |

---

## 🆕 Creating a New Plugin

### Step-by-Step Guide

#### 1. Create Project Structure
```bash
cd src/Plugins

# Create plugin directory
mkdir -p Nop.Plugin.YourGroup.YourPlugin/{Controllers,Models,Services,Infrastructure,Views,Domain,Data}

cd Nop.Plugin.YourGroup.YourPlugin
```

#### 2. Create .csproj File
```bash
cat > Nop.Plugin.YourGroup.YourPlugin.csproj << 'CSPROJ'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Copyright>Copyright © Your Company</Copyright>
    <Company>Your Company</Company>
    <Authors>Your Name</Authors>
    <OutputPath>..\..\Presentation\Nop.Web\Plugins\YourGroup.YourPlugin</OutputPath>
    <OutDir>$(OutputPath)</OutDir>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Presentation\Nop.Web.Framework\Nop.Web.Framework.csproj" />
    <ProjectReference Include="..\..\Libraries\Nop.Data\Nop.Data.csproj" />
    <ProjectReference Include="..\Nop.Plugin.Marketplace.Core\Nop.Plugin.Marketplace.Core.csproj" />
    <ClearPluginAssemblies Include="$(MSBuildProjectDirectory)\..\..\Build\ClearPluginAssemblies.proj" />
  </ItemGroup>

  <ItemGroup>
    <None Remove="plugin.json" />
    <Content Include="plugin.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="Views\**\*.*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <Target Name="NopTarget" AfterTargets="Build">
    <MSBuild Projects="@(ClearPluginAssemblies)" Properties="PluginPath=$(MSBuildProjectDirectory)\$(OutDir)" Targets="NopClear" />
  </Target>
</Project>
CSPROJ
```

#### 3. Create plugin.json
```bash
cat > plugin.json << 'JSON'
{
  "Group": "YourGroup",
  "FriendlyName": "Your Plugin Name",
  "SystemName": "YourGroup.YourPlugin",
  "Version": "1.0.0",
  "SupportedVersions": [ "4.90" ],
  "Author": "Your Company",
  "DisplayOrder": 1,
  "FileName": "Nop.Plugin.YourGroup.YourPlugin.dll",
  "Description": "Your plugin description",
  "LimitedToStores": [],
  "LimitedToCustomerRoles": [],
  "DependsOnSystemNames": [ "Marketplace.Core" ]
}
JSON
```

#### 4. Create Main Plugin Class
```bash
cat > YourPlugin.cs << 'CS'
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.YourGroup.YourPlugin;

public class YourPlugin : BasePlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;

    public YourPlugin(
        IWebHelper webHelper,
        ISettingService settingService,
        ILocalizationService localizationService)
    {
        _webHelper = webHelper;
        _settingService = settingService;
        _localizationService = localizationService;
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/YourPlugin/Configure";
    }

    public override async Task InstallAsync()
    {
        // Install settings
        var settings = new YourPluginSettings
        {
            Enabled = true
        };
        await _settingService.SaveSettingAsync(settings);

        // Install localization
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.YourGroup.YourPlugin.Fields.Enabled"] = "Enabled"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        // Delete settings
        await _settingService.DeleteSettingAsync<YourPluginSettings>();
        
        // Delete localization
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.YourGroup.YourPlugin");

        await base.UninstallAsync();
    }
}
CS
```

#### 5. Create Settings Class
```bash
cat > YourPluginSettings.cs << 'CS'
using Nop.Core.Configuration;

namespace Nop.Plugin.YourGroup.YourPlugin;

public class YourPluginSettings : ISettings
{
    public bool Enabled { get; set; }
}
CS
```

#### 6. Create Infrastructure/NopStartup.cs
```bash
mkdir -p Infrastructure

cat > Infrastructure/NopStartup.cs << 'CS'
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.YourGroup.YourPlugin.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register services here
        // services.AddScoped<IYourService, YourService>();
    }

    public void Configure(IApplicationBuilder application)
    {
        // Add middleware if needed
    }

    public int Order => 3000;
}
CS
```

#### 7. Create Controller
```bash
mkdir -p Controllers

cat > Controllers/YourPluginController.cs << 'CS'
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.YourGroup.YourPlugin.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class YourPluginController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;

    public YourPluginController(
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService)
    {
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Configure()
    {
        var settings = await _settingService.LoadSettingAsync<YourPluginSettings>();
        
        var model = new ConfigurationModel
        {
            Enabled = settings.Enabled
        };

        return View("~/Plugins/YourGroup.YourPlugin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var settings = await _settingService.LoadSettingAsync<YourPluginSettings>();
        settings.Enabled = model.Enabled;
        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }
}
CS
```

#### 8. Create Model
```bash
mkdir -p Models

cat > Models/ConfigurationModel.cs << 'CS'
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.YourGroup.YourPlugin.Models;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.YourGroup.YourPlugin.Fields.Enabled")]
    public bool Enabled { get; set; }
}
CS
```

#### 9. Create Views
```bash
mkdir -p Views

cat > Views/_ViewImports.cshtml << 'CSHTML'
@inherits Nop.Web.Framework.Mvc.Razor.NopRazorPage<TModel>
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, Nop.Web.Framework

@using Microsoft.AspNetCore.Mvc.ViewFeatures
@using Nop.Web.Framework.UI
@using Nop.Web.Framework.Extensions
@using System.Text.Encodings.Web
CSHTML

cat > Views/Configure.cshtml << 'CSHTML'
@model ConfigurationModel

@{
    Layout = "_ConfigurePlugin";
    ViewBag.PageTitle = "Configure Your Plugin";
}

<form asp-controller="YourPlugin" asp-action="Configure" asp-area="Admin" method="post">
    <div class="cards-group">
        <div class="card card-default">
            <div class="card-header">
                <div class="card-title">
                    <i class="fas fa-cogs"></i>
                    Configuration
                </div>
            </div>
            <div class="card-body">
                <div class="form-group row">
                    <div class="col-md-3">
                        <nop-label asp-for="Enabled" />
                    </div>
                    <div class="col-md-9">
                        <nop-editor asp-for="Enabled" />
                        <span asp-validation-for="Enabled"></span>
                    </div>
                </div>
            </div>
            <div class="card-footer">
                <button type="submit" name="save" class="btn btn-primary">
                    <i class="far fa-save"></i>
                    Save
                </button>
            </div>
        </div>
    </div>
</form>
CSHTML
```

#### 10. Build & Deploy
```bash
# Build
dotnet build Nop.Plugin.YourGroup.YourPlugin.csproj

# Files are automatically copied to:
# src/Presentation/Nop.Web/Plugins/YourGroup.YourPlugin/

# Clear cache
rm -f ../../Presentation/Nop.Web/App_Data/plugins.json

# Restart NopCommerce
cd ../../Presentation/Nop.Web
dotnet run
```

---

## 🔷 Working with Core

### Using Core Entities
```csharp
using Nop.Plugin.Marketplace.Core.Domain;

public class YourService
{
    private readonly IRepository<OrderCommission> _orderCommissionRepository;
    private readonly IRepository<VendorCurrentAccount> _vendorAccountRepository;

    public YourService(
        IRepository<OrderCommission> orderCommissionRepository,
        IRepository<VendorCurrentAccount> vendorAccountRepository)
    {
        _orderCommissionRepository = orderCommissionRepository;
        _vendorAccountRepository = vendorAccountRepository;
    }

    public async Task<decimal> GetVendorCommissionAsync(int vendorId)
    {
        var total = await _orderCommissionRepository.Table
            .Where(oc => oc.VendorId == vendorId)
            .SumAsync(oc => oc.CommissionAmount);
            
        return total;
    }
}
```

### Don't Create Duplicate Entities
```csharp
// ❌ WRONG - Creating duplicate entity
public class OrderCommission : BaseEntity
{
    public int OrderId { get; set; }
    // ...
}

// ✅ CORRECT - Use Core entity
using Nop.Plugin.Marketplace.Core.Domain;
// Use OrderCommission from Core
```

---

## 🗄️ Database Operations

### Adding New Tables (In Core Plugin Only!)

**⚠️ IMPORTANT:** New marketplace tables should be added to Core plugin, not individual plugins.

**If you need a new table:**

1. Add entity to `Core/Domain/Entities.cs`
2. Add table creation to `Core/Data/InstallationData.cs`
3. Rebuild Core
4. Uninstall/Reinstall Core (only in development!)

**MySQL Table Creation Template:**
```sql
CREATE TABLE IF NOT EXISTS `MarketplaceYourTable` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(200) NOT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceYourTable_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### Querying Data
```csharp
// Simple query
var commissions = await _repository.Table
    .Where(c => c.VendorId == vendorId)
    .ToListAsync();

// With join
var result = await (
    from oc in _orderCommissionRepository.Table
    join v in _vendorRepository.Table on oc.VendorId equals v.Id
    where oc.CreatedOnUtc >= startDate
    select new
    {
        oc.OrderId,
        v.Name,
        oc.CommissionAmount
    }
).ToListAsync();

// Raw SQL (when needed)
var sql = "SELECT * FROM `MarketplaceOrderCommission` WHERE VendorId = @VendorId";
var data = await _dataProvider.QueryAsync<OrderCommission>(sql, 
    new DataParameter("@VendorId", vendorId));
```

### MySQL Syntax Notes
```sql
-- ✅ CORRECT MySQL Syntax
SELECT IFNULL(SUM(Amount), 0) FROM `Table`
SELECT * FROM `Table` ORDER BY Id DESC LIMIT 100
CREATE TABLE `Table` (...) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
`IsActive` tinyint(1) NOT NULL DEFAULT 0
`Id` int NOT NULL AUTO_INCREMENT

-- ❌ WRONG SQL Server Syntax (Don't use these!)
SELECT ISNULL(SUM(Amount), 0) FROM [Table]
SELECT TOP 100 * FROM [Table] ORDER BY Id DESC
CREATE TABLE [Table] (...)
[IsActive] [bit] NOT NULL DEFAULT 0
[Id] [int] IDENTITY(1,1) NOT NULL
```

---

## 🔄 Event System

### Creating Event Consumers
```csharp
using Nop.Services.Events;

namespace Nop.Plugin.YourGroup.YourPlugin.Infrastructure;

public class YourEventConsumer : 
    IConsumer<EntityInsertedEvent<Order>>,
    IConsumer<EntityUpdatedEvent<Order>>
{
    private readonly IYourService _yourService;

    public YourEventConsumer(IYourService yourService)
    {
        _yourService = yourService;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<Order> eventMessage)
    {
        var order = eventMessage.Entity;
        await _yourService.ProcessNewOrderAsync(order);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
    {
        var order = eventMessage.Entity;
        await _yourService.ProcessUpdatedOrderAsync(order);
    }
}
```

### Common Events
```csharp
// Order events
EntityInsertedEvent<Order>
EntityUpdatedEvent<Order>
OrderPaidEvent
OrderPlacedEvent
OrderRefundedEvent

// Customer events
CustomerRegisteredEvent
CustomerLoggedinEvent

// Product events
EntityInsertedEvent<Product>
EntityUpdatedEvent<Product>
```

### Reflection-Based Event Handling (Avoid Circular Dependencies)
```csharp
public class GenericEventConsumer : IConsumer<EntityInsertedEvent<BaseEntity>>
{
    public async Task HandleEventAsync(EntityInsertedEvent<BaseEntity> eventMessage)
    {
        var entity = eventMessage.Entity;
        var entityType = entity.GetType();
        
        // Check by type name (no reference to other plugin!)
        if (entityType.Name == "OrderCommission" && 
            entityType.FullName.Contains("Marketplace.Commission"))
        {
            // Use reflection to read properties
            var orderIdProp = entityType.GetProperty("OrderId");
            var orderId = (int)orderIdProp.GetValue(entity);
            
            await ProcessCommissionAsync(orderId);
        }
    }
}
```

---

## ✅ Best Practices

### 1. Dependency Injection
```csharp
// ✅ CORRECT - Register in NopStartup.cs
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<IYourService, YourService>();
}

// ❌ WRONG - Don't use Autofac directly
// ❌ WRONG - Don't use DependencyRegistrar.cs (obsolete in 4.90)
```

### 2. Async/Await
```csharp
// ✅ CORRECT - Async all the way
public async Task<Product> GetProductAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ WRONG - Blocking
public Product GetProduct(int id)
{
    return _repository.GetByIdAsync(id).Result;  // Don't do this!
}
```

### 3. Controller Actions
```csharp
// ✅ CORRECT
[Area(AreaNames.ADMIN)]  // Use AreaNames.ADMIN (uppercase)
[AuthorizeAdmin]  // Sufficient for permission check
public class YourController : BasePluginController
{
    public async Task<IActionResult> Configure()
    {
        // No manual permission check needed
        // ...
    }
}

// ❌ WRONG
[Area("Admin")]  // Don't use string literal
public class YourController : Controller  // Don't inherit from Controller directly
{
    public async Task<IActionResult> Configure()
    {
        // Manual permission check not needed
        if (!await _permissionService.AuthorizeAsync("ManagePlugins"))
            return AccessDeniedView();
        // ...
    }
}
```

### 4. AJAX Requests
```javascript
// ✅ CORRECT
$.post('@Url.Action("Action", "Controller", new { area = "Admin" })', {
    data: value,
    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
}, function(response) {
    // Handle response
});

// ❌ WRONG - Missing area
$.post('@Url.Action("Action", "Controller")', { ... })

// ❌ WRONG - Missing anti-forgery token
$.post(url, { data: value })
```

### 5. Localization
```csharp
// ✅ CORRECT - Use localization service
var message = await _localizationService.GetResourceAsync(
    "Plugins.YourGroup.YourPlugin.Fields.Name");

// ❌ WRONG - Hardcoded strings
var message = "Name";
```

### 6. File-Scoped Namespaces
```csharp
// ✅ CORRECT (C# 10+)
namespace Nop.Plugin.YourGroup.YourPlugin;

public class YourClass
{
}

// ❌ OLD STYLE (Still works but not preferred)
namespace Nop.Plugin.YourGroup.YourPlugin
{
    public class YourClass
    {
    }
}
```

### 7. Record Types for Models
```csharp
// ✅ CORRECT - Use record for models
public record ConfigurationModel : BaseNopModel
{
    public string Name { get; set; }
}

// ❌ OLD STYLE - Class (still works but record is preferred)
public class ConfigurationModel : BaseNopModel
{
    public string Name { get; set; }
}
```

---

## ⚠️ Common Pitfalls

### 1. Case Sensitivity in MySQL
```sql
-- ❌ WRONG - Case sensitive on macOS/Linux
SELECT * FROM MarketplaceOrderCommission

-- ✅ CORRECT - Use backticks
SELECT * FROM `MarketplaceOrderCommission`
```

### 2. Plugin Cache
```bash
# Always clear cache after DLL changes
rm -f src/Presentation/Nop.Web/App_Data/plugins.json

# And restart NopCommerce
```

### 3. Views Not Updating
```bash
# Make sure Views are copied
# Check .csproj has this:
<Content Include="Views\**\*.*">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</Content>

# Or manually copy:
cp -r Views src/Presentation/Nop.Web/Plugins/YourGroup.YourPlugin/
```

### 4. Circular Dependencies
```csharp
// ❌ WRONG
// Plugin A references Plugin B
// Plugin B references Plugin A
// = Circular dependency!

// ✅ CORRECT
// Both plugins reference Core
// Use events for communication
```

### 5. Uninstall Deletes Data
```csharp
// ❌ WRONG - Deletes tables on uninstall
public override async Task UninstallAsync()
{
    await _dataProvider.ExecuteNonQueryAsync(DropTablesScript);
    await base.UninstallAsync();
}

// ✅ CORRECT - Only delete settings
public override async Task UninstallAsync()
{
    await _settingService.DeleteSettingAsync<YourSettings>();
    await _localizationService.DeleteLocaleResourcesAsync("Plugins.YourGroup.YourPlugin");
    await base.UninstallAsync();
}
```

---

## 🧪 Testing

### Manual Testing Checklist

- [ ] Plugin installs without errors
- [ ] Configure page opens
- [ ] Settings save and persist
- [ ] All pages/actions work
- [ ] AJAX requests succeed
- [ ] No console errors (F12)
- [ ] Localization works
- [ ] Uninstall works
- [ ] Reinstall works (data preserved if Core-based)

### Test with Different Roles

- [ ] Administrator
- [ ] Vendor
- [ ] Registered customer
- [ ] Guest

### Browser Testing

- [ ] Chrome
- [ ] Firefox
- [ ] Safari
- [ ] Edge

---

## 🐛 Debugging

### Enable Detailed Errors
```json
// appsettings.json
{
  "Hosting": {
    "UseDetailedErrors": true
  }
}
```

### Check Logs
```bash
tail -f src/Presentation/Nop.Web/App_Data/Logs/nopcommerce-*.txt
```

### VS Code Debugging

1. Open plugin folder in VS Code
2. Set breakpoint (click left of line number)
3. Press F5 or Run → Start Debugging
4. Attach to `dotnet` process
5. Trigger action in browser
6. Breakpoint hits → Step through code

### Common Debug Points
```csharp
// Controller action
public async Task<IActionResult> Configure()
{
    // Set breakpoint here
    var settings = await _settingService.LoadSettingAsync<YourSettings>();
    // And here
    return View("~/Path/To/View.cshtml", model);
}

// Service method
public async Task ProcessAsync()
{
    // Set breakpoint here
    var data = await _repository.GetAllAsync();
    // And here
    await DoSomethingAsync(data);
}
```

---

## 📚 Additional Resources

- [NopCommerce Official Docs](https://docs.nopcommerce.com)
- [Plugin Development](https://docs.nopcommerce.com/en/developer/plugins/)
- [Core README](../src/Plugins/Nop.Plugin.Marketplace.Core/README.md)
- [Architecture Guide](ARCHITECTURE.md)
- [Database Schema](DATABASE.md)

---

**Last Updated:** January 13, 2026  
**NopCommerce Version:** 4.90  
**.NET Version:** 9.0
