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
    internal class EscuelaOfTheSleep
    {
        // Booleans
        private static bool isDriving = false;

        private static uint fTimer;

        private static int targetPed;
        private static int pVeh;
        private static int checkPointNum;

        private static Vector3[] checkPoints = new Vector3[5];

        public static void UnInit()
        {
            SET_CAR_DENSITY_MULTIPLIER(1.0f);
            targetPed = -1;
            pVeh = -1;
            checkPointNum = -1;
        }
        public static void Init()
        {
            targetPed = -1;
            pVeh = -1;
            checkPointNum = -1;

            checkPoints[0] = new Vector3(958.958f, 1911.297f, 22.781f);
            checkPoints[1] = new Vector3(951.624f, 1721.242f, 16.296f);
            checkPoints[2] = new Vector3(847.391f, 1777.954f, 17.218f);
            checkPoints[3] = new Vector3(740.178f, 1582.625f, 21.964f);
            checkPoints[4] = new Vector3(738.763f, 1365.499f, 13.986f);
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("manny1"))
            {
                if (!isDriving && (IS_THIS_PRINT_BEING_DISPLAYED("MANNY1_3", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)))
                    isDriving = true;

                if (isDriving)
                {
                    if (checkPointNum < 0)
                    {
                        foreach (var ped in PedHelper.PedHandles)
                        {
                            int pedHandle = ped.Value;
                            if (!IS_PED_A_MISSION_PED(pedHandle))
                                continue;
                            if (pedHandle == Main.PlayerHandle)
                                continue;

                            GET_CHAR_MODEL(pedHandle, out int pModel);

                            if (pModel == GET_HASH_KEY("m_y_gafr_hi_01"))
                            {
                                SET_CAR_DENSITY_MULTIPLIER(0.2f);
                                GET_CAR_CHAR_IS_USING(pedHandle, out pVeh);
                                targetPed = pedHandle;
                                GET_GAME_TIMER(out fTimer);
                                checkPointNum = 0;

                                // 958.958f, 1911.297f, 22.781f
                                // 951.624f, 1721.242f, 16.296f
                                // 847.391f, 1777.954f, 17.218f
                                // 740.178f, 1582.625f, 21.964f
                                // 738.763f, 1364.499f, 13.986f
                                // 686.204f, 1456.223f, 14.145f
                            }
                        }
                    }
                    else if (checkPointNum >= 0 && checkPointNum < 5)
                    {
                        if (Main.gTimer >= fTimer + 1000)
                        {
                            GET_GAME_TIMER(out fTimer);
                            CLEAR_CHAR_TASKS(targetPed);
                            _TASK_CAR_DRIVE_TO_COORD(targetPed, pVeh, checkPoints[checkPointNum].X, checkPoints[checkPointNum].Y, checkPoints[checkPointNum].Z, 15.0f, 0, 0, 2, 4.0f, -1);
                        }

                        if (LOCATE_CHAR_IN_CAR_3D(targetPed, checkPoints[checkPointNum].X, checkPoints[checkPointNum].Y, checkPoints[checkPointNum].Z, 5.0f, 5.0f, 5.0f, false))
                            checkPointNum++;
                    }
                }
            }
            else if (isDriving)
            {
                SET_CAR_DENSITY_MULTIPLIER(1.0f);
                targetPed = -1;
                pVeh = -1;
                checkPointNum = -1;
            }
        }
    }
}
