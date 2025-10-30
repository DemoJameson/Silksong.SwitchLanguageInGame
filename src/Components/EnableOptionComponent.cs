using GlobalEnums;
using HarmonyLib;
using Silksong.SwitchLanguageInGame.Configs;
using UnityEngine.UI;

namespace Silksong.SwitchLanguageInGame.Components;

[HarmonyPatch]
public class EnableOptionComponent : PluginComponent {
    [HarmonyPatch(typeof(GameMenuOptions), nameof(GameMenuOptions.ConfigureNavigation))]
    [HarmonyPostfix]
    private static void GameMenuOptionsConfigureNavigation(GameMenuOptions __instance) {
        if (PluginConfig.Enabled.Value && GameManager.instance.GameState != GameState.MAIN_MENU) {
            var languageOption = __instance.languageOption;
            languageOption.interactable = true;
            languageOption.transform.parent.gameObject.SetActive(value: true);
            __instance.languageOptionDescription.SetActive(value: false);
            __instance.gameOptionsMenuScreen.defaultHighlight = languageOption;

            if (languageOption is MenuLanguageSetting setting)
                setting.UpdateAlpha();
        }
    }
}