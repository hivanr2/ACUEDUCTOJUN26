using ACUEDUCTO2.Components;
using ACUEDUCTO2.Components.Account;
using ACUEDUCTO2.Data;
using ACUEDUCTO2.Share.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using ACUEDUCTO2.Services.ProcesoServices;
using ACUEDUCTO2.Services.PeriodoServices;
using ACUEDUCTO2.Services.PredioServices;
using ACUEDUCTO2.Services.MigrationServices;
using ACUEDUCTO2.Services.ConsumoServices;
using ACUEDUCTO2.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Ensure Blazor component services are registered before building the app
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

// Configure services
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure middleware
app.UseApplicationMiddleware();

app.Run();
