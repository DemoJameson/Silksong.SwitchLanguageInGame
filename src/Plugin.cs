using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using GlobalEnums;
using HarmonyLib;
using Silksong.SwitchLanguageInGame.Config;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
[BepInAutoPlugin(id: "com.demojameson.switchlanguageingame", name: "Switch Language in Game")]
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

        gameObject.AddComponent<ShortcutComponent>();
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

    [HarmonyPatch(typeof(GameMenuOptions), nameof(GameMenuOptions.ConfigureNavigation))]
    [HarmonyPostfix]
    private static void GameMenuOptionsConfigureNavigation(GameMenuOptions __instance) {
        if (PluginConfig.Enabled.Value && GameManager.instance.GameState != GameState.MAIN_MENU) {
            var languageOption = __instance.languageOption;
            languageOption.interactable = true;
            languageOption.transform.parent.gameObject.SetActive(value: true);
            __instance.languageOptionDescription.SetActive(value: false);
            __instance.gameOptionsMenuScreen.defaultHighlight = languageOption;

            if (languageOption is MenuLanguageSetting setting)
                setting.UpdateAlpha();
        }
    }

    private static void LanguageDoSwitchPostfix() {
        PluginConfig.SelectedLanguage.Value = Language._currentLanguage.ToWord();
        LanguageUtils.AddReversedEntrySheets();

        if (SceneManager.GetActiveScene().name == "Menu_Title") {
            return;
        }

        UpdateSetting();
        UpdateComponents();
        UpdatePanel();
        UpdateQuestBoard();
        UpdateQuestShop();
        UpdateMsgBox();
        OnLanguageSwitched?.Invoke(Language._currentLanguage);
    }

    private static void UpdateSetting() {
        var gameManager = GameManager._instance;
        if (!gameManager) return;

        gameManager.gameSettings.gameLanguage = (SupportedLanguages)Language._currentLanguage;
        gameManager.RefreshLocalization();
        UIManager.instance.languageSetting.UpdateText();
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

    private static void UpdatePanel() {
        var gameManager = GameManager._instance;
        if (!gameManager) return;

        var mapManager = gameManager.gameMap.mapManager;
        mapManager.paneList.currentPaneText.text = mapManager.pane.DisplayName;
        mapManager.UpdateKeyPromptState(false);
    }

    private static void UpdateQuestBoard() {
        UpdateComponents<QuestItemBoard>(component => {
            if (!component.itemList) return;

            try {
                var inventoryItemQuests = component.GetSelectables(null);
                var basicQuestBases = component.GetItems();
                if (inventoryItemQuests == null || basicQuestBases == null) return;

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
            } catch {
                // ignored
            }
        });
    }

    private static void UpdateQuestShop() {
        var shopMenu = SimpleShopMenuOwner._spawnedMenu;
        if (!shopMenu) return;

        var owner = shopMenu.owner;
        if (owner) {
            if (shopMenu.titleText) shopMenu.titleText.text = owner.ShopTitle;
            if (shopMenu.purchaseText) shopMenu.purchaseText.text = owner.PurchaseText;
        }

        for (var i = 0; i < shopMenu.activeItemCount; i++) {
            var display = shopMenu.spawnedItemDisplays[i];
            var item = shopMenu.shopItems[i];
            if (display.titleText) {
                display.titleText.text = item.GetDisplayName();
            }
        }
    }

    private static void UpdateMsgBox() {
        UpdateComponents<NeedolinMsgBox>(component => {
            if (component.primaryText && LanguageUtils.guessLocalisedString(component.primaryText.text, "Song", "Lore") is { } text) {
                component.primaryText.text = text;
            }

            if (component.secondaryText && LanguageUtils.guessLocalisedString(component.secondaryText.text, "Song", "Lore") is { } text2) {
                component.secondaryText.text = text2;
            }
        });

        UpdateComponents<MemoryMsgBox>(component => {
            if (component.textDisplays == null) return;

            foreach (var display in component.textDisplays) {
                if (!display) continue;

                if (LanguageUtils.guessLocalisedString(display.text, specifiedSheet: "Lore") is { } text) {
                    display.text = text;
                }
            }
        });
    }

    public static void UpdateComponents<T>(Action<T> action) where T : MonoBehaviour {
        var components = Resources.FindObjectsOfTypeAll<T>();
        if (components == null) return;

        foreach (var component in components) {
            action(component);
        }
    }

    public static event Action<LanguageCode>? OnLanguageSwitched;
}