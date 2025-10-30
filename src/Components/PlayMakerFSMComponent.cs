using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using MonoMod.Utils;

namespace Silksong.SwitchLanguageInGame.Components;

[HarmonyPatch]
[PluginComponentPriority(int.MaxValue)]
public class PlayMakerFSMComponent : PluginComponent {
    private static List<WeakReference<PlayMakerFSM>> playMakerFSMList = [];
    private static WeakReference<PlayMakerFSM>? itemListControl;

    private void Awake() {
        SwitchComponent.AfterLanguageSwitched += _ => UpdateText();
        var dynData = new DynData<GameManager>(GameManager._instance);
        playMakerFSMList = dynData.Get<List<WeakReference<PlayMakerFSM>>?>(nameof(playMakerFSMList)) ?? [];
        itemListControl = dynData.Get<WeakReference<PlayMakerFSM>?>(nameof(itemListControl));
    }

    private void OnDestroy() {
        var dynData = new DynData<GameManager>(GameManager._instance);
        dynData.Set(nameof(playMakerFSMList), playMakerFSMList);
        dynData.Set(nameof(itemListControl), itemListControl);
    }

    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Awake))]
    [HarmonyPostfix]
    private static void PlayMakerFSMAwake(PlayMakerFSM __instance) {
        if (__instance.GetName() == "Item List" && __instance.FsmName == "Item List Control") {
            itemListControl = new WeakReference<PlayMakerFSM>(__instance);
        }

        foreach (var state in __instance.FsmStates) {
            if (state.Actions.OfType<SetTextMeshProText>().Any()) {
                playMakerFSMList.Add(new WeakReference<PlayMakerFSM>(__instance));
            }
        }
    }

    // TODO 送货员任务名称和确认按钮没有实时更新
    private static void UpdateText() {
        if (GameManager._instance && GameManager._instance.isPaused) return;

        foreach (var weakReference in playMakerFSMList.ToList()) {
            if (weakReference.TryGetTarget(out var fsm) && fsm) {
                if (!fsm.gameObject || !fsm.gameObject.activeInHierarchy) {
                    continue;
                }
                
                var isShopFsm = fsm == itemListControl.GetTarget();
                foreach (var state in fsm.FsmStates) {
                    if (isShopFsm) {
                        if (state.name is "Purchase" or "Craft" or "Repair") {
                            var purchaseType = fsm.FsmVariables.FindFsmEnum("Purchase Type").value;
                            if (purchaseType != null && !string.Equals(state.name, purchaseType.ToString())) {
                                continue;
                            }
                        }
                    }

                    var actions = state.actions;
                    for (var i = 0; i < actions.Length; i++) {
                        var action = actions[i];
                        if (action is SetTextMeshProText text) {
                            if (!text.textString.value.IsNullOrWhiteSpace() && text.textString.value != "!!/!!") {
                                // Log.LogInfo($"gameObject: {fsm.GetName()}, fsm: {fsm.FsmName}, state: {state.name}, text: {text.textString.value}");
                                action.OnEnter();
                            }

                            continue;
                        }

                        if (action is GetLanguageStringProcessed or GetLanguageString) {
                            action.OnEnter();
                            continue;
                        }

                        var nextIsSetText = i < actions.Length - 1 && actions[i + 1] is SetTextMeshProText;
                        if (!nextIsSetText) {
                            continue;
                        }

                        // 只运行下一个 action 为 SetTextMeshProText 的 CallMethodProper
                        if (action is CallMethodProper) {
                            action.OnEnter();
                        }
                    }
                }
            } else {
                playMakerFSMList.Remove(weakReference);
            }
        }
    }
}

public static class WeakReferenceExtensions {
    public static PlayMakerFSM? GetTarget(this WeakReference<PlayMakerFSM>? weakReference) {
        if (weakReference != null && weakReference.TryGetTarget(out var fsm) && fsm) {
            return fsm;
        }

        return null;
    }

    public static void Invoke(this WeakReference<PlayMakerFSM>? weakReference, Action<PlayMakerFSM> action) {
        var fsm = weakReference.GetTarget();
        if (fsm) {
            action.Invoke(fsm);
        }
    }

    public static T? Invoke<T>(this WeakReference<PlayMakerFSM>? weakReference, Func<PlayMakerFSM, T> func) {
        var fsm = weakReference.GetTarget();
        return fsm ? func.Invoke(fsm) : default;
    }
}