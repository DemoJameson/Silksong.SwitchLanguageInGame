using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.SwitchLanguageInGame;

public static class PluginConfig {
    public static ConfigEntry<bool> Enabled = null!;
    public static ConfigEntry<KeyboardShortcut> PrevLanguageKey = null!;
    public static ConfigEntry<KeyboardShortcut> NextLanguageKey = null!;
    public static ConfigEntry<LanguageCode> SelectedLanguage = null!;

    public static void Bind(BaseUnityPlugin plugin) {
        var pluginConfig = plugin.Config;

        Enabled = pluginConfig.Bind("General", "Switch Language in Game", true, new ConfigDescription("Support language switching in the game"));
        Enabled.SettingChanged += OnEnabledSettingChanged;

        PrevLanguageKey = pluginConfig.Bind("General", "Switch to Previous Language", new KeyboardShortcut(KeyCode.None),
            new ConfigDescription("Key for switching to previous language"));

        NextLanguageKey = pluginConfig.Bind("General", "Switch to Next Language", new KeyboardShortcut(KeyCode.None),
            new ConfigDescription("Key for switching to next language"));

        var languageCodes = Language.GetLanguages().Select(LocalizationSettings.GetLanguageEnum).ToArray();
        SelectedLanguage = pluginConfig.Bind("General", "Selected Language", Language._currentLanguage,
            new ConfigDescription("Switch languages in the available languages", new AcceptableValueEnum<LanguageCode>(languageCodes)));
        SelectedLanguage.Value = Language._currentLanguage;
        SelectedLanguage.SettingChanged += OnSelectedLanguageSettingChanged;
    }

    private static void OnEnabledSettingChanged(object sender, EventArgs eventArgs) {
        if (Enabled.Value || !UIManager._instance) return;

        var description = UIManager._instance.gameOptionsMenuScreen.transform.Find("Content/LanguageSetting/LanguageOption/Description");
        if (!description) return;

        var text = description.GetComponent<Text>();
        if (text) {
            text.enabled = true;
        }
    }

    private static void OnSelectedLanguageSettingChanged(object sender, EventArgs eventArgs) {
        if (SelectedLanguage.Value != Language._currentLanguage) {
            Language.SwitchLanguage(SelectedLanguage.Value);
        }
    }
}