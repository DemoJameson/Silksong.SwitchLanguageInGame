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
        gameObject.AddComponent<PlayMakerFSMComponent>();
        gameObject.AddComponent<DialogueBoxComponent>();
        gameObject.AddComponent<PromptMarkerComponent>();

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
        LanguageUtils.AddReversedEntrySheets();

        if (SceneManager.GetActiveScene().name == "Menu_Title") {
            return;
        }

        UpdateSetting();
        UpdateComponents();
        UpdatePanel();
        UpdateQuest();
        OnLanguageSwitched?.Invoke(Language._currentLanguage);
    }

    // TODO 地图“隐藏图钉”文本没有更新
    private static void UpdatePanel() {
        var gameManager = GameManager._instance;
        if (!gameManager) return;

        var mapManager = gameManager.gameMap.mapManager;
        mapManager.paneList.currentPaneText.text = mapManager.pane.DisplayName;
    }

    private static void UpdateSetting() {
        var gameManager = GameManager._instance;
        if (gameManager) {
            gameManager.gameSettings.gameLanguage = (SupportedLanguages)Language._currentLanguage;
            gameManager.RefreshLocalization();
            UIManager.instance.languageSetting.UpdateText();
        }
    }

    private static void UpdateQuest() {
        UpdateComponents<QuestItemBoard>(component => {
            if (component.itemList) {
                try {
                    var inventoryItemQuests = component.GetSelectables(null);
                    var basicQuestBases = component.GetItems();
                    if (inventoryItemQuests != null && basicQuestBases != null) {
                        for (var i = 0; i < inventoryItemQuests.Count; i++) {
                            var item = inventoryItemQuests[i];
                            var quest = basicQuestBases[i];
                            if (quest.QuestType) {
                                if (item.icon) {
                                    item.icon.sprite = quest.QuestType.Icon;
                                }

                                if (item.typeText) {
                                    item.typeText.text = quest.QuestType.DisplayName;
                                }
                            }

                            if (item.nameText) {
                                item.nameText.text = quest.DisplayName;
                            }
                        }
                    }
                } catch (Exception) {
                    // ignored
                }
            }
        });
    }

    private static void UpdateComponents() {
        UpdateComponents<SetTextMeshProGameText>(component => component.UpdateText());
        UpdateComponents<ActivatePerLanguage>(component => component.UpdateLanguage());
        UpdateComponents<ChangeByLanguageBase>(component => component.DoUpdate());
        UpdateComponents<InventoryItemManager>(component => {
            if (component.CurrentSelected) {
                component.SetDisplay(component.CurrentSelected);
            }
        });
        UpdateComponents<ChangeFontByLanguage>(component => {
            if (component.defaultMaterial) {
                component.SetFont();
            }
        });
    }

    public static void UpdateComponents<T>(Action<T> action) where T : MonoBehaviour {
        var components = Resources.FindObjectsOfTypeAll<T>();
        if (components == null) {
            return;
        }

        foreach (var component in components) {
            action(component);
        }
    }

    public static event Action<LanguageCode>? OnLanguageSwitched;
}