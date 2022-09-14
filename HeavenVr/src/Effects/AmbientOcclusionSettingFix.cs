using UnityEngine;

namespace HeavenVr.Effects;

// I think the game devs accidentally left this AmplifyAmbientOcclusion thing in the game.
// They have two (three?) ambient occlusion components at once, and this one doesn't work correctly in the base game.
// They have an ambient occlusion toggle, but it only affects this component on game startup.
// This makes it so the setting actually toggles the state of this ambient occlusion component.
public class AmbientOcclusionSettingFix : MonoBehaviour
{
    private AmplifyOcclusionRendererFeature _amplifyOcclusionRendererFeature;

    public static void Create(AmplifyOcclusionRendererFeature amplifyOcclusionRendererFeature)
    {
        var instance = new GameObject("AmbientOcclusionSettingFix").AddComponent<AmbientOcclusionSettingFix>();
        instance._amplifyOcclusionRendererFeature = amplifyOcclusionRendererFeature;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        _amplifyOcclusionRendererFeature.SetActive(GameDataManager.prefs.ao);
    }
}