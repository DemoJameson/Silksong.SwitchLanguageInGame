using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Silksong.SwitchLanguageInGame;

[BepInAutoPlugin(id: "com.demojameson.switchlanguageingame", name: "Switch Language in Game")]
public partial class Plugin : BaseUnityPlugin {
    public static Plugin Instance { get; private set; } = null!;
    public static new ManualLogSource Logger { get; private set; } = null!;
    public static Harmony HarmonyInstance { get; } = new(Id);

    private void Awake() {
        Instance = this;
        Logger = base.Logger;
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        PluginComponent.Initialize(gameObject, Logger);
    }

    private void OnDestroy() {
        HarmonyInstance.UnpatchSelf();
    }
}