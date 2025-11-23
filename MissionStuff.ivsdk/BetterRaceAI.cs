using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System.Drawing;
using CCL.GTAIV.Extensions;
using static IVSDKDotNet.Native.Natives;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Configuration;
using System.Runtime;

namespace MissionStuff.ivsdk
{
    internal class BetterRaceAI
    {
        private static float downforceAmt;
        private static float speedBoostAmt;
        private static float brakeSlowAmt;
        public static void Init(SettingsFile settings)
        {
            downforceAmt = settings.GetFloat("MAIN", "DownforceAmount", 0);
            speedBoostAmt = settings.GetFloat("MAIN", "SpeedBoostAmount", 0);
            brakeSlowAmt = settings.GetFloat("MAIN", "BrakeSlowAmount", 0);
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("brucie4") || NativeGame.IsScriptRunning("brucie5m"))
            {
                GET_FRAME_TIME(out float frameTime);
                VehHelper.GrabAllVehicles();
                SET_CAR_DENSITY_MULTIPLIER(0.1f);

                foreach (var veh in VehHelper.VehHandles)
                {
                    int vehHandle = veh.Value;

                    if (!DOES_VEHICLE_EXIST(vehHandle) || !IS_CAR_A_MISSION_CAR(vehHandle))
                        continue;

                    GET_DRIVER_OF_CAR(vehHandle, out int pedHandle);

                    if (pedHandle == Main.PlayerHandle)
                        continue;

                    IVVehicle pedVeh = NativeWorld.GetVehicleInstanceFromHandle(vehHandle);

                    if (pedVeh.GetSpeedVector(true).Y > 0.1f)
                    {
                        pedVeh.ApplyForceRelative(new Vector3(0, 0, -1 * downforceAmt * frameTime), Vector3.Zero);

                        if (pedVeh.BrakePedal > 0.1f)
                            pedVeh.ApplyForceRelative(new Vector3(0, (-1 * pedVeh.GetSpeedVector(true).Y * brakeSlowAmt * frameTime), 0), Vector3.Zero);
                        else if (pedVeh.GasPedal > 0.1f)
                            pedVeh.ApplyForceRelative(new Vector3(0, ((speedBoostAmt * frameTime) / pedVeh.GetSpeedVector(true).Y), 0), Vector3.Zero);
                    }
                }
            }
        }
    }
}
