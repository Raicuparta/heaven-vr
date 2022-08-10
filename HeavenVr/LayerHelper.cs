using System;
using UnityEngine;

namespace HeavenVr;

public enum GameLayer
{
    // Layers included in base game:
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2, // Note: layer name is actually "Ignore Raycast".
    Water = 4,
    UI = 5,
    EverythingExceptPlayer = 8,
    Player = 9,
    OnlyPlayer = 10,
    Background = 11,
    Trigger = 12,
    Enemy = 13,
    Projectile = 14,
    ProjectileFence = 15,
    Critical = 16,
    Nothing = 17,
    DefaultSeeThru = 18,
    Loading = 19,
    Dialogue = 20,
    Map = 21,
    PlayerProjectileFence = 22,
    OnlyWorld = 23,
    MeleeFence = 24,
    DistanceCulling1 = 25,
    DistanceCulling2 = 26,
    DistanceCulling3 = 27,
    DistanceCulling4 = 28,
    DistanceCulling5 = 29,
    DistanceCulling6 = 30,
    DistanceCulling7 = 31,

    // Custom VR layers:
    VrUi = 3,
    VrPlayerUi = 6,
    Unused = 7,
}

public static class LayerHelper
{
    public static int GetMask(GameLayer layer, int baseMask = 0)
    {
        return baseMask | (1 << (int) layer);
    }

    public static int GetMask(params GameLayer[] layers)
    {
        if (layers == null) throw new ArgumentNullException(nameof(layers));
        var result = 0;
        foreach (var layer in layers) result = GetMask(layer, result);

        return result;
    }

    public static void SetLayer(Component component, GameLayer layer)
    {
        SetLayer(component.gameObject, layer);
    }

    public static void SetLayer(GameObject gameObject, GameLayer layer)
    {
        gameObject.layer = (int) layer;
    }

    public static void SetLayerRecursive(GameObject gameObject, GameLayer layer)
    {
        if (gameObject.GetComponent<VrStage>()) return;

        SetLayer(gameObject, layer);
        foreach (Transform child in gameObject.transform) SetLayerRecursive(child.gameObject, layer);
    }
}