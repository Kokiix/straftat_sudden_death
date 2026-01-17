using BepInEx;
using ComputerysModdingUtilities;
using HarmonyLib;
using UnityEngine;

[assembly: StraftatMod(isVanillaCompatible: true)]

namespace straftat_sudden_death;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin 
{

    private void Awake()
    {
        // The mod is loaded as a MonoBehaviour attached to a GameObject; need to hide / make indestructible
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        // Scan ahead in this file and load in all patches
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

        PauseManager.OnBeforeSpawn += SDTimer.ResetTimer;
    }

    // Subscribe frequently with injection bc i dont trust that my code subbed to BeforeSpawn event
    // [HarmonyPatch(typeof(GameManager))]
    // class Patch
    // {
    //     [HarmonyPatch("ResetGame")]
    //     [HarmonyPatch("ProgressToNextTake")]
    //     [HarmonyPostfix]
    //     static void SubToBeforeSpawnEvent()
    //     {
            
            
    //     }
    // }
}