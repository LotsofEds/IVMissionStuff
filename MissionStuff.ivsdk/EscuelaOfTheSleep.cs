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

            checkPoints[0] = new Vector3(964.704f, 1879.377f, 22.987f);
            checkPoints[1] = new Vector3(907.906f, 1757.646f, 16.765f);
            checkPoints[2] = new Vector3(738.846f, 1558.068f, 22.342f);
            checkPoints[3] = new Vector3(724.196f, 1362.297f, 14.251f);
            checkPoints[4] = new Vector3(690.368f, 1389.113f, 14.279f);
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
                                SET_CAR_DENSITY_MULTIPLIER(0.1f);
                                GET_CAR_CHAR_IS_USING(pedHandle, out pVeh);
                                targetPed = pedHandle;
                                GET_GAME_TIMER(out fTimer);
                                checkPointNum = 0;
                            }
                        }
                    }
                    else if (checkPointNum >= 0 && checkPointNum < 5)
                    {
                        if (Main.gTimer >= fTimer + 1000)
                        {
                            GET_GAME_TIMER(out fTimer);
                            CLEAR_CHAR_TASKS(targetPed);
                            _TASK_CAR_DRIVE_TO_COORD(targetPed, pVeh, checkPoints[checkPointNum].X, checkPoints[checkPointNum].Y, checkPoints[checkPointNum].Z, 20.0f, 0, 0, 2, 4.0f, -1);
                        }

                        if (LOCATE_CHAR_IN_CAR_3D(targetPed, checkPoints[checkPointNum].X, checkPoints[checkPointNum].Y, checkPoints[checkPointNum].Z, 7.5f, 7.5f, 7.5f, false))
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
