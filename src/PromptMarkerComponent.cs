using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
public class PromptMarkerComponent : MonoBehaviour {
    private static ManualLogSource Log => Plugin.Log;
    private static readonly ConditionalWeakTable<PromptMarker, string> labelNameTable = new();

    private void Awake() {
        Plugin.OnLanguageSwitched += _ => UpdateText();
    }

    [HarmonyPatch(typeof(PromptMarker), nameof(PromptMarker.SetLabel))]
    [HarmonyPostfix]
    private static void PromptMarkerSetLabel(PromptMarker __instance, string labelName) {
        labelNameTable.AddOrUpdate(__instance, labelName);
    }

    private static void UpdateText() {
        Plugin.UpdateComponents<PromptMarker>(component => {
            if (labelNameTable.TryGetValue(component, out var labelName)) {
                component.SetLabel(labelName);
            }
        });
    }
}