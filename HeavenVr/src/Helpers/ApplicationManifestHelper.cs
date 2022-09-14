using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;

namespace HeavenVr.Helpers;

public static class ApplicationManifestHelper
{
    public static void UpdateManifest()
    {
        try
        {
            var vrManifest = new VrManifest();

            vrManifest.applications[0].last_played_time = CurrentUnixTimestamp().ToString();

            var manifestPath = Paths.ExecutablePath + @"\..\neonwhite.vrmanifest";

            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(vrManifest, Formatting.Indented));
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to write VR manifest: {exception}");
        }
    }

    private static long CurrentUnixTimestamp()
    {
        var foo = DateTime.Now;
        return ((DateTimeOffset)foo).ToUnixTimeSeconds();
    }

    [JsonObject(MemberSerialization.OptOut)]
    private class VrManifestApplication
    {
        // ReSharper disable InconsistentNaming
        public string app_key = "steam.app.1533420";
        public string image_path = "https://steamcdn-a.akamaihd.net/steam/apps/1533420/header.jpg";
        public string launch_type = "url";
        public string url = "steam://launch/1533420/VR";

        public Dictionary<string, Dictionary<string, string>> strings = new()
        {
            {
                "en_us", new Dictionary<string, string>
                {
                    { "name", "Neon White VR" }
                }
            }
        };

        public string last_played_time = "";
        // ReSharper restore InconsistentNaming
    }

    [JsonObject(MemberSerialization.OptOut)]
    private class VrManifest
    {
        // ReSharper disable InconsistentNaming
        public string source = "builtin";

        public VrManifestApplication[] applications = { new() };
        // ReSharper restore InconsistentNaming
    }
}