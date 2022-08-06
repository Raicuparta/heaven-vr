using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HeavenVr;

/*
 * The default UI input module needs to be disabled for the VR laser input to work with the UI.
 * But UnityExplorer (a separate plugin I use for debugging) needs the default input module to be enabled in order for
 * mouse input to work in its UI.
 * By default, the game EventSystem gets disabled whenever UnityExplorer is open. So I'm detecting whether the default
 * EventSystem is enabled or not, and only enable the default input module when the EventSystem is disabled.
 */
public class DefaultInputModuleDisabler: MonoBehaviour
{
    private EventSystem eventSystem;
    private InputSystemUIInputModule inputModule;

    public static void Create(EventSystem eventSystem)
    {
        if (eventSystem.GetComponent<DefaultInputModuleDisabler>()) return;

        var instance = eventSystem.gameObject.AddComponent<DefaultInputModuleDisabler>();
        instance.eventSystem = eventSystem;
        instance.inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
    }
    
    private void Update()
    {
        inputModule.enabled = !eventSystem.enabled;
    }
}