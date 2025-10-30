using System.Runtime.CompilerServices;
using HarmonyLib;

namespace Silksong.SwitchLanguageInGame.Components;

[HarmonyPatch]
public class PromptMarkerComponent : PluginComponent {
    private static readonly ConditionalWeakTable<PromptMarker, string> labelNameTable = new();

    private void Awake() {
        SwitchComponent.AfterLanguageSwitched += _ => UpdateText();
    }

    [HarmonyPatch(typeof(PromptMarker), nameof(PromptMarker.SetLabel))]
    [HarmonyPostfix]
    private static void PromptMarkerSetLabel(PromptMarker __instance, string labelName) {
        labelNameTable.AddOrUpdate(__instance, labelName);
    }

    private static void UpdateText() {
        UpdateComponents<PromptMarker>(component => {
            if (labelNameTable.TryGetValue(component, out var labelName)) {
                component.SetLabel(labelName);
            }
        });
    }
}