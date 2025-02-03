using XmlApiNfseGissApiBusiness.Interfaces;
using XmlApiNfseGissBusiness.Interfaces;
using XmlApiNfseGissBusiness.Services;
using XmlApiNfseGissInfra.Http;
using XmlApiNfseGissInfra.Interfaces;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHttpClient("NfseClient")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });

        builder.Services.AddSingleton<IHttpNfseClientFactory, HttpNfseClientFactory>();

        builder.Services.AddScoped<INfseService, NfseService>();
        builder.Services.AddSingleton<INfseHttpService, NfseHttpService>();
        builder.Services.AddSingleton<ICertificateService, CertificateService>();



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
    }
}