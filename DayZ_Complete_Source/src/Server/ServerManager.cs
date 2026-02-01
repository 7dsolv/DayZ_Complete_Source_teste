using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DayZ.Server
{
    public class ServerConfiguration
    {
        [JsonProperty("server_name")]
        public string ServerName { get; set; } = "DayZ 1.25 Server";

        [JsonProperty("server_ip")]
        public string ServerIP { get; set; } = "0.0.0.0";

        [JsonProperty("server_port")]
        public int ServerPort { get; set; } = 27015;

        [JsonProperty("max_players")]
        public int MaxPlayers { get; set; } = 60;

        [JsonProperty("map_name")]
        public string MapName { get; set; } = "Chernarus";

        [JsonProperty("difficulty")]
        public string Difficulty { get; set; } = "Normal";

        [JsonProperty("zombie_count")]
        public int ZombieCount { get; set; } = 1000;

        [JsonProperty("animal_count")]
        public int AnimalCount { get; set; } = 500;

        [JsonProperty("weather_enabled")]
        public bool WeatherEnabled { get; set; } = true;

        [JsonProperty("pvp_enabled")]
        public bool PVPEnabled { get; set; } = true;

        [JsonProperty("respawn_delay")]
        public float RespawnDelay { get; set; } = 30f;

        [JsonProperty("save_interval")]
        public float SaveInterval { get; set; } = 300f;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static ServerConfiguration LoadFromJson(string json)
        {
            return JsonConvert.DeserializeObject<ServerConfiguration>(json) ?? new ServerConfiguration();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class PlayerProfile
    {
        [JsonProperty("player_id")]
        public string PlayerId { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("player_name")]
        public string PlayerName { get; set; } = "Player";

        [JsonProperty("character_id")]
        public string CharacterId { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("health")]
        public float Health { get; set; } = 100f;

        [JsonProperty("hunger")]
        public float Hunger { get; set; } = 50f;

        [JsonProperty("thirst")]
        public float Thirst { get; set; } = 50f;

        [JsonProperty("blood_type")]
        public string BloodType { get; set; } = "O+";

        [JsonProperty("position")]
        public Dictionary<string, float> Position { get; set; } = new() { { "x", 0 }, { "y", 0 }, { "z", 0 } };

        [JsonProperty("inventory")]
        public List<string> Inventory { get; set; } = new();

        [JsonProperty("last_login")]
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        [JsonProperty("playtime_seconds")]
        public long PlaytimeSeconds { get; set; } = 0;
    }

    public class ServerManager
    {
        private ServerConfiguration _config;
        private Dictionary<string, PlayerProfile> _players = new();
        private List<string> _bannedPlayers = new();
        private List<string> _whitelistedPlayers = new();

        public ServerConfiguration Configuration => _config;
        public int PlayerCount => _players.Count;

        public ServerManager(ServerConfiguration? config = null)
        {
            _config = config ?? new ServerConfiguration();
        }

        public PlayerProfile CreatePlayer(string playerName)
        {
            var profile = new PlayerProfile { PlayerName = playerName };
            _players[profile.PlayerId] = profile;
            return profile;
        }

        public PlayerProfile? GetPlayer(string playerId)
        {
            return _players.TryGetValue(playerId, out var player) ? player : null;
        }

        public PlayerProfile? GetPlayerByName(string playerName)
        {
            return _players.Values.FirstOrDefault(p => p.PlayerName == playerName);
        }

        public bool RemovePlayer(string playerId)
        {
            return _players.Remove(playerId);
        }

        public IEnumerable<PlayerProfile> GetAllPlayers()
        {
            return _players.Values.ToList();
        }

        public void BanPlayer(string playerId)
        {
            var player = GetPlayer(playerId);
            if (player != null && !_bannedPlayers.Contains(player.PlayerName))
            {
                _bannedPlayers.Add(player.PlayerName);
            }
        }

        public void UnbanPlayer(string playerName)
        {
            _bannedPlayers.Remove(playerName);
        }

        public bool IsPlayerBanned(string playerName)
        {
            return _bannedPlayers.Contains(playerName);
        }

        public void WhitelistPlayer(string playerName)
        {
            if (!_whitelistedPlayers.Contains(playerName))
            {
                _whitelistedPlayers.Add(playerName);
            }
        }

        public void RemoveFromWhitelist(string playerName)
        {
            _whitelistedPlayers.Remove(playerName);
        }

        public bool IsPlayerWhitelisted(string playerName)
        {
            return _whitelistedPlayers.Count == 0 || _whitelistedPlayers.Contains(playerName);
        }

        public string GetServerStatus()
        {
            return $"{_config.ServerName} - Players: {PlayerCount}/{_config.MaxPlayers} - Zombies: {_config.ZombieCount}";
        }
    }
}
