using BepInEx;
using BepInEx.Configuration;
using ComputerysModdingUtilities;
using UnityEngine;

[assembly: StraftatMod(isVanillaCompatible: false)]

namespace straftat_sudden_death;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin 
{
    static BepInEx.Logging.ManualLogSource log;
    private void Awake()
    {
        // Standard for every mod -----------------------
        string nonLoadBearingColonThree = ":3";
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        log = Logger;
        ConfigInit();
        // ----------------------------------------------

        PauseManager.OnBeforeSpawn += ResetTimer;
    }

    static int ticksUntilSD;
    static bool isSD = false;
    static Vector3 center;
    
    static float radius;
    static ConfigEntry<float> startRadius;
    static ConfigEntry<float> minRadius;
    static ConfigEntry<float> shrinkRate;
    static ConfigEntry<int> secUntilSD;
    static ConfigEntry<float> zoneAlpha;

    public void ConfigInit()
    {
        zoneColor = Config.Bind("general", "Death Zone Color", "110, 53, 45", "R, G, B");
        startRadius = Config.Bind("general", "Death Zone Starting Radius", 37.5f, "zone is a cylinder, radius measured in arbitrary in game units");
        minRadius = Config.Bind("general", "Death Zone Minimum Radius", 10f, "zone is a cylinder, radius measured in arbitrary in game units");
        shrinkRate = Config.Bind("general", "Units / Second that the Zone Shrinks at", 1f, "zone is a cylinder, radius measured in arbitrary in game units");
        secUntilSD = Config.Bind("general", "Seconds until Zone Appears", 45, "");
        damagePerSec = Config.Bind("general", "Damage per Second while in Zone", 10f, "");
        zoneAlpha = Config.Bind("general", "Death Zone Transparency", 0.5f, "Transparency of the zone (0.0 to 1.0)");
    }

    public static void ResetTimer()
    {
        radius = startRadius.Value;
        ticksUntilSD = secUntilSD.Value * 60;
        isSD = false;
        center = Vector3.zero;

        if (SDCylinder) GameObject.Destroy(SDCylinder);

        FishNet.InstanceFinder.TimeManager.OnTick -= TickCountdown;
        FishNet.InstanceFinder.TimeManager.OnTick -= TickSD;
        FishNet.InstanceFinder.TimeManager.OnTick += TickCountdown;
        FishNet.InstanceFinder.TimeManager.OnTick += TickSD;
    }

    public static void TickCountdown()
    {
        if (ticksUntilSD > 0) ticksUntilSD--;
        else if (!isSD)
        {
            isSD = true;
            if (SDCylinder) GameObject.Destroy(SDCylinder);
            SpawnSDCylinder();
        }
    }

    static GameObject SDCylinder;
    static PlayerHealth[] players;
    static ConfigEntry<float> damagePerSec;
    public static void TickSD()
    {
        if (isSD)
        {
            var tm = FishNet.InstanceFinder.TimeManager;
            // Damage players
            if (tm.Tick % 60 == 0 && FishNet.InstanceFinder.IsServer)
            {
                foreach (var player in players)
                {
                    if (!player) continue;
                    Vector3 pos = player.transform.position;
                    float dist = Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(center.x, 0, center.z));
                    if (dist > radius) player.RemoveHealth(damagePerSec.Value / 25f);
                }
            }
            // Shrink cylinder
            if (!SDCylinder) return;
            if (radius > minRadius.Value) radius -= shrinkRate.Value / 60;
            SDCylinder.transform.localScale = new Vector3(radius * 2, 200f, radius * 2);
        }
    }


    static ConfigEntry<string> zoneColor;
    public static void SpawnSDCylinder()
    {
        SDCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        SDCylinder.GetComponent<Collider>().enabled = false;

        // Set center at avg player position
        players = GameObject.FindObjectsOfType<PlayerHealth>();
        foreach (var p in players)
        {
            if (p != null && !p.isKilled) center += p.transform.position;
        }
        center /= players.Length;
        SDCylinder.transform.position = new Vector3(center.x, 0, center.z);

        var mat = SDCylinder.GetComponent<MeshRenderer>().material;
        mat.shader = Shader.Find("UI/Default");

        string[] rgb = zoneColor.Value.Split(", ");
        Color color = new (
            float.Parse(rgb[0]) / 255f, 
            float.Parse(rgb[1]) / 255f, 
            float.Parse(rgb[2]) / 255f, 
            zoneAlpha.Value
        );
        
        mat.SetColor("_Color", color);
        mat.SetInt("_Cull", 0);
    }

}