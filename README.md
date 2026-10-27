# ACUEDUCTO - Sistema de Gestión de Acueductos Comunitarios

## 🌊 ¿Qué es ACUEDUCTO?

ACUEDUCTO es un sistema web integral de gestión diseñado para acueductos comunitarios. Simplifica todo el ciclo de vida del servicio de agua: registro de consumos, facturación, recaudo y gestión de solicitudes de servicio.

## 🎯 ¿Qué puede hacer ACUEDUCTO por usted?

### 📊 Funcionalidades principales

#### 1. **Gestión de ciclos de facturación bimestral (Planillas)**
- Crear y administrar periodos de facturación bimestrales
- Definir fechas límite para el registro de consumos
- Establecer fechas de pago y de suspensión del servicio
- Controlar el estado del periodo (abierto/cerrado) para garantizar la integridad de los datos
- Evitar periodos superpuestos y mantener trazabilidad para auditoría

#### 2. **Registro de consumos de agua (Consumos)**
- Registrar lecturas de medidor por predio
- Calcular el consumo automáticamente
- Importar datos desde hojas de cálculo de Excel
- Hacer seguimiento a patrones y clasificaciones de consumo
- Consultar lecturas del periodo anterior
- Clasificar el consumo en 8 categorías (desde extremadamente bajo hasta extremadamente alto)

#### 3. **Generación de facturas (Facturación)**
- Generar facturas automáticamente a partir del consumo
- Incluir cargos fijos y costos variables por consumo
- Calcular subsidios e intereses de mora
- Manejar saldos de periodos anteriores
- Enviar facturas por correo electrónico directamente a los usuarios
- Imprimir facturas con formato profesional y gráficas de consumo
- Hacer seguimiento al estado de pago

#### 4. **Recaudo de pagos (Recaudo)**
- Registrar pagos totales y parciales
- Aplicar pagos al periodo actual o a periodos anteriores
- Controlar el recaudo diario y acumulado
- Filtrar facturas pendientes de pago
- Generar informes y gráficas de recaudo
- Actualizar saldos de cuentas automáticamente
- Expedir recibos de pago

#### 5. **Gestión de cuentas por cobrar (CxC)**
- Controlar saldos pendientes
- Administrar cuotas de pago
- Aplicar cuotas automáticamente a las facturas vigentes
- Verificar y sincronizar saldos de cuentas
- Consultar el historial de pagos

#### 6. **Gestión de predios y usuarios (Predios y Usuarios)**
- Mantener la base de datos de usuarios con información de contacto
- Administrar registros de predios con geolocalización
- Registrar información del medidor (marca, estado)
- Guardar observaciones e información catastral del predio
- Clasificar patrones de consumo por predio
- Cargar y administrar imágenes y documentos del predio

#### 7. **Sistema de gestión de solicitudes (SGS)**
- Crear y hacer seguimiento a solicitudes de servicio
- Definir tipos y estados de solicitud
- Asignar y administrar órdenes de trabajo (Trabajos)
- Controlar el avance en la resolución de solicitudes
- Generar informes por tipo y estado
- Adjuntar documentación de soporte

#### 8. **Mantenimiento de infraestructura (Mantenimiento)**
- Registrar actividades de mantenimiento por periodo
- Controlar materiales y costos
- Registrar responsables
- Adjuntar evidencias y documentación
- Conservar registros históricos para cumplimiento normativo

#### 9. **Informes y análisis**
- Gráficas de recaudo diario con metas y fechas límite
- Análisis de consumo por predio
- Informes de clasificación por tipo de consumo
- Visualización de fechas límite de pago y de suspensión
- Comparativos con periodos anteriores
- Resúmenes financieros

#### 10. **Gestión documental**
- Cargar imágenes y archivos multimedia por predio
- Administrar la documentación de inventario
- Guardar documentos de soporte de las solicitudes
- Organizar archivos por código de predio o de inventario

## 🚀 Características técnicas

- **Tecnologías**: ASP.NET Core 9.0, Blazor Server y WebAssembly
- **Base de datos**: SQL Server con Entity Framework Core
- **Generación de PDF**: QuestPDF para formatos de factura profesionales
- **Integración con Excel**: EPPlus para importación y exportación de datos
- **Correo electrónico**: integración SMTP para comunicaciones automáticas
- **Gráficas**: Chart.js para visualizaciones interactivas
- **Diseño responsivo**: Bootstrap 5.3 para una interfaz compatible con dispositivos móviles
- **Actualizaciones en tiempo real**: componentes interactivos de Blazor

## 📋 Requisitos previos

- .NET 9.0 SDK o superior
- SQL Server (2019 o superior)
- Navegador web moderno (Chrome, Firefox, Edge)
- Servidor SMTP para la funcionalidad de correo electrónico

## 🔧 Instalación y configuración

### 1. Clonar el repositorio
```bash
git clone <url-de-su-repositorio>
cd acueduct
```

### 2. Configurar la conexión a la base de datos
Edite `ACUEDUCTO/ACUEDUCTO/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SU_SERVIDOR;Database=ACUEDUCTO;User Id=SU_USUARIO;Password=SU_CONTRASEÑA;TrustServerCertificate=True;"
  }
}
```

**Nota de seguridad**: para entornos de producción:
- Nunca suba al control de versiones cadenas de conexión con credenciales reales
- Use variables de entorno o proveedores de configuración seguros (Azure Key Vault, AWS Secrets Manager, etc.)
- En SQL Server de producción, use certificados SSL/TLS apropiados en lugar de `TrustServerCertificate=True`
- Considere usar autenticación de Windows o identidad administrada en lugar de autenticación SQL

### 3. Configurar el correo electrónico
```json
{
  "EmailSettings": {
    "SmtpServer": "su-servidor-smtp.com",
    "SmtpPort": 587,
    "SmtpUser": "su-correo@dominio.com",
    "SmtpPassword": "su-contraseña"
  }
}
```

### 4. Aplicar las migraciones de base de datos
```bash
cd ACUEDUCTO/ACUEDUCTO
dotnet ef database update
```

### 5. Compilar y ejecutar
```bash
dotnet build
dotnet run
```

La aplicación quedará disponible en `https://localhost:5001` o `http://localhost:5000`

## 📖 Guías de usuario

### Crear un nuevo periodo de facturación (Planilla)

1. Vaya a **Menú → Planillas → Crear Planilla**
2. Ingrese la información del periodo:
   - **Nombre**: formato "AAAA-MM DESCRIPCIÓN" (p. ej., "2025-02 ENERO - FEBRERO")
   - **Descripción corta**: (p. ej., "ENE-FEB")
   - **Descripción larga**: (p. ej., "ENERO - FEBRERO")
   - **Fecha de consumo**: fecha límite para las lecturas de medidor
   - **Fecha de pago**: fecha límite de pago
   - **Fecha de suspensión**: fecha de suspensión del servicio
3. Guarde: el periodo inicia en estado "Abierta"
4. Mientras esté abierto: registre los consumos y genere las facturas
5. Cierre el periodo cuando las facturas estén validadas para evitar cambios

### Registrar consumos de agua

1. Verifique que el periodo de facturación (Planilla) esté abierto
2. Vaya a **Consumos**
3. Importe desde Excel o registre manualmente:
   - Lectura actual del medidor
   - El sistema calcula el consumo automáticamente
   - Se clasifica el tipo de consumo
4. Complete todas las lecturas antes de generar las facturas

### Generar facturas

1. Verifique que el periodo de facturación esté creado y abierto
2. Complete todas las lecturas de medidor
3. Vaya a **Facturación**
4. Ejecute la generación de facturas
5. Revise cada factura para confirmar que sea correcta
6. Use "Verificar CXC" para aplicar las cuotas
7. Cierre el periodo cuando esté estable

### Procesar pagos

1. Vaya a **Recaudo**
2. Seleccione el periodo de facturación (o se selecciona automáticamente)
3. Busque el usuario o la factura
4. Ingrese los datos del pago:
   - Pago total o parcial (abono)
   - Aplicar al periodo actual o a uno anterior
5. El sistema actualiza los saldos automáticamente
6. Expida el recibo si es necesario

### Gestionar solicitudes de servicio

1. Vaya a **SGS → Solicitudes**
2. Cree una nueva solicitud:
   - Seleccione el tipo de solicitud
   - Defina el estado inicial
   - Asóciela con un usuario
   - Agregue observaciones
3. Cree una orden de trabajo (Trabajo) si se requiere
4. Actualice el estado a medida que avanza el trabajo
5. Adjunte evidencias o documentación

## 🛠️ Información para desarrolladores

### Estructura del proyecto

```
acueduct/
├── ACUEDUCTO/
│   ├── ACUEDUCTO/              # Proyecto principal (servidor)
│   │   ├── Components/         # Componentes Blazor
│   │   ├── Models/            # Modelos de datos de EF Core
│   │   ├── Services/          # Servicios de lógica de negocio
│   │   ├── DTO/               # Objetos de transferencia de datos
│   │   └── wwwroot/           # Archivos estáticos
│   ├── ACUEDUCTO.Client/      # Cliente Blazor WebAssembly
│   └── Models/                # Modelos compartidos
├── FACTURA/                   # Plantillas de factura
└── wwwroot/                   # Recursos compartidos
```

### Tecnologías clave

- **ASP.NET Core 9.0**: framework web
- **Blazor**: framework de interfaz interactiva
- **Entity Framework Core 9.0**: ORM
- **EPPlus**: procesamiento de archivos Excel
- **QuestPDF**: generación de PDF
- **Chart.js**: visualización de datos
- **Bootstrap 5**: framework de interfaz

### Esquema de base de datos

El sistema usa SQL Server con las siguientes entidades principales:
- Planilla (periodos de facturación)
- Predio (predios)
- Usuario (usuarios)
- Consumo (lecturas de consumo)
- Facturacion (facturas)
- Ingreso (pagos)
- CuentasPorCobrar (cuentas por cobrar)
- Solicitud (solicitudes de servicio)
- Trabajo (órdenes de trabajo)
- Mantenimiento (registros de mantenimiento)

### Ejecutar migraciones

```bash
# Crear una nueva migración
dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones
dotnet ef database update

# Generar modelos desde una base de datos existente (usar con precaución)
dotnet ef dbcontext scaffold "Server=SU_SERVIDOR;Database=ACUEDUCTO;..." Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force
```

**Advertencia de seguridad**: nunca use cadenas de conexión con credenciales reales en los comandos de scaffold ni las suba al control de versiones. Use siempre valores de ejemplo en la documentación.

## 🔐 Consideraciones de seguridad

**Prácticas de seguridad críticas**:

1. **Gestión de credenciales**:
   - Nunca suba contraseñas, claves de API ni cadenas de conexión al control de versiones
   - Use variables de entorno o bóvedas seguras (Azure Key Vault, AWS Secrets Manager)
   - Rote las credenciales periódicamente
   - Elimine datos sensibles de NOTAS.txt y demás archivos de documentación

2. **Seguridad de la base de datos**:
   - Use autenticación de Windows o identidad administrada cuando sea posible
   - En producción, configure certificados SSL/TLS apropiados (evite `TrustServerCertificate=True`)
   - Aplique el principio de mínimo privilegio para el acceso a la base de datos
   - Realice copias de seguridad periódicas con cifrado

3. **Seguridad de la aplicación**:
   - Habilite HTTPS en producción (nunca use HTTP en despliegues de producción)
   - Implemente autenticación y autorización adecuadas
   - Valide todas las entradas del usuario para prevenir ataques de inyección
   - Mantenga las dependencias actualizadas para corregir vulnerabilidades de seguridad

4. **Seguridad del correo electrónico**:
   - Proteja las credenciales SMTP mediante proveedores de configuración
   - Use contraseñas de aplicación u OAuth2 cuando estén disponibles
   - Valide las direcciones de correo antes de enviar

5. **Seguridad en la carga de archivos**:
   - Valide tipos y tamaños de archivo
   - Analice los archivos cargados en busca de malware
   - Guarde los archivos fuera del webroot cuando sea posible
   - Implemente controles de acceso para los documentos cargados

## 📦 Despliegue

### Despliegue en IIS

1. Publique la aplicación:
   ```bash
   dotnet publish -c Release
   ```

2. Copie los artefactos a la raíz del sitio en IIS
3. Verifique que existan las subcarpetas de wwwroot:
   - `imagenes/predios/`
   - `imagenes/inventario/`
   - `FACTURAS/`
4. Configure el grupo de aplicaciones (versión de .NET CLR: Sin código administrado)
5. Asocie el certificado SSL para HTTPS
6. Verifique que los puertos SMTP estén abiertos

### Requisitos de alojamiento

- Windows Server con IIS
- Runtime de .NET 9.0
- Acceso a SQL Server
- Acceso saliente por el puerto SMTP 587
- Almacenamiento suficiente para los archivos cargados

## 📞 Soporte y mantenimiento

### Estrategia de copias de seguridad

- **Base de datos**: copias completas nocturnas, retención de 30 días
- **Archivos**: respaldar wwwroot/imagenes/* periódicamente
- **Código fuente**: mantener el repositorio Git

### Problemas comunes

1. **Falla la generación de facturas**: verifique que el periodo esté abierto y que todos los consumos estén registrados
2. **No se envían correos**: verifique la configuración SMTP y el acceso al puerto
3. **Errores en SGS**: verifique que todos los tipos y estados de solicitud tengan valores válidos (no nulos)
4. **Problemas de sincronización de CxC**: ejecute "Verificar CXC" después de ajustar facturas

## 📝 Historial de versiones

Versión actual: **1.5.2**

Consulte `NOTAS.txt` en la carpeta ACUEDUCTO/ACUEDUCTO para el historial de versiones detallado y el registro de cambios.

## 📄 Licencia

Este proyecto es software propietario para la gestión de acueductos comunitarios.

## 🤝 Contribuciones

Para reportar errores, solicitar funcionalidades o contribuir, contacte a los responsables del proyecto.

---

**Hecho con ❤️ para los acueductos comunitarios**
