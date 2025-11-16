using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class WantedStars
    {
        private static readonly List<string> SCOList = new List<string>();

        private static string missionName;

        // IniShit
        private static bool dontDrop;
        private static string wantedGXT;
        private static uint maxWanted;
        private static uint setWanted;

        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("MAIN", "MoreWantedSCOList", "");

            SCOList.Clear();
            foreach (string SCOName in SCOString.Split(','))
            {
                SCOList.Add(SCOName);
            }
        }
        private static void LoadWantedData(SettingsFile settings, string scoName)
        {
            wantedGXT = settings.GetValue(scoName, "WantedGXT", "");
            maxWanted = settings.GetUInteger(scoName, "MaxWantedStars", 0);
            setWanted = settings.GetUInteger(scoName, "SetWantedStars", 0);
            dontDrop = settings.GetBoolean(scoName, "DontDropWanted", false);
        }
        public static void Tick()
        {
            foreach (string MissionSCO in SCOList)
            {
                if (NativeGame.IsScriptRunning(MissionSCO))
                {
                    missionName = MissionSCO;
                    LoadWantedData(Main.mainSettings, MissionSCO);
                    if (IS_THIS_PRINT_BEING_DISPLAYED(wantedGXT, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    {
                        SET_MAX_WANTED_LEVEL(maxWanted);
                        if (dontDrop)
                            ALTER_WANTED_LEVEL_NO_DROP(Main.PlayerIndex, setWanted);
                        else
                            ALTER_WANTED_LEVEL(Main.PlayerIndex, setWanted);
                        APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                    }
                }
                else if (missionName == MissionSCO)
                    SET_MAX_WANTED_LEVEL(6);
            }
        }
    }
}
