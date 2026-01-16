using System;
using System.Threading.Tasks;
using BepInEx;
using ComputerysModdingUtilities;
using HarmonyLib;
using HarmonyLib.Tools;
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
    }

    private static void OnSDStartBroadcastReceived(SDStartBroadcast broadcast)
    {
        Debug.Log($"Received SDStartBroadcast with center at {broadcast.center}");
        SDTimer.BeginSD(broadcast.center);
    }

    [HarmonyPatch(typeof(GameManager))]
    class Patch
    {
        private static bool broadcastRegistered = false;
        [HarmonyPatch("ResetGame")]
        [HarmonyPatch("ProgressToNextTake")]
        [HarmonyPostfix]
        static void SetSDCountdown()
        {
            
            if (!broadcastRegistered)
            {
                FishNet.InstanceFinder.ClientManager.RegisterBroadcast<SDStartBroadcast>(OnSDStartBroadcastReceived);
                broadcastRegistered = true;
            }
            SDTimer.StartCountdown(10);
        }
    }

    [HarmonyPatch(typeof(RoundManager))]
    class Patch2
    {
        [HarmonyPatch("NextRoundCall")]
        [HarmonyPostfix]
        static void SetSDCountdown()
        {
            SDTimer.StartCountdown(15);
        }
    }
}