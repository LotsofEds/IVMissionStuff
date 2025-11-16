using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class RemoveProofs
    {
        private static readonly List<string> SCOList = new List<string>();

        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("MAIN", "NoProofSCOList", "");

            SCOList.Clear();
            foreach (string SCOName in SCOString.Split(','))
            {
                SCOList.Add(SCOName);
            }
        }
        public static void Tick()
        {
            foreach (string MissionSCO in SCOList)
            {
                if (NativeGame.IsScriptRunning(MissionSCO))
                {
                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!IS_PED_A_MISSION_PED(pedHandle)) continue;
                        if (pedHandle == Main.PlayerHandle) continue;
                        if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 57)) continue;

                        SET_CHAR_INVINCIBLE(pedHandle, false);
                        SET_CHAR_PROOFS(pedHandle, false, false, false, false, false);
                    }
                }
            }
        }
    }
}
