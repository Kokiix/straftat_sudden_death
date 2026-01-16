using System;
using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object;
using HarmonyLib;
using UnityEngine;

public class SDTimer : MonoBehaviour
{
    private static double _secRemaining = 0f;
    private static bool _suddenDeath = false;
    private static double _currRadius = 50f;
    private static double _shrinkRate = 1.25f;
    private static GameObject _zoneVisual;
    private static Vector3 _center = Vector3.zero;
    private static PlayerHealth[] _players;

    private static void RoundInit()
    {
        if (_zoneVisual != null)
        {
            GameObject.Destroy(_zoneVisual);
            _zoneVisual = null;
        }
        
        var timeManager = FishNet.InstanceFinder.TimeManager;
        timeManager.OnTick -= OnTickSD;
        timeManager.OnTick += OnTickSD;
    }

    public static void StartCountdown(double delay)
    {
        _suddenDeath = false;
        _secRemaining = delay;
        RoundInit();
    }

    public static void BeginSD(Vector3 center)
    {
        _center = center;
        _suddenDeath = true;
        RoundInit();
    }

    private static void OnTickSD()
    {
        var timeManager = FishNet.InstanceFinder.TimeManager;
        if (_suddenDeath)
        {
            if (_zoneVisual == null) return;
            if (_currRadius > 5f) _currRadius -= _shrinkRate * timeManager.TickDelta;
            _zoneVisual.transform.localScale = new Vector3((float)_currRadius * 2, 100f, (float)_currRadius * 2);
            if (timeManager.Tick % 60 == 0 && FishNet.InstanceFinder.IsServer)
            {
                foreach (var player in _players)
                {
                    Vector3 pos = player.transform.position;
                    double dist = Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(_center.x, 0, _center.z));
                    if (dist > _currRadius) player.RemoveHealth(0.25f);
                }
            }
            return;
        }

        if (_secRemaining > 0)
        {
            _secRemaining -= timeManager.TickDelta;

            if (_secRemaining <= 0) 
            {
                _suddenDeath = true;

                _zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                _zoneVisual.transform.position = Vector3.zero;
                GameObject.Destroy(_zoneVisual.GetComponent<Collider>());

                Vector3 centerSum = Vector3.zero;
                _players = GameObject.FindObjectsOfType<PlayerHealth>();
                foreach (var p in _players)
                {
                    if (p != null && !p.isKilled) centerSum += p.transform.position;
                }
                _center = centerSum / _players.Length;
                _zoneVisual.transform.position = new Vector3(_center.x, 0, _center.z);

                var renderer = _zoneVisual.GetComponent<MeshRenderer>();
                Material mat = renderer.material;

                // 1. Switch to a shader that supports transparency better than the Window shader.
                // "UI/Default" or "Unlit/Transparent" are great because they aren't affected by shadows/grey lighting.
                Shader zoneShader = Shader.Find("UI/Default"); 
                if (zoneShader == null) zoneShader = Shader.Find("Unlit/Transparent");

                mat.shader = zoneShader;

                // 2. The Color Property
                // UI/Default and Unlit shaders use "_Color"
                Color redZone = new Color(1f, 0f, 0f, 0.3f); // Red with 30% opacity
                mat.SetColor("_Color", redZone);

                // 3. THE FIX FOR "INSIDE" VISIBILITY (Culling)
                // 0 = Off (Double Sided), 1 = Front, 2 = Back
                // This makes the cylinder visible even when the player is inside it.
                mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

                // 4. Force Transparency Settings
                mat.SetFloat("_Mode", 3); // 3 is usually the "Transparent" tag in Unity standard shaders
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000; // Force it into the Transparent render queue

                if (FishNet.InstanceFinder.IsServer)
                {
                    FishNet.InstanceFinder.ServerManager.Broadcast(new SDStartBroadcast()
                    {
                        center = _center
                    });
                }
            }
        }
    }
}