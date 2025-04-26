using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace dotamix.Services
{
    public class SteamService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public SteamService(IConfiguration configuration)
        {
            _apiKey = configuration["Steam:ApiKey"] ?? string.Empty;
            _httpClient = new HttpClient();
        }

        public async Task<SteamUserInfo> GetUserInfoAsync(string steamId)
        {
            try
            {
                // Используем прямой запрос к API Steam
                var response = await _httpClient.GetAsync($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={_apiKey}&steamids={steamId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<SteamApiResponse>(content);

                    if (result?.Response?.Players?.Count > 0)
                    {
                        var player = result.Response.Players[0];
                        return new SteamUserInfo
                        {
                            SteamId = player.SteamId,
                            Nickname = player.PersonaName,
                            ProfileUrl = player.ProfileUrl,
                            AvatarUrl = player.AvatarFull,
                            IsOnline = player.PersonaState == 1
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                // В реальном приложении здесь должно быть логирование
                Console.WriteLine($"Ошибка при получении информации о пользователе Steam: {ex.Message}");
                return null;
            }
        }

        public async Task<Dota2PlayerInfo> GetDota2PlayerInfoAsync(string steamId)
        {
            try
            {
                // Используем прямой запрос к API Dota 2
                var response = await _httpClient.GetAsync($"https://api.steampowered.com/IDOTA2Match_570/GetPlayerStats/v1/?key={_apiKey}&steamid={steamId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dota2ApiResponse>(content);

                    if (result?.Result?.Stats != null)
                    {
                        var stats = result.Result.Stats;
                        return new Dota2PlayerInfo
                        {
                            SteamId = steamId,
                            MatchesPlayed = GetStatValue(stats, "matches_played"),
                            Wins = GetStatValue(stats, "wins"),
                            Losses = GetStatValue(stats, "losses"),
                            Kills = GetStatValue(stats, "kills"),
                            Deaths = GetStatValue(stats, "deaths"),
                            Assists = GetStatValue(stats, "assists"),
                            Gpm = GetStatValue(stats, "gpm"),
                            Xpm = GetStatValue(stats, "xpm"),
                            HeroDamage = GetStatValue(stats, "hero_damage"),
                            TowerDamage = GetStatValue(stats, "tower_damage"),
                            HeroHealing = GetStatValue(stats, "hero_healing"),
                            Mmr = 0, // MMR не доступен через API
                            Rank = 0  // Ранг не доступен через API
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                // В реальном приложении здесь должно быть логирование
                Console.WriteLine($"Ошибка при получении статистики Dota 2: {ex.Message}");
                return null;
            }
        }

        private int GetStatValue(JsonElement stats, string name)
        {
            try
            {
                foreach (var stat in stats.EnumerateArray())
                {
                    if (stat.GetProperty("name").GetString() == name)
                    {
                        return stat.GetProperty("value").GetInt32();
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при парсинге
            }

            return 0;
        }
    }

    public class SteamUserInfo
    {
        public string SteamId { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string ProfileUrl { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
    }

    public class Dota2PlayerInfo
    {
        public string SteamId { get; set; } = string.Empty;
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int Gpm { get; set; }
        public int Xpm { get; set; }
        public int HeroDamage { get; set; }
        public int TowerDamage { get; set; }
        public int HeroHealing { get; set; }
        public int Mmr { get; set; }
        public int Rank { get; set; }
    }

    // Классы для десериализации ответов API
    public class SteamApiResponse
    {
        public SteamResponse Response { get; set; } = new SteamResponse();
    }

    public class SteamResponse
    {
        public List<SteamPlayer> Players { get; set; } = new List<SteamPlayer>();
    }

    public class SteamPlayer
    {
        public string SteamId { get; set; } = string.Empty;
        public string PersonaName { get; set; } = string.Empty;
        public string ProfileUrl { get; set; } = string.Empty;
        public string AvatarFull { get; set; } = string.Empty;
        public int PersonaState { get; set; }
    }

    public class Dota2ApiResponse
    {
        public Dota2Result Result { get; set; } = new Dota2Result();
    }

    public class Dota2Result
    {
        public JsonElement Stats { get; set; }
    }
}