using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACUEDUCTO2.Migrations
{
    /// <inheritdoc />
    public partial class uno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predio_Estrato",
                table: "Predio");

            migrationBuilder.DropForeignKey(
                name: "FK_Predio_TipoConsumo",
                table: "Predio");

            migrationBuilder.DropForeignKey(
                name: "FK_Predio_Usuario",
                table: "Predio");

            migrationBuilder.DropTable(
                name: "Autorizado");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Predio_IdentificacionFacturacion",
                table: "Predio");

            migrationBuilder.DropIndex(
                name: "IX_Predio_IdEstrato",
                table: "Predio");

            migrationBuilder.DropIndex(
                name: "IX_Predio_IdTipoConsumo",
                table: "Predio");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_Identificacion",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Periodo",
                table: "Periodo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Facturacion",
                table: "Facturacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Estrato",
                table: "Estrato");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Consumo",
                table: "Consumo");

            migrationBuilder.DropColumn(
                name: "FechaHoraIniciaRegistroConsumo",
                table: "Periodo");

            migrationBuilder.RenameTable(
                name: "Periodo",
                newName: "Periodos");

            migrationBuilder.RenameTable(
                name: "Facturacion",
                newName: "Facturacions");

            migrationBuilder.RenameTable(
                name: "Estrato",
                newName: "Estratos");

            migrationBuilder.RenameTable(
                name: "Consumo",
                newName: "Consumos");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "TipoProceso",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdTipoConsumo",
                table: "TipoConsumo",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ApellidosYNombresRegistraSolicitud",
                table: "Solicitud",
                newName: "ApellidosNombresRegistraSolicitud");

            migrationBuilder.RenameColumn(
                name: "IdEstrato",
                table: "Predio",
                newName: "Estrato");

            migrationBuilder.RenameColumn(
                name: "IDPeriodo",
                table: "Periodos",
                newName: "PeriodoReferenciaId");

            migrationBuilder.RenameColumn(
                name: "IDPlanilla",
                table: "Periodos",
                newName: "Guid");

            migrationBuilder.RenameColumn(
                name: "IDPredio",
                table: "Facturacions",
                newName: "IdPredio");

            migrationBuilder.RenameColumn(
                name: "NotasRecaudo",
                table: "Facturacions",
                newName: "NotaDeRecaudo");

            migrationBuilder.RenameColumn(
                name: "NotasFactura",
                table: "Facturacions",
                newName: "NotaDeFactura");

            migrationBuilder.RenameColumn(
                name: "IDPlanilla",
                table: "Facturacions",
                newName: "IdPeriodo");

            migrationBuilder.RenameColumn(
                name: "IDFactura",
                table: "Facturacions",
                newName: "Guid");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Proceso",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "rowversion",
                oldRowVersion: true,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdentificacionFacturacion",
                table: "Predio",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstratoNavigationIdEstrato",
                table: "Predio",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoConsumoNavigationId",
                table: "Predio",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Identificacion",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioSistemaCreaRegistro",
                table: "Periodos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UltimaFacturaRegistrada",
                table: "Periodos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Periodos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NombreLargoPeriodo",
                table: "Periodos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NombreCortoPeriodo",
                table: "Periodos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaSuspension",
                table: "Periodos",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaLimitePago",
                table: "Periodos",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraFacturacion",
                table: "Periodos",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EncargadoRegistroConsumos",
                table: "Periodos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EncargadoFacturacion",
                table: "Periodos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Periodos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Guid",
                table: "Periodos",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "IdPredio",
                table: "Facturacions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraRegistro",
                table: "Facturacions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoUsuario",
                table: "Facturacions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoFactura",
                table: "Facturacions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotaDeRecaudo",
                table: "Facturacions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotaDeFactura",
                table: "Facturacions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdPeriodo",
                table: "Facturacions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Guid",
                table: "Facturacions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "IdEstrato",
                table: "Estratos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Periodos",
                table: "Periodos",
                column: "Guid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Facturacions",
                table: "Facturacions",
                column: "Guid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Estratos",
                table: "Estratos",
                column: "IdEstrato");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Consumos",
                table: "Consumos",
                column: "Guid");

            migrationBuilder.CreateIndex(
                name: "IX_Predio_EstratoNavigationIdEstrato",
                table: "Predio",
                column: "EstratoNavigationIdEstrato");

            migrationBuilder.CreateIndex(
                name: "IX_Predio_TipoConsumoNavigationId",
                table: "Predio",
                column: "TipoConsumoNavigationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Predio_Estratos_EstratoNavigationIdEstrato",
                table: "Predio",
                column: "EstratoNavigationIdEstrato",
                principalTable: "Estratos",
                principalColumn: "IdEstrato");

            migrationBuilder.AddForeignKey(
                name: "FK_Predio_TipoConsumo_TipoConsumoNavigationId",
                table: "Predio",
                column: "TipoConsumoNavigationId",
                principalTable: "TipoConsumo",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
