using UnityEngine;

public static class GameConstants
{
    public static class Tags
    {
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string Minimarket = "Minimarket";
        public const string Defense = "Defense";
        public const string Ground = "Ground";
    }

    public static class Layers
    {
        public const string Ground = "Ground";
        public const string Default = "Default";
        public const string OneWayPlatform = "OneWayPlatform";
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string IgnoreRaycast = "Ignore Raycast";
    }

    public static class PrefKeys
    {
        public const string CandyTotal = "candy_total";
        public const string UnlockedLevel = "unlocked_level";
        public const string LevelStarsPrefix = "level_stars_";
        public const string EquippedRangedWeapon = "equipped_ranged_weapon";
        public const string EquippedMeleeWeapon = "equipped_melee_weapon";
        public const string EquippedArmor = "equipped_armor";
        public const string EquippedJetpack = "equipped_jetpack";
        public const string EquippedConsumable1 = "equipped_consumable_1";
        public const string EquippedConsumable2 = "equipped_consumable_2";
        public const string ActiveWeaponType = "active_weapon_type";
        public const string OwnedEquipmentPrefix = "owned_eq_";
        public const string UpgradePrefix = "upgrade_";
        public const string ConsumablePrefix = "consumable_";
        public const string MusicVolume = "music_volume";
        public const string SfxVolume = "sfx_volume";
    }

    public static class UpgradeKeys
    {
        public const string WeaponDamage = "weapon_damage";
        public const string WeaponFireRate = "weapon_fire_rate";
        public const string WeaponAmmo = "weapon_ammo";
    }

    public static class EquipmentNames
    {
        public const string DefaultRanged = "Water Pistol";
        public const string DefaultMelee = "Bubble Stick";
        public const string None = "None";
        public const string DefaultConsumable1 = "Soap Bomb";
        public const string DefaultConsumable2 = "First Aid Kit";

        public const string ConsumableSoapBomb = "Soap Bomb";
        public const string ConsumableFirstAidKit = "First Aid Kit";
        public const string ConsumableEnergyDrink = "Energy Drink";
        public const string ConsumableSoapWaterBucket = "Soap Water Bucket";
        public const string ConsumableToolsBox = "Tools Box";
        public const string ConsumableSonarBatarang = "Sonar Batarang";

        public const string ArmorWorkersHat = "Worker's Hat";
        public const string ArmorIAmNumberOne = "I am Number One";
        public const string ArmorOctosoap = "Octosoap";

        public const string JetpackJoyride = "Joyride";
        public const string JetpackVengeance = "I am Vengeance";
    }

    public static class AudioKeys
    {
        public const string MusicMainMenu = "(main menu) into the sleeping bin";
        public const string MusicInGameBattle = "(ingame battle) battle! Wazeel!";
        public const string GameStart = "gamestart";
        public const string UiClick = "ui_click";
        public const string UiTransition = "ui_transition";
        public const string Purchase = "purchase";
        public const string Defeat = "defeat";
        public const string JingleWin = "jingle_win";
        public const string HugeWaveIncoming = "huge_wave_incoming";
        public const string PlayerHurt = "player_hurt";
        public const string PlayerDeath = "player_death";
        public const string PlayerHeal = "player_heal";
        public const string PlayerDrink = "player_drink";
        public const string Jetpack = "jetpack";
        public const string EnemyAttack = "enemy_attack";
        public const string EnemyHurt = "enemy_hurt";
        public const string EnemyDeath = "enemy_death";
        public const string BaseHit = "base_hit";
        public const string FenceDamage = "fence_damage";
        public const string FenceBreak = "fence_break";
        public const string FencePlace = "fence_place";
        public const string BalloonPop = "balloon_pop";
        public const string BalloonInflate = "balloon_inflate";
        public const string BarrelRoll = "barrel_roll";
        public const string BarrelBreak = "barrel_break";
        public const string CannonFire = "cannon_fire";
        public const string WhistleBlow = "whistle_blow";
        public const string Repair = "repair";
        public const string HitImelda = "hit_imelda";
        public const string VictorySmall = "victory_small";
        public const string PistolShoot = "pistol_shoot";
        public const string RifleLoop = "rifle_loop";
        public const string PlasmaShoot = "plasma_shoot";
        public const string BubbleShoot = "bubble_shoot";
        public const string SwingImelda = "swing_imelda";
        public const string SwingHammer = "swing_hammer";
        public const string SwingStick = "swing_stick";
        public const string HitHammer = "hit_hammer";
        public const string HitStick = "hit_stick";
        public const string PickupCandy = "pickup_candy";
        public const string PlayerLand = "player_land";
        public const string GunEmpty = "gun_empty";
    }

    public static class AnimParams
    {
        public static readonly int IsMoving = Animator.StringToHash("is_moving");
        public static readonly int Attack = Animator.StringToHash("attack");
        public static readonly int Hit = Animator.StringToHash("hit");
        public static readonly int Death = Animator.StringToHash("death");
        public static readonly int Speed = Animator.StringToHash("speed");
        public static readonly int IsGrounded = Animator.StringToHash("is_grounded");
        public static readonly int YVelocity = Animator.StringToHash("y_velocity");
        public static readonly int AttackImeldaSword = Animator.StringToHash("Attack_ImeldaSword");
        public static readonly int AttackSoapHammer = Animator.StringToHash("Attack_SoapHammer");

        /// <summary>Hand-animator trigger for a melee style. False when the style has no hand animation.</summary>
        public static bool TryGetMeleeTrigger(MeleeAttackStyle style, out int trigger_hash)
        {
            switch (style)
            {
                case MeleeAttackStyle.ImeldaSword:
                    trigger_hash = AttackImeldaSword;
                    return true;
                case MeleeAttackStyle.SoapHammer:
                    trigger_hash = AttackSoapHammer;
                    return true;
                default:
                    trigger_hash = 0;
                    return false;
            }
        }
    }
}
