using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DayZ.Mods
{
    public interface IMod
    {
        string ModId { get; }
        string ModName { get; }
        string Version { get; }
        void Initialize();
        void OnGameStart();
        void OnGameStop();
        void OnPlayerJoin(string playerId);
        void OnPlayerLeave(string playerId);
    }

    public abstract class BaseMod : IMod
    {
        public virtual string ModId { get; } = Guid.NewGuid().ToString();
        public virtual string ModName { get; } = "Unknown Mod";
        public virtual string Version { get; } = "1.0.0";

        public virtual void Initialize() { }
        public virtual void OnGameStart() { }
        public virtual void OnGameStop() { }
        public virtual void OnPlayerJoin(string playerId) { }
        public virtual void OnPlayerLeave(string playerId) { }
    }

    public class ModMetadata
    {
        [JsonProperty("mod_id")]
        public string ModId { get; set; } = "";

        [JsonProperty("mod_name")]
        public string ModName { get; set; } = "";

        [JsonProperty("version")]
        public string Version { get; set; } = "1.0.0";

        [JsonProperty("author")]
        public string Author { get; set; } = "Unknown";

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("dependencies")]
        public List<string> Dependencies { get; set; } = new();

        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("load_priority")]
        public int LoadPriority { get; set; } = 0;
    }

    public class ModManager
    {
        private Dictionary<string, IMod> _loadedMods = new();
        private Dictionary<string, ModMetadata> _modMetadata = new();
        private List<string> _modLoadOrder = new();

        public int LoadedModCount => _loadedMods.Count;

        public void RegisterMod(IMod mod, ModMetadata metadata)
        {
            if (_loadedMods.ContainsKey(mod.ModId))
                throw new InvalidOperationException($"Mod {mod.ModId} already registered");

            _loadedMods[mod.ModId] = mod;
            _modMetadata[mod.ModId] = metadata;
            _modLoadOrder.Add(mod.ModId);
        }

        public IMod? GetMod(string modId)
        {
            return _loadedMods.TryGetValue(modId, out var mod) ? mod : null;
        }

        public ModMetadata? GetModMetadata(string modId)
        {
            return _modMetadata.TryGetValue(modId, out var metadata) ? metadata : null;
        }

        public bool UnloadMod(string modId)
        {
            if (_loadedMods.Remove(modId))
            {
                _modMetadata.Remove(modId);
                _modLoadOrder.Remove(modId);
                return true;
            }
            return false;
        }

        public IEnumerable<IMod> GetAllMods()
        {
            return _loadedMods.Values.ToList();
        }

        public IEnumerable<string> GetModLoadOrder()
        {
            return _modLoadOrder.ToList();
        }

        public void InitializeAllMods()
        {
            foreach (var modId in _modLoadOrder)
            {
                if (_loadedMods.TryGetValue(modId, out var mod))
                {
                    try
                    {
                        mod.Initialize();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error initializing mod {mod.ModName}: {ex.Message}");
                    }
                }
            }
        }

        public void StartAllMods()
        {
            foreach (var mod in _loadedMods.Values)
            {
                try
                {
                    mod.OnGameStart();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting mod {mod.ModName}: {ex.Message}");
                }
            }
        }

        public void StopAllMods()
        {
            foreach (var mod in _loadedMods.Values)
            {
                try
                {
                    mod.OnGameStop();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping mod {mod.ModName}: {ex.Message}");
                }
            }
        }

        public void NotifyPlayerJoin(string playerId)
        {
            foreach (var mod in _loadedMods.Values)
            {
                try
                {
                    mod.OnPlayerJoin(playerId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in mod {mod.ModName} (player join): {ex.Message}");
                }
            }
        }

        public void NotifyPlayerLeave(string playerId)
        {
            foreach (var mod in _loadedMods.Values)
            {
                try
                {
                    mod.OnPlayerLeave(playerId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in mod {mod.ModName} (player leave): {ex.Message}");
                }
            }
        }
    }

    // Example mods for DayZ expansion
    public class ExpansionMod : BaseMod
    {
        public override string ModId => "expansion";
        public override string ModName => "DayZ Expansion";
        public override string Version => "1.5.0";

        public override void Initialize()
        {
            Console.WriteLine($"Initializing {ModName} v{Version}");
        }

        public override void OnGameStart()
        {
            Console.WriteLine($"{ModName}: Game started - loading custom content");
        }

        public override void OnPlayerJoin(string playerId)
        {
            Console.WriteLine($"{ModName}: Player {playerId} joined - applying expansion features");
        }
    }

    public class CommunityOnlineToolsMod : BaseMod
    {
        public override string ModId => "community_online_tools";
        public override string ModName => "Community Online Tools";
        public override string Version => "1.3.0";

        public override void Initialize()
        {
            Console.WriteLine($"Initializing {ModName} v{Version}");
        }

        public override void OnGameStart()
        {
            Console.WriteLine($"{ModName}: Admin tools enabled");
        }
    }

    public class DabsFrameworkMod : BaseMod
    {
        public override string ModId => "dabs_framework";
        public override string ModName => "Dabs Framework";
        public override string Version => "1.0.0";

        public override void Initialize()
        {
            Console.WriteLine($"Initializing {ModName} v{Version}");
        }
    }
}
