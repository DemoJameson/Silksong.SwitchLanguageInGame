using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
[BepInAutoPlugin(id: "com.demojameson.silksong.switchlanguageingame", name: "Switch Language in Game")]
public partial class Plugin : BaseUnityPlugin {
    private static ManualLogSource logger = null!;
    private static ConfigEntry<bool> enableSwitching = null!;

    private Harmony? harmony;

    private void Awake() {
        logger = Logger;
        enableSwitching = Config.Bind("General", "Switch Language in Game", true, new ConfigDescription("Support language switching in the game"));
        enableSwitching.SettingChanged += OnEnableSwitchingOnSettingChanged;
        harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start() {
        var original = AccessTools.Method(typeof(Language), nameof(Language.SwitchLanguage), [typeof(LanguageCode)]);
        var postfix = new HarmonyMethod(typeof(Plugin), nameof(LanguageSwitchPostfix));
        harmony?.Patch(original, postfix: postfix);
    }

    private void OnDestroy() {
        harmony?.UnpatchSelf();
        enableSwitching.SettingChanged -= OnEnableSwitchingOnSettingChanged;
    }

    private void OnEnableSwitchingOnSettingChanged(object obj, EventArgs eventArgs) {
        if (!enableSwitching.Value && UIManager._instance) {
            var description = UIManager._instance.gameOptionsMenuScreen.transform.Find("Content/LanguageSetting/LanguageOption/Description");
            if (description) {
                var text = description.GetComponent<Text>();
                if (text) {
                    text.enabled = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Selectable), nameof(Selectable.interactable), MethodType.Setter)]
    [HarmonyPrefix]
    private static void SelectableSetInteractable(Selectable __instance, ref bool value) {
        if (enableSwitching.Value && !value && UIManager._instance && __instance == UIManager._instance.languageSetting) {
            var description = __instance.gameObject.transform.Find("Description");
            if (description) {
                var text = description.GetComponent<Text>();
                if (text) {
                    text.enabled = false;
                }
            }

            value = true;
        }
    }

    private static void LanguageSwitchPostfix() {
        if (SceneManager.GetActiveScene().name == "Menu_Title") {
            return;
        }

        var texts = Resources.FindObjectsOfTypeAll<SetTextMeshProGameText>();
        foreach (var text in texts) {
            text.UpdateText();
        }
    }
}