using System;
using System.Collections.Generic;
using TeamCherry.Localization;

namespace Silksong.SwitchLanguageInGame;

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
        if (language == Language.CurrentLanguage()) return;

        Language.SwitchLanguage(language);
        UIManager._instance.uiAudioPlayer.PlaySubmit();
    }

    public static string ToWord(this LanguageCode languageCode) {
        return languageCode switch {
            LanguageCode.DE => "German",
            LanguageCode.EN => "English",
            LanguageCode.ES => "Spanish",
            LanguageCode.FR => "French",
            LanguageCode.IT => "Italian",
            LanguageCode.JA => "Japanese",
            LanguageCode.KO => "Korean",
            LanguageCode.PT => "Portuguese",
            LanguageCode.RU => "Russian",
            LanguageCode.ZH => "Chinese",
            _ => languageCode.ToString()
        };
    }

    public static LanguageCode ToLanguageCode(this string word) {
        foreach (LanguageCode code in Enum.GetValues(typeof(LanguageCode))) {
            if (string.Equals(code.ToWord(), word.Trim(), StringComparison.OrdinalIgnoreCase)) {
                return code;
            }
        }

        return LanguageCode.EN;
    }
}