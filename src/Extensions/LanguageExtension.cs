using System;
using TeamCherry.Localization;

namespace Silksong.SwitchLanguageInGame.Extensions;

public static class LanguageExtension {
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