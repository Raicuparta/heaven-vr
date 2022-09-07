using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using HeavenVr.Helpers;
using HeavenVr.ModSettings;
using HeavenVr.VrInput;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace HeavenVr;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class HeavenVrPlugin : BaseUnityPlugin
{
	private void Awake()
	{
		// Referencing so it gets loaded. TODO: figure out how to clean this up.
		VRIK vrIk;
		
		Debug.Log($"Game version: {Application.version}");
	        
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		VrSettings.SetUp(Config);
		VrAssetLoader.Init();
		SetUpXr();
		InputManager.Create();
	}

	private static void SetUpXr()
	{
		var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
		var managerSetings = ScriptableObject.CreateInstance<XRManagerSettings>();
		var features = new OpenXRInteractionFeature[]
		{
			ScriptableObject.CreateInstance<HTCViveControllerProfile>(),
			ScriptableObject.CreateInstance<OculusTouchControllerProfile>(),
			ScriptableObject.CreateInstance<MicrosoftMotionControllerProfile>(),
			ScriptableObject.CreateInstance<ValveIndexControllerProfile>()
		};
		var xrLoader = ScriptableObject.CreateInstance<OpenXRLoader>();
		OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.MultiPass;
		OpenXRSettings.Instance.SetValue("features", features);
		foreach (var feature in features)
		{
			feature.enabled = true;
		}

		generalSettings.Manager = managerSetings;
#pragma warning disable CS0618
		/*
		 * ManagerSettings.loaders is deprecated but very useful, allows me to add the xr loader without reflection.
		 * Should be fine unless the game's Unity version gets majorly updated, in which case the whole mod will be
		 * broken, so I'll have to update it anyway.
		 */
		managerSetings.loaders.Add(xrLoader);
#pragma warning restore CS0618

		managerSetings.InitializeLoaderSync();
		if (managerSetings.activeLoader == null) throw new Exception("Cannot initialize OpenVR Loader");

			

		managerSetings.InitializeLoaderSync();
		managerSetings.StartSubsystems();
	}
}