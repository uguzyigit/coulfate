# 💳 Payments.Iyzico Plugin

**İyzico 3DS payment gateway integration for NopCommerce 4.90**

---

## 📋 Overview

Payments.Iyzico provides secure payment processing through İyzico payment gateway with full 3D Secure (3DS) support for Turkish e-commerce.

**Version:** 1.0  
**Depends On:** Marketplace.Core, Commission, VendorExtensions  
**Status:** Production Ready

---

## ✨ Features

### 3D Secure Payment Flow
- ✅ **Initialize Payment** - Create payment request
- ✅ **3DS Page** - Redirect to bank 3DS page
- ✅ **Callback** - Handle bank response
- ✅ **Complete Payment** - Finalize transaction
- ✅ **Order Status Update** - Update order payment status

### Payment Methods
- ✅ Credit Card (Visa, Mastercard, American Express)
- ✅ Debit Card
- ✅ Full 3DS support
- ✅ Test cards support

### Integration Features
- ✅ **Test Environment** - Sandbox for testing
- ✅ **Production Ready** - Live payment processing
- ✅ **Order Tracking** - Link payments to orders
- ✅ **Error Handling** - Graceful error management
- ✅ **Logging** - Detailed transaction logs

### Commission Integration
- ✅ **Automatic trigger** - Payment completion triggers commission
- ✅ **Vendor transactions** - Creates vendor account entries
- ✅ **Balance updates** - Updates vendor balances

---

## 📦 Installation

### Prerequisites
- ✅ Marketplace.Core plugin installed
- ✅ Marketplace.Commission plugin installed
- ✅ Marketplace.VendorExtensions plugin installed
- ✅ İyzico merchant account
- ✅ İyzico API credentials

### Getting İyzico Credentials

1. **Register with İyzico:**
   - Visit: https://www.iyzico.com
   - Create merchant account
   - Complete verification

2. **Get API Credentials:**
   - Login to İyzico panel
   - Navigate to Settings → API Keys
   - Copy API Key and Secret Key

3. **Test Credentials:**
   - Sandbox API Key: `sandbox-xxx`
   - Sandbox Secret Key: `sandbox-yyy`

### Installation Steps

1. **Install dependencies first:**
   - Marketplace.Core
   - Marketplace.Commission
   - Marketplace.VendorExtensions

2. **Build the plugin:**
```bash
cd src/Plugins/Nop.Plugin.Payments.Iyzico
dotnet build
```

3. **Install via admin panel:**
   - Admin → Configuration → Local Plugins
   - Find "İyzico Payment"
   - Click "Install"
   - Restart when prompted

4. **Configure plugin:**
   - Admin → Configuration → Payment Methods
   - Find "İyzico"
   - Click "Configure"
   - Enter API credentials

---

## 🔧 Configuration

### Plugin Settings

Navigate to: **Admin → Configuration → Payment Methods → İyzico → Configure**

**Required Settings:**

| Setting | Description | Example |
|---------|-------------|---------|
| API Key | İyzico API Key | `sandbox-xxx` or production key |
| Secret Key | İyzico Secret Key | `sandbox-yyy` or production key |
| Base URL | İyzico API endpoint | Test: `https://sandbox-api.iyzipay.com` |
| | | Production: `https://api.iyzipay.com` |

**Optional Settings:**

| Setting | Description | Default |
|---------|-------------|---------|
| Additional Fee | Extra fee for using this method | 0.00 |
| Additional Fee Percentage | Percentage-based fee | false |

---

## 🔄 Payment Flow

### Customer Journey
```
1. Customer adds products to cart
   ↓
2. Customer proceeds to checkout
   ↓
3. Customer selects "İyzico Payment"
   ↓
4. Customer enters credit card details
   ↓
5. Order created with PaymentStatus = Pending
   ↓
6. Initialize Payment (İyzico API call)
   ↓
7. Redirect to 3DS page (bank's page)
   ↓
8. Customer enters 3DS code (SMS)
   ↓
9. Bank validates and redirects back
   ↓
10. Callback handler receives response
   ↓
11. Complete Payment (İyzico API call)
   ↓
12. Update Order: PaymentStatus = Paid
   ↓
13. OrderPaidEvent fired
   ↓
14. Commission calculated (Commission plugin)
   ↓
15. Vendor transactions created (VendorExtensions plugin)
```

### Technical Flow
```
Initialize Payment Request:
POST /payment/iyzipos/initialize3ds
{
  "price": "100.00",
  "paidPrice": "100.00",
  "currency": "TRY",
  "basketId": "B12345",
  "paymentCard": { ... },
  "buyer": { ... },
  "billingAddress": { ... },
  "basketItems": [ ... ]
}

Response:
{
  "status": "success",
  "threeDSHtmlContent": "<html>...</html>"
}

↓ Display 3DS page ↓

Callback URL:
POST /PaymentIyzico/Callback
{
  "status": "success",
  "paymentId": "12345",
  "conversationId": "ORDER-123"
}

↓ Complete Payment ↓

Complete Payment Request:
POST /payment/iyzipos/auth3ds
{
  "paymentId": "12345",
  "conversationId": "ORDER-123"
}

Response:
{
  "status": "success",
  "paymentId": "12345",
  "paymentStatus": "SUCCESS"
}

↓ Update Order Status ↓
```

---

## 🧪 Testing

### Test Cards

**Successful Payment:**
```
Card Number: 5528 7900 0000 0001
Expiry: 12/30
CVV: 123
3DS Code: Will be sent via SMS (test: any 6 digits)
```

**Insufficient Funds:**
```
Card Number: 5406 6700 0000 0009
```

**Do Not Honor:**
```
Card Number: 4543 5900 0000 0006
```

### Test Mode Setup

1. Set Base URL to: `https://sandbox-api.iyzipay.com`
2. Use test API credentials
3. Use test cards above
4. No real money charged

### Production Checklist

Before going live:
- [ ] Production API credentials configured
- [ ] Base URL changed to production
- [ ] Test payments successful
- [ ] Error handling tested
- [ ] Commission flow verified
- [ ] Vendor transactions verified

---

## 💻 Code Integration

### Payment Method Implementation
```csharp
public class IyzicoPaymentProcessor : BasePlugin, IPaymentMethod
{
    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        var result = new ProcessPaymentResult();
        
        try
        {
            // Initialize 3DS payment
            var initializeRequest = PrepareInitializeRequest(request);
            var response = await _iyzicoService.Initialize3DSAsync(initializeRequest);
            
            if (response.Status == "success")
            {
                result.NewPaymentStatus = PaymentStatus.Pending;
                result.AllowStoringCreditCardNumber = false;
            }
            else
            {
                result.AddError($"İyzico Error: {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            result.AddError($"Payment failed: {ex.Message}");
        }
        
        return result;
    }
}
```

---

## 🔐 Security

### Best Practices
- ✅ Never store credit card numbers
- ✅ All API calls over HTTPS
- ✅ API credentials in secure storage
- ✅ 3DS for all transactions
- ✅ Request signing with Secret Key
- ✅ Callback URL validation

### PCI Compliance
- ✅ Card data never touches your server
- ✅ İyzico handles PCI compliance
- ✅ 3DS reduces fraud liability

---

## 📊 Monitoring

### Transaction Logs
```bash
# View İyzico transaction logs
tail -f src/Presentation/Nop.Web/App_Data/Logs/nopcommerce-*.txt | grep -i iyzico
```

### Common Log Entries

**Successful Payment:**
```
[INFO] İyzico payment initialized: OrderId=123, PaymentId=xyz
[INFO] 3DS completed successfully: OrderId=123
[INFO] Payment completed: OrderId=123, Status=SUCCESS
```

**Failed Payment:**
```
[ERROR] İyzico payment failed: OrderId=123, Error=Insufficient funds
```

---

## 🐛 Troubleshooting

### Payment Initialization Fails

**Error:** "Payment could not be initialized"

**Solutions:**
1. Check API credentials are correct
2. Verify Base URL (test vs production)
3. Check customer billing address is complete
4. Review request in logs

### 3DS Callback Not Working

**Error:** Payment stuck at "Pending"

**Solutions:**
1. Verify callback URL is accessible from internet
2. Check firewall allows İyzico IPs
3. Ensure callback handler is not blocked by antiforgery
4. Review callback logs

### Commission Not Calculated

**Error:** Payment completed but no commission

**Solutions:**
1. Verify order status changed to "Paid"
2. Check Commission plugin is installed
3. Check OrderPaidEvent is firing
4. Review commission calculation logs

---

## 🔄 Refunds

### Processing Refunds

Refunds can be processed through İyzico panel:

1. Login to İyzico merchant panel
2. Navigate to Transactions
3. Find transaction
4. Click "Refund"
5. Enter amount
6. Confirm refund

**Note:** Plugin does not yet support automatic refunds (planned feature)

---

## 📚 Related Documentation

- [İyzico API Documentation](https://dev.iyzipay.com)
- [Commission Plugin](../Nop.Plugin.Marketplace.Commission/README.md)
- [VendorExtensions Plugin](../Nop.Plugin.Marketplace.VendorExtensions/README.md)
- [Core Plugin](../Nop.Plugin.Marketplace.Core/README.md)

---

## 🎯 Roadmap

### Planned Features
- [ ] Automatic refund support
- [ ] Installment payments
- [ ] BKM Express integration
- [ ] Stored card support
- [ ] Subscription payments
- [ ] Multi-currency support

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-01-13 | Initial release with 3DS support |

---

## 👥 Support

**İyzico Support:** https://www.iyzico.com/destek  
**Technical Issues:** support@yourcompany.com  
**Documentation:** [Full Documentation](../../../README.md)

---

**Plugin Type:** Payment Gateway  
**Dependencies:** Core, Commission, VendorExtensions  
**Payment Methods:** Credit/Debit Card  
**3DS Support:** Full  
**Status:** Production Ready
