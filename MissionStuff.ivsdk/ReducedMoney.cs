using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class ReducedMoney
    {
        // IniShit
        private static readonly List<string> SCOList = new List<string>();
        //private static int originalReward;
        private static int moneyLost;
        private static string moneyGXT;
        private static bool debug;

        // OtherShit
        private static string missionName;
        private static bool reductMoney;
        private static int missionsPassed;

        public static void IngameStart()
        {
            missionName = "";
            reductMoney = false;
        }
        public static void UnInit()
        {
            missionName = "";
            reductMoney = false;
        }
        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("REDUCED REWARDS", "SCOList", "");
            debug = settings.GetBoolean("REDUCED REWARDS", "Debug", false);

            SCOList.Clear();
            foreach (string SCOName in SCOString.Split(','))
            {
                if (!Main.scoSettings.DoesSectionExists(SCOName))
                    IVGame.Console.Print("~r~ERROR: Script name in ReducedMoneyRewards SCOList does not have a section in SCOSettings.ini!");
                else
                    SCOList.Add(SCOName);
            }
        }
        private static void LoadMissionData(SettingsFile settings, string scoName)
        {
            //originalReward = settings.GetInteger(scoName, "OriginalReward", 0);
            moneyGXT = settings.GetValue(scoName, "RMOnlyIfThisGXTShown", "none");
            moneyLost = settings.GetInteger(scoName, "MoneyReduction", 0);
        }
        public static void Tick()
        {
            foreach (string MissionSCO in SCOList)
            {
                if (NativeGame.IsScriptRunning(MissionSCO))
                {
                    if (missionName != MissionSCO)
                    {
                        LoadMissionData(Main.scoSettings, MissionSCO);
                        missionName = MissionSCO;
                        missionsPassed = GET_INT_STAT(253);
                    }
                    if (!reductMoney && (IS_THIS_PRINT_BEING_DISPLAYED(moneyGXT, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) || moneyGXT == "none"))
                        reductMoney = true;
                }
                else if (missionName == MissionSCO)
                {
                    STORE_SCORE(Main.PlayerIndex, out uint newMoney);
                    if (missionsPassed < GET_INT_STAT(253) && reductMoney)
                    {
                        if (debug)
                            IVGame.ShowSubtitleMessage(moneyLost.ToString());
                        ADD_SCORE(Main.PlayerIndex, -moneyLost);

                        if (!GET_IS_AUTOSAVE_OFF())
                            DO_AUTO_SAVE();
                    }

                    UnInit();
                }
            }
        }
    }
}
