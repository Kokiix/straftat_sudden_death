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
    
    // TODO: add to config file
    static float radius;
    static ConfigEntry<float> startRadius;
    static ConfigEntry<float> minRadius;
    static ConfigEntry<float> shrinkRate;
    static ConfigEntry<int> secUntilSD;

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
                    if (dist > radius) player.RemoveHealth(0.25f);
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

        // Set center at avg player position
        players = GameObject.FindObjectsOfType<PlayerHealth>();
        var c = SDCylinder.GetComponent<Collider>();
        c.enabled = false;
        c.isTrigger = false;

        foreach (var p in players)
        {
            if (p != null && !p.isKilled) center += p.transform.position;
        }
        center /= players.Length;
        SDCylinder.transform.position = new Vector3(center.x, 0, center.z);

        var renderer = SDCylinder.GetComponent<MeshRenderer>();
        Material mat = renderer.material;

        // IDK what shaders exist on what maps
        var defaultShader = Shader.Find("UI/Default");
        mat.shader = defaultShader ? defaultShader : Shader.Find("Unlit/Transparent");

        // zone color
        string[] colorStrings = zoneColor.Value.Split(", ");
        float[] normalizedColors = [0f, 0f, 0f];
        for (int i = 0; i < 3; i++) 
        {
            normalizedColors[i] = float.Parse(colorStrings[i]) / 255f;
        }
        mat.SetColor("_Color", new Color(normalizedColors[0], normalizedColors[1], normalizedColors[2], 1f));
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

        // AI black magic for now to create transparency :(
        // mat.SetFloat("_Mode", 3);
        // mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        // mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // mat.SetInt("_ZWrite", 0);
        // mat.renderQueue = 3000;
    }

    public void ConfigInit()
    {
        zoneColor = Config.Bind("general", "Death Zone Color", "110, 53, 45", "R, G, B");
        startRadius = Config.Bind("general", "Death Zone Starting Radius", 37.5f, "zone is a cylinder, radius measured in arbitrary in game units");
        minRadius = Config.Bind("general", "Death Zone Minimum Radius", 10f, "zone is a cylinder, radius measured in arbitrary in game units");
        shrinkRate = Config.Bind("general", "Units / Second that the Zone Shrinks at", 1f, "zone is a cylinder, radius measured in arbitrary in game units");
        secUntilSD = Config.Bind("general", "Seconds until Zone Appears", 45, "");
    }
}