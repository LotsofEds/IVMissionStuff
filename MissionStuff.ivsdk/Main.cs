using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    public class Main : Script
    {
        // PlayerStuff
        public static IVPed PlayerPed { get; set; }
        public static int PlayerIndex { get; set; }
        public static int PlayerHandle { get; set; }
        public static Vector3 PlayerPos { get; set; }

        // IniShit
        private static bool tripSkipEnable;
        private static bool buffAlliesEnable;
        private static bool noProofsEnable;
        private static bool moreWantedEnable;

        // MissionShit
        private static bool timedToBlowEnable;
        private static bool heartTimeEnable;
        private static bool buoysAhoyEnable;

        // SettingsFileShit
        public static SettingsFile mainSettings;
        public Main()
        {
            Uninitialize += Main_Uninitialize;
            Initialized += Main_Initialized;
            GameLoad += Main_GameLoad;
            Tick += Main_Tick;
        }
        private void Main_GameLoad(object sender, EventArgs e)
        {
            if (tripSkipEnable)
                TripSkip.GameLoad();
        }
        private void Main_Uninitialize(object sender, EventArgs e)
        {
            TripSkip.UnInit();
            TimedToBlow.UnInit();
            BuoysAhoy.UnInit();
        }

        private void Main_Initialized(object sender, EventArgs e)
        {
            mainSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff.ini", IVGame.GameStartupPath));
            mainSettings.Load();
            Init(Settings);
            if (tripSkipEnable)
                TripSkip.Init(Settings);
            if (buffAlliesEnable)
                ArmoredAllies.Init(Settings);
            if (noProofsEnable)
                RemoveProofs.Init(Settings);
            if (moreWantedEnable)
                WantedStars.Init(Settings);
            if (timedToBlowEnable)
                TimedToBlow.Init(Settings);
            if (heartTimeEnable)
                HaveAHeartTimed.Init(Settings);
            if (buoysAhoyEnable)
                BuoysAhoy.Init(Settings);
        }
        public static bool InitialChecks()
        {
            if (IS_PAUSE_MENU_ACTIVE()) return false;
            return true;
        }
        private static void Init(SettingsFile settings)
        {
            tripSkipEnable = settings.GetBoolean("MAIN", "TripSkip", false);
            buffAlliesEnable = settings.GetBoolean("MAIN", "BuffAllies", false);
            noProofsEnable = settings.GetBoolean("MAIN", "RemoveEnemyProofs", false);
            moreWantedEnable = settings.GetBoolean("MAIN", "MoreWantedStars", false);

            timedToBlowEnable = settings.GetBoolean("MAIN", "TimedToBlow", false);
            heartTimeEnable = settings.GetBoolean("MAIN", "Pacemaker", false);
            buoysAhoyEnable = settings.GetBoolean("MAIN", "BuoysAhoyRevamp", false);
        }
        private void Main_Tick(object sender, EventArgs e)
        {
            PlayerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            PlayerHandle = PlayerPed.GetHandle();
            PlayerIndex = (int)GET_PLAYER_ID();
            PlayerPos = PlayerPed.Matrix.Pos;

            if (!InitialChecks())
                return;
            if (PlayerPed == null)
                return;

            PedHelper.GrabAllPeds();
            if (tripSkipEnable)
                TripSkip.Tick();
            if (buffAlliesEnable)
                ArmoredAllies.Tick();
            if (noProofsEnable)
                RemoveProofs.Tick();
            if (moreWantedEnable)
                WantedStars.Tick();

            if (timedToBlowEnable)
                TimedToBlow.Tick();
            if (heartTimeEnable)
                HaveAHeartTimed.Tick();
            if (buoysAhoyEnable)
                BuoysAhoy.Tick();
        }
    }
}
