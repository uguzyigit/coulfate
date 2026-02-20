# 📋 Accounting.Parasut Plugin

**Paraşüt accounting system integration for NopCommerce 4.90**

---

## 📋 Overview

Accounting.Parasut provides automatic invoice generation and management through Paraşüt cloud accounting system.

**Version:** 1.0  
**Depends On:** None (Independent)  
**Status:** Production Ready

---

## ✨ Features

### Automatic Invoice Generation
- ✅ **Order-based invoicing** - Invoice created on order completion
- ✅ **Vendor-specific invoices** - Separate invoices per vendor
- ✅ **PDF generation** - Automatic PDF invoice creation
- ✅ **PDF storage** - Store invoices in NopCommerce

### Paraşüt Integration
- ✅ **Contact management** - Sync customers to Paraşüt
- ✅ **Product sync** - Sync products to Paraşüt
- ✅ **Sales invoice** - Create sales invoices
- ✅ **E-Archive** - E-Archive invoice support (if enabled)
- ✅ **E-Invoice** - E-Invoice support (if enabled)

### Invoice Management
- ✅ **Invoice listing** - View all generated invoices
- ✅ **PDF download** - Download invoice PDFs
- ✅ **Invoice status** - Track invoice status
- ✅ **Error handling** - Graceful error management

---

## 📦 Installation

### Prerequisites
- ✅ NopCommerce 4.90+
- ✅ Paraşüt account
- ✅ Paraşüt API credentials

### Getting Paraşüt Credentials

1. **Register with Paraşüt:**
   - Visit: https://www.parasut.com
   - Create account
   - Complete verification

2. **Create API Application:**
   - Login to Paraşüt
   - Navigate to Settings → Integrations → API
   - Create new application
   - Copy Client ID and Client Secret
   - Set redirect URI: `http://yourdomain.com/Admin/Parasut/Callback`

3. **Authorize Application:**
   - Complete OAuth flow
   - Get access token

### Installation Steps

1. **Build the plugin:**
```bash
cd src/Plugins/Nop.Plugin.Accounting.Parasut
dotnet build
```

2. **Install via admin panel:**
   - Admin → Configuration → Local Plugins
   - Find "Paraşüt Accounting"
   - Click "Install"
   - Restart when prompted

3. **Configure plugin:**
   - Admin → Configuration → Plugins
   - Find "Paraşüt"
   - Click "Configure"
   - Enter API credentials
   - Complete OAuth authorization

---

## 🔧 Configuration

### Plugin Settings

Navigate to: **Admin → Configuration → Plugins → Paraşüt → Configure**

**OAuth Settings:**

| Setting | Description | Example |
|---------|-------------|---------|
| Client ID | Paraşüt application client ID | `xxx-yyy-zzz` |
| Client Secret | Application secret | `abc123def456` |
| Company ID | Your Paraşüt company ID | `12345` |
| Access Token | OAuth access token | Auto-generated after authorization |
| Refresh Token | OAuth refresh token | Auto-generated |

**Invoice Settings:**

| Setting | Description | Default |
|---------|-------------|---------|
| Auto Generate Invoice | Create invoice on order completion | true |
| Include Tax | Include tax in invoice | true |
| Invoice Category | Default category for invoices | "Sales" |
| E-Archive Enabled | Enable e-Archive invoices | false |
| E-Invoice Enabled | Enable e-Invoice | false |

---

## 🔄 Invoice Generation Flow

### Automatic Flow
```
1. Order completed (PaymentStatus = Paid)
   ↓
2. OrderPaidEvent fired
   ↓
3. ParasutInvoiceEventConsumer handles event
   ↓
4. Check if customer exists in Paraşüt
   ↓ (if not exists)
5. Create contact in Paraşüt
   ↓
6. Check if products exist in Paraşüt
   ↓ (if not exists)
7. Create products in Paraşüt
   ↓
8. Create sales invoice in Paraşüt
   ↓
9. Get invoice PDF from Paraşüt
   ↓
10. Store PDF in NopCommerce
   ↓
11. Link invoice to order
```

### Manual Generation

Admin can manually generate invoice:

1. Navigate to order details
2. Click "Generate Paraşüt Invoice"
3. Invoice created and PDF downloaded
4. PDF stored and linked to order

---

## 💻 API Integration

### Contact (Customer) Creation
```csharp
var contact = new ParasutContact
{
    Name = customer.GetFullName(),
    Email = customer.Email,
    Phone = customer.Phone,
    TaxOffice = customer.TaxOffice,
    TaxNumber = customer.TaxNumber,
    Address = customer.BillingAddress
};

var response = await _parasutService.CreateContactAsync(contact);
```

### Sales Invoice Creation
```csharp
var invoice = new ParasutSalesInvoice
{
    ContactId = parasutContactId,
    InvoiceDate = DateTime.Now,
    DueDate = DateTime.Now.AddDays(30),
    Currency = "TRY",
    Items = orderItems.Select(item => new ParasutInvoiceItem
    {
        ProductId = GetParasutProductId(item.ProductId),
        Quantity = item.Quantity,
        UnitPrice = item.UnitPriceInclTax,
        VatRate = item.Product.TaxCategoryId
    }).ToList()
};

var response = await _parasutService.CreateSalesInvoiceAsync(invoice);
```

---

## 📄 E-Invoice & E-Archive

### E-Archive Invoice

**Requirements:**
- Paraşüt e-Archive subscription
- Customer without tax number

**Flow:**
1. Create invoice as e-Archive
2. Paraşüt generates unique invoice number
3. PDF includes QR code for verification
4. Invoice submitted to tax authority

### E-Invoice

**Requirements:**
- Paraşüt e-Invoice subscription
- Customer with valid tax number
- Customer registered in e-Invoice system

**Flow:**
1. Create invoice as e-Invoice
2. Send to customer's e-Invoice inbox
3. Customer receives notification
4. Customer can view/download from e-Invoice portal

---

## 🔐 Security

### OAuth 2.0 Authentication
- ✅ Secure token-based authentication
- ✅ Automatic token refresh
- ✅ Credentials encrypted in database

### API Security
- ✅ All requests over HTTPS
- ✅ Request signing
- ✅ Rate limiting handled

---

## 📊 Monitoring

### Invoice Logs
```bash
# View Paraşüt logs
tail -f src/Presentation/Nop.Web/App_Data/Logs/nopcommerce-*.txt | grep -i parasut
```

### Common Log Entries

**Successful Invoice:**
```
[INFO] Creating Paraşüt invoice for Order #123
[INFO] Contact created: ContactId=456
[INFO] Invoice created: InvoiceId=789
[INFO] PDF downloaded and stored
```

**Failed Invoice:**
```
[ERROR] Paraşüt API error: Invalid tax number
[ERROR] Failed to create invoice for Order #123
```

---

## 🐛 Troubleshooting

### Invoice Not Generated

**Issue:** Order completed but no invoice

**Solutions:**
1. Check Paraşüt credentials are valid
2. Verify access token not expired
3. Check customer data is complete (name, address)
4. Review error logs

### OAuth Token Expired

**Error:** "Unauthorized: Token expired"

**Solutions:**
1. Refresh token automatically (plugin handles this)
2. If fails, re-authorize application:
   - Admin → Parasut → Configure
   - Click "Authorize"
   - Complete OAuth flow

### PDF Download Fails

**Error:** "Failed to download invoice PDF"

**Solutions:**
1. Check network connectivity
2. Verify invoice was created successfully
3. Check Paraşüt API status
4. Retry download manually

---

## 🔄 Uninstall

### Safe Uninstall

**What happens:**
- ✅ Plugin UI removed
- ✅ Settings deleted
- ✅ **Invoice records preserved** (if stored in NopCommerce)

**What to do before uninstall:**
1. Download all invoice PDFs
2. Export invoice data
3. Backup database

---

## 📚 Related Documentation

- [Paraşüt API Documentation](https://api.parasut.com)
- [Paraşüt Developer Portal](https://developer.parasut.com)
- [Core Plugin](../Nop.Plugin.Marketplace.Core/README.md)

---

## 🎯 Roadmap

### Planned Features
- [ ] Expense invoice support
- [ ] Payment tracking integration
- [ ] Automatic reconciliation
- [ ] Tax report generation
- [ ] Multi-company support
- [ ] Invoice templates

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-01-13 | Initial release with basic invoicing |

---

## 👥 Support

**Paraşüt Support:** https://www.parasut.com/destek  
**Technical Issues:** support@yourcompany.com  
**Documentation:** [Full Documentation](../../../README.md)

---

**Plugin Type:** Accounting Integration  
**Dependencies:** None  
**Integration:** Paraşüt Cloud Accounting  
**Invoice Types:** Sales, E-Archive, E-Invoice  
**Status:** Production Ready
