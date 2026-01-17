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

    static int ticksUntilSD;
    static bool isSD = false;
    
    // TODO: add to config file
    static double radius;
    static double startRadius = 100f;
    static double minRadius = 10f;
    static double shrinkRate = 5f;
    static int startingTicksToSD = 500;

    public static void ResetTimer()
    {
        radius = startRadius;
        ticksUntilSD = startingTicksToSD;
        isSD = false;
        FishNet.InstanceFinder.TimeManager.OnTick += TickCountdown;
        FishNet.InstanceFinder.TimeManager.OnTick += TickSD;
    }

    public static void TickCountdown()
    {
        if (ticksUntilSD > 0) ticksUntilSD--;
        else if (!isSD)
        {
            isSD = true;
            SpawnSDCylinder();
        }
    }

    static GameObject SDCylinder;
    static PlayerHealth[] players;
    static Vector3 center;
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
            if (radius > minRadius) radius -= shrinkRate * tm.TickDelta;
            SDCylinder.transform.localScale = new Vector3((float)radius * 2, 100f, (float)radius * 2);
        }
    }

    public static void SpawnSDCylinder()
    {
        SDCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        // Set center at avg player position
        Vector3 centerSum = Vector3.zero;
        players = GameObject.FindObjectsOfType<PlayerHealth>();
        foreach (var p in players)
        {
            if (p != null && !p.isKilled) centerSum += p.transform.position;
        }
        center = centerSum / players.Length;
        SDCylinder.transform.position = new Vector3(center.x, 0, center.z);

        GameObject.Destroy(SDCylinder.GetComponent<Collider>());
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