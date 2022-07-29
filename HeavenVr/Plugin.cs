using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using InputDevice = UnityEngine.XR.InputDevice;

namespace HeavenVr
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
	        
		    var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
	        var managerSetings = ScriptableObject.CreateInstance<XRManagerSettings>();
			var feature = ScriptableObject.CreateInstance<HTCViveControllerProfile>();
			feature.enabled = true;
	        var xrLoader = ScriptableObject.CreateInstance<OpenXRLoader>();
			// Reference OpenXRSettings just to make this work.
	        // TODO figure out how to do this properly.
	        OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.MultiPass;
	        OpenXRSettings.Instance.SetValue("features", new [] { feature });
			feature.enabled = true;

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