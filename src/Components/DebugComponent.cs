using System.Diagnostics;
using HarmonyLib;
using TeamCherry.Localization;
using TMProOld;

namespace Silksong.SwitchLanguageInGame.Components;

public class DebugComponent : PluginComponent {
    private void Start() {
#if DEBUG
        Plugin.HarmonyInstance?.PatchAll(typeof(DebugComponent));
#endif
    }

    [HarmonyPatch(typeof(Language), nameof(Language.Get), typeof(string), typeof(string))]
    [HarmonyPostfix]
    private static void LanguageGet(string key, string sheetTitle, string __result) {
        Logger.LogWarning($"Language.Get({key}, {sheetTitle}) = {__result}");
        Logger.LogInfo(new StackTrace());
    }

    [HarmonyPatch(typeof(TMP_Text), nameof(TMP_Text.text), MethodType.Setter)]
    [HarmonyPostfix]
    private static void TMP_TextSetText(string value) {
        Logger.LogWarning($"TMP_Text.set_text({value})");
        Logger.LogInfo(new StackTrace());
    }

    [HarmonyPatch(typeof(LocalisedString), nameof(LocalisedString.ToString), typeof(bool))]
    [HarmonyPostfix]
    private static void LocalisedStringToString(LocalisedString __instance, bool allowBlankText, string __result) {
        Logger.LogWarning($"LocalisedString.ToString(sheet: {__instance.Sheet}, key: {__instance.Key}, allowBlankText: {allowBlankText}) = {__result}");
        Logger.LogInfo(new StackTrace());
    }
}