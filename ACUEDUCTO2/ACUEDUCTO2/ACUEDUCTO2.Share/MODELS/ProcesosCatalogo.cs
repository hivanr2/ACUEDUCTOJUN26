using System.Collections.Generic;
using System.Linq;

namespace ACUEDUCTO2.Share.Models
{
    public class DefinicionProceso
    {
        public string Objeto { get; set; }
        public string Nombre { get; set; }
        public int Tipo { get; set; }

        public DefinicionProceso(string objeto, string nombre, int tipo)
        {
            Objeto = objeto;
            Nombre = nombre;
            Tipo = tipo;
        }
    }

    public static class ProcesosCatalogo
    {
        public static readonly List<DefinicionProceso> Todos = new List<DefinicionProceso>
        {
            // ===== USUARIO - EDITAR =====
            new DefinicionProceso("Usuario", "Actualizacion Datos Usuario", 2),
            new DefinicionProceso("Usuario", "Cancelacion Servicio Acueducto Predio", 2),
            new DefinicionProceso("Usuario", "Elaboracion Contrato Prestacion Servicio", 2),
            new DefinicionProceso("Usuario", "Eliminacion Datos Usuario", 2),
            new DefinicionProceso("Usuario", "Pagar Factura", 2),
            new DefinicionProceso("Usuario", "Pagar Otros Cobros", 2),
            new DefinicionProceso("Usuario", "Registro Nuevo Usuario Propietario Predio", 2),
            new DefinicionProceso("Usuario", "Registro Nuevo Usuario Residente Predio", 2),
            // ===== USUARIO - Informacion =====
            new DefinicionProceso("Usuario", "Busqueda por Datos Usuario", 1),
            new DefinicionProceso("Usuario", "Cedula Ciudadania o similar de Propietarios", 1),
            new DefinicionProceso("Usuario", "Certificado Libertad de Predio", 1),
            new DefinicionProceso("Usuario", "Contrato de Prestacion Servicios", 1),
            new DefinicionProceso("Usuario", "Estatutos Acueducto", 1),
            new DefinicionProceso("Usuario", "Instructivo para Registro de Nuevo Usuario", 1),
            new DefinicionProceso("Usuario", "Listado de Solicitudes por Usuario", 1),
            new DefinicionProceso("Usuario", "Listado de Usuarios", 1),
            new DefinicionProceso("Usuario", "Requisitos Inscripcion Nuevo Usuario", 1),
            new DefinicionProceso("Usuario", "Total a Pagar Bimestre", 1),
            new DefinicionProceso("Usuario", "Total a Pagar Deuda", 1),
            // ===== AUTORIZADO =====
            new DefinicionProceso("Autorizado", "Registrar Nuevo Autorizado", 2),
            new DefinicionProceso("Autorizado", "Actualizacion Datos Autorizado", 2),
            new DefinicionProceso("Autorizado", "Listado Autorizados", 1),
            // ===== CARGOJAC (Sub-objeto de Usuario) =====
            new DefinicionProceso("Usuario", "Registrar Nuevo Cargo", 2),
            new DefinicionProceso("Usuario", "Actualizacion Datos Cargo", 2),
            new DefinicionProceso("Usuario", "Listado Cargos", 1),

            // ===== PREDIO - EDITAR =====
            new DefinicionProceso("Predio", "Actualizacion Datos Predio", 2),
            new DefinicionProceso("Predio", "Cambio Contador por parte del Acueducto Con Cobro de Instalacion", 2),
            new DefinicionProceso("Predio", "Cambio Contador por parte del Acueducto Sin Cobro de Instalacion", 2),
            new DefinicionProceso("Predio", "Cambio Contador por parte del Cliente Con Cobro de Instalacion", 2),
            new DefinicionProceso("Predio", "Cambio Contador por parte del Cliente Sin Cobro de Instalacion", 2),
            new DefinicionProceso("Predio", "Cambio de Usuarios al que se Factura el Predio", 2),
            new DefinicionProceso("Predio", "Cobro cuota Extraordinaria a predios activos", 2),
            new DefinicionProceso("Predio", "Eliminar Informacion de Predio o Punto de Agua de Servicio de Acueducto", 2),
            new DefinicionProceso("Predio", "Eliminar Predio o Punto de Agua de Servicio de Acueducto", 2),
            new DefinicionProceso("Predio", "Envio de Mensaje a Residentes y Usuario de Predios Activos", 2),
            new DefinicionProceso("Predio", "Mantenimiento de Caja de Contador", 2),
            new DefinicionProceso("Predio", "Mantenimiento de Contador de Predio", 2),
            new DefinicionProceso("Predio", "Mantenimiento de Instalaciones Externas de Predio", 2),
            new DefinicionProceso("Predio", "Mantenimiento de Instalaciones Internas de Predio", 2),
            new DefinicionProceso("Predio", "Mantenimiento de Tapa de Contador", 2),
            new DefinicionProceso("Predio", "Reconexion de Servicio de Acueducto", 2),
            new DefinicionProceso("Predio", "Registrar Nuevo Predio o Punto de Agua de Servicio de Acueducto", 2),
            new DefinicionProceso("Predio", "Suspencion de Servicio de Acueducto por Mandato Legal", 2),
            new DefinicionProceso("Predio", "Suspencion de Servicio de Acueducto por No Pago", 2),
            // ===== PREDIO - Informacion =====
            new DefinicionProceso("Predio", "Busqueda por Datos Predio", 1),
            new DefinicionProceso("Predio", "Carta de Informacion de Cambio de Contador", 1),
            new DefinicionProceso("Predio", "Copia Certificado Libertad no mayor a 60 dias", 1),
            new DefinicionProceso("Predio", "Instructivo para crear Nuevo Punto de Agua de Servicio Acueducto", 1),
            new DefinicionProceso("Predio", "Instructivo para Eliminar Predio o Punto de Agua de Servicio Acueducto", 1),
            new DefinicionProceso("Predio", "Listado de Estado de Contadores por Predio", 1),
            new DefinicionProceso("Predio", "Listado de Mantenimientos de Predios", 1),
            new DefinicionProceso("Predio", "Listado de Predios", 1),
            new DefinicionProceso("Predio", "Listado para Actualizacion Datos", 1),
            new DefinicionProceso("Predio", "Listado para Asistencia a Reuniones", 1),
            // ===== RESIDENTE (Sub-objeto de Predio) =====
            new DefinicionProceso("Predio", "Registrar Nuevo Residente", 2),
            new DefinicionProceso("Predio", "Actualizacion Datos Residente", 2),
            new DefinicionProceso("Predio", "Listado Residentes", 1),
            // ===== DOCUMENTO =====
            new DefinicionProceso("Documento", "Cedula de Cuidadanía o Similar de Propietarios", 1),
            new DefinicionProceso("Documento", "Certificado de Libertad Predio", 1),
            new DefinicionProceso("Documento", "Contrato de Prestación de Servicios", 1),
            new DefinicionProceso("Documento", "Carta de Información a Cliente de Cambio de Contador", 1),
            // ===== TARIFA =====
            //new DefinicionProceso("Tarifa", "Actualizacion Datos Tarifa", 2),
            //new DefinicionProceso("Tarifa", "Listado Tarifas", 1),
            // ===== ESTRATO =====
            //new DefinicionProceso("Estrato", "Registrar Nuevo Estrato", 2),
            //new DefinicionProceso("Estrato", "Actualizacion Datos Estrato", 2),
            //new DefinicionProceso("Estrato", "Listado Estratos", 1),
            // ===== TIPOCONSUMO =====
            //new DefinicionProceso("TipoConsumo", "Registrar Nuevo Tipo Consumo", 2),
            //new DefinicionProceso("TipoConsumo", "Actualizacion Datos Tipo Consumo", 2),
            //new DefinicionProceso("TipoConsumo", "Listado de Tipos Consumo", 1),

            // ===== PERIODO - EDITAR =====
            new DefinicionProceso("Periodo", "Actualizar Periodo", 2),
            new DefinicionProceso("Periodo", "Eliminar Periodo", 2),
            new DefinicionProceso("Periodo", "Registrar Nuevo Periodo", 2),
            // ===== PERIODO - Informacion =====
            new DefinicionProceso("Periodo", "Busqueda por Datos Periodo", 1),
            new DefinicionProceso("Periodo", "Listado de Periodos", 1),

            // ===== FACTURA - EDITAR =====
            new DefinicionProceso("Factura", "Actualizar Factura por Recaudo Banco No Reportado", 2),
            new DefinicionProceso("Factura", "Actualizar Factura por Otros Motivos", 2),
            // ===== FACTURA - Informacion =====
            new DefinicionProceso("Factura", "Busqueda por Datos Factura", 1),
            new DefinicionProceso("Factura", "Listado de Facturas Por Mes", 1),
            new DefinicionProceso("Factura", "Listado de Facturas Por Bimestre", 1),

            // ===== RECAUDO - EDITAR =====
            new DefinicionProceso("Recaudo", "Actualizar Recaudo", 2),
            new DefinicionProceso("Recaudo", "Correccion de Recaudo", 2),
            new DefinicionProceso("Recaudo", "Eliminar Recaudo", 2),
            new DefinicionProceso("Recaudo", "Nuevo Recaudo", 2),
            // ===== RECAUDO - Informacion =====
            new DefinicionProceso("Recaudo", "Busqueda por Datos Recaudo", 1),
            new DefinicionProceso("Recaudo", "Listado de Recaudos por Mes", 1),
            new DefinicionProceso("Recaudo", "Listado de Recaudos por Bimestre", 1),
            new DefinicionProceso("Recaudo", "Listado de Impresion para Toma de Datos de Recaudo", 1),

            // ===== CONSUMO - EDITAR =====
            new DefinicionProceso("Consumo", "Actualizar Consumo por Nuevo Contador", 2),
            new DefinicionProceso("Consumo", "Correccion de Consumo", 2),
            new DefinicionProceso("Consumo", "Registrar Consumo de Servicio de Acueducto Bimestral", 2),
            // ===== CONSUMO - Informacion =====
            new DefinicionProceso("Consumo", "Busqueda por Datos Consumos", 1),
            new DefinicionProceso("Consumo", "Listado de Consumos por Predio", 1),
            new DefinicionProceso("Consumo", "Listado de Impresion para Toma de Datos de Consumo", 1),

            // ===== INVENTARIO - EDITAR =====
            new DefinicionProceso("Inventario", "Actualizar Inventario", 2),
            new DefinicionProceso("Inventario", "Correccion de Inventario", 2),
            new DefinicionProceso("Inventario", "Eliminar Inventario", 2),
            new DefinicionProceso("Inventario", "Elaborar Inventario Fisico de Productos Y Servicios", 2),
            new DefinicionProceso("Inventario", "Mantenimiento Inventarios", 2),
            new DefinicionProceso("Inventario", "Nuevo Producto o Servicio para Inventario", 2),
            // ===== INVENTARIO - Informacion =====
            new DefinicionProceso("Inventario", "Busqueda por Datos Inventario", 1),
            new DefinicionProceso("Inventario", "Listado de Inventario", 1),
            new DefinicionProceso("Inventario", "Listado de Mantenimiento de Inventarios", 1),
            new DefinicionProceso("Inventario", "Listado de Inventario Fisico de Existencias", 1),

            // ===== SOLICITUD - EDITAR =====
            new DefinicionProceso("Solicitud", "Actualizar Solicitud", 2),
            new DefinicionProceso("Solicitud", "Eliminar Solicitud", 2),
            new DefinicionProceso("Solicitud", "Nueva Solicitud", 2),
            // ===== SOLICITUD - Informacion =====
            new DefinicionProceso("Solicitud", "Busqueda por Datos Solicitud", 1),
            new DefinicionProceso("Solicitud", "Listado de Solicitudes por Tipo", 1),
            new DefinicionProceso("Solicitud", "Reporte de Solicitudes", 1),
            //// ===== TIPOSOLICITUD =====
            //new DefinicionProceso("TipoSolicitud", "Registrar Nuevo Tipo Solicitud", 2),
            //new DefinicionProceso("TipoSolicitud", "Actualizacion Datos Tipo Solicitud", 2),
            //new DefinicionProceso("TipoSolicitud", "Listado Tipo Solicitudes", 1),
            //// ===== ESTADOSOLICITUD =====
            //new DefinicionProceso("EstadoSolicitud", "Registrar Nuevo Estado Solicitud", 2),
            //new DefinicionProceso("EstadoSolicitud", "Actualizacion Datos Estado Solicitud", 2),
            //new DefinicionProceso("EstadoSolicitud", "Listado Estados Solicitud", 1),

            // ===== SEGURIDAD - EDITAR =====
            new DefinicionProceso("Seguridad", "Actualizar Acceso", 2),
            new DefinicionProceso("Seguridad", "Eliminar Acceso", 2),
            new DefinicionProceso("Seguridad", "Nuevo Acceso", 2),
            new DefinicionProceso("Seguridad", "Gestión de Roles", 2),
            new DefinicionProceso("Seguridad", "Asignar Roles a Usuarios", 2),
            // ===== SEGURIDAD - Informacion =====
            new DefinicionProceso("Seguridad", "Busqueda por Datos Acceso", 1),
            new DefinicionProceso("Seguridad", "Listado de Accesos", 1),
            new DefinicionProceso("Seguridad", "Busqueda de Acceso por Usuario", 1),
        };

        public static DefinicionProceso BuscarPorNombre(string nombre)
        {
            return Todos.FirstOrDefault(p => p.Nombre == nombre);
        }

        public static List<string> ObtenerObjetos()
        {
            return Todos.Select(p => p.Objeto).Distinct().ToList();
        }

        public static List<DefinicionProceso> ObtenerPorObjeto(string objeto)
        {
            return Todos.Where(p => p.Objeto == objeto).ToList();
        }
    }
}
