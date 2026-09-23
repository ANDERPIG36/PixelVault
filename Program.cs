using MongoDB.Driver;
using PixelVault.Models;
using PixelVault.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetSection("MongoDbSettings:ConnectionString").Value 
    ?? "mongodb://localhost:27017";
var databaseName = builder.Configuration.GetSection("MongoDbSettings:DatabaseName").Value 
    ?? "PixelVaultDb";

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
builder.Services.AddScoped<IMongoDatabase>(sp => {
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

// Registrazione servizi dell'applicazione
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<GenreService>();
builder.Services.AddScoped<PlatformService>();
builder.Services.AddScoped<DeveloperService>();
builder.Services.AddScoped<PublisherService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<StatisticsService>();

// Registrazione HttpClient e del servizio di importazione da API esterne
builder.Services.AddHttpClient();
builder.Services.AddScoped<GameImporterService>();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Inizializzazione Indici e Importazione Dati Reali al primo avvio
using (var scope = app.Services.CreateScope()) {
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    var importer = scope.ServiceProvider.GetRequiredService<GameImporterService>();

    // 1. Inizializza gli indici del DB
    await DbInitializer.InitializeIndexesAsync(database);

    // 2. Se la collezione dei giochi è vuota, effettua l'importazione live
    var gamesCount = await database.GetCollection<Game>("Games").CountDocumentsAsync(_ => true);
    if (gamesCount == 0) {
        // Legge la chiave API definita in appsettings.json sotto "RawgSettings:ApiKey"
        var rawgApiKey = builder.Configuration["RawgSettings:ApiKey"];

        if (!string.IsNullOrWhiteSpace(rawgApiKey)) {
            // Scarica 2 pagine di giochi reali popolari (40 giochi con Metacritic e prezzi reali da CheapShark)
            await importer.ImportRealGamesAsync(rawgApiKey, pageCount: 2);
        }
    }
}

app.Run();