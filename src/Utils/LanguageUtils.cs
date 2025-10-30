using System.Collections.Generic;
using TeamCherry.Localization;

namespace Silksong.SwitchLanguageInGame.Utils;

public static class LanguageUtils {
    private static readonly Dictionary<LanguageCode, Dictionary<string, Dictionary<string, string>>> reversedEntrySheets = new();

    public static LocalisedString? guessLocalisedString(string? text, LanguageCode? specifiedLanguage = null, string? specifiedSheet = null) {
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

    public static LocalisedString? guessLocalisedString(string? text, params string[] sheets) {
        foreach (var sheet in sheets) {
            if (guessLocalisedString(text, specifiedSheet: sheet) is { } localised) {
                return localised;
            }
        }

        return null;
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

    public static void Switch(LanguageCode language) {
        UIManager._instance.uiAudioPlayer.PlaySubmit();
        if (language != Language.CurrentLanguage()) {
            Language.SwitchLanguage(language);
        }
    }
}