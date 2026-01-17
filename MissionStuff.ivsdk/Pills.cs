using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Linq;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class Pills
    {
        // IniShit
        private static float packieStat;
        private static Keys antiDepressKey;
        private static Keys adrenalineKey;
        private static Keys painKillerKey;
        private static int antiDepressHeal;
        private static int adrenalineDuration;
        private static int painKillerDuration;
        private static string antiDepressScreenFx;
        private static string adrenalineScreenFx;
        private static string painKillerScreenFx;
        private static int antiDepressCost;
        private static int adrenalineCost;
        private static int painkillerCost;
        private static int maxPills;
        private static int pillCooldown;

        // BooleShit
        private static bool canBuyPills;
        private static bool inMenu;
        private static bool keyPressed;
        private static bool pillActive;
        private static bool takeAntiDepress;
        private static bool takeAdrenaline;
        private static bool takePainkiller;
        private static bool adrenalineActive;
        private static bool painKillerActive;
        private static bool gotHealth;
        private static bool gotPillCount;

        // OtherShit
        private static int time;
        private static int pillCost;
        private static int pillIndex;
        private static string pillName;
        private static string pillDesc;
        private static int pillBlip;
        private static int pillCount;
        private static int aPillCount;
        private static int pPillCount;
        private static int dPillCount;
        private static uint fTimer;
        private static int objHandle;
        private static uint oldHealth;

        private static bool CheckCooldown()
        {
            if (Main.gTimer >= fTimer + pillCooldown)
                return true;
            else 
                return false;
        }
        public static void KeyDown()
        {
            if (IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                if (IVGame.IsKeyPressed(adrenalineKey) && !keyPressed && aPillCount > 0)
                {
                    keyPressed = true;
                    if (CheckCooldown())
                    {
                        if (!pillActive)
                            Adrenaline();
                    }
                    else
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "~r~You cannot overdose on pills!");

                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDER_1"))
                            PRINT_HELP("PLACEHOLDER_1");
                    }
                }
                else if (IVGame.IsKeyPressed(painKillerKey) && !keyPressed && pPillCount > 0)
                {
                    keyPressed = true;
                    if (CheckCooldown())
                    {
                        if (!pillActive)
                        PainKiller();
                    }
                    else
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "~r~You cannot overdose on pills!");

                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDER_1"))
                            PRINT_HELP("PLACEHOLDER_1");
                    }
                }
                else if (IVGame.IsKeyPressed(antiDepressKey) && !keyPressed && dPillCount > 0)
                {
                    keyPressed = true;
                    if (CheckCooldown())
                    {
                        if (!pillActive)
                        AntiDepressant();
                    }
                    else
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "~r~You cannot overdose on pills!");

                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDER_1"))
                            PRINT_HELP("PLACEHOLDER_1");
                    }
                }
            }
        }
        public static void KeyUp()
        {
            if (!IVGame.IsKeyPressed(adrenalineKey) && !IVGame.IsKeyPressed(painKillerKey) && !IVGame.IsKeyPressed(antiDepressKey) && keyPressed)
                keyPressed = false;
        }
        public static void UnInit()
        {
            REMOVE_BLIP(pillBlip);
            DELETE_OBJECT(ref objHandle);
            CLEAR_TIMECYCLE_MODIFIER();
            SET_TIME_SCALE(1.0f);
            SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, 1f);
        }
        public static void GameLoad()
        {
            gotPillCount = false;
            fTimer = 0;
            REMOVE_BLIP(pillBlip);
        }

        public static void Init (SettingsFile settings)
        {
            packieStat = settings.GetFloat("MAIN", "PackieLikeRequirement", 80);
            antiDepressKey = settings.GetKey("MAIN", "AntiDepressantKey", Keys.J);
            adrenalineKey = settings.GetKey("MAIN", "AdrenalineKey", Keys.K);
            painKillerKey = settings.GetKey("MAIN", "PainkillerKey", Keys.L);
            antiDepressHeal = settings.GetInteger("MAIN", "AntiDepressantHealAmount", 25);
            adrenalineDuration = settings.GetInteger("MAIN", "AdrenalineDuration", 20000);
            painKillerDuration = settings.GetInteger("MAIN", "PainkillerDuration", 20000);
            antiDepressScreenFx = settings.GetValue("MAIN", "AntiDepressantScreenFilter", "");
            adrenalineScreenFx = settings.GetValue("MAIN", "AdrenalineScreenFilter", "");
            painKillerScreenFx = settings.GetValue("MAIN", "PainkillerScreenFilter", "");
            antiDepressCost = settings.GetInteger("MAIN", "AntiDepressantCost", 200);
            adrenalineCost = settings.GetInteger("MAIN", "AdrenalineCost", 200);
            painkillerCost = settings.GetInteger("MAIN", "PainkilllerCost", 200);
            maxPills = settings.GetInteger("MAIN", "MaxPills", 3);
            pillCooldown = settings.GetInteger("MAIN", "PillCooldown", 300000);
        }

        public static void SavePillCount(SettingsFile settings, int adrPillCount, int pkPillCount, int antPillCount)
        {
            if (!settings.DoesSectionExists(IVGenericGameStorage.ValidSaveName))
                settings.AddSection(IVGenericGameStorage.ValidSaveName);
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "AdrenalineCount"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "AdrenalineCount");
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "PainkillerCount"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "PainkillerCount");
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "AntiDepressantCount"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "AntiDepressantCount");

            settings.SetInteger(IVGenericGameStorage.ValidSaveName, "AdrenalineCount", adrPillCount);
            settings.SetInteger(IVGenericGameStorage.ValidSaveName, "PainkillerCount", pkPillCount);
            settings.SetInteger(IVGenericGameStorage.ValidSaveName, "AntiDepressantCount", antPillCount);

            settings.Save();
            settings.Load();
        }
        private static void ProcessBuying()
        {
            if (!inMenu)
            {
                if (GET_FLOAT_STAT(20) >= packieStat)
                    canBuyPills = true;
                else
                {
                    REMOVE_BLIP(pillBlip);
                    canBuyPills = false;
                }
            }
            //canBuyPills = true;
            if (canBuyPills)
            {
                if (!DOES_BLIP_EXIST(pillBlip))
                {
                    // 1371.646, 621.487, 35.829
                    ADD_BLIP_FOR_COORD(1371.646f, 621.487f, 35.829f, out pillBlip);

                    NativeBlip pBlip = new NativeBlip(pillBlip);

                    pBlip.Icon = BlipIcon.Pickup_Health;
                    pBlip.Name = "Pills";
                    pBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP;
                    pBlip.Scale = 0.5f;
                    pBlip.ShowOnlyWhenNear = true;
                }

                if (LOCATE_CHAR_ON_FOOT_3D(Main.PlayerHandle, 1371.646f, 621.487f, 35.829f, 1.0f, 1.0f, 1.0f, true) || inMenu)
                {
                    if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDERSL") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDER_1") && !inMenu)
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDERSL", "~s~Press ~INPUT_PICKUP~ to buy pills.");
                        PRINT_HELP_WITH_STRING_NO_SOUND("PLACEHOLDERSL", "");
                        //DISPLAY_HELP_TEXT_THIS_FRAME("PLACEHOLDERSL", false);
                    }
                    //IVGame.ShowSubtitleMessage("ass");
                    if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.Action) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.Action))
                    {
                        pillIndex = 0;
                        inMenu = !inMenu;
                        if (inMenu)
                        {
                            CLEAR_HELP();
                            SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                        }
                        else
                        {
                            CLEAR_HELP();
                            SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                        }
                    }

                    if (inMenu)
                    {
                        if (pillIndex == 0)
                        {
                            pillName = "Adrenaline pills";
                            pillCost = adrenalineCost;
                            pillCount = aPillCount;
                            pillDesc = "~s~Slows down time and increases movement speed for a short period of time. Press K to take.";
                        }
                        else if (pillIndex == 1)
                        {
                            pillName = "Painkillers";
                            pillCost = painkillerCost;
                            pillCount = pPillCount;
                            pillDesc = "~s~Reduces damage taken by half for a short period of time. Press L to take.";
                        }
                        else if (pillIndex == 2)
                        {
                            pillName = "Anti-depressants";
                            pillCost = antiDepressCost;
                            pillCount = dPillCount;
                            pillDesc = "~s~Replenishes a small amount of health immediately. Press J to take.";
                        }
                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDERSL"))
                            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "~s~Use ~PAD_LEFT~ and ~PAD_RIGHT~ to browse pills. ~n~~s~Press ~INPUT_PICKUP~ to cancel. ~n~~s~Press ~INPUT_FRONTEND_ACCEPT~ to buy. ~n~~g~" + pillName + " $" + pillCost.ToString() + " ~n~~s~Currently have " + pillCount.ToString());

                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDER_1") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDERSL"))
                        {
                            PRINT_HELP_FOREVER("PLACEHOLDER_1");
                            // Description
                            IVGame.ShowSubtitleMessage(pillDesc);
                        }

                        if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavEnter) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavEnter))
                        {
                            STORE_SCORE(Main.PlayerIndex, out uint pMoney);
                            if (pMoney >= pillCost && (pPillCount + aPillCount + dPillCount) < maxPills)
                            {
                                ADD_SCORE(Main.PlayerIndex, -pillCost);
                                if (pillIndex == 0)
                                    aPillCount++;
                                
                                else if (pillIndex == 1)
                                    pPillCount++;

                                else if (pillIndex == 2)
                                    dPillCount++;
                            }
                            else if ((pPillCount + aPillCount + dPillCount) >= maxPills)
                            {
                                CLEAR_HELP();
                                IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDERSL", "~s~~r~You cannot carry any more pills!");
                                PRINT_HELP("PLACEHOLDERSL");
                            }
                            else if (pMoney < pillCost)
                            {
                                CLEAR_HELP();
                                IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDERSL", "~s~~r~You don't have enough money!");
                                PRINT_HELP("PLACEHOLDERSL");
                            }
                        }

                        else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavLeft) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavLeft))
                        {
                            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDERSL"))
                                CLEAR_HELP();
                            if (pillIndex > 0)
                                pillIndex--;
                            else
                                pillIndex = 2;
                        }

                        else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavRight) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavRight))
                        {
                            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDERSL"))
                                CLEAR_HELP();
                            if (pillIndex < 2)
                                pillIndex++;
                            else
                                pillIndex = 0;
                        }
                    }
                }
            }
        }
        public static void Tick()
        {
            if (!gotPillCount)
            {
                aPillCount = Main.mainSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "AdrenalineCount", 0);
                pPillCount = Main.mainSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "PainkillerCount", 0);
                dPillCount = Main.mainSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "AntiDepressantCount", 0);
                gotPillCount = true;
            }
            ProcessBuying();

            if (!HAVE_ANIMS_LOADED("amb@sprunk_plyr"))
                REQUEST_ANIMS("amb@sprunk_plyr");

            if (takeAdrenaline)
            {
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink", out float animTime);
                if (animTime > 0.85f || !IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink"))
                {
                    takeAdrenaline = false;
                    GET_GAME_TIMER(out fTimer);

                    DELETE_OBJECT(ref objHandle);

                    time = adrenalineDuration;

                    SET_TIMECYCLE_MODIFIER(adrenalineScreenFx);
                    SET_TIME_SCALE(0.75f);
                    SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, (float)(1 / 0.75));
                    adrenalineActive = true;
                }
            }

            else if (takePainkiller)
            {
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink", out float animTime);
                if (animTime > 0.85f || !IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink"))
                {
                    takePainkiller = false;
                    GET_GAME_TIMER(out fTimer);

                    DELETE_OBJECT(ref objHandle);

                    time = painKillerDuration;

                    SET_TIMECYCLE_MODIFIER(painKillerScreenFx);
                    painKillerActive = true;
                }
            }

            else if (takeAntiDepress)
            {
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink", out float animTime);
                if (animTime > 0.85f || !IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink"))
                {
                    DELETE_OBJECT(ref objHandle);
                    CLEAR_TIMECYCLE_MODIFIER();
                    takeAntiDepress = false;
                    pillActive = false;
                }
            }

            if (adrenalineActive && (Main.gTimer >= fTimer + time || !IS_PLAYER_CONTROL_ON(Main.PlayerIndex)))
            {
                CLEAR_TIMECYCLE_MODIFIER();
                SET_TIME_SCALE(1.0f);
                SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, 1f);
                adrenalineActive = false;
                pillActive = false;
            }

            else if (painKillerActive)
            {
                GET_CHAR_HEALTH(Main.PlayerHandle, out uint pHealth);

                if (!gotHealth)
                {
                    oldHealth = pHealth;
                    gotHealth = true;
                }

                if (oldHealth > pHealth && gotHealth)
                {
                    SET_CHAR_HEALTH(Main.PlayerHandle, pHealth + ((oldHealth - pHealth) / 2));
                    gotHealth = false;
                }

                if ((Main.gTimer >= fTimer + time) || !IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                {
                    CLEAR_TIMECYCLE_MODIFIER();
                    painKillerActive = false;
                    pillActive = false;
                }
            }

            if (DID_SAVE_COMPLETE_SUCCESSFULLY() && GET_IS_DISPLAYINGSAVEMESSAGE())
                SavePillCount(Main.mainSettings, aPillCount, pPillCount, dPillCount);
        }
        private static void TakePill()
        {
            pillActive = true;
            CREATE_OBJECT(GET_HASH_KEY("cspillbottle"), Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z + 10f, out objHandle, true);
            SET_OBJECT_COLLISION(objHandle, false);
            ATTACH_OBJECT_TO_PED(objHandle, Main.PlayerHandle, (uint)eBone.BONE_RIGHT_HAND, 0.1f, 0.02f, -0.02f, 0f, 0f, 0f, 0);
            _TASK_PLAY_ANIM_SECONDARY_NO_INTERRUPT(Main.PlayerHandle, "partial_drink", "amb@sprunk_plyr", 4, 0, 0, 0, 0, -1);
        }
        private static void Adrenaline()
        {
            TakePill();
            aPillCount--;
            takeAdrenaline = true;
        }
        private static void PainKiller()
        {
            TakePill();
            pPillCount--;
            takePainkiller = true;
        }
        private static void AntiDepressant()
        {
            TakePill();
            dPillCount--;
            GET_CHAR_HEALTH(Main.PlayerHandle, out uint pHealth);
            SET_CHAR_HEALTH(Main.PlayerHandle, pHealth + (uint)antiDepressHeal);
            SET_TIMECYCLE_MODIFIER(antiDepressScreenFx);
            GET_GAME_TIMER(out fTimer);
            takeAntiDepress = true;
        }
    }
}
