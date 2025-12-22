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
    internal class VCSBuyBackWeapons
    {
        // IniShit
        private static bool buyBackWeapons;

        // WeaponShit
        private static List<int> inventory = new List<int>();
        private static Dictionary<int, int> ammo = new Dictionary<int, int>();
        private static int fullPrice;

        // OtherShit
        private static bool hasDied;
        private static bool messageShown;
        private static uint fTimer;
        public static void Init(SettingsFile settings)
        {
            buyBackWeapons = settings.GetBoolean("MAIN", "BuyBackWeapons", false);
        }
        public static void Tick()
        {
            if (IS_CHAR_DEAD(Main.PlayerHandle) && IS_SCREEN_FADING_OUT() && !hasDied)
            {
                inventory = GetWeaponInventory(true);
                ammo = GetWeaponAmmoCounts();

                REMOVE_ALL_CHAR_WEAPONS(Main.PlayerPed.GetHandle());
                SET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_UNARMED, true);
                if (buyBackWeapons)
                    hasDied = true;
            }
            else if (hasDied && IS_SCREEN_FADED_IN() && !IS_CHAR_DEAD(Main.PlayerHandle))
            {
                messageShown = false;
                ShowBribeNotification();
                hasDied = false;
            }
            if (messageShown && NativeControls.IsGameKeyPressed(0, GameKey.Action) && IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDER_1"))
            {
                RestoreWeapons();
            }
        }
        public static List<int> GetWeaponInventory(bool IncludeMelee)
        {
            List<int> inventory = new List<int>();

            for (int i = 0; i <= 8; i++)
            {
                GET_CHAR_WEAPON_IN_SLOT(Main.PlayerPed.GetHandle(), i, out int weaponInSlot, out _, out _);
                if (weaponInSlot == 0) continue;

                var info = IVWeaponInfo.GetWeaponInfo((uint)weaponInSlot);
                if (info == null) continue;

                if (info.FireType != 0 || IncludeMelee)
                {
                    inventory.Add(weaponInSlot);
                }
            }

            return inventory;
        }
        public static Dictionary<int, int> GetWeaponAmmoCounts()
        {
            Dictionary<int, int> ammoCounts = new Dictionary<int, int>();

            foreach (int weapon in GetWeaponInventory(false))
            {
                GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)weapon, out int ammo);
                ammoCounts[weapon] = ammo;
            }

            return ammoCounts;
        }

        private static void ShowBribeNotification()
        {
            if (!IS_HELP_MESSAGE_BEING_DISPLAYED() && !messageShown)
            {
                BribeForWeaponsNotification(inventory);
                messageShown = true;
            }
        }

        private static void RestoreWeapons()
        {
            if (Main.PlayerPed.PlayerInfo.GetMoney() < fullPrice)
                return;

            PLAY_SOUND_FRONTEND(-1, "FRONTEND_OTHER_INFO");

            foreach (var weapon in inventory)
            {
                int ammoToGive = ammo.ContainsKey(weapon) ? ammo[weapon] : 0;
                GIVE_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), weapon, ammoToGive, true);
            }

            IVPlayerInfoExtensions.RemoveMoney(Main.PlayerPed.PlayerInfo, fullPrice);
            fullPrice = 0;

            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDER_1"))
            {
                CLEAR_HELP();
                messageShown = false;
            }
        }
        private static int GetWeaponBribePrice(int weapon)
        {
            int weapPrice = Main.bribeSettings.GetInteger(weapon.ToString(), "WeaponBribePrice", 500);
            return weapPrice;
        }
        private static int GeAmmoBribePrice(int weapon)
        {
            int ammoPrice = Main.bribeSettings.GetInteger(weapon.ToString(), "AmmoBribePriceMult", 2);
            return ammoPrice;
        }
        private static void BribeForWeaponsNotification(List<int> inventory)
        {
            CalculateBribePrice(inventory);
            string message;

            if (fullPrice == 0) return;

            if (Main.PlayerPed.PlayerInfo.GetMoney() < fullPrice)
            {
                message = $"You don't have enough money to bribe.";
            }
            else
            {
                message = $"Pay ~g~${fullPrice} ~s~bribe to get back your weapons?" +
                $"~n~Press ~INPUT_PICKUP~ to pay.";
            }


            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP("PLACEHOLDER_1");
        }
        private static void CalculateBribePrice(List<int> inventory)
        {
            fullPrice = 0;

            foreach (int weapon in inventory)
            {
                fullPrice += GetWeaponBribePrice(weapon);
                fullPrice += ammo.ContainsKey(weapon) ? (ammo[weapon] * GeAmmoBribePrice(weapon)) : 0;
            }
        }
    }
}
