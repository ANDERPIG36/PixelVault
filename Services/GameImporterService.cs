using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public class GameImporterService {
    private readonly IMongoDatabase _database;
    private readonly HttpClient _httpClient;

    public GameImporterService(IMongoDatabase database, HttpClient httpClient) {
        _database = database;
        _httpClient = httpClient;
    }

    public async Task<int> ImportRealGamesAsync(string rawgApiKey, int pageCount = 2) {
        var gamesCol = _database.GetCollection<Game>("Games");
        var genresCol = _database.GetCollection<Genre>("Genres");
        var devsCol = _database.GetCollection<Developer>("Developers");
        var pubsCol = _database.GetCollection<Publisher>("Publishers");

        int totalImported = 0;

        for (int page = 1; page <= pageCount; page++) {
            // Recupera i giochi più popolari da RAWG con Metacritic e informazioni reali
            var rawgUrl = $"https://api.rawg.io/api/games?key={rawgApiKey}&page={page}&page_size=20&ordering=-added";
            var rawgResponse = await _httpClient.GetAsync(rawgUrl);
            if (!rawgResponse.IsSuccessStatusCode) break;

            var rawgJson = await rawgResponse.Content.ReadAsStringAsync();
            var rawgData = JsonSerializer.Deserialize<RawgResponse>(rawgJson);

            if (rawgData?.Results == null) continue;

            foreach (var item in rawgData.Results) {
                if (string.IsNullOrWhiteSpace(item.Name)) continue;

                // Evita duplicati nel DB
                var existingGame = await gamesCol.Find(g => g.Title == item.Name).FirstOrDefaultAsync();
                if (existingGame != null) continue;

                // 1. Recupera il dettaglio completo del gioco su RAWG per avere sviluppatori e publisher reali
                var detailUrl = $"https://api.rawg.io/api/games/{item.Id}?key={rawgApiKey}";
                var detailResponse = await _httpClient.GetAsync(detailUrl);
                RawgGameDetail? gameDetail = null;

                if (detailResponse.IsSuccessStatusCode) {
                    var detailJson = await detailResponse.Content.ReadAsStringAsync();
                    gameDetail = JsonSerializer.Deserialize<RawgGameDetail>(detailJson);
                }

                // 2. Mappatura Generi Reali
                var genreIds = new List<string>();
                if (item.Genres != null) {
                    foreach (var g in item.Genres) {
                        var genre = await genresCol.Find(x => x.Name == g.Name).FirstOrDefaultAsync();
                        if (genre == null) {
                            genre = new Genre { Name = g.Name };
                            await genresCol.InsertOneAsync(genre);
                        }
                        if (genre.Id != null) {
                            genreIds.Add(genre.Id);
                        }
                    }
                }

                // 3. Mappatura Sviluppatore Reale
                string? developerId = null;
                var devName = gameDetail?.Developers?.FirstOrDefault()?.Name ?? "Sconosciuto";
                if (devName != "Sconosciuto") {
                    var dev = await devsCol.Find(d => d.Name == devName).FirstOrDefaultAsync();
                    if (dev == null) {
                        dev = new Developer { Name = devName };
                        await devsCol.InsertOneAsync(dev);
                    }
                    developerId = dev.Id;
                }

                // 4. Mappatura Editore/Publisher Reale
                string? publisherId = null;
                var pubName = gameDetail?.Publishers?.FirstOrDefault()?.Name ?? "Sconosciuto";
                if (pubName != "Sconosciuto") {
                    var pub = await pubsCol.Find(p => p.Name == pubName).FirstOrDefaultAsync();
                    if (pub == null) {
                        pub = new Publisher { Name = pubName };
                        await pubsCol.InsertOneAsync(pub);
                    }
                    publisherId = pub.Id;
                }

                // 5. Recupero PREZZO REALE e SCONTO REALE via CheapShark API (senza API Key)
                var (realPrice, realDiscount) = await GetRealPriceAndDiscountAsync(item.Name);

                // 6. VALUTAZIONE REALE: Priorità a Metacritic (scala 1-100 convertita in 1-10), altrimenti Rating utenti RAWG
                double realRating = 0;
                if (item.Metacritic.HasValue && item.Metacritic.Value > 0) {
                    realRating = Math.Round(item.Metacritic.Value / 10.0, 1);
                } else if (item.Rating.HasValue && item.Rating.Value > 0) {
                    realRating = Math.Round((item.Rating.Value / 5.0) * 10, 1);
                }

                var newGame = new Game {
                    Title = item.Name,
                    GenreIds = genreIds,
                    DeveloperId = developerId,
                    PublisherId = publisherId,
                    ReleaseDate = item.Released ?? DateTime.Today,
                    Rating = realRating,
                    Price = realPrice,
                    Discount = realDiscount
                };

                await gamesCol.InsertOneAsync(newGame);
                totalImported++;
            }
        }

        return totalImported;
    }

    private async Task<(decimal Price, int Discount)> GetRealPriceAndDiscountAsync(string title) {
        try {
            var encodedTitle = Uri.EscapeDataString(title);
            var cheapSharkUrl = $"https://www.cheapshark.com/api/1.0/deals?title={encodedTitle}&limit=1";
            var response = await _httpClient.GetAsync(cheapSharkUrl);

            if (response.IsSuccessStatusCode) {
                var json = await response.Content.ReadAsStringAsync();
                var deals = JsonSerializer.Deserialize<List<CheapSharkDeal>>(json);

                if (deals != null && deals.Any()) {
                    var deal = deals.First();
                    if (decimal.TryParse(deal.RetailPrice, CultureInfo.InvariantCulture, out decimal price) &&
                        decimal.TryParse(deal.Savings, CultureInfo.InvariantCulture, out decimal savings)) {
                        
                        int discount = (int)Math.Round(savings);
                        return (price, discount);
                    }
                }
            }
        } catch {
            // Ignora eventuali micro-timeout di CheapShark
        }

        // Default reale di listino per giochi tripla A dove non viene trovata un'offerta attiva
        return (59.99m, 0);
    }

    // Classi di deserializzazione RAWG
    private class RawgResponse {
        [JsonPropertyName("results")] public List<RawgItem>? Results { get; set; }
    }

    private class RawgItem {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("released")] public DateTime? Released { get; set; }
        [JsonPropertyName("rating")] public double? Rating { get; set; }
        [JsonPropertyName("metacritic")] public int? Metacritic { get; set; }
        [JsonPropertyName("genres")] public List<NamedEntity>? Genres { get; set; }
    }

    private class RawgGameDetail {
        [JsonPropertyName("developers")] public List<NamedEntity>? Developers { get; set; }
        [JsonPropertyName("publishers")] public List<NamedEntity>? Publishers { get; set; }
    }

    private class NamedEntity {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    }

    // Classi di deserializzazione CheapShark
    private class CheapSharkDeal {
        [JsonPropertyName("retailPrice")] public string? RetailPrice { get; set; }
        [JsonPropertyName("savings")] public string? Savings { get; set; }
    }
}