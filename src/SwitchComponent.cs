using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
public class SwitchComponent : MonoBehaviour {
    private List<LanguageCode> availableLanguages;

    private void Start() {
        availableLanguages = Language.GetLanguages()
            .Select(LocalizationSettings.GetLanguageEnum)
            .Distinct()
            .ToList();
    }

    private void Update() {
        if (SceneManager.GetActiveScene().name == "Menu_Title") {
            return;
        }

        if (PluginConfig.PrevLanguageKey.IsDown()) {
            var index = availableLanguages.IndexOf(Language._currentLanguage);
            var prevIndex = (index - 1 + availableLanguages.Count) % availableLanguages.Count;
            Language.SwitchLanguage(availableLanguages[prevIndex]);
            UIManager._instance.uiAudioPlayer.PlaySubmit();
        } else if (PluginConfig.NextLanguageKey.IsDown()) {
            var indexOf = availableLanguages.IndexOf(Language._currentLanguage);
            var nextIndex = (indexOf + 1) % availableLanguages.Count;
            Language.SwitchLanguage(availableLanguages[nextIndex]);
            UIManager._instance.uiAudioPlayer.PlaySubmit();
        }
    }
}