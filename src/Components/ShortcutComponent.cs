using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Silksong.SwitchLanguageInGame.Configs;
using Silksong.SwitchLanguageInGame.Extensions;
using Silksong.SwitchLanguageInGame.Utils;
using TeamCherry.Localization;

namespace Silksong.SwitchLanguageInGame.Components;

[HarmonyPatch]
public class ShortcutComponent : PluginComponent {
    private List<LanguageCode> availableLanguages = [];

    private void Start() {
        availableLanguages = Language.GetLanguages()
            .Select(LocalizationSettings.GetLanguageEnum)
            .ToList();
    }

    private void Update() {
        if (PluginConfig.PrevLanguageKey.IsDown()) {
            var index = availableLanguages.IndexOf(Language._currentLanguage);
            var prevIndex = (index - 1 + availableLanguages.Count) % availableLanguages.Count;
            LanguageUtils.Switch(availableLanguages[prevIndex]);
        } else if (PluginConfig.NextLanguageKey.IsDown()) {
            var indexOf = availableLanguages.IndexOf(Language._currentLanguage);
            var nextIndex = (indexOf + 1) % availableLanguages.Count;
            LanguageUtils.Switch(availableLanguages[nextIndex]);
        } else {
            foreach (var (code, configEntry) in PluginConfig.LanguagesKey) {
                if (configEntry.IsDown()) {
                    LanguageUtils.Switch(code);
                    break;
                }
            }
        }
    }
}