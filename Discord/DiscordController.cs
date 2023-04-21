using System;
using Discord;
using UnityEngine;

namespace OriDeDiscord
{
    public class DiscordController : MonoBehaviour
    {
        public static DiscordController Instance { get; private set; }

        private Discord.Discord discord;
        public bool DiscordEnabled { get; private set; } = false;

        private Activity lastActivity;

        void Awake()
        {
            Instance = this;
            Initialise();
            UpdateActivity();
        }

        private const long OriGameID = 425440642173239296L;

        private void Initialise()
        {
            try
            {
                discord = new Discord.Discord(OriGameID, (ulong)CreateFlags.NoRequireDiscord);
            }
            catch (ResultException)
            {
                // Discord is not running
                return;
            }

            discord.SetLogHook(LogLevel.Debug, (level, message) =>
            {
                Debug.Log($"Log[{level}] {message}");
            });

            lastActivity = new Activity
            {
                State = "In menus"
            };

            DiscordEnabled = true;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                discord.Dispose();
                Instance = null;
            }
        }

        const float ActivityUpdateIntervalSeconds = 4f;
        private float activityUpdateTimeRemaining = ActivityUpdateIntervalSeconds;

        void FixedUpdate()
        {
            if (!DiscordEnabled) return;

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
            if (!DiscordEnabled) return;

            if (!GameStateMachine.Instance)
                return;

            var activity = GetCurrentActivity();

            discord.GetActivityManager().UpdateActivity(activity, result =>
            {
                if (result != Result.Ok) Debug.Log("Failed to update Discord activity: " + result);
                else Debug.Log("Activity updated");
            });

            lastActivity = activity;
        }

        private Activity GetCurrentActivity()
        {
            switch (GameStateMachine.Instance.CurrentState)
            {
                case GameStateMachine.State.Game:
                    string location = GetSeinLocation();
                    if (location == null)
                        return lastActivity;

                    return new Activity
                    {
                        Details = "In game",
                        State = location,
                        Timestamps = new ActivityTimestamps
                        {
                            Start = (long)(DateTime.UtcNow - TimeSpan.FromSeconds(GameTimer.Instance.CurrentTime) - new DateTime(1970, 1, 1)).TotalSeconds
                        }
                    };

                case GameStateMachine.State.Prologue:
                    return new Activity
                    {
                        Details = "Watching the prologue"
                    };

                default:
                    return new Activity
                    {
                        Details = "In menus"
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
    }
}
