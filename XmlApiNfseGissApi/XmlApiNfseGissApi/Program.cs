using XmlApiNfseGissApiBusiness.Interfaces;
using XmlApiNfseGissBusiness.Interfaces;
using XmlApiNfseGissBusiness.Services;
using XmlApiNfseGissInfra.Http;
using XmlApiNfseGissInfra.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("NfseClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // Ignorar SSL inválido (se necessário)
    });

builder.Services.AddSingleton<IHttpNfseClientFactory, HttpNfseClientFactory>();

builder.Services.AddScoped<INfseService, NfseService>();
builder.Services.AddSingleton<INfseHttpService, NfseHttpService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
