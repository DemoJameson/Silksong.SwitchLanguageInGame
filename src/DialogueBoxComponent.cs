using System;
using System.Collections;
using BepInEx.Logging;
using HarmonyLib;
using TeamCherry.Localization;
using TMProOld;
using UnityEngine;

namespace Silksong.SwitchLanguageInGame;

[HarmonyPatch]
public class DialogueBoxComponent : MonoBehaviour {
    private static ManualLogSource Log => Plugin.Log;
    private static LocalisedString? savedText;
    private static bool savedOverrideContinue;
    private static DialogueBox.DisplayOptions savedDisplayOptions;
    private static Action? savedOnDialogueEnd;
    private static Action? savedOnDialogueCancelled;

    private void Awake() {
        Plugin.OnLanguageSwitched += _ => UpdateText();
    }
    
    private static void UpdateText() {
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
            var localisedString = LanguageUtils.guessLocalisedString(textMeshPro.text, specifiedSheet: "Titles");
            if (localisedString != null) {
                textMeshPro.text = localisedString.Value.ToString();
            }
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

        var localisedString = LanguageUtils.guessLocalisedString(text, specifiedLanguage: Language._currentLanguage);
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
    
}