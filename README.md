# 🛒 NopCommerce 4.90 Marketplace Platform

**Enterprise-grade marketplace solution built on NopCommerce 4.90**

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Plugins](#plugins)
- [Quick Start](#quick-start)
- [Documentation](#documentation)
- [System Requirements](#system-requirements)
- [Support](#support)

---

## 🎯 Overview

This is a comprehensive multi-vendor marketplace platform built on NopCommerce 4.90 with advanced commission management, vendor account tracking, and integrated payment/accounting systems.

**Key Capabilities:**
- ✅ Multi-vendor marketplace operations
- ✅ Automated commission calculation and tracking
- ✅ Vendor financial account management (cari hesap)
- ✅ Integrated payment processing (İyzico 3DS)
- ✅ Accounting system integration (Paraşüt)
- ✅ Real-time transaction tracking
- ✅ Comprehensive reporting and analytics

---

## ✨ Features

### Commission Management
- Category-based commission rates
- Product-specific commission overrides
- Automatic commission calculation on payment
- Tax withholding and marketplace fees
- Detailed commission reports with date filters
- Excel export capability

### Vendor Account Management
- Current account (cari hesap) tracking
- Automatic transaction creation
- Balance management (credit/debit)
- Transaction history with filters
- Multiple transaction types support
- Vendor-specific settings

### Payment Integration
- İyzico 3DS payment gateway
- Secure payment processing
- Order tracking and status management
- Test environment support

### Accounting Integration
- Paraşüt automatic invoice generation
- PDF storage and management
- Vendor-specific invoicing

---

## 🏗️ Architecture

The platform uses a **Core-based modular architecture** where all shared entities and database tables are managed by a central Core plugin, with UI plugins depending on it.
```
┌─────────────────────────────────────┐
│   Marketplace.Core (Foundation)     │
│   - 12 Database Tables              │
│   - All Marketplace Entities        │
│   - Base Functionality              │
└─────────────────────────────────────┘
          ↑           ↑           ↑
          │           │           │
    ┌─────┴─────┐ ┌───┴────┐ ┌───┴─────────────┐
    │Commission │ │ Vendor │ │    Payment      │
    │Management │ │Extens. │ │  & Accounting   │
    └───────────┘ └────────┘ └─────────────────┘
```

**Benefits:**
- ✅ Safe plugin uninstall (data preserved in Core)
- ✅ Single source of truth for entities
- ✅ Dependency management by NopCommerce
- ✅ Easy maintenance and updates

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for detailed architecture documentation.

---

## 🔌 Plugins

### Core Plugins

#### 1. [Marketplace.Core](src/Plugins/Nop.Plugin.Marketplace.Core/) (Foundation)
**Status:** Required for all marketplace operations  
**Description:** Core entities, database tables, and base functionality

**Key Features:**
- 12 database tables (vendors, orders, commissions, transactions)
- All marketplace entity definitions
- MySQL 8.0+ compatible
- Dependency protection

[📖 Documentation](src/Plugins/Nop.Plugin.Marketplace.Core/README.md)

---

#### 2. [Commission Management](src/Plugins/Nop.Plugin.Marketplace.Commission/)
**Status:** Active  
**Depends On:** Marketplace.Core

**Key Features:**
- Category commission rates (4 active)
- Product-specific overrides (3 active)
- Automatic calculation on order payment
- Reports with 27 tracked orders
- Marketplace fee and tax withholding
- Localization (EN/TR)

[📖 Documentation](src/Plugins/Nop.Plugin.Marketplace.Commission/README.md)

---

#### 3. [Vendor Extensions](src/Plugins/Nop.Plugin.Marketplace.VendorExtensions/)
**Status:** Active  
**Depends On:** Marketplace.Core

**Key Features:**
- Vendor current account (cari hesap)
- Transaction history with filters
- Automatic transaction creation from commissions
- Balance management
- Admin and vendor panels
- 13 transaction types

[📖 Documentation](src/Plugins/Nop.Plugin.Marketplace.VendorExtensions/README.md)

---

### Integration Plugins

#### 4. [İyzico Payment](src/Plugins/Nop.Plugin.Payments.Iyzico/)
**Status:** Active  
**Depends On:** Marketplace.Core, Commission, VendorExtensions

**Key Features:**
- 3DS secure payment processing
- Test environment support
- Order tracking
- Vendor settings integration

[📖 Documentation](src/Plugins/Nop.Plugin.Payments.Iyzico/README.md)

---

#### 5. [Paraşüt Accounting](src/Plugins/Nop.Plugin.Accounting.Parasut/)
**Status:** Active  
**Depends On:** None (Independent)

**Key Features:**
- Automatic invoice generation
- PDF management
- Vendor integration

[📖 Documentation](src/Plugins/Nop.Plugin.Accounting.Parasut/README.md)

---

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- MySQL 8.0+
- NopCommerce 4.90

### Installation

1. **Clone the repository:**
```bash
git clone <repository-url>
cd nopcommerce-4.90
```

2. **Configure database:**
```bash
# Update connection string in appsettings.json
nano src/Presentation/Nop.Web/App_Data/appsettings.json
```

3. **Build and run:**
```bash
dotnet build
cd src/Presentation/Nop.Web
dotnet run
```

4. **Install plugins (in order):**
   - Marketplace.Core (Required first!)
   - Commission Management
   - Vendor Extensions
   - İyzico Payment
   - Paraşüt Accounting

5. **Access admin panel:**
```
http://localhost:5000/Admin
Default: admin@yourstore.com / admin
```

See [docs/DEVELOPMENT_GUIDE.md](docs/DEVELOPMENT_GUIDE.md) for detailed setup instructions.

---

## 📚 Documentation

### General Documentation
- [🏗️ Architecture Guide](docs/ARCHITECTURE.md) - System architecture and design
- [🗄️ Database Schema](docs/DATABASE.md) - All tables and relationships
- [💻 Development Guide](docs/DEVELOPMENT_GUIDE.md) - How to develop new plugins
- [🚀 Deployment Guide](docs/DEPLOYMENT.md) - Production deployment steps
- [🐛 Troubleshooting](docs/TROUBLESHOOTING.md) - Common issues and solutions
- [📋 Changelog](docs/CHANGELOG.md) - Version history

### Plugin Documentation
- [Core Plugin](src/Plugins/Nop.Plugin.Marketplace.Core/README.md)
- [Commission Plugin](src/Plugins/Nop.Plugin.Marketplace.Commission/README.md)
- [VendorExtensions Plugin](src/Plugins/Nop.Plugin.Marketplace.VendorExtensions/README.md)
- [İyzico Plugin](src/Plugins/Nop.Plugin.Payments.Iyzico/README.md)
- [Paraşüt Plugin](src/Plugins/Nop.Plugin.Accounting.Parasut/README.md)

---

## 💻 System Requirements

### Minimum Requirements
- **OS:** Windows 10+, macOS 12+, or Linux (Ubuntu 20.04+)
- **Runtime:** .NET 9.0 SDK
- **Database:** MySQL 8.0+ or MariaDB 10.6+
- **RAM:** 4 GB minimum, 8 GB recommended
- **Storage:** 10 GB minimum

### Recommended for Production
- **OS:** Ubuntu 22.04 LTS or Windows Server 2022
- **Runtime:** .NET 9.0 SDK
- **Database:** MySQL 8.0+ with InnoDB engine
- **RAM:** 16 GB+
- **Storage:** 50 GB+ SSD
- **Web Server:** Nginx or IIS

---

## 📊 Current Statistics

**As of:** January 13, 2026

| Metric | Count |
|--------|-------|
| Total Plugins | 5 |
| Core Plugins | 3 |
| Integration Plugins | 2 |
| Database Tables | 12 |
| Order Commissions Tracked | 27 |
| Active Category Rates | 4 |
| Active Product Rates | 3 |
| Registered Vendors | 2 |

---

## 🔐 Security

- All payment processing through secure 3DS flow
- Database credentials encrypted
- HTTPS enforced in production
- Regular security audits
- GDPR compliant

---

## 🛠️ Technology Stack

- **Framework:** .NET 9.0
- **Platform:** NopCommerce 4.90
- **Database:** MySQL 8.0+
- **ORM:** LinqToDB
- **Frontend:** Razor Pages, jQuery, Bootstrap
- **Payment:** İyzico API
- **Accounting:** Paraşüt API

---

## 📞 Support

### Issues & Bug Reports
Please use the issue tracker for bug reports and feature requests.

### Commercial Support
For commercial support and custom development:
- Email: support@yourcompany.com
- Website: https://yourcompany.com

---

## 📄 License

Proprietary - All rights reserved.

---

## 👥 Contributors

- Development Team
- Product Management
- QA Team

---

## 🎯 Roadmap

### Q1 2026
- [ ] Excel export for all reports
- [ ] Advanced analytics dashboard
- [ ] Vendor performance metrics

### Q2 2026
- [ ] Multi-currency support
- [ ] API endpoints for mobile apps
- [ ] Webhook support

### Q3 2026
- [ ] AI-powered commission optimization
- [ ] Advanced fraud detection
- [ ] International payment gateways

---

**Built with ❤️ using NopCommerce 4.90**

Last Updated: January 13, 2026
