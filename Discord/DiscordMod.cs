using System.Runtime.InteropServices;
using System;
using BaseModLib;
using HarmonyLib;
using OriDeModLoader;
using System.Reflection;
using System.IO;

namespace OriDeDiscord
{
    public class DiscordMod : IMod
    {
        public string Name => "Discord";

        private Harmony harmony;

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        public void Init()
        {
            LoadLibrary(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "discord_game_sdk.dll"));

            harmony = new Harmony("com.ori.discord");
            harmony.PatchAll();

            Controllers.Add<DiscordController>(null, "Discord");
        }

        public void Unload()
        {

        }
    }
}
