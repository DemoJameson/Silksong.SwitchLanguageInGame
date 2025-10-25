using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using TeamCherry.Localization;
using TMProOld;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
public class DialogueBoxUpdater {
    private static ManualLogSource Log => Plugin.Log;
    private static readonly Dictionary<LanguageCode, Dictionary<string, Dictionary<string, string>>> reversedEntrySheets = new();
    private static LocalisedString? savedText;
    private static bool savedOverrideContinue;
    private static DialogueBox.DisplayOptions savedDisplayOptions;
    private static Action? savedOnDialogueEnd;
    private static Action? savedOnDialogueCancelled;

    public static void UpdateText() {
        UpdateAreaTitle();
        
        var dialogueBox = DialogueBox._instance;
        if (!dialogueBox || !dialogueBox.isDialogueRunning) {
            return;
        }

        if (savedText == null) {
            return;
        }

        DialogueBox.StartConversation(savedText.Value.ToString(false),  dialogueBox.instigator, savedOverrideContinue, savedDisplayOptions, savedOnDialogueEnd, savedOnDialogueCancelled);
    }

    private static void UpdateAreaTitle() {
        var areaTitle = AreaTitle.Instance;
        if (!areaTitle) return;

        var texts = areaTitle.gameObject.GetComponentsInChildren<TextMeshPro>();
        if (texts == null) return;

        foreach (var textMeshPro in texts) {
            var localisedString = guessLocalisedString(textMeshPro.text, specifiedSheet: "Titles");
            if (localisedString != null) {
                textMeshPro.text = localisedString.Value.ToString();
            }
        }
    }
    
    public static void AddReversedEntrySheets() {
        if (reversedEntrySheets.ContainsKey(Language._currentLanguage)) {
            return;
        }

        Dictionary<string, Dictionary<string, string>> entrySheets = new();
        reversedEntrySheets.Add(Language._currentLanguage, entrySheets);

        foreach (var entrySheet in Language._currentEntrySheets) {
            var sheetValue = entrySheet.Value;
            Dictionary<string, string> reverseValue = new();
            foreach (var pair in sheetValue) {
                reverseValue[pair.Value] = pair.Key;
            }
            entrySheets.Add(entrySheet.Key, reverseValue);
        }
    }


    [HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.StartConversation),
        typeof(string), typeof(NPCControlBase), typeof(bool), typeof(DialogueBox.DisplayOptions), typeof(Action), typeof(Action))]
    [HarmonyPrefix]
    private static void DialogueBoxStartConversation(string text, NPCControlBase instigator, bool overrideContinue, DialogueBox.DisplayOptions displayOptions, Action onDialogueEnd, Action onDialogueCancelled) {
        var dialogueBox = DialogueBox._instance;
        if (!dialogueBox) {
            return;
        }

        var localisedString = guessLocalisedString(text, specifiedLanguage: Language._currentLanguage);
        if (localisedString != null) {
            savedText = localisedString.Value;
            savedOverrideContinue = overrideContinue;
            savedDisplayOptions = displayOptions;
            savedOnDialogueEnd = onDialogueEnd;
            savedOnDialogueCancelled = onDialogueCancelled;
        }
    }
    
    
    [HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.CloseAndEnd))]
    [HarmonyPostfix]
    private static IEnumerator DialogueBoxCloseAndEnd(IEnumerator __result) {
        while (__result.MoveNext()) {
            yield return __result.Current;
        }

        savedText = null;
        savedOnDialogueEnd = null;
        savedOnDialogueCancelled = null;
    }
    
    private static LocalisedString? guessLocalisedString(string? text, LanguageCode? specifiedLanguage = null, string? specifiedSheet = null) {
        if (string.IsNullOrEmpty(text)) {
            return null;
        }

        foreach (var (languageCode, entrySheets) in reversedEntrySheets) {
            if (specifiedLanguage != null && languageCode != specifiedLanguage) {
                continue;
            }

            foreach (var (sheetName, keys) in entrySheets) {
                if (specifiedSheet != null && sheetName != specifiedSheet) {
                    continue;
                }

                if (keys.TryGetValue(text, out var key)) {
                    return new LocalisedString(sheetName, key);
                }
            }
        }

        return null;
    }
}