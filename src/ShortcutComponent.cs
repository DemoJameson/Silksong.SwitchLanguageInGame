using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Silksong.SwitchLanguageInGame.config;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
public class ShortcutComponent : MonoBehaviour {
    private List<LanguageCode> availableLanguages = [];

    private void Start() {
        availableLanguages = Language.GetLanguages()
            .Select(LocalizationSettings.GetLanguageEnum)
            .ToList();
    }

    private void Update() {
        if (SceneManager.GetActiveScene().name == "Menu_Title") {
            return;
        }

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