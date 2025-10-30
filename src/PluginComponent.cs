using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace Silksong.SwitchLanguageInGame;

/// <summary>
/// All plugin component will be added in Plugin.Awake();
/// </summary>
public abstract class PluginComponent : MonoBehaviour {
    public static ManualLogSource Logger { get; private set; } = null!;

    /// <summary>
    /// Must be called on plugin.Awake();
    /// </summary>
    /// <param name="gameObject">plugin.gameObject</param>
    /// <param name="logger">plugin.Logger</param>
    public static void Initialize(GameObject gameObject, ManualLogSource logger) {
        Logger = logger;

        List<Type> componentTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(PluginComponent))).ToList();
        componentTypes.Sort((type, otherType) => GetPriority(otherType) - GetPriority(type));

        foreach (Type type in componentTypes) {
            gameObject.AddComponent(type);
        }
    }

    public static void UpdateComponents<T>(Action<T> action) where T : MonoBehaviour {
        var components = Resources.FindObjectsOfTypeAll<T>();
        if (components == null) return;

        foreach (var component in components) {
            action(component);
        }
    }

    private static int GetPriority(Type type) {
        foreach (object attribute in type.GetCustomAttributes(typeof(PluginComponentPriorityAttribute), false)) {
            return ((PluginComponentPriorityAttribute)attribute).Priority;
        }

        return 0;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PluginComponentPriorityAttribute(int priority) : Attribute {
    /// <summary>
    /// The higher the priority the earlier it is added to the plugin
    /// </summary>
    public int Priority { get; } = priority;
}
