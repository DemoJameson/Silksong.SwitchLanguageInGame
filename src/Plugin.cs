using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using GlobalEnums;
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
            Logger.LogError(e);
        }

        gameObject.AddComponent<SwitchComponent>();

#if DEBUG
        gameObject.AddComponent<DebugComponent>();
#endif
    }

    private void Start() {
        PluginConfig.Bind(this);

        var original = AccessTools.Method(typeof(Language), nameof(Language.SwitchLanguage), [typeof(LanguageCode)]);
        var postfix = new HarmonyMethod(typeof(Plugin), nameof(LanguageDoSwitchPostfix));
        HarmonyInstance?.Patch(original, postfix: postfix);
        LanguageDoSwitchPostfix();
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

    private static void LanguageDoSwitchPostfix() {
        PluginConfig.SelectedLanguage.Value = Language._currentLanguage;

        DialogueBoxUpdater.AddReversedEntrySheets();

        if (SceneManager.GetActiveScene().name == "Menu_Title") {
            return;
        }

        UpdateSetting();
        UpdateLanguageCpomponents();
        DialogueBoxUpdater.UpdateText();
    }

    private static void UpdateSetting() {
        var gameManager = GameManager._instance;
        if (gameManager) {
            gameManager.gameSettings.gameLanguage = (SupportedLanguages)Language._currentLanguage;
            gameManager.RefreshLocalization();
            UIManager.instance.languageSetting.UpdateText();
        }
    }

    private static void UpdateLanguageCpomponents() {
        UpdateComponents<SetTextMeshProGameText>(component => component.UpdateText());
        UpdateComponents<ChangePositionByLanguage>(component => component.DoOffset());
        UpdateComponents<ActivatePerLanguage>(component => component.UpdateLanguage());
        UpdateComponents<ChangeByLanguageBase>(component => component.DoUpdate());
        UpdateComponents<ChangeFontByLanguage>(component => {
            if (component.defaultMaterial) {
                component.SetFont();
            }
        });
    }

    private static void UpdateComponents<T>(Action<T> action) where T : MonoBehaviour {
        var components = Resources.FindObjectsOfTypeAll<T>();
        if (components == null) {
            return;
        }

        foreach (var component in components) {
            action(component);
        }
    }
}