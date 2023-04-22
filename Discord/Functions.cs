using System;

namespace OriDeDiscord
{
    public static class Functions
    {
        public static Func<string> getDetails = GetActivityDetailsDefault;

        public static string GetActivityDetailsDefault()
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
}
