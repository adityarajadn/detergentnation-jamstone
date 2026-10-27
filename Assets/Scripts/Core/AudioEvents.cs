using System;
using UnityEngine;

public static class AudioEvents
{
    // UI
    public static event Action OnTitleSelect;
    public static event Action OnGameStart;
    public static event Action OnSceneTransition;
    public static event Action OnPurchase;

    // Player
    public static event Action OnPlayerHurt;
    public static event Action OnPlayerDeath;

    // Weapons (Ranged & Melee)
    public static event Action<string> OnWeaponShoot; // "WaterPistol", "WaterRifle", "WaterPlasma", "BubbleGun"
    public static event Action<string> OnMeleeSwing;  // "SoapHammer", "BubbleStick", "Imelda"
    public static event Action<string> OnMeleeHit;    // "SoapHammer", "BubbleStick", "Imelda"

    // Enemies
    public static event Action OnEnemyHurt;
    public static event Action OnEnemyDeath;
    public static event Action OnEnemyAttack;
    public static event Action OnEnemyFootstep;

    // Defenses & Traps
    public static event Action OnFencePlace;
    public static event Action OnFenceHurt;
    public static event Action OnFenceBreak;
    public static event Action OnTrapSet;
    public static event Action OnTrapTrigger;
    public static event Action OnBarrelRoll;
    public static event Action OnBarrelBreak;
    public static event Action OnCannonFire;
    public static event Action OnCannonImpact;

    // Consumables
    public static event Action OnConsumableHealing;
    public static event Action OnConsumableDrink;
    public static event Action OnConsumableWhistle;
    public static event Action OnConsumableRepair;

    // Waves & Level
    public static event Action OnHugeWaveAlarm;
    public static event Action OnVictory;
    public static event Action OnDefeat;

    // Trigger methods
    public static void TriggerTitleSelect() => OnTitleSelect?.Invoke();
    public static void TriggerGameStart() => OnGameStart?.Invoke();
    public static void TriggerSceneTransition() => OnSceneTransition?.Invoke();
    public static void TriggerPurchase() => OnPurchase?.Invoke();

    public static void TriggerPlayerHurt() => OnPlayerHurt?.Invoke();
    public static void TriggerPlayerDeath() => OnPlayerDeath?.Invoke();

    public static void TriggerWeaponShoot(string name) => OnWeaponShoot?.Invoke(name);
    public static void TriggerMeleeSwing(string name) => OnMeleeSwing?.Invoke(name);
    public static void TriggerMeleeHit(string name) => OnMeleeHit?.Invoke(name);

    public static void TriggerEnemyHurt() => OnEnemyHurt?.Invoke();
    public static void TriggerEnemyDeath() => OnEnemyDeath?.Invoke();
    public static void TriggerEnemyAttack() => OnEnemyAttack?.Invoke();
    public static void TriggerEnemyFootstep() => OnEnemyFootstep?.Invoke();

    public static void TriggerFencePlace() => OnFencePlace?.Invoke();
    public static void TriggerFenceHurt() => OnFenceHurt?.Invoke();
    public static void TriggerFenceBreak() => OnFenceBreak?.Invoke();
    public static void TriggerTrapSet() => OnTrapSet?.Invoke();
    public static void TriggerTrapTrigger() => OnTrapTrigger?.Invoke();
    public static void TriggerBarrelRoll() => OnBarrelRoll?.Invoke();
    public static void TriggerBarrelBreak() => OnBarrelBreak?.Invoke();
    public static void TriggerCannonFire() => OnCannonFire?.Invoke();
    public static void TriggerCannonImpact() => OnCannonImpact?.Invoke();

    public static void TriggerConsumableHealing() => OnConsumableHealing?.Invoke();
    public static void TriggerConsumableDrink() => OnConsumableDrink?.Invoke();
    public static void TriggerConsumableWhistle() => OnConsumableWhistle?.Invoke();
    public static void TriggerConsumableRepair() => OnConsumableRepair?.Invoke();

    public static void TriggerHugeWaveAlarm() => OnHugeWaveAlarm?.Invoke();
    public static void TriggerVictory() => OnVictory?.Invoke();
    public static void TriggerDefeat() => OnDefeat?.Invoke();
}
