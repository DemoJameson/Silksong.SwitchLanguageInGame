using System;
using System.Collections.Generic;
using HarmonyLib;
using TeamCherry.Localization;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
public class DialogueBoxUpdater {
    private static readonly Dictionary<string, Dictionary<string, string>> reverseEntrySheets = new();
    private static LocalisedString? savedText;
    private static bool savedOverrideContinue;
    private static DialogueBox.DisplayOptions savedDisplayOptions;
    private static Action savedOnDialogueEnd;
    private static Action savedOnDialogueCancelled;

    public static void UpdateText() {
        var dialogueBox = DialogueBox._instance;
        if (!dialogueBox || !dialogueBox.isDialogueRunning) {
            return;
        }

        if (savedText == null) {
            return;
        }

        DialogueBox.StartConversation(savedText.Value.ToString(false),  dialogueBox.instigator, savedOverrideContinue, savedDisplayOptions, savedOnDialogueEnd, savedOnDialogueCancelled);
    }
    
    public static void InitReverseEntrySheets() {
        reverseEntrySheets.Clear();
        foreach (var entrySheet in Language._currentEntrySheets) {
            var sheetValue = entrySheet.Value;
            Dictionary<string, string> reverseValue = new();
            foreach (var pair in sheetValue) {
                reverseValue[pair.Value] = pair.Key;
            }
            reverseEntrySheets.Add(entrySheet.Key, reverseValue);
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

        foreach (var (sheetName, keys) in reverseEntrySheets) {
            if (keys.TryGetValue(text, out var key)) {
                savedText = new LocalisedString(sheetName, key);
                savedOverrideContinue = overrideContinue;
                savedDisplayOptions = displayOptions;
                savedOnDialogueEnd = onDialogueEnd;
                savedOnDialogueCancelled = onDialogueCancelled;
                break;
            }
        }
    }
}