using PricePaid.Api.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var csvPath = builder.Configuration["CsvFilePath"] 
    ?? throw new InvalidOperationException("CsvFilePath is not configured");

builder.Services.AddSingleton<ICsvReaderService>(new CsvReaderService(csvPath));
builder.Services.AddScoped<ITransactionsService, TransactionsService>();

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program { }