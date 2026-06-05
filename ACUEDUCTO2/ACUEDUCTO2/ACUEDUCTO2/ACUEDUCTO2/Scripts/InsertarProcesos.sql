-- =============================================
-- Script: Insertar Procesos desde version 2.csv
-- Tabla: [Proceso] (Id NVARCHAR(450) GUID, Objeto, Nombre, IdTipoProceso)
-- IdTipoProceso: 1 (EDITAR) = Requiere Procesos Autorizados
--              2 (VER/INFORMACION) = Requiere Solo Informacion Autorizada
-- =============================================

-- Limpiar tabla si se desea reinsertar
-- DELETE FROM [Proceso];

-- Verificar que la tabla no tenga datos antes de insertar
IF NOT EXISTS (SELECT 1 FROM [Proceso])
BEGIN

    -- ===== USUARIO - EDITAR =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Actualizacion Datos Usuario', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Cancelacion Servicio Acueducto Predio', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Elaboracion Contrato Prestacion Servicio', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Eliminacion Datos Usuario', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Pagar Factura', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Pagar Otros Cobros', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Registro Nuevo Usuario Propietario Predio', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Registro Nuevo Usuario Residente Predio', 1);

    -- ===== USUARIO - VER =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Busqueda por Datos Usuario', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Cedula Ciudadania o similar de Propietarios', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Certificado Libertad de Predio', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Contrato de Prestacion Servicios', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Estatutos Acueducto', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Instructivo para Registro de Nuevo Usuario', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Listado de Solicitudes por Usuario', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Listado de Usuarios', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Requisitos Inscripcion Nuevo Usuario', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Total a Pagar Bimestre', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Total a Pagar Deuda', 2);

    -- ===== AUTORIZADO =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Autorizado', N'Registrar Nuevo Autorizado', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Autorizado', N'Actualizacion Datos Autorizado', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Autorizado', N'Listado Autorizados', 2);

    -- ===== CARGOJAC (Sub-objeto de Usuario) =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Registrar Nuevo CargoJAC', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Actualizacion Datos CargoJAC', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Usuario', N'Listado CargosJAC', 2);

    -- ===== PREDIO - EDITAR =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Actualizacion Datos Predio', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Cambio Contador por parte del Acueducto Con Cobro de Instalacion', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Cambio Contador por parte del Acueducto Sin Cobro de Instalacion', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Cambio Contador por parte del Cliente Con Cobro de Instalacion', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Cambio Contador por parte del Cliente Sin Cobro de Instalacion', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Cambio de Usuarios al que se Factura el Predio', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Cobro cuota Extraordinaria a predios activos', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Eliminar Informacion de Predio o Punto de Agua de Servicio de Acueducto', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Eliminar Predio o Punto de Agua de Servicio de Acueducto', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Envio de Mensaje a Residentes y Usuario de Predios Activos', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Mantenimiento de Caja de Contador', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Mantenimiento de Contador de Predio', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Mantenimiento de Instalaciones Externas de Predio', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Mantenimiento de Instalaciones Internas de Predio', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Mantenimiento de Tapa de Contador', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Reconexion de Servicio de Acueducto', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Registrar Nuevo Predio o Punto de Agua de Servicio de Acueducto', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Suspencion de Servicio de Acueducto por Mandato Legal', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Suspencion de Servicio de Acueducto por No Pago', 1);

    -- ===== PREDIO - VER =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Busqueda por Datos Predio', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Carta de Informacion de Cambio de Contador', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Copia Certificado Libertad no mayor a 60 dias', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Instructivo para crear Nuevo Punto de Agua de Servicio Acueducto', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Instructivo para Eliminar Predio o Punto de Agua de Servicio Acueducto', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Listado de Estado de Contadores por Predio', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Listado de Mantenimientos de Predios', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Listado de Predios', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Listado para Actualizacion Datos', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Listado para Asistencia a Reuniones', 2);

    -- ===== RESIDENTE (Sub-objeto de Predio) =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Registrar Nuevo Residente', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Actualizacion Datos Residente', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Predio', N'Listado Residentes', 2);

    -- ===== TARIFA =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Tarifa', N'Registrar Nueva Tarifa', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Tarifa', N'Actualizacion Datos Tarifa', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Tarifa', N'Listado Tarifas', 2);

    -- ===== ESTRATO =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Estrato', N'Registrar Nuevo Estrato', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Estrato', N'Actualizacion Datos Estrato', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Estrato', N'Listado Estratos', 2);

    -- ===== TIPOCONSUMO =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'TipoConsumo', N'Registrar Nuevo Tipo Consumo', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'TipoConsumo', N'Actualizacion Datos Tipo Consumo', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'TipoConsumo', N'Listado de Tipos Consumo', 2);

    -- ===== PERIODO - EDITAR =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Periodo', N'Actualizar Periodo', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Periodo', N'Eliminar Periodo', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Periodo', N'Registrar Nuevo Periodo', 1);

    -- ===== PERIODO - VER =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Periodo', N'Busqueda por Datos Periodo', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Periodo', N'Listado de Periodos', 2);

    -- ===== FACTURA - EDITAR =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Factura', N'Actualizar Factura por Recaudo Banco No Reportado', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Factura', N'Actualizar Factura por Otros Motivos', 1);

    -- ===== FACTURA - VER =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Factura', N'Busqueda por Datos Factura', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Factura', N'Listado de Facturas Por Mes', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Factura', N'Listado de Facturas Por Bimestre', 2);

    -- ===== RECAUDO - EDITAR =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Recaudo', N'Actualizar Recaudo', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Recaudo', N'Correccion de Recaudo', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Recaudo', N'Eliminar Recaudo', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Recaudo', N'Nuevo Recaudo', 1);

    -- ===== RECAUDO - VER =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Recaudo', N'Busqueda por Datos Recaudo', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Recaudo', N'Listado de Recaudos por Mes', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Recaudo', N'Listado de Recaudos por Bimestre', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Recaudo', N'Listado de Impresion para Toma de Datos de Recaudo', 2);

    -- ===== CONSUMO - EDITAR =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Consumo', N'Actualizar Consumo por Nuevo Contador', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Consumo', N'Correccion de Consumo', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Consumo', N'Registrar Consumo de Servicio de Acueducto Bimestral', 1);

    -- ===== CONSUMO - VER =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Consumo', N'Busqueda por Datos Consumos', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Consumo', N'Listado de Consumos por Predio', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Consumo', N'Listado de Impresion para Toma de Datos de Consumo', 2);

    -- ===== INVENTARIO - EDITAR =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Actualizar Inventario', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Correccion de Inventario', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Eliminar Inventario', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Elaborar Inventario Fisico de Productos Y Servicios', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Mantenimiento Inventarios', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Nuevo Producto o Servicio para Inventario', 1);

    -- ===== INVENTARIO - VER =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Busqueda por Datos Inventario', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Listado de Inventario', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Listado de Mantenimiento de Inventarios', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Inventario', N'Listado de Inventario Fisico de Existencias', 2);

    -- ===== SOLICITUD - EDITAR =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Solicitud', N'Actualizar Solicitud', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Solicitud', N'Eliminar Solicitud', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Solicitud', N'Nueva Solicitud', 1);

    -- ===== SOLICITUD - VER =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Solicitud', N'Busqueda por Datos Solicitud', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Solicitud', N'Listado de Solicitudes por Tipo', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Solicitud', N'Reporte de Solicitudes', 2);

    -- ===== TIPOSOLICITUD =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'TipoSolicitud', N'Registrar Nuevo Tipo Solicitud', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'TipoSolicitud', N'Actualizacion Datos Tipo Solicitud', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'TipoSolicitud', N'Listado Tipo Solicitudes', 2);

    -- ===== ESTADOSOLICITUD =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'EstadoSolicitud', N'Registrar Nuevo Estado Solicitud', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'EstadoSolicitud', N'Actualizacion Datos Estado Solicitud', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'EstadoSolicitud', N'Listado Estados Solicitud', 2);

    -- ===== SEGURIDAD - EDITAR =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Seguridad', N'Actualizar Acceso', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Seguridad', N'Eliminar Acceso', 1);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Seguridad', N'Nuevo Acceso', 1);

    -- ===== SEGURIDAD - VER =====
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Seguridad', N'Busqueda por Datos Acceso', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Seguridad', N'Listado de Accesos', 2);
    INSERT INTO [Proceso] ([Id], [Objeto], [Nombre], [IdTipoProceso]) VALUES (NEWID(), N'Seguridad', N'Busqueda de Acceso por Usuario', 2);

    PRINT 'Se insertaron todos los procesos correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Proceso ya contiene datos. No se insertaron registros.';
END

-- Verificar resultados
SELECT [Objeto], [IdTipoProceso], COUNT(*) AS [Cantidad]
FROM [Proceso]
GROUP BY [Objeto], [IdTipoProceso]
ORDER BY [Objeto], [IdTipoProceso];
