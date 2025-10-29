using System;
using System.Linq;
using BepInEx.Configuration;

namespace Silksong.SwitchLanguageInGame.Config;

public class AcceptableValueEnum<T> : AcceptableValueBase where T : Enum {
    public virtual T[] AcceptableValues { get; }

    public AcceptableValueEnum(params T[]? acceptableValues) : base(typeof(T)) {
        if (acceptableValues == null || acceptableValues.Length == 0)
            acceptableValues = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

        AcceptableValues = acceptableValues;
    }

    public override object Clamp(object value) {
        if (IsValid(value))
            return value;

        return AcceptableValues[0];
    }

    public override bool IsValid(object value) {
        return value is T v && AcceptableValues.Any(x => x.Equals(v));
    }

    public override string ToDescriptionString() {
        return "# Acceptable values: " + string.Join(", ", AcceptableValues.Select(x => x.ToString()).ToArray());
    }
}