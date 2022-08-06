﻿using System;
using System.IO;
using LIV.SDK.Unity;
using UnityEngine;

namespace HeavenVr;

public static class VrAssetLoader
{
    private const string assetsDir = "/BepInEx/plugins/HeavenVr/Assets/";
    public static GameObject RunAnimationPrefab;
    public static GameObject VrUiQuadPrefab;
    
    public static void Init()
    {
        SDKShaders.LoadFromAssetBundle(LoadBundle("liv-shaders"));
        RunAnimationPrefab = LoadBundle("animation").LoadAsset<GameObject>("RunAnimation");
        VrUiQuadPrefab = LoadBundle("ui").LoadAsset<GameObject>("VrUiQuad");
    }
    
    private static AssetBundle LoadBundle(string assetName)
    {
        var bundle = AssetBundle.LoadFromFile($"{Directory.GetCurrentDirectory()}{assetsDir}{assetName}");

        if (bundle == null)
        {
            throw new Exception("Failed to load asset bundle" + assetName);
        }

        return bundle;
    }
}