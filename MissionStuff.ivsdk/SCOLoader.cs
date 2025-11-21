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
    internal class SCOLoader
    {
        private static readonly List<string> SCOList = new List<string>();
        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("MAIN", "SCOsToLoad", "");

            SCOList.Clear();
            foreach (string SCOName in SCOString.Split(','))
            {
                SCOList.Add(SCOName);
            }
        }
        public static void Tick()
        {
            foreach (string SCOName in SCOList)
            {
                if (!NativeGame.IsScriptRunning(SCOName))
                {
                    REQUEST_SCRIPT(SCOName);
                    if (HAS_SCRIPT_LOADED(SCOName))
                        NativeGame.StartNewScript(SCOName, 1024);
                }
            }
        }
    }
}
