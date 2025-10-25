using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using MonoMod.Utils;
using TeamCherry.Localization;
using UnityEngine;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
public class PlayMakerFSMComponent : MonoBehaviour {
    private static ManualLogSource Log => Plugin.Log;
    private static readonly List<WeakReference<PlayMakerFSM>> playMakerFSMList = [];

    private void Awake() {
        Plugin.OnLanguageSwitched += _ => UpdateText();
        var dynData = new DynData<GameManager>(GameManager._instance);
        var weakReferences = dynData.Get<List<WeakReference<PlayMakerFSM>>?>(nameof(playMakerFSMList));
        if (weakReferences != null) {
            playMakerFSMList.AddRange(weakReferences);
        }
    }

    private void OnDestroy() {
        new DynData<GameManager>(GameManager._instance).Set(nameof(playMakerFSMList), playMakerFSMList);
    }

    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Awake))]
    [HarmonyPostfix]
    private static void PlayMakerFSMAwake(PlayMakerFSM __instance) {
        foreach (var state in __instance.FsmStates) {
            foreach (var action in state.Actions) {
                if (action is SetTextMeshProText) {
                    playMakerFSMList.Add(new WeakReference<PlayMakerFSM>(__instance));
                    break;
                }
            }
        }
    }

    private static void UpdateText() {
        foreach (var weakReference in playMakerFSMList.ToList()) {
            if (weakReference.TryGetTarget(out var fsm) && fsm) {
                foreach (var state in fsm.FsmStates) {
                    for (var i = 0; i < state.actions.Length; i++) {
                        var action = state.actions[i];
                        if (action is SetTextMeshProText text && !text.textString.value.IsNullOrWhiteSpace() && text.textString.value != "!!/!!") {
                            action.OnEnter();
                        } else {
                            var nextIsSetText = i < state.actions.Length - 1 && state.actions[i + 1] is SetTextMeshProText;
                            if (!nextIsSetText) {
                                continue;
                            }

                            if (action is GetLanguageString getLanStr) {
                                // TODO 找到更好的办法正切设置购买、制作、修复
                                getLanStr.OnEnter();
                                if (getLanStr.sheetName.value != "UI" || getLanStr.convName.value != "CTRL_REPAIR") continue;
                                if (state.actions[i + 1] is not SetTextMeshProText setText) continue;

                                var go = setText.fsm.GetOwnerDefaultTarget(setText.gameObject);
                                if (!go) continue;

                                try {
                                    var parent = go.transform.parent.parent;
                                    if (parent.name != "Shop Menu(Clone)") continue;
                                    var meshPro = parent.Find("Item List Group/Item Details/Item name").GetComponent<TMProOld.TextMeshPro>();
                                    var shopMenuStock = parent.GetComponent<ShopMenuStock>();
                                    foreach (var stats in shopMenuStock.spawnedStock) {
                                        if (stats.GetName() == meshPro.text) {
                                            var key = "CTRL_" + stats.shopItem.GetPurchaseType() switch {
                                                ShopItem.PurchaseTypes.Purchase => "BUY",
                                                ShopItem.PurchaseTypes.Craft => "CRAFT",
                                                ShopItem.PurchaseTypes.Repair => "REPAIR",
                                                _ => "BUY"
                                            };
                                            getLanStr.storeValue.value = Language.Get(key, "UI").Replace("<br>", "\n");
                                            break;
                                        }
                                    }
                                } catch (Exception) {
                                    // ignore
                                }
                            } else if (action is CallMethodProper) {
                                action.OnEnter();
                            }
                        }
                    }
                }
            } else {
                playMakerFSMList.Remove(weakReference);
            }
        }
    }
}