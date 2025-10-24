using System.Linq;
using BepInEx;
using BepInEx.Configuration;

namespace Silksong.SwitchLanguageInGame;

public static class ConfigEntryExtensions {
    public static bool IsDown(this ConfigEntry<KeyboardShortcut> configEntry) {
        if (!configEntry.Value.Modifiers.Any()) {
            return UnityInput.Current.GetKeyDown(configEntry.Value.MainKey);
        } else {
            return configEntry.Value.IsDown();
        }
    }
}
