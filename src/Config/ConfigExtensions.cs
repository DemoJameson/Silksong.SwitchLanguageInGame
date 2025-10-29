using System.Linq;
using BepInEx;
using BepInEx.Configuration;

namespace Silksong.SwitchLanguageInGame.Config;

public static class ConfigExtensions {
    public static bool IsDown(this ConfigEntry<KeyboardShortcut> configEntry) {
        if (!configEntry.Value.Modifiers.Any()) {
            return UnityInput.Current.GetKeyDown(configEntry.Value.MainKey);
        } else {
            return configEntry.Value.IsDown();
        }
    }
    
    public static ConfigEntry<T> BindEx<T>(
        this ConfigFile config,
        string section,
        string key,
        string description,
        T defaultValue,
        int? order = null,
        AcceptableValueBase? acceptableValue = null
    ) {
        return config.Bind(section, key, defaultValue, new ConfigDescription(description, acceptableValue, new ConfigurationManagerAttributes {
            Order = order
        }));
    }
}