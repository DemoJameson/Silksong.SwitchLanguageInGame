using System.Diagnostics;
using HarmonyLib;
using TeamCherry.Localization;
using TMProOld;
using UnityEngine;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
public class DebugComponent : MonoBehaviour {
    private void Start() {
        var original = AccessTools.Method(typeof(Language), nameof(Language.Get), [typeof(string), typeof(string)]);
        var postfix = new HarmonyMethod(typeof(DebugComponent), nameof(LanguageGet));
        Plugin.HarmonyInstance?.Patch(original, postfix: postfix);

        original = AccessTools.PropertySetter(typeof(TMP_Text), nameof(TMP_Text.text));
        postfix = new HarmonyMethod(typeof(DebugComponent), nameof(TextMeshProSetText));
        Plugin.HarmonyInstance?.Patch(original, postfix: postfix);
    }

    private static void LanguageGet(string key, string sheetTitle, string __result) {
        Plugin.Log.LogWarning($"Language.Get({key}, {sheetTitle}) = {__result}");
        Plugin.Log.LogInfo(new StackTrace());
    }

    private static void TextMeshProSetText(string value) {
        Plugin.Log.LogWarning($"TMP_Text.set_text({value})");
        Plugin.Log.LogInfo(new StackTrace());
    }
}