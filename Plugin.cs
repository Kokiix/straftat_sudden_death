using BepInEx;
using ComputerysModdingUtilities;
using HarmonyLib;
using UnityEngine;

[assembly: StraftatMod(isVanillaCompatible: true)]

namespace straftat_sudden_death;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin 
{
    static BepInEx.Logging.ManualLogSource log;
    static int ticksUntilSD;
    static bool isSD = false;

    private void Awake()
    {
        // Standard for every mod -----------------------
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        log = Logger;
        // ----------------------------------------------

        PauseManager.OnBeforeSpawn += ResetTimer;
    }

    public static void ResetTimer()
    {
        ticksUntilSD = 500;
        FishNet.InstanceFinder.TimeManager.OnTick += tickCountdown;
        FishNet.InstanceFinder.TimeManager.OnTick += tickSD;
    }

    public static void tickCountdown()
    {
        if (ticksUntilSD > 0) ticksUntilSD--;
        else
        {
            // Spawn cylinder
        }
    }

    public static void tickSD()
    {
        if (isSD)
        {
            // Damage players
            // Shrink cylinder
        }
    }
}