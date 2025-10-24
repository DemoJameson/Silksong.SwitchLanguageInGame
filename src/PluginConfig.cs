using System;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.SwitchLanguageInGame;

public static class PluginConfig {
    public static ConfigEntry<bool> Enabled = null!;
    public static ConfigEntry<KeyboardShortcut> PrevLanguageKey = null!;
    public static ConfigEntry<KeyboardShortcut> NextLanguageKey = null!;

    public static void Bind(BaseUnityPlugin plugin) {
        var pluginConfig = plugin.Config;

        Enabled = pluginConfig.Bind("General", "Switch Language in Game", true, new ConfigDescription("Support language switching in the game"));
        PrevLanguageKey = pluginConfig.Bind("General", "Switch to Previous Language", new KeyboardShortcut(KeyCode.None),
            new ConfigDescription("Key for switching to previous language"));
        NextLanguageKey = pluginConfig.Bind("General", "Switch to Next Language", new KeyboardShortcut(KeyCode.None),
            new ConfigDescription("Key for switching to next language"));

        Enabled.SettingChanged += OnEnableSwitchingOnSettingChanged;
    }
    
    private static void OnEnableSwitchingOnSettingChanged(object obj, EventArgs eventArgs) {
        if (Enabled.Value || !UIManager._instance) return;

        var description = UIManager._instance.gameOptionsMenuScreen.transform.Find("Content/LanguageSetting/LanguageOption/Description");
        if (!description) return;

        var text = description.GetComponent<Text>();
        if (text) {
            text.enabled = true;
        }
    }
}