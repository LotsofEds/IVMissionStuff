using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;
using static System.Net.Mime.MediaTypeNames;

namespace MissionStuff.ivsdk
{
    internal class FasterAllies
    {
        private static float minDist;
        private static float maxSpd;
        private static float scaleFactor;

        public static void Init(SettingsFile settings)
        {
            minDist = settings.GetFloat("KEEP UP, MOTHERFURCKER", "SpeedUpStartDistance", 5.0f);
            maxSpd = settings.GetFloat("KEEP UP, MOTHERFURCKER", "MaxSpeedMult", 1.2f);
            scaleFactor = settings.GetFloat("KEEP UP, MOTHERFURCKER", "SpeedUpAmtPerMeter", 0.01f);
        }
        public static void Tick()
        {
            GET_PLAYER_GROUP(Main.PlayerIndex, out int pGroup);

            foreach (var ped in PedHelper.PedHandles)
            {
                int pedHandle = ped.Value;
                if (!DOES_CHAR_EXIST(pedHandle)) continue;
                if (!IS_PED_A_MISSION_PED(pedHandle)) continue;
                if (!IS_GROUP_MEMBER(pedHandle, pGroup)) continue;
                if (pedHandle == Main.PlayerHandle) continue;
                if (IS_CHAR_INJURED(pedHandle)) continue;

                GET_CHAR_COORDINATES(pedHandle, out Vector3 pedPos);

                GET_DISTANCE_BETWEEN_COORDS_3D(Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, pedPos.X, pedPos.Y, pedPos.Z, out float pDist);
                
                if (pDist >= minDist)
                {
                    float moveSpd = Main.Clamp(1 + ((pDist - minDist) * scaleFactor), 1.0f, maxSpd);
                    //IVGame.ShowSubtitleMessage(moveSpd.ToString());
                    SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(pedHandle, moveSpd);
                }
            }
        }

    }
}
