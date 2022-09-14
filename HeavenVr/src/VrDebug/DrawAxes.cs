using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenVr.VrDebug;

public class DrawAxes : MonoBehaviour
{
    private static readonly Dictionary<string, DrawAxes> Objects = new();

    public static DrawAxes Create(Transform parent, string key)
    {
        Objects.TryGetValue(key, out var instance);

        if (instance == null)
        {
            instance = Instantiate(VrAssetLoader.DebugHelperPrefab, parent, false).AddComponent<DrawAxes>();
            instance.GetComponentInChildren<Text>().text = key;
            Objects[key] = instance;
        }
        else
        {
            Objects.Remove(key);
        }

        return instance;
    }
}