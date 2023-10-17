using System;
using System.Runtime.InteropServices;
using BepInEx;
using Discord;
using OriModding.BF.Core;
using UnityEngine;

namespace KFT.OriBF.DiscordLib;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public string Name => "Discord";

    [DllImport("Kernel32.dll")]
    private static extern IntPtr LoadLibrary(string path);


    private Discord.Discord discord;

    public bool DiscordEnabled { get; private set; } = false;

    private Activity lastActivity;

    void Awake()
    {
        LoadLibrary(this.GetAssetPath("discord_game_sdk.dll"));

        GetCurrentActivity = GetCurrentActivityDefault;
        GetActivityDetails = GetActivityDetailsDefault;

        InitialiseDiscord();
    }

    void Start()
    {
        UpdateActivity();
    }

    private const long OriGameID = 425440642173239296L;

    public delegate string ActivityDetailsDelegate();
    /// <summary>Get the smaller text in the discord activity</summary>
    public ActivityDetailsDelegate GetActivityDetails { get; set; }

    public delegate Activity ActivityDelegate();
    /// <summary>Get the larger text in the discord activity/// </summary>
    public ActivityDelegate GetCurrentActivity { get; set; }

    private void InitialiseDiscord()
    {
        try
        {
            discord = new(OriGameID, (ulong)CreateFlags.NoRequireDiscord);
        }
        catch (ResultException)
        {
            // Discord is not running
            return;
        }

        discord.SetLogHook(LogLevel.Debug, (level, message) =>
        {
            Logger.LogDebug($"DISCORD [{level}] {message}");
        });

        lastActivity = new Activity
        {
            Details = "In menus"
        };

        DiscordEnabled = true;
    }

    const float ActivityUpdateIntervalSeconds = 4f;
    private float activityUpdateTimeRemaining = ActivityUpdateIntervalSeconds;

    void FixedUpdate()
    {
        if (!DiscordEnabled)
            return;

        discord.RunCallbacks();

        activityUpdateTimeRemaining -= Time.deltaTime;
        if (activityUpdateTimeRemaining <= 0)
        {
            activityUpdateTimeRemaining += ActivityUpdateIntervalSeconds;
            UpdateActivity();
        }
    }

    private void UpdateActivity()
    {
        if (!DiscordEnabled)
            return;

        if (!GameStateMachine.Instance)
            return;

        var activity = GetCurrentActivity();
        activity.Assets.SmallImage = "https://cdn.discordapp.com/app-icons/425440642173239296/a3b78d910a0b76981ed977437ebc970c.webp";

        discord.GetActivityManager().UpdateActivity(activity, result =>
        {
            if (result != Result.Ok) Debug.Log("Failed to update Discord activity: " + result);
            else Debug.Log("Activity updated");
        });

        lastActivity = activity;
    }

    private Activity GetCurrentActivityDefault()
    {
        switch (GameStateMachine.Instance.CurrentState)
        {
            case GameStateMachine.State.Game:
                string location = GetSeinLocation();
                if (location == null)
                    return lastActivity;

                return new Activity
                {
                    Details = GetActivityDetails(),
                    State = location,
                    Timestamps = new ActivityTimestamps
                    {
                        Start = (long)(DateTime.UtcNow - TimeSpan.FromSeconds(GameTimer.Instance.CurrentTime) - new DateTime(1970, 1, 1)).TotalSeconds
                    }
                };

            case GameStateMachine.State.Prologue:
                return new Activity
                {
                    Details = GetActivityDetails()
                };

            default:
                return new Activity
                {
                    Details = GetActivityDetails()
                };
        }
    }

    private string GetSeinLocation()
    {
        var sein = Game.Characters.Sein;
        if (!sein)
            return "In game";

        string areaIdentifier = GameWorld.Instance.WorldAreaAtPosition(sein.Position)?.AreaIdentifier;

        switch (areaIdentifier)
        {
            case "sunkenGlades": return "Sunken Glades";
            case "hollowGrove": return "Hollow Grove";
            case "moonGrotto": return "Moon Grotto";
            case "ginsoTree": return "Ginso Tree";
            case "thornfeltSwamp": return "Thornfelt Swamp";
            case "mistyWoods": return "Misty Woods";
            case "valleyOfTheWind": return "Valley of the Wind";
            case "sorrowPass": return "Sorrow Pass";
            case "forlornRuins": return "Forlorn Ruins";
            case "mangrove": return "Black Root Burrows";
            case "mountHoru": return "Mount Horu";
            default: return null; // Activity won't be updated if this returns null - it's the void, so keep it at whatever it was last
        }
    }

    private string GetActivityDetailsDefault()
    {
        switch (GameStateMachine.Instance.CurrentState)
        {
            case GameStateMachine.State.Game:
                return "In game";

            case GameStateMachine.State.Prologue:
                return "Watching the prologue";

            default:
                return "In menus";
        }
    }
}
