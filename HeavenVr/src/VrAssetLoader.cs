using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace HeavenVr;

public static class VrAssetLoader
{
    private const string AssetsDir = "HeavenVr/Assets/";
    public static GameObject RunAnimationPrefab;
    public static RenderTexture VrUiRenderTexture;
    public static GameObject VrUiQuadPrefab;
    public static GameObject RightHandPrefab;
    public static GameObject DebugHelperPrefab;
    public static GameObject PlayerBodyIk;
    public static AssetBundle LivShadersBundle;
    
    public static void Init()
    {
        LivShadersBundle = LoadBundle("liv-shaders");
        RunAnimationPrefab = LoadBundle("animation").LoadAsset<GameObject>("RunAnimation");
        VrUiQuadPrefab = LoadBundle("ui").LoadAsset<GameObject>("VrUiQuad");
        RightHandPrefab = LoadBundle("hands").LoadAsset<GameObject>("RightHand");
        DebugHelperPrefab = LoadBundle("debug").LoadAsset<GameObject>("DebugHelper");
        PlayerBodyIk = LoadBundle("player").LoadAsset<GameObject>("Player");
        VrUiRenderTexture = VrUiQuadPrefab.GetComponentInChildren<Renderer>().material.mainTexture as RenderTexture;
    }
    
    private static AssetBundle LoadBundle(string assetName)
    {
        var bundle = AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, Path.Combine(AssetsDir, assetName)));

        if (bundle == null)
        {
            throw new Exception("Failed to load asset bundle" + assetName);
        }

        return bundle;
    }
}
