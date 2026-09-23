using MongoDB.Driver;
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

builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<GenreService>();
builder.Services.AddScoped<PlatformService>();
builder.Services.AddScoped<DeveloperService>();
builder.Services.AddScoped<PublisherService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<StatisticsService>();

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

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    await DbInitializer.InitializeIndexesAsync(database);
}

app.Run();
