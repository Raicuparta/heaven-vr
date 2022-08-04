using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace HeavenVr;

public static class VrMaterialHelper
{
    public static void MakeMaterialDrawOnTop(Material material)
    {
        material.shader = Canvas.GetDefaultCanvasMaterial().shader;
        material.SetInt(Shader.PropertyToID("unity_GUIZTestMode"), (int) CompareFunction.Always);
    }

    private static void MakeGraphicDrawOnTop(Graphic graphic)
    {
        if (graphic.material == Canvas.GetDefaultCanvasMaterial())
            graphic.material = new Material(graphic.material);
        MakeMaterialDrawOnTop(graphic.material);
    }

    public static void MakeGraphicChildrenDrawOnTop(GameObject parent)
    {
        var graphics = parent.GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in graphics) MakeGraphicDrawOnTop(graphic);
    }
}