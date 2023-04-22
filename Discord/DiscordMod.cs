using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BaseModLib;
using HarmonyLib;
using OriDeModLoader;
using UnityEngine;

namespace OriDeDiscord
{
    public class DiscordMod : IMod
    {
        public string Name => "Discord";

        private Harmony harmony;

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        public DiscordMod()
        {
            IPC.RegisterListener("Discord.ActivityDetails", SetActivityDetailsFunc);
        }


        public void Init()
        {
            LoadLibrary(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "discord_game_sdk.dll"));

            harmony = new Harmony("com.ori.discord");
            harmony.PatchAll();

            Controllers.Add<DiscordController>(null, "Discord");
        }

        public void Unload()
        {
            UnityEngine.Object.Destroy(DiscordController.Instance.gameObject);
            IPC.UnregisterListener("Discord.ActivityDetails", SetActivityDetailsFunc);
        }

        private void SetActivityDetailsFunc(string key, object value)
        {
            Debug.Log("Updatin activity details");
            Functions.getDetails = value as Func<string>;
        }
    }
}
