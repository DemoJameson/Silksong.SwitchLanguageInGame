using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using TeamCherry.Localization;
using UnityEngine;

namespace Silksong.SwitchLanguageInGame.Config;

public static class PluginConfig {
    public static ConfigEntry<bool> Enabled = null!;
    public static ConfigEntry<KeyboardShortcut> PrevLanguageKey = null!;
    public static ConfigEntry<KeyboardShortcut> NextLanguageKey = null!;
    public static readonly Dictionary<LanguageCode, ConfigEntry<KeyboardShortcut>> LanguagesKey = new();
    public static ConfigEntry<string> SelectedLanguage = null!;

    public static void Bind(BaseUnityPlugin plugin) {
        var pluginConfig = plugin.Config;

        int order = 0;

        Enabled = pluginConfig.BindEx("General", "Switch Language in Game", "Support language switching in the game", true, --order);

        var languageCodes = Language.GetLanguages().Select(LocalizationSettings.GetLanguageEnum).Select(code => code.ToWord()).ToArray();
        SelectedLanguage = pluginConfig.BindEx("General", "Selected Language", "Switch languages in the available languages", Language._currentLanguage.ToWord(), --order,
            new AcceptableValueList<string>(languageCodes));
        SelectedLanguage.Value = Language._currentLanguage.ToWord();
        SelectedLanguage.SettingChanged += OnSelectedLanguageSettingChanged;

        PrevLanguageKey = pluginConfig.BindEx("Shortcut Key", "Switch to Previous Language", "Key for switching to previous language", new KeyboardShortcut(KeyCode.None), --order);
        NextLanguageKey = pluginConfig.BindEx("Shortcut Key", "Switch to Next Language", "Key for switching to next language", new KeyboardShortcut(KeyCode.None), --order);

        foreach (var languageCode in Language.GetLanguages().Select(LocalizationSettings.GetLanguageEnum)) {
            LanguagesKey.Add(
                languageCode,
                pluginConfig.BindEx("Shortcut Key", $"Switch to {languageCode.ToWord()}", $"Key for switching to {languageCode.ToWord()}", new KeyboardShortcut(KeyCode.None), --order)
            );
        }
    }

    private static void OnSelectedLanguageSettingChanged(object sender, EventArgs eventArgs) {
        LanguageUtils.Switch(SelectedLanguage.Value.ToLanguageCode());
    }
}