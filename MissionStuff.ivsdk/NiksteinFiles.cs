using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class NiksteinFiles
    {
        private static bool gotFiles;
        private static bool removeRecords;
        private static bool evidenceRemoved;
        private static bool noWanted;

        private static int secBlip;
        public static void Init()
        {
            gotFiles = false;
            removeRecords = false;
            evidenceRemoved = false;
            noWanted = false;
        }
        public static void UnInit()
        {
            REMOVE_BLIP(secBlip);
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("francis2b"))
            {
                if (!gotFiles && !removeRecords && !evidenceRemoved)
                {
                    if (IS_THIS_PRINT_BEING_DISPLAYED("FM_COMMAND_16", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    {
                        IVGame.ShowSubtitleMessage("~s~Go to the ~b~security room ~s~and remove all your records.");
                        gotFiles = true;
                        removeRecords = true;
                    }
                    else if (IS_THIS_PRINT_BEING_DISPLAYED("FM_COMMAND_18", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    {
                        IVGame.ShowSubtitleMessage("~s~Go to the ~b~security room ~s~and remove all your records.");
                        gotFiles = true;
                        removeRecords = true;
                        noWanted = true;
                    }
                }
                else if (removeRecords)
                {
                    if (!DOES_BLIP_EXIST(secBlip))
                    {
                        ADD_BLIP_FOR_COORD(93.8f, -683.321f, 9.598f, out secBlip);

                        NativeBlip pBlip = new NativeBlip(secBlip);

                        pBlip.Icon = BlipIcon.Misc_Destination;
                        pBlip.Name = "Security Room";
                        pBlip.Color = eBlipColor.BLIP_COLOR_CYAN;
                        pBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP;
                    }
                    if (evidenceRemoved)
                    {
                        REMOVE_BLIP(secBlip);
                        if (IS_SCREEN_FADED_OUT())
                        {
                            SET_CHAR_COORDINATES(Main.PlayerHandle, 93.8f, -683.321f, 8.598f);
                            SET_CHAR_HEADING(Main.PlayerHandle, 90.0f);
                            DO_SCREEN_FADE_IN(1000);
                            SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                            IVGame.ShowSubtitleMessage("~s~Your tracks have been covered. Leave the building.");
                            removeRecords = false;
                        }
                    }
                    else if (noWanted)
                    {
                        if (!LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, 93.8f, -683.321f, 9.598f, 150.0f, 150.0f, 150.0f, false))
                            noWanted = false;
                        else if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, 93.8f, -683.321f, 9.598f, 1.0f, 1.0f, 1.0f, true))
                        {
                            SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                            DO_SCREEN_FADE_OUT(1000);
                            evidenceRemoved = true;
                        }
                    }
                    else
                    {
                        ALTER_WANTED_LEVEL_NO_DROP(Main.PlayerIndex, 2);
                        APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                        if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, 93.8f, -683.321f, 9.598f, 1.0f, 1.0f, 1.0f, true))
                        {
                            SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                            DO_SCREEN_FADE_OUT(1000);
                            evidenceRemoved = true;
                        }
                    }
                }
            }
            else if (gotFiles || removeRecords || evidenceRemoved)
            {
                REMOVE_BLIP(secBlip);
                gotFiles = false;
                removeRecords = false;
                evidenceRemoved = false;
                noWanted = false;
            }
        }
    }
}
