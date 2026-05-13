using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Logic.Town.Items;

namespace IncreaseMaxFarmHandsMod;

/// <summary>
/// Farm Together 2 - Farmhand Stall max level expansion mod
/// Increases TownFarmhandsInstance.LevelCap when the configured NEW_MAX_LEVEL is higher than vanilla.
/// Config file: BepInEx/config/increasemaxfarmhandsmod.cfg
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static ConfigEntry<uint> NewMaxLevel { get; private set; } = null!;

    public override void Load()
    {
        NewMaxLevel = Config.Bind(
            "General",
            "NEW_MAX_LEVEL",
            50u,
            new ConfigDescription(
                "Farmhand stall max level (LevelCap). Used when vanilla cap is lower than this value.",
                new AcceptableValueRange<uint>(1, 100)));

        Log.LogInfo($"[IncreaseMaxFarmHandsMod] Loaded - farmhand stall max level target: {NewMaxLevel.Value} (edit NEW_MAX_LEVEL in BepInEx/config/{MyPluginInfo.PLUGIN_GUID}.cfg).");

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(Plugin).Assembly);

        Log.LogInfo("[IncreaseMaxFarmHandsMod] Harmony patches applied successfully.");
    }
}

/// <summary>
/// Patches TownFarmhandsInstance.LevelCap to return the configured NEW_MAX_LEVEL
/// if the current cap is lower.
/// </summary>
[HarmonyPatch(typeof(TownFarmhandsInstance), "get_LevelCap")]
public static class TownFarmhandsInstance_LevelCap_Patch
{
    private static bool _logged = false;

    [HarmonyPostfix]
    public static void Postfix(ref uint __result)
    {
        var cap = Plugin.NewMaxLevel.Value;
        if (__result < cap)
        {
            if (!_logged)
            {
                BepInEx.Logging.Logger.CreateLogSource("IncreaseMaxFarmHandsMod")
                    .LogInfo($"[IncreaseMaxFarmHandsMod] LevelCap overridden: {__result} -> {cap}");
                _logged = true;
            }
            __result = cap;
        }
    }
}
