# ACUEDUCTO - Community Water Management System

## 🌊 What is ACUEDUCTO?

ACUEDUCTO is a comprehensive web-based management system designed for community water utilities (Acueductos Comunitarios). It streamlines the entire water service lifecycle including consumption tracking, billing, payment collection, and service management.

## 🎯 What Can ACUEDUCTO Do For You?

### 📊 Core Capabilities

#### 1. **Bimonthly Billing Cycle Management (Planillas)**
- Create and manage bimonthly billing periods
- Set consumption recording deadlines
- Define payment and service suspension dates
- Control period status (open/closed) for data integrity
- Prevent overlapping periods and ensure audit trails

#### 2. **Water Consumption Tracking (Consumos)**
- Record meter readings per property
- Calculate consumption automatically
- Import data from Excel spreadsheets
- Track consumption patterns and classifications
- Monitor previous period readings
- Classify consumption into 8 categories (from extremely low to extremely high)

#### 3. **Invoice Generation (Facturación)**
- Automatically generate invoices based on consumption
- Include fixed charges and variable consumption costs
- Calculate subsidies and late payment fees (mora)
- Handle previous period balances
- Email invoices directly to customers
- Print professional invoice formats with consumption charts
- Track payment status

#### 4. **Payment Collection (Recaudo)**
- Register full and partial payments
- Apply payments to current or previous periods
- Track daily and accumulated collections
- Filter unpaid invoices
- Generate collection reports and charts
- Update account balances automatically
- Issue payment receipts

#### 5. **Accounts Receivable Management (CxC)**
- Track outstanding balances
- Manage payment installments (cuotas)
- Automatically apply installments to current invoices
- Verify and synchronize account balances
- Monitor payment history

#### 6. **Property & Customer Management (Predios & Usuarios)**
- Maintain customer database with contact information
- Manage property records with geolocation
- Track meter information (brand, status)
- Store property observations and cadastral information
- Classify consumption patterns per property
- Upload and manage property images and documents

#### 7. **Service Request System (SGS)**
- Create and track service requests
- Define request types and statuses
- Assign and manage work orders (Trabajos)
- Track request resolution progress
- Generate reports by type and status
- Attach supporting documentation

#### 8. **Infrastructure Maintenance (Mantenimiento)**
- Log maintenance activities by period
- Track materials and costs
- Record responsible parties
- Attach evidence and documentation
- Maintain historical records for compliance

#### 9. **Reporting & Analytics**
- Daily collection charts with targets and deadlines
- Consumption analysis by property
- Classification reports by consumption type
- Payment deadline and suspension date visualization
- Previous period comparisons
- Financial summaries

#### 10. **Document Management**
- Upload images and multimedia files per property
- Manage inventory documentation
- Store supporting documents for requests
- Organize files by property/inventory codes

## 🚀 Technical Features

- **Technology Stack**: ASP.NET Core 9.0, Blazor Server & WebAssembly
- **Database**: SQL Server with Entity Framework Core
- **PDF Generation**: QuestPDF for professional invoice formats
- **Excel Integration**: EPPlus for data import/export
- **Email**: SMTP integration for automated communications
- **Charts**: Chart.js for interactive visualizations
- **Responsive Design**: Bootstrap 5.3 for mobile-friendly interface
- **Real-time Updates**: Blazor interactive components

## 📋 Prerequisites

- .NET 9.0 SDK or later
- SQL Server (2019 or later)
- Modern web browser (Chrome, Firefox, Edge)
- SMTP server for email functionality

## 🔧 Setup & Installation

### 1. Clone the Repository
```bash
git clone <your-repository-url>
cd ACUEDUCTO09
```

### 2. Configure Database Connection
Edit `ACUEDUCTO/ACUEDUCTO/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ACUEDUCTO;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

**Security Note**: For production environments:
- Never commit connection strings with real credentials to version control
- Use environment variables or secure configuration providers (Azure Key Vault, AWS Secrets Manager, etc.)
- For production SQL Server, use proper SSL/TLS certificates instead of `TrustServerCertificate=True`
- Consider using Windows Authentication or Managed Identity instead of SQL authentication

### 3. Configure Email Settings
```json
{
  "EmailSettings": {
    "SmtpServer": "your-smtp-server.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@domain.com",
    "SmtpPassword": "your-password"
  }
}
```

### 4. Apply Database Migrations
```bash
cd ACUEDUCTO/ACUEDUCTO
dotnet ef database update
```

### 5. Build and Run
```bash
dotnet build
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`

## 📖 User Guides

### Creating a New Billing Period (Planilla)

1. Navigate to **Menu → Planillas → Crear Planilla**
2. Enter period information:
   - **Name**: Format "YYYY-MM DESCRIPTION" (e.g., "2025-02 ENERO - FEBRERO")
   - **Short Description**: (e.g., "ENE-FEB")
   - **Long Description**: (e.g., "ENERO - FEBRERO")
   - **Consumption Date**: Deadline for meter readings
   - **Payment Date**: Payment deadline
   - **Suspension Date**: Service suspension date
3. Save - the period starts as "Abierta" (Open)
4. While open: enter consumption data and generate invoices
5. Close period when invoices are validated to prevent changes

### Recording Water Consumption

1. Ensure the billing period (Planilla) is open
2. Navigate to **Consumos**
3. Import from Excel or enter manually:
   - Current meter reading
   - System calculates consumption automatically
   - Consumption type is classified
4. Complete all readings before generating invoices

### Generating Invoices

1. Ensure billing period is created and open
2. Complete all meter readings
3. Navigate to **Facturación**
4. Trigger invoice generation
5. Review each invoice for accuracy
6. Use "Verificar CXC" to apply installments
7. Close the period when stable

### Processing Payments

1. Navigate to **Recaudo**
2. Select billing period (or auto-selected)
3. Search for customer/invoice
4. Enter payment details:
   - Full payment or partial (abono)
   - Apply to current or previous period
5. System updates balances automatically
6. Issue receipt if needed

### Managing Service Requests

1. Navigate to **SGS → Solicitudes**
2. Create new request:
   - Select request type
   - Set initial status
   - Associate with customer
   - Add observations
3. Create work order (Trabajo) if needed
4. Update status as work progresses
5. Attach evidence/documentation

## 🛠️ Developer Information

### Project Structure

```
ACUEDUCTO09/
├── ACUEDUCTO/
│   ├── ACUEDUCTO/              # Main server project
│   │   ├── Components/         # Blazor components
│   │   ├── Models/            # EF Core data models
│   │   ├── Services/          # Business logic services
│   │   ├── DTO/               # Data transfer objects
│   │   └── wwwroot/           # Static files
│   ├── ACUEDUCTO.Client/      # Blazor WebAssembly client
│   └── Models/                # Shared models
├── FACTURA/                   # Invoice templates
└── wwwroot/                   # Shared resources
```

### Key Technologies

- **ASP.NET Core 9.0**: Web framework
- **Blazor**: Interactive UI framework
- **Entity Framework Core 9.0**: ORM
- **EPPlus**: Excel file processing
- **QuestPDF**: PDF generation
- **Chart.js**: Data visualization
- **Bootstrap 5**: UI framework

### Database Schema

The system uses SQL Server with the following main entities:
- Planilla (Billing Periods)
- Predio (Properties)
- Usuario (Customers)
- Consumo (Consumption Readings)
- Facturacion (Invoices)
- Ingreso (Payments)
- CuentasPorCobrar (Accounts Receivable)
- Solicitud (Service Requests)
- Trabajo (Work Orders)
- Mantenimiento (Maintenance Records)

### Running Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Scaffold from existing database (use with caution)
dotnet ef dbcontext scaffold "Server=YOUR_SERVER;Database=ACUEDUCTO;..." Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force
```

**Security Warning**: Never use real connection strings with actual credentials in scaffold commands or commit them to version control. Always use placeholder values in documentation and examples.

## 🔐 Security Considerations

**Critical Security Practices**:

1. **Credential Management**:
   - Never commit passwords, API keys, or connection strings to version control
   - Use environment variables or secure vaults (Azure Key Vault, AWS Secrets Manager)
   - Rotate credentials regularly
   - Remove sensitive data from NOTAS.txt and other documentation files

2. **Database Security**:
   - Use Windows Authentication or Managed Identity when possible
   - For production, configure proper SSL/TLS certificates (avoid `TrustServerCertificate=True`)
   - Follow principle of least privilege for database access
   - Regularly backup database with encryption

3. **Application Security**:
   - Enable HTTPS in production (never use HTTP for production deployments)
   - Implement proper authentication and authorization
   - Validate all user inputs to prevent injection attacks
   - Keep dependencies up to date to patch security vulnerabilities

4. **Email Security**:
   - Secure SMTP credentials using configuration providers
   - Use application-specific passwords or OAuth2 when available
   - Validate email addresses before sending

5. **File Upload Security**:
   - Validate file types and sizes
   - Scan uploaded files for malware
   - Store files outside of webroot when possible
   - Implement access controls for uploaded documents

## 📦 Deployment

### IIS Deployment

1. Publish the application:
   ```bash
   dotnet publish -c Release
   ```

2. Deploy artifacts to IIS site root
3. Ensure wwwroot subfolders exist:
   - `imagenes/predios/`
   - `imagenes/inventario/`
   - `FACTURAS/`
4. Configure application pool (.NET CLR Version: No Managed Code)
5. Bind SSL certificate for HTTPS
6. Verify SMTP ports are open

### Hosting Requirements

- Windows Server with IIS
- .NET 9.0 Runtime
- SQL Server access
- SMTP port 587 outbound access
- Sufficient storage for uploaded files

## 📞 Support & Maintenance

### Backup Strategy

- **Database**: Nightly full backups, 30-day retention
- **Files**: Backup wwwroot/imagenes/* regularly
- **Source Code**: Maintain Git repository

### Common Issues

1. **Invoice generation fails**: Ensure billing period is open and all consumptions are recorded
2. **Email not sending**: Verify SMTP settings and port access
3. **SGS errors**: Check that all request types and statuses have valid (non-null) values
4. **CxC sync issues**: Run "Verificar CXC" after invoice adjustments

## 📝 Version History

Current version: **1.5.2**

See `NOTAS.txt` in the ACUEDUCTO/ACUEDUCTO folder for detailed version history and changelog.

## 📄 License

This project is proprietary software for community water utilities management.

## 🤝 Contributing

For bugs, feature requests, or contributions, please contact the project maintainers.

---

**Built with ❤️ for Community Water Utilities**