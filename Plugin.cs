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
        log.LogFatal("take start?");
    }
}