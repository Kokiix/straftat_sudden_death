using BepInEx;
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
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        log = Logger;
        // ----------------------------------------------

        PauseManager.OnBeforeSpawn += ResetTimer;
    }

    static int ticksUntilSD;
    static bool isSD = false;
    static Vector3 center;
    
    // TODO: add to config file
    static double radius;
    static double startRadius = 100f;
    static double minRadius = 10f;
    static double shrinkRate = 5f;
    static int secUntilSD = 5;

    public static void ResetTimer()
    {
        radius = startRadius;
        ticksUntilSD = secUntilSD * 60;
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
                    Vector3 pos = player.transform.position;
                    double dist = Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(center.x, 0, center.z));
                    if (dist > radius) player.RemoveHealth(0.25f);
                }
            }
            // Shrink cylinder
            if (!SDCylinder) return;
            if (radius > minRadius) radius -= shrinkRate;
            SDCylinder.transform.localScale = new Vector3((float)radius * 2, 200f, (float)radius * 2);
        }
    }

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
        mat.SetColor("_Color", new Color(1f, 0f, 0f, 0.3f));
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

        // AI black magic for now :(
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
    }
}