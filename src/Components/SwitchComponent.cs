using System;
using BepInEx;
using GlobalEnums;
using HarmonyLib;
using Silksong.SwitchLanguageInGame.Configs;
using Silksong.SwitchLanguageInGame.Extensions;
using Silksong.SwitchLanguageInGame.Utils;
using TeamCherry.Localization;
using UnityEngine.UI;

namespace Silksong.SwitchLanguageInGame.Components;

[HarmonyPatch]
public class SwitchComponent : PluginComponent {
    public static event Action<LanguageCode>? AfterLanguageSwitched;

    private void Start() {
        Plugin.HarmonyInstance?.PatchAll(typeof(SwitchComponent));
        LanguageDoSwitchPostfix();
    }
    
    [HarmonyPatch(typeof(Language), nameof(Language.SwitchLanguage), [typeof(LanguageCode)])]
    [HarmonyPostfix]
    private static void LanguageDoSwitchPostfix() {
        PluginConfig.SelectedLanguage.Value = Language._currentLanguage.ToWord();
        LanguageUtils.AddReversedEntrySheets();

        UpdateSetting();
        UpdateSlotButton();
        UpdateComponents();
        UpdatePanel();
        UpdateQuestBoard();
        UpdateQuestShop();
        UpdateMsgBox();

        AfterLanguageSwitched?.Invoke(Language._currentLanguage);
    }

    private static void UpdateSetting() {
        var gameManager = GameManager._instance;
        if (!gameManager) return;

        gameManager.gameSettings.gameLanguage = (SupportedLanguages)Language._currentLanguage;
        gameManager.RefreshLocalization();
        UIManager.instance.languageSetting.UpdateText();
    }

    private static void UpdateSlotButton() {
        UpdateComponents<SaveSlotButton>(button => {
            if (!button.locationText) return;
            var text = button.locationText.text;
            if (text.IsNullOrWhiteSpace()) return;
            text = text.Replace(Environment.NewLine, "<br>");
            if (LanguageUtils.guessLocalisedString(text, "Map Zones") is { } localised) {
                button.locationText.text = localised.ToString().Replace("<br>", Environment.NewLine);
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

    private static void UpdatePanel() {
        var gameManager = GameManager._instance;
        if (!gameManager || !gameManager.gameMap) return;

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
}