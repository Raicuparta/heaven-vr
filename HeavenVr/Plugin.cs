using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace HeavenVr
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
	        Debug.Log($"Game version: {Application.version}");
	        
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
	        
		    var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
	        var managerSetings = ScriptableObject.CreateInstance<XRManagerSettings>();
	        var features = new OpenXRInteractionFeature[]
	        {
		        ScriptableObject.CreateInstance<HTCViveControllerProfile>(),
		        ScriptableObject.CreateInstance<OculusTouchControllerProfile>(),
		        ScriptableObject.CreateInstance<MicrosoftMotionControllerProfile>(),
		        ScriptableObject.CreateInstance<ValveIndexControllerProfile>(),
	        };
	        var xrLoader = ScriptableObject.CreateInstance<OpenXRLoader>();
	        OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.MultiPass;
	        OpenXRSettings.Instance.SetValue("features", features);
	        foreach (var feature in features)
	        {
		        feature.enabled = true;
	        }

	        generalSettings.Manager = managerSetings;
	        managerSetings.SetValue("m_RegisteredLoaders", new HashSet<XRLoader> {xrLoader});
	        managerSetings.TrySetLoaders(new List<XRLoader> {xrLoader});

	        managerSetings.InitializeLoaderSync();
			if (managerSetings.activeLoader == null) throw new Exception("Cannot initialize OpenVR Loader");

			

	        managerSetings.InitializeLoaderSync();
			managerSetings.StartSubsystems();
			
			VrInputManager.Create();
        }

        private void Update()
        {
	        // var devices = new List<UnityEngine.XR.InputDevice>();
	        // InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
	        // // InputDevices.GetDevices(devices);
	        // devices[0].TryGetFeatureValue(CommonUsages.triggerButton, out var __result);
	        // Debug.Log("count " + __result);
        }
    }
}