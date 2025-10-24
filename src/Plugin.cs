using System;
using System.Reflection;
using BepInEx;
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
    public static ManualLogSource Log = null!;
    public static Harmony? HarmonyInstance { get; private set; }

    private void Awake() {
        Log = Logger;

        try {
            HarmonyInstance = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        } catch (Exception e) {
            Logger.LogError(e.Message);
        }

        gameObject.AddComponent<SwitchComponent>();

#if DEBUG
        gameObject.AddComponent<DebugComponent>();
#endif
    }

    private void Start() {
        PluginConfig.Bind(this);

        var original = AccessTools.Method(typeof(Language), nameof(Language.SwitchLanguage), [typeof(LanguageCode)]);
        var postfix = new HarmonyMethod(typeof(Plugin), nameof(LanguageSwitchPostfix));
        HarmonyInstance?.Patch(original, postfix: postfix);
        LanguageSwitchPostfix();
    }

    private void OnDestroy() {
        HarmonyInstance?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Selectable), nameof(Selectable.interactable), MethodType.Setter)]
    [HarmonyPrefix]
    private static void SelectableSetInteractable(Selectable __instance, ref bool value) {
        if (PluginConfig.Enabled.Value && !value && UIManager._instance && __instance == UIManager._instance.languageSetting) {
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
        DialogueBoxUpdater.InitReverseEntrySheets();

        if (SceneManager.GetActiveScene().name == "Menu_Title") {
            return;
        }

        UpdateMeshProText();
        DialogueBoxUpdater.UpdateText();
    }

    private static void UpdateMeshProText() {
        var texts = Resources.FindObjectsOfTypeAll<SetTextMeshProGameText>();
        if (texts == null) {
            return;
        }

        foreach (var text in texts) {
            text.UpdateText();
        }
    }
}