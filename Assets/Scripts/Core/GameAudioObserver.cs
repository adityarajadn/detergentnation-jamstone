using UnityEngine;

public class GameAudioObserver : MonoBehaviour
{
    [SerializeField] private AudioDatabase database;

    private void Awake()
    {
        if (database == null)
        {
            database = Resources.Load<AudioDatabase>("AudioDatabase");
        }
    }

    private void OnEnable()
    {
        AudioEvents.OnTitleSelect += HandleTitleSelect;
        AudioEvents.OnGameStart += HandleGameStart;
        AudioEvents.OnSceneTransition += HandleSceneTransition;
        AudioEvents.OnPurchase += HandlePurchase;

        AudioEvents.OnPlayerHurt += HandlePlayerHurt;
        AudioEvents.OnPlayerDeath += HandlePlayerDeath;

        AudioEvents.OnWeaponShoot += HandleWeaponShoot;
        AudioEvents.OnMeleeSwing += HandleMeleeSwing;
        AudioEvents.OnMeleeHit += HandleMeleeHit;

        AudioEvents.OnEnemyHurt += HandleEnemyHurt;
        AudioEvents.OnEnemyDeath += HandleEnemyDeath;
        AudioEvents.OnEnemyAttack += HandleEnemyAttack;
        AudioEvents.OnEnemyFootstep += HandleEnemyFootstep;

        AudioEvents.OnFencePlace += HandleFencePlace;
        AudioEvents.OnFenceHurt += HandleFenceHurt;
        AudioEvents.OnFenceBreak += HandleFenceBreak;
        AudioEvents.OnTrapSet += HandleTrapSet;
        AudioEvents.OnTrapTrigger += HandleTrapTrigger;
        AudioEvents.OnBarrelRoll += HandleBarrelRoll;
        AudioEvents.OnBarrelBreak += HandleBarrelBreak;
        AudioEvents.OnCannonFire += HandleCannonFire;
        AudioEvents.OnCannonImpact += HandleCannonImpact;

        AudioEvents.OnConsumableHealing += HandleConsumableHealing;
        AudioEvents.OnConsumableDrink += HandleConsumableDrink;
        AudioEvents.OnConsumableWhistle += HandleConsumableWhistle;
        AudioEvents.OnConsumableRepair += HandleConsumableRepair;

        AudioEvents.OnHugeWaveAlarm += HandleHugeWaveAlarm;
        AudioEvents.OnVictory += HandleVictory;
        AudioEvents.OnDefeat += HandleDefeat;
    }

    private void OnDisable()
    {
        AudioEvents.OnTitleSelect -= HandleTitleSelect;
        AudioEvents.OnGameStart -= HandleGameStart;
        AudioEvents.OnSceneTransition -= HandleSceneTransition;
        AudioEvents.OnPurchase -= HandlePurchase;

        AudioEvents.OnPlayerHurt -= HandlePlayerHurt;
        AudioEvents.OnPlayerDeath -= HandlePlayerDeath;

        AudioEvents.OnWeaponShoot -= HandleWeaponShoot;
        AudioEvents.OnMeleeSwing -= HandleMeleeSwing;
        AudioEvents.OnMeleeHit -= HandleMeleeHit;

        AudioEvents.OnEnemyHurt -= HandleEnemyHurt;
        AudioEvents.OnEnemyDeath -= HandleEnemyDeath;
        AudioEvents.OnEnemyAttack -= HandleEnemyAttack;
        AudioEvents.OnEnemyFootstep -= HandleEnemyFootstep;

        AudioEvents.OnFencePlace -= HandleFencePlace;
        AudioEvents.OnFenceHurt -= HandleFenceHurt;
        AudioEvents.OnFenceBreak -= HandleFenceBreak;
        AudioEvents.OnTrapSet -= HandleTrapSet;
        AudioEvents.OnTrapTrigger -= HandleTrapTrigger;
        AudioEvents.OnBarrelRoll -= HandleBarrelRoll;
        AudioEvents.OnBarrelBreak -= HandleBarrelBreak;
        AudioEvents.OnCannonFire -= HandleCannonFire;
        AudioEvents.OnCannonImpact -= HandleCannonImpact;

        AudioEvents.OnConsumableHealing -= HandleConsumableHealing;
        AudioEvents.OnConsumableDrink -= HandleConsumableDrink;
        AudioEvents.OnConsumableWhistle -= HandleConsumableWhistle;
        AudioEvents.OnConsumableRepair -= HandleConsumableRepair;

        AudioEvents.OnHugeWaveAlarm -= HandleHugeWaveAlarm;
        AudioEvents.OnVictory -= HandleVictory;
        AudioEvents.OnDefeat -= HandleDefeat;
    }

    private void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null) AudioManager.PlaySFX(clip, volume);
    }

    private void HandleTitleSelect() => Play(database?.title_select);
    private void HandleGameStart() => Play(database?.game_start);
    private void HandleSceneTransition() => Play(database?.scene_transition);
    private void HandlePurchase() => Play(database?.purchase_sound);

    private void HandlePlayerHurt() => Play(database?.player_damage);
    private void HandlePlayerDeath() => Play(database?.player_death);

    private void HandleWeaponShoot(string name)
    {
        if (database == null) return;
        string n = name.ToLower();
        if (n.Contains("pistol")) Play(database.shoot_water_pistol);
        else if (n.Contains("rifle")) Play(database.shoot_water_rifle);
        else if (n.Contains("plasma")) Play(database.shoot_water_plasma);
        else if (n.Contains("bubble")) Play(database.shoot_bubble_gun);
    }

    private void HandleMeleeSwing(string name)
    {
        if (database == null) return;
        string n = name.ToLower();
        if (n.Contains("hammer")) Play(database.swing_soap_hammer);
        else if (n.Contains("stick")) Play(database.swing_bubble_stick);
        else if (n.Contains("imelda") || n.Contains("curse")) Play(database.swing_curse_sword);
        else Play(database.swing_soap_hammer);
    }

    private void HandleMeleeHit(string name)
    {
        if (database == null) return;
        string n = name.ToLower();
        if (n.Contains("hammer")) Play(database.hit_soap_hammer);
        else if (n.Contains("stick")) Play(database.hit_bubble_stick);
        else if (n.Contains("imelda") || n.Contains("curse")) Play(database.hit_curse_sword);
        else Play(database.hit_soap_hammer);
    }

    private void HandleEnemyHurt() => Play(database?.enemy_hurt);
    private void HandleEnemyDeath() => Play(database?.enemy_death);
    private void HandleEnemyAttack() => Play(database?.enemy_attack);
    private void HandleEnemyFootstep() => Play(database?.enemy_walk_heavy);

    private void HandleFencePlace() => Play(database?.fence_place);
    private void HandleFenceHurt() => Play(database?.fence_hurt);
    private void HandleFenceBreak() => Play(database?.fence_break);
    private void HandleTrapSet() => Play(database?.trap_set);
    private void HandleTrapTrigger() => Play(database?.trap_trigger);
    private void HandleBarrelRoll() => Play(database?.soap_barrel_roll);
    private void HandleBarrelBreak() => Play(database?.soap_barrel_break);
    private void HandleCannonFire() => Play(database?.cannon_fire);
    private void HandleCannonImpact() => Play(database?.cannon_impact);

    private void HandleConsumableHealing() => Play(database?.consumable_healing);
    private void HandleConsumableDrink() => Play(database?.consumable_drink);
    private void HandleConsumableWhistle() => Play(database?.consumable_whistle);
    private void HandleConsumableRepair() => Play(database?.consumable_repair);

    private void HandleHugeWaveAlarm() => Play(database?.huge_wave_alarm);
    private void HandleVictory() => Play(database?.victory_jingle);
    private void HandleDefeat() => Play(database?.defeat_jingle);
}
