using ACUEDUCTO.API.Options;
using ACUEDUCTO.API.Services;
using ACUEDUCTO.Models;
using Microsoft.EntityFrameworkCore;
using ACUEDUCTO.API.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContextFactory<AcueductoContext>(options => options.UseSqlServer(connectionString));
}

builder.Services.AddOptions<OcrOptions>()
    .Bind(builder.Configuration.GetSection(OcrOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.AddHttpClient<GptVisionConsumoOcrService>();

builder.Services.AddScoped<IConsumoOcrService>(sp =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OcrOptions>>().Value;
    if (opts.UseGptVisionAlways)
        return sp.GetRequiredService<GptVisionConsumoOcrService>();

    return ActivatorUtilities.CreateInstance<TesseractConsumoOcrService>(sp);
});
builder.Services.AddScoped<ConsumoOcrWorkflow>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
