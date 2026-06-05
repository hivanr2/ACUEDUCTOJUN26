using System.Collections.Generic;
using System.Linq;

namespace ACUEDUCTO2-0_0.Compartir.MODELS
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
            new DefinicionProceso("Usuario", "Actualizacion Datos Usuario", TipoProceso.Editar),
            new DefinicionProceso("Usuario", "Cancelacion Servicio Acueducto Predio", TipoProceso.Editar),
            new DefinicionProceso("Usuario", "Elaboracion Contrato Prestacion Servicio", TipoProceso.Editar),
            new DefinicionProceso("Usuario", "Eliminacion Datos Usuario", TipoProceso.Editar),
            new DefinicionProceso("Usuario", "Pagar Factura", TipoProceso.Editar),
            new DefinicionProceso("Usuario", "Pagar Otros Cobros", TipoProceso.Editar),
            new DefinicionProceso("Usuario", "Registro Nuevo Usuario Propietario Predio", TipoProceso.Editar),
            new DefinicionProceso("Usuario", "Registro Nuevo Usuario Residente Predio", TipoProceso.Editar),
            // ===== USUARIO - Informacion =====
            new DefinicionProceso("Usuario", "Busqueda por Datos Usuario", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Cedula Ciudadania o similar de Propietarios", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Certificado Libertad de Predio", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Contrato de Prestacion Servicios", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Estatutos Acueducto", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Instructivo para Registro de Nuevo Usuario", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Listado de Solicitudes por Usuario", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Listado de Usuarios", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Requisitos Inscripcion Nuevo Usuario", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Total a Pagar Bimestre", TipoProceso.Informacion),
            new DefinicionProceso("Usuario", "Total a Pagar Deuda", TipoProceso.Informacion),
            // ===== AUTORIZADO =====
            new DefinicionProceso("Autorizado", "Registrar Nuevo Autorizado", TipoProceso.Editar),
            new DefinicionProceso("Autorizado", "Actualizacion Datos Autorizado", TipoProceso.Editar),
            new DefinicionProceso("Autorizado", "Listado Autorizados", TipoProceso.Informacion),
            // ===== CARGOJAC (Sub-objeto de Usuario) =====
            new DefinicionProceso("Usuario", "Registrar Nuevo Cargo", TipoProceso.Editar),
            new DefinicionProceso("Usuario", "Actualizacion Datos Cargo", TipoProceso.Editar),
            new DefinicionProceso("Usuario", "Listado Cargos", TipoProceso.Informacion),

            // ===== PREDIO - EDITAR =====
            new DefinicionProceso("Predio", "Actualizacion Datos Predio", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Cambio Contador por parte del Acueducto Con Cobro de Instalacion", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Cambio Contador por parte del Acueducto Sin Cobro de Instalacion", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Cambio Contador por parte del Cliente Con Cobro de Instalacion", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Cambio Contador por parte del Cliente Sin Cobro de Instalacion", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Cambio de Usuarios al que se Factura el Predio", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Cobro cuota Extraordinaria a predios activos", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Eliminar Informacion de Predio o Punto de Agua de Servicio de Acueducto", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Eliminar Predio o Punto de Agua de Servicio de Acueducto", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Envio de Mensaje a Residentes y Usuario de Predios Activos", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Mantenimiento de Caja de Contador", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Mantenimiento de Contador de Predio", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Mantenimiento de Instalaciones Externas de Predio", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Mantenimiento de Instalaciones Internas de Predio", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Mantenimiento de Tapa de Contador", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Reconexion de Servicio de Acueducto", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Registrar Nuevo Predio o Punto de Agua de Servicio de Acueducto", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Suspencion de Servicio de Acueducto por Mandato Legal", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Suspencion de Servicio de Acueducto por No Pago", TipoProceso.Editar),
            // ===== PREDIO - Informacion =====
            new DefinicionProceso("Predio", "Busqueda por Datos Predio", TipoProceso.Informacion),
            new DefinicionProceso("Predio", "Carta de Informacion de Cambio de Contador", TipoProceso.Informacion),
            new DefinicionProceso("Predio", "Copia Certificado Libertad no mayor a 60 dias", TipoProceso.Informacion),
            new DefinicionProceso("Predio", "Instructivo para crear Nuevo Punto de Agua de Servicio Acueducto", TipoProceso.Informacion),
            new DefinicionProceso("Predio", "Instructivo para Eliminar Predio o Punto de Agua de Servicio Acueducto", TipoProceso.Informacion),
            new DefinicionProceso("Predio", "Listado de Estado de Contadores por Predio", TipoProceso.Informacion),
            new DefinicionProceso("Predio", "Listado de Mantenimientos de Predios", TipoProceso.Informacion),
            new DefinicionProceso("Predio", "Listado de Predios", TipoProceso.Informacion),
            new DefinicionProceso("Predio", "Listado para Actualizacion Datos", TipoProceso.Informacion),
            new DefinicionProceso("Predio", "Listado para Asistencia a Reuniones", TipoProceso.Informacion),
            // ===== RESIDENTE (Sub-objeto de Predio) =====
            new DefinicionProceso("Predio", "Registrar Nuevo Residente", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Actualizacion Datos Residente", TipoProceso.Editar),
            new DefinicionProceso("Predio", "Listado Residentes", TipoProceso.Informacion),
            // ===== TARIFA =====
            //new DefinicionProceso("Tarifa", "Registrar Nueva Tarifa", TipoProceso.Editar),
            //new DefinicionProceso("Tarifa", "Actualizacion Datos Tarifa", TipoProceso.Editar),
            //new DefinicionProceso("Tarifa", "Listado Tarifas", TipoProceso.Informacion),
            // ===== ESTRATO =====
            //new DefinicionProceso("Estrato", "Registrar Nuevo Estrato", TipoProceso.Editar),
            //new DefinicionProceso("Estrato", "Actualizacion Datos Estrato", TipoProceso.Editar),
            //new DefinicionProceso("Estrato", "Listado Estratos", TipoProceso.Informacion),
            // ===== TIPOCONSUMO =====
            //new DefinicionProceso("TipoConsumo", "Registrar Nuevo Tipo Consumo", TipoProceso.Editar),
            //new DefinicionProceso("TipoConsumo", "Actualizacion Datos Tipo Consumo", TipoProceso.Editar),
            //new DefinicionProceso("TipoConsumo", "Listado de Tipos Consumo", TipoProceso.Informacion),

            // ===== PERIODO - EDITAR =====
            new DefinicionProceso("Periodo", "Actualizar Periodo", TipoProceso.Editar),
            new DefinicionProceso("Periodo", "Eliminar Periodo", TipoProceso.Editar),
            new DefinicionProceso("Periodo", "Registrar Nuevo Periodo", TipoProceso.Editar),
            // ===== PERIODO - Informacion =====
            new DefinicionProceso("Periodo", "Busqueda por Datos Periodo", TipoProceso.Informacion),
            new DefinicionProceso("Periodo", "Listado de Periodos", TipoProceso.Informacion),

            // ===== FACTURA - EDITAR =====
            new DefinicionProceso("Factura", "Actualizar Factura por Recaudo Banco No Reportado", TipoProceso.Editar),
            new DefinicionProceso("Factura", "Actualizar Factura por Otros Motivos", TipoProceso.Editar),
            // ===== FACTURA - Informacion =====
            new DefinicionProceso("Factura", "Busqueda por Datos Factura", TipoProceso.Informacion),
            new DefinicionProceso("Factura", "Listado de Facturas Por Mes", TipoProceso.Informacion),
            new DefinicionProceso("Factura", "Listado de Facturas Por Bimestre", TipoProceso.Informacion),

            // ===== RECAUDO - EDITAR =====
            new DefinicionProceso("Recaudo", "Actualizar Recaudo", TipoProceso.Editar),
            new DefinicionProceso("Recaudo", "Correccion de Recaudo", TipoProceso.Editar),
            new DefinicionProceso("Recaudo", "Eliminar Recaudo", TipoProceso.Editar),
            new DefinicionProceso("Recaudo", "Nuevo Recaudo", TipoProceso.Editar),
            // ===== RECAUDO - Informacion =====
            new DefinicionProceso("Recaudo", "Busqueda por Datos Recaudo", TipoProceso.Informacion),
            new DefinicionProceso("Recaudo", "Listado de Recaudos por Mes", TipoProceso.Informacion),
            new DefinicionProceso("Recaudo", "Listado de Recaudos por Bimestre", TipoProceso.Informacion),
            new DefinicionProceso("Recaudo", "Listado de Impresion para Toma de Datos de Recaudo", TipoProceso.Informacion),

            // ===== CONSUMO - EDITAR =====
            new DefinicionProceso("Consumo", "Actualizar Consumo por Nuevo Contador", TipoProceso.Editar),
            new DefinicionProceso("Consumo", "Correccion de Consumo", TipoProceso.Editar),
            new DefinicionProceso("Consumo", "Registrar Consumo de Servicio de Acueducto Bimestral", TipoProceso.Editar),
            // ===== CONSUMO - Informacion =====
            new DefinicionProceso("Consumo", "Busqueda por Datos Consumos", TipoProceso.Informacion),
            new DefinicionProceso("Consumo", "Listado de Consumos por Predio", TipoProceso.Informacion),
            new DefinicionProceso("Consumo", "Listado de Impresion para Toma de Datos de Consumo", TipoProceso.Informacion),

            // ===== INVENTARIO - EDITAR =====
            new DefinicionProceso("Inventario", "Actualizar Inventario", TipoProceso.Editar),
            new DefinicionProceso("Inventario", "Correccion de Inventario", TipoProceso.Editar),
            new DefinicionProceso("Inventario", "Eliminar Inventario", TipoProceso.Editar),
            new DefinicionProceso("Inventario", "Elaborar Inventario Fisico de Productos Y Servicios", TipoProceso.Editar),
            new DefinicionProceso("Inventario", "Mantenimiento Inventarios", TipoProceso.Editar),
            new DefinicionProceso("Inventario", "Nuevo Producto o Servicio para Inventario", TipoProceso.Editar),
            // ===== INVENTARIO - Informacion =====
            new DefinicionProceso("Inventario", "Busqueda por Datos Inventario", TipoProceso.Informacion),
            new DefinicionProceso("Inventario", "Listado de Inventario", TipoProceso.Informacion),
            new DefinicionProceso("Inventario", "Listado de Mantenimiento de Inventarios", TipoProceso.Informacion),
            new DefinicionProceso("Inventario", "Listado de Inventario Fisico de Existencias", TipoProceso.Informacion),

            // ===== SOLICITUD - EDITAR =====
            new DefinicionProceso("Solicitud", "Actualizar Solicitud", TipoProceso.Editar),
            new DefinicionProceso("Solicitud", "Eliminar Solicitud", TipoProceso.Editar),
            new DefinicionProceso("Solicitud", "Nueva Solicitud", TipoProceso.Editar),
            // ===== SOLICITUD - Informacion =====
            new DefinicionProceso("Solicitud", "Busqueda por Datos Solicitud", TipoProceso.Informacion),
            new DefinicionProceso("Solicitud", "Listado de Solicitudes por Tipo", TipoProceso.Informacion),
            new DefinicionProceso("Solicitud", "Reporte de Solicitudes", TipoProceso.Informacion),
            //// ===== TIPOSOLICITUD =====
            //new DefinicionProceso("TipoSolicitud", "Registrar Nuevo Tipo Solicitud", TipoProceso.Editar),
            //new DefinicionProceso("TipoSolicitud", "Actualizacion Datos Tipo Solicitud", TipoProceso.Editar),
            //new DefinicionProceso("TipoSolicitud", "Listado Tipo Solicitudes", TipoProceso.Informacion),
            //// ===== ESTADOSOLICITUD =====
            //new DefinicionProceso("EstadoSolicitud", "Registrar Nuevo Estado Solicitud", TipoProceso.Editar),
            //new DefinicionProceso("EstadoSolicitud", "Actualizacion Datos Estado Solicitud", TipoProceso.Editar),
            //new DefinicionProceso("EstadoSolicitud", "Listado Estados Solicitud", TipoProceso.Informacion),

            // ===== SEGURIDAD - EDITAR =====
            new DefinicionProceso("Seguridad", "Actualizar Acceso", TipoProceso.Editar),
            new DefinicionProceso("Seguridad", "Eliminar Acceso", TipoProceso.Editar),
            new DefinicionProceso("Seguridad", "Nuevo Acceso", TipoProceso.Editar),
            new DefinicionProceso("Seguridad", "Gestión de Roles", TipoProceso.Editar),
            new DefinicionProceso("Seguridad", "Asignar Roles a Usuarios", TipoProceso.Editar),
            // ===== SEGURIDAD - Informacion =====
            new DefinicionProceso("Seguridad", "Busqueda por Datos Acceso", TipoProceso.Informacion),
            new DefinicionProceso("Seguridad", "Listado de Accesos", TipoProceso.Informacion),
            new DefinicionProceso("Seguridad", "Busqueda de Acceso por Usuario", TipoProceso.Informacion),
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
