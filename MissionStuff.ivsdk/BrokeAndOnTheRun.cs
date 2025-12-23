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
    internal class BrokeAndOnTheRun
    {
        // IniShit
        private static bool setMoney;
        private static int moneyRemain;
        private static int moneyLost;

        // OtherShit
        private static bool loseMoney;

        public static void Init(SettingsFile settings)
        {
            setMoney = settings.GetBoolean("MAIN", "NSSetMoneyRemaining", false);
            moneyRemain = settings.GetInteger("MAIN", "NSMoneyRemaining", 25);
            moneyLost = settings.GetInteger("MAIN", "NSMoneyLost", 10000);
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("roman11"))
            {
                if (!loseMoney && IS_THIS_PRINT_BEING_DISPLAYED("R11003", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                {
                    STORE_SCORE(Main.PlayerIndex, out uint pMoney);
                    if (setMoney)
                        ADD_SCORE(Main.PlayerIndex, (moneyRemain - (int)pMoney));
                    else
                        ADD_SCORE(Main.PlayerIndex, - moneyLost);
                }
            }
            else if (loseMoney)
                loseMoney = false;
        }
    }
}
