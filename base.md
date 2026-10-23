# DETERGENT NATION - UNITY MIGRATION SPECIFICATION

> Panduan lengkap arsitektur, data, logika, dan source code C# untuk memigrasikan game **Detergent Nation** dari Godot Engine ke Unity 2D.

---

## 1. GAME OVERVIEW & ARSITEKTUR

### 1.1 Konsep & Core Loop
- **Genre:** 2D Side-scrolling Wave Defense / Action Platformer Shooter.
- **Tujuan:** Lindungi toko **RnA Mart (Minimarket)** di tengah stage dari serbuan monster makanan (*Foodonians*).
- **Core Loop:**
  1. **Planning Phase:** Pemain memiliki poin pertahanan (*Defense Points*) untuk memasang trap/turret di titik *Defense Slots*.
  2. **Wave Combat Phase:** Musuh muncul dari portal kiri, kanan, dan atas. Pemain menembak/menebas musuh, mengumpulkan permen (*Candy*), menjaga HP minimarket, mengisi amunisi di restock zone, dan memperbaiki kerusakan toko dengan repair points.
  3. **Wave Clear & Results:** Evaluasi bintang (1-3 bintang) berdasarkan sisa HP minimarket dan kill streak combo.
  4. **Shop & Upgrades:** Candy dipakai untuk membeli senjata jarak jauh (Rifles, Plasma), senjata melee, armor/topi, jetpack, consumable, dan upgrade stats permanen.

### 1.2 Resolusi & Input
- Target Resolusi: 1280 x 720 (16:9 aspect ratio).
- Kontrol Standar:
  - Bergerak: `A` / `D` (Horizontal)
  - Melompat / Jetpack: `Space` / `W`
  - Tembak Jarak Jauh: `Mouse 0` (Klik Kiri)
  - Serangan Melee: `Mouse 1` (Klik Kanan)
  - Reload: `R`
  - Interaksi (Restock / Repair / Gunakan Turret): `E`
  - Pilih Consumable Slot: `1` / `2` atau Scroll Mouse
  - Pakai Consumable: `Q`
  - Pause: `Escape`

---

## 2. GODOT TO UNITY MAPPING TABLE

| Komponen Godot | Ekivalen Unity 2D | Catatan Implementasi |
| :--- | :--- | :--- |
| `Node2D` | `GameObject` + `Transform` | Posisi koordinat 2D (gunakan meter, misal 1 unit = 100 pixel). |
| `CharacterBody2D` | `Rigidbody2D` (Kinematic/Dynamic) + `Collider2D` | Gerakan platformer memakai `rb.linearVelocity` atau raycast collision. |
| `StaticBody2D` | `Rigidbody2D` (Static) + `BoxCollider2D` / `CompositeCollider2D` | Lantai, dinding batas arena, dan minimarket. |
| `Area2D` | `Collider2D` dengan `isTrigger = true` | Deteksi peluru, area interaksi, drop item magnet. |
| `Signals` (`signal xxx`) | C# `event System.Action<...>` / `UnityEvent` | Komunikasi decoupling antar modul. |
| `Autoload (Singletons)` | C# Static Singleton (`Instance`) atau `ScriptableObject` | `GameManager`, `GameState`, `EventBus`, `AudioManager`. |
| `Resource` (`.gd` / `.tres`) | `ScriptableObject` | `WeaponDataSO`, `EnemyDataSO`, `LevelConfigSO`. |
| `Tween` | DOTween / Coroutine / LeanTween | UI pop, recoil melee, animasi flash damage. |
| `CanvasLayer` + Control | Unity UI (`Canvas`, `CanvasScaler`, `TextMeshPro`, `Button`, `Slider`) | HUD, Shop, Menus. |
| `user://save_data.json` | `Application.persistentDataPath + "/save_data.json"` | Serialisasi `JsonUtility` / `Newtonsoft.Json`. |

---

## 3. TAGS & PHYSICS LAYERS

### 3.1 Tags
- `Player`
- `Enemy`
- `Minimarket`
- `Defense`
- `DefenseTarget`
- `DropItem`

### 3.2 Physics 2D Layers & Matrix
- Layer 0: `Default`
- Layer 6: `Ground`
- Layer 7: `Player`
- Layer 8: `Enemy`
- Layer 9: `PlayerProjectile`
- Layer 10: `EnemyProjectile`
- Layer 11: `Interactable`
- Layer 12: `Drop`

*Matrix Rekomendasi:*
- `PlayerProjectile` hanya bentrok dengan `Enemy` dan `Ground`.
- `EnemyProjectile` hanya bentrok dengan `Player`, `Minimarket`, dan `Defense`.
- `Enemy` hanya bentrok dengan `Ground`, `Minimarket`, dan `Defense`.

---

## 4. DATA MODEL & SCRIPTABLE OBJECTS (C#)

### 4.1 WeaponDataSO.cs
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "DetergentNation/WeaponData")]
public class WeaponDataSO : ScriptableObject
{
    public string weaponId = "water_pistol";
    public string weaponName = "Water Pistol";
    public string slot = "range"; // "range" atau "melee"
    public int damage = 25;
    public float fireRateDelay = 0.3f;
    public int clipMax = 12;
    public GameObject projectilePrefab;
    public int price = 0;
    [TextArea] public string description;
}
```

### 4.2 EnemyDataSO.cs
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "DetergentNation/EnemyData")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyType = "jellien";
    public int healthMax = 60;
    public float moveSpeed = 1.8f;
    public int attackDamage = 10;
    public float attackDelay = 1.0f;
    public bool isFlying = false;
    public int candyReward = 15;
    public Color bodyColor = new Color(0.2f, 0.8f, 0.5f, 0.9f);
    public Sprite enemySprite;
}
```

### 4.3 LevelWaveData.cs
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnGroup
{
    public string enemyType;
    public string spawnLocation; // "left", "right", "top", "random"
    public int count;
}

[Serializable]
public class WaveData
{
    public bool isHugeWave;
    public float preDelay = 5.0f;
    public float spawnInterval = 1.0f;
    public List<EnemySpawnGroup> enemyGroups = new List<EnemySpawnGroup>();
}

[CreateAssetMenu(fileName = "LevelConfig", menuName = "DetergentNation/LevelConfig")]
public class LevelConfigSO : ScriptableObject
{
    public int levelId = 1;
    public string levelName = "Level 1: First Splash";
    public int defensePoints = 6;
    public List<WaveData> waves = new List<WaveData>();
}
```

---

## 5. PERSISTENSI & GAME STATE

### 5.1 GameState.cs
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int candy = 100;
    public int unlockedLevels = 1;
    public int currentLevel = 1;
    public int[] levelStars = new int[6]; // index 1..5

    public string equippedRange = "water_pistol";
    public string equippedMelee = "bubble_stick";
    public string equippedArmor = "none";
    public string equippedJetpack = "none";
    public List<string> equippedConsumables = new List<string> { "soap_bomb", "first_aid" };

    public List<string> unlockedItems = new List<string> { "water_pistol", "bubble_stick", "none" };

    // Consumable inventory quantities
    public int qtySoapBomb = 3;
    public int qtyFirstAid = 2;
    public int qtyEnergyDrink = 0;
    public int qtySoapBucket = 0;
    public int qtyToolsBox = 0;
    public int qtySonarBatarang = 0;

    // Upgrades level (1..10)
    public int upRangeDamage = 1;
    public int upRangeFireRate = 1;
    public int upRangeAmmo = 1;
    public int upMeleeDamage = 1;
    public int upDefenseDamage = 1;
    public int upDefenseAmmo = 1;
    public int upDefenseDuration = 1;

    public float volumeMaster = 1.0f;
    public float volumeSfx = 1.0f;
    public float volumeBgm = 0.8f;
}

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }
    public SaveData data = new SaveData();

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "save_data.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadGame();
    }

    public void AddCandy(int amount)
    {
        data.candy += amount;
        SaveGame();
        EventBus.RaiseCurrencyChanged(data.candy);
    }

    public bool SpendCandy(int amount)
    {
        if (data.candy >= amount)
        {
            data.candy -= amount;
            SaveGame();
            EventBus.RaiseCurrencyChanged(data.candy);
            return true;
        }
        return false;
    }

    public int GetUpgradeCost(string key)
    {
        int lvl = GetUpgradeLevel(key);
        if (lvl >= 10) return 999999;
        if (key.EndsWith("damage")) return Mathf.Max(40, lvl * 40);
        if (key.EndsWith("fire_rate")) return Mathf.Max(20, lvl * 20);
        if (key.EndsWith("ammo") || key.EndsWith("duration")) return Mathf.Max(25, lvl * 25);
        return 50;
    }

    public int GetUpgradeLevel(string key)
    {
        return key switch
        {
            "range_damage" => data.upRangeDamage,
            "range_fire_rate" => data.upRangeFireRate,
            "range_ammo" => data.upRangeAmmo,
            "melee_damage" => data.upMeleeDamage,
            "defense_damage" => data.upDefenseDamage,
            "defense_ammo" => data.upDefenseAmmo,
            "defense_duration" => data.upDefenseDuration,
            _ => 1
        };
    }

    public bool BuyUpgrade(string key)
    {
        int cost = GetUpgradeCost(key);
        if (SpendCandy(cost))
        {
            switch (key)
            {
                case "range_damage": data.upRangeDamage++; break;
                case "range_fire_rate": data.upRangeFireRate++; break;
                case "range_ammo": data.upRangeAmmo++; break;
                case "melee_damage": data.upMeleeDamage++; break;
                case "defense_damage": data.upDefenseDamage++; break;
                case "defense_ammo": data.upDefenseAmmo++; break;
                case "defense_duration": data.upDefenseDuration++; break;
            }
            SaveGame();
            return true;
        }
        return false;
    }

    public int GetConsumableQty(string id)
    {
        return id switch
        {
            "soap_bomb" => data.qtySoapBomb,
            "first_aid" => data.qtyFirstAid,
            "energy_drink" => data.qtyEnergyDrink,
            "soap_bucket" => data.qtySoapBucket,
            "tools_box" => data.qtyToolsBox,
            "sonar_batarang" => data.qtySonarBatarang,
            _ => 0
        };
    }

    public void ModifyConsumableQty(string id, int delta)
    {
        switch (id)
        {
            case "soap_bomb": data.qtySoapBomb = Mathf.Max(0, data.qtySoapBomb + delta); break;
            case "first_aid": data.qtyFirstAid = Mathf.Max(0, data.qtyFirstAid + delta); break;
            case "energy_drink": data.qtyEnergyDrink = Mathf.Max(0, data.qtyEnergyDrink + delta); break;
            case "soap_bucket": data.qtySoapBucket = Mathf.Max(0, data.qtySoapBucket + delta); break;
            case "tools_box": data.qtyToolsBox = Mathf.Max(0, data.qtyToolsBox + delta); break;
            case "sonar_batarang": data.qtySonarBatarang = Mathf.Max(0, data.qtySonarBatarang + delta); break;
        }
        SaveGame();
    }

    public void RecordLevelResult(int levelId, int stars)
    {
        if (levelId < 1 || levelId > 5) return;
        if (stars > data.levelStars[levelId])
            data.levelStars[levelId] = stars;
        if (stars > 0 && levelId == data.unlockedLevels && data.unlockedLevels < 5)
            data.unlockedLevels++;
        SaveGame();
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SaveFilePath, json);
    }

    public void LoadGame()
    {
        if (File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            data = JsonUtility.FromJson<SaveData>(json);
        }
    }
}
```

### 5.2 EventBus.cs
```csharp
using System;
using UnityEngine;

public static class EventBus
{
    public static event Action<int> OnCurrencyChanged;
    public static event Action OnPlayerDied;
    public static event Action OnPlayerDamaged;
    public static event Action<int> OnLevelCompleted;
    public static event Action<string> OnLevelFailed;
    public static event Action<int, int, bool> OnWaveStarted;
    public static event Action<string, Vector2> OnEnemyKilled;

    public static void RaiseCurrencyChanged(int amount) => OnCurrencyChanged?.Invoke(amount);
    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
    public static void RaisePlayerDamaged() => OnPlayerDamaged?.Invoke();
    public static void RaiseLevelCompleted(int stars) => OnLevelCompleted?.Invoke(stars);
    public static void RaiseLevelFailed(string reason) => OnLevelFailed?.Invoke(reason);
    public static void RaiseWaveStarted(int wave, int total, bool huge) => OnWaveStarted?.Invoke(wave, total, huge);
    public static void RaiseEnemyKilled(string type, Vector2 pos) => OnEnemyKilled?.Invoke(type, pos);
}
```

---

## 6. PLAYER ENTITY & MODULAR COMPONENTS

### 6.1 Player.cs (Main Coordinator)
```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Components")]
    public PlayerMovement movement;
    public PlayerHealth health;
    public PlayerCombat combat;
    public PlayerInteraction interaction;

    [Header("Mount Points")]
    public Transform visualBody;
    public Transform aimArm;
    public Transform muzzle;
    public Transform meleeMount;
    public Transform armorMount;
    public Transform jetpackMount;

    [Header("Camera & Visuals")]
    public Camera mainCamera;
    public bool isPlanningMode = true;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        ApplyEquipmentStats();
    }

    public void ApplyEquipmentStats()
    {
        movement.SetupEquipment(GameState.Instance.data.equippedJetpack);
        health.SetupArmor(GameState.Instance.data.equippedArmor);
        combat.SetupWeapons(GameState.Instance.data.equippedRange, GameState.Instance.data.equippedMelee);
    }

    private void Update()
    {
        HandleCameraFollow();
    }

    private void HandleCameraFollow()
    {
        if (mainCamera == null) return;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 offset = mouseWorld - transform.position;
        float targetX = Mathf.Clamp(offset.x * 0.35f, -2.0f, 2.0f);
        float targetY = Mathf.Clamp(Mathf.Min(0f, offset.y * 0.35f), -1.5f, 0f) + 0.6f;

        Vector3 targetCamPos = new Vector3(transform.position.x + targetX, transform.position.y + targetY, -10f);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCamPos, Time.deltaTime * 5f);
    }

    public void TakeDamage(int dmg) => health.TakeDamage(dmg);
    public void Heal(int pts) => health.Heal(pts);
    public void AddRepairPoints(int pts) => interaction.AddRepairPoints(pts);
}
```

### 6.2 PlayerMovement.cs
```csharp
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform visualBody;
    public float moveSpeed = 4.5f;
    public float jumpForce = 9.0f;
    public float jetpackFuelMax = 100f;

    [HideInInspector] public float jetpackFuel = 100f;
    [HideInInspector] public float speedMultiplier = 1.0f;
    public float speedBoostTimer = 0f;

    public bool canDoubleJump = false;
    private bool hasDoubleJumped = false;
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isGrounded;

    public void SetupEquipment(string jetpackId)
    {
        canDoubleJump = (jetpackId == "joyride");
    }

    private void Update()
    {
        if (speedBoostTimer > 0f)
        {
            speedBoostTimer -= Time.deltaTime;
            if (speedBoostTimer <= 0f) speedMultiplier = 1f;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.15f, groundLayer);
        if (isGrounded)
        {
            hasDoubleJumped = false;
            jetpackFuel = Mathf.Min(jetpackFuelMax, jetpackFuel + 40f * Time.deltaTime);
        }

        // Horizontal Movement
        float h = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(h * moveSpeed * speedMultiplier, rb.linearVelocity.y);

        if (h != 0)
        {
            Vector3 s = visualBody.localScale;
            s.x = h > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            visualBody.localScale = s;
        }

        // Jump & Double Jump
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                float jmp = jumpForce;
                if (GameState.Instance.data.equippedJetpack == "vengeance") jmp *= 1.35f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jmp);
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                hasDoubleJumped = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.95f);
                AudioManager.Instance.PlaySfx("shoot");
            }
        }

        // Jetpack Vengeance Slow-fall Glide
        if (Input.GetButton("Jump") && !isGrounded)
        {
            if (GameState.Instance.data.equippedJetpack == "vengeance" && jetpackFuel > 2f)
            {
                if (rb.linearVelocity.y < 0f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.MoveTowards(rb.linearVelocity.y, -1.0f, 15f * Time.deltaTime));
                    jetpackFuel -= 18f * Time.deltaTime;
                }
            }
        }
    }
}
```

### 6.3 PlayerCombat.cs
```csharp
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Player player;
    public Transform aimArm;
    public Transform muzzle;
    public Transform meleeMount;
    public LayerMask enemyLayer;

    public int clipMax = 12;
    public int carriedAmmo = 64;
    public int clipCurrent = 12;
    public float fireRateDelay = 0.3f;
    private float fireCooldown = 0f;

    public bool isReloading = false;
    public float reloadDuration = 1.0f;
    private float reloadTimer = 0f;

    public int meleeDamage = 30;
    public float meleeDelay = 0.4f;
    private float meleeCooldown = 0f;

    public GameObject soapBulletPrefab;
    public GameObject bubbleMinePrefab;

    public string currentRangeWeapon = "water_pistol";
    public string currentMeleeWeapon = "bubble_stick";

    public void SetupWeapons(string rangeId, string meleeId)
    {
        currentRangeWeapon = rangeId;
        currentMeleeWeapon = meleeId;
        int ammoUp = GameState.Instance.GetUpgradeLevel("range_ammo") - 1;

        switch (rangeId)
        {
            case "water_pistol":
                clipMax = 12 + (ammoUp * 2);
                fireRateDelay = 0.28f;
                break;
            case "water_rifle":
                clipMax = 30 + (ammoUp * 3);
                fireRateDelay = 0.12f;
                break;
            case "water_plasma":
                clipMax = 4 + ammoUp;
                fireRateDelay = 0.55f;
                break;
            case "bubble_gun":
                clipMax = 50 + (ammoUp * 5);
                fireRateDelay = 0.08f;
                break;
        }

        int rateUp = GameState.Instance.GetUpgradeLevel("range_fire_rate") - 1;
        fireRateDelay = Mathf.Max(0.06f, fireRateDelay - (rateUp * 0.02f));
        clipCurrent = clipMax;

        int meleeUp = GameState.Instance.GetUpgradeLevel("melee_damage") - 1;
        switch (meleeId)
        {
            case "bubble_stick": meleeDamage = 30 + (meleeUp * 8); break;
            case "soap_hammer": meleeDamage = 60 + (meleeUp * 15); break;
            case "loofah_mace": meleeDamage = 90 + (meleeUp * 20); break;
            case "imelda": meleeDamage = 999; break;
        }
    }

    private void Update()
    {
        // Aiming arm toward mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = (mousePos - aimArm.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        aimArm.rotation = Quaternion.Euler(0, 0, angle);

        if (player.isPlanningMode) return;

        if (fireCooldown > 0f) fireCooldown -= Time.deltaTime;
        if (meleeCooldown > 0f) meleeCooldown -= Time.deltaTime;

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f) FinishReload();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R)) StartReload();

        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            FireRange();
        }

        if (Input.GetMouseButtonDown(1) && meleeCooldown <= 0f)
        {
            PerformMelee();
        }
    }

    private void FireRange()
    {
        if (clipCurrent <= 0)
        {
            StartReload();
            return;
        }

        clipCurrent--;
        fireCooldown = fireRateDelay;
        AudioManager.Instance.PlaySfx("shoot");

        Vector2 aimDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - muzzle.position).normalized;
        int dmgUp = GameState.Instance.GetUpgradeLevel("range_damage") - 1;

        if (currentRangeWeapon == "bubble_gun")
        {
            GameObject b = Instantiate(bubbleMinePrefab, muzzle.position, Quaternion.identity);
            b.GetComponent<BubbleMine>().damage = 35 + (dmgUp * 8);
        }
        else if (currentRangeWeapon == "water_plasma")
        {
            for (int i = 0; i < 3; i++)
            {
                float spread = (i - 1) * 12f;
                Vector2 spreadDir = Quaternion.Euler(0, 0, spread) * aimDir;
                GameObject b = Instantiate(soapBulletPrefab, muzzle.position, Quaternion.Euler(0, 0, Mathf.Atan2(spreadDir.y, spreadDir.x) * Mathf.Rad2Deg));
                var bullet = b.GetComponent<SoapBullet>();
                bullet.direction = spreadDir;
                bullet.speed = 10f;
                bullet.damage = 30 + (dmgUp * 10);
                bullet.maxRange = 8f;
            }
        }
        else
        {
            GameObject b = Instantiate(soapBulletPrefab, muzzle.position, Quaternion.Euler(0, 0, Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg));
            var bullet = b.GetComponent<SoapBullet>();
            bullet.direction = aimDir;
            bullet.damage = 25 + (dmgUp * 7);
            if (currentRangeWeapon == "water_rifle")
            {
                bullet.speed = 17f;
                bullet.damage = 22 + (dmgUp * 6);
            }
        }
    }

    private void PerformMelee()
    {
        meleeCooldown = meleeDelay;
        AudioManager.Instance.PlaySfx("shoot");

        // Overlap circle in front of player
        Collider2D[] hits = Physics2D.OverlapCircleAll(meleeMount.position, 1.2f, enemyLayer);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(meleeDamage, transform.position);
                AudioManager.Instance.PlaySfx("hit");
            }
        }
    }

    public void StartReload()
    {
        if (isReloading || clipCurrent >= clipMax || carriedAmmo <= 0) return;
        isReloading = true;
        reloadTimer = reloadDuration;
        AudioManager.Instance.PlaySfx("reload");
    }

    private void FinishReload()
    {
        isReloading = false;
        int needed = clipMax - clipCurrent;
        int transfer = Mathf.Min(needed, carriedAmmo);
        clipCurrent += transfer;
        carriedAmmo -= transfer;
    }
}
```

### 6.4 PlayerHealth.cs
```csharp
using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int healthMax = 100;
    public int healthCurrent = 100;
    public int armorBonus = 0;
    public int shieldCharges = 0;

    public event Action<int, int> OnHealthChanged;

    public void SetupArmor(string armorId)
    {
        armorBonus = armorId switch
        {
            "worker_hat" => 1,
            "number_one" => 2,
            "octosoap" => 3,
            _ => 0
        };
    }

    public void TakeDamage(int amount)
    {
        if (shieldCharges > 0)
        {
            shieldCharges--;
            AudioManager.Instance.PlaySfx("hit");
            return;
        }

        int actualDmg = Mathf.Max(1, amount - armorBonus);
        healthCurrent = Mathf.Max(0, healthCurrent - actualDmg);
        OnHealthChanged?.Invoke(healthCurrent, healthMax);
        EventBus.RaisePlayerDamaged();
        AudioManager.Instance.PlaySfx("hit");

        if (healthCurrent <= 0)
        {
            EventBus.RaisePlayerDied();
        }
    }

    public void Heal(int amount)
    {
        healthCurrent = Mathf.Min(healthMax, healthCurrent + amount);
        OnHealthChanged?.Invoke(healthCurrent, healthMax);
    }
}
```

### 6.5 PlayerInteraction.cs
```csharp
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Player player;
    public int repairPoints = 0;
    public int activeSlot = 0;
    public GameObject soapBombPrefab;

    public void AddRepairPoints(int pts) => repairPoints += pts;

    private void Update()
    {
        // Interaksi E dengan Toko Minimarket
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            foreach (var col in overlaps)
            {
                if (col.TryGetComponent<Minimarket>(out var store))
                {
                    if (store.playerInRestockZone)
                    {
                        player.combat.carriedAmmo = 64;
                        AudioManager.Instance.PlaySfx("reload");
                        return;
                    }
                    else if (store.playerInRepairZone && repairPoints > 0 && store.healthCurrent < store.healthMax)
                    {
                        int needed = store.healthMax - store.healthCurrent;
                        int use = Mathf.Min(needed, repairPoints);
                        repairPoints -= use;
                        store.Repair(use);
                        return;
                    }
                }
            }
        }

        // Slot Switch
        if (Input.GetKeyDown(KeyCode.Alpha1)) activeSlot = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) activeSlot = 1;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) activeSlot = 0;
        if (scroll < 0f) activeSlot = 1;

        // Use consumable
        if (Input.GetKeyDown(KeyCode.Q) && !player.isPlanningMode)
        {
            UseActiveConsumable();
        }
    }

    private void UseActiveConsumable()
    {
        var list = GameState.Instance.data.equippedConsumables;
        if (activeSlot >= list.Count) return;
        string id = list[activeSlot];
        int qty = GameState.Instance.GetConsumableQty(id);
        if (qty <= 0) return;

        GameState.Instance.ModifyConsumableQty(id, -1);

        switch (id)
        {
            case "soap_bomb":
                Vector2 throwDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.muzzle.position).normalized;
                GameObject bomb = Instantiate(soapBombPrefab, player.muzzle.position, Quaternion.identity);
                bomb.GetComponent<Rigidbody2D>().linearVelocity = throwDir * 9.0f + new Vector2(0, 3.5f);
                break;
            case "first_aid":
                player.health.Heal(50);
                break;
            case "energy_drink":
                player.movement.speedMultiplier = 1.45f;
                player.movement.speedBoostTimer = 8.0f;
                AudioManager.Instance.PlaySfx("streak");
                break;
            case "soap_bucket":
                player.health.shieldCharges += 3;
                AudioManager.Instance.PlaySfx("repair");
                break;
            case "tools_box":
                AddRepairPoints(30);
                break;
            case "sonar_batarang":
                AudioManager.Instance.PlaySfx("alarm");
                foreach (var enemy in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
                {
                    enemy.Stun(10.0f);
                }
                break;
        }
    }
}
```

---

## 7. MINIMARKET & DEFENSE SYSTEMS

### 7.1 Minimarket.cs
```csharp
using System;
using UnityEngine;

public class Minimarket : MonoBehaviour
{
    public int healthMax = 100;
    public int healthCurrent = 100;
    public bool isDestroyed = false;

    public bool playerInRestockZone = false;
    public bool playerInRepairZone = false;

    public event Action<int, int> OnHealthChanged;

    public void TakeDamage(int dmg)
    {
        if (isDestroyed) return;
        healthCurrent = Mathf.Max(0, healthCurrent - dmg);
        OnHealthChanged?.Invoke(healthCurrent, healthMax);
        AudioManager.Instance.PlaySfx("hit");

        if (healthCurrent <= 0)
        {
            isDestroyed = true;
            EventBus.RaiseLevelFailed("RnA Mart ice cream was looted!");
        }
    }

    public void Repair(int amount)
    {
        if (isDestroyed || healthCurrent >= healthMax) return;
        healthCurrent = Mathf.Min(healthMax, healthCurrent + amount);
        OnHealthChanged?.Invoke(healthCurrent, healthMax);
        AudioManager.Instance.PlaySfx("repair");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.transform.position.x < transform.position.x) playerInRestockZone = true;
            else playerInRepairZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRestockZone = false;
            playerInRepairZone = false;
        }
    }
}
```

### 7.2 Defenses (C#)
1. **SoapCannon.cs:**
   - 10 amunisi.
   - Deteksi musuh terdekat dalam radius, mengarahkan laras, menembakkan 4 peluru soap bullet spread.
   - Dipicu otomatis saat pemain menekan `E` di dekatnya.
2. **SoapBarrelSpawner.cs:**
   - 5 amunisi drum sabun.
   - Meluncurkan `SoapBarrel` yang menggelinding di lantai dan menabrak musuh.
3. **Fence.cs:**
   - Barikade dengan 100 HP. Memblokir gerak musuh. Bisa diperbaiki pemain dengan repair points.
4. **BalloonTrap.cs:**
   - Ranjau balon berisi deterjen pekat. Meledak saat tersentuh musuh atau peluru pemain, memberikan damage area (AoE 90 radius).

---

## 8. ENEMY BASE & AI

### 8.1 EnemyBase.cs
```csharp
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    public string enemyType = "jellien";
    public int healthMax = 60;
    public int healthCurrent = 60;
    public float moveSpeed = 1.8f;
    public int attackDamage = 10;
    public float attackDelay = 1.0f;
    public bool isFlying = false;
    public int candyReward = 15;

    public Slider hpSlider;
    public SpriteRenderer spriteRenderer;
    public GameObject dropItemPrefab;
    public GameObject enemyProjectilePrefab;

    private float attackCooldown = 0f;
    private float stunTimer = 0f;
    private float hitstopTimer = 0f;
    private Rigidbody2D rb;
    private Transform target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        healthCurrent = healthMax;
        if (hpSlider != null)
        {
            hpSlider.maxValue = healthMax;
            hpSlider.value = healthCurrent;
        }
        var market = FindFirstObjectByType<Minimarket>();
        if (market != null) target = market.transform;
    }

    private void Update()
    {
        if (hitstopTimer > 0f) { hitstopTimer -= Time.deltaTime; return; }
        if (stunTimer > 0f) { stunTimer -= Time.deltaTime; return; }
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;

        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= 0.8f)
            {
                if (attackCooldown <= 0f) ExecuteAttack();
                return;
            }
            else if ((enemyType == "cheetozard" || enemyType == "marshquito") && dist <= 6.0f)
            {
                if (attackCooldown <= 0f) ExecuteRangedAttack();
                return;
            }
        }

        // Bergerak menuju Minimarket di posisi X = 0
        float dir = transform.position.x > 0 ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, isFlying ? 0 : rb.linearVelocity.y);

        Vector3 s = transform.localScale;
        s.x = dir > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    public void ExecuteAttack()
    {
        attackCooldown = attackDelay;
        AudioManager.Instance.PlaySfx("hit");
        if (target != null && target.TryGetComponent<Minimarket>(out var store))
        {
            store.TakeDamage(attackDamage);
        }
    }

    public void ExecuteRangedAttack()
    {
        attackCooldown = attackDelay * 1.5f;
        GameObject p = Instantiate(enemyProjectilePrefab, transform.position + new Vector3(0, -0.2f, 0), Quaternion.identity);
        Vector2 dir = (target.position - transform.position).normalized;
        p.GetComponent<EnemyProjectile>().Setup(dir, attackDamage);
        AudioManager.Instance.PlaySfx("shoot");
    }

    public void Stun(float duration) => stunTimer = duration;

    public void TakeDamage(int dmg, Vector2 hitSource = default)
    {
        healthCurrent = Mathf.Max(0, healthCurrent - dmg);
        if (hpSlider != null) hpSlider.value = healthCurrent;

        // Visual flash & knockback
        hitstopTimer = 0.06f;
        Vector2 kbDir = hitSource != default ? ((Vector2)transform.position - hitSource).normalized : new Vector2(transform.position.x > 0 ? 1 : -1, 0.4f);
        rb.linearVelocity = kbDir * 3.0f;

        AudioManager.Instance.PlaySfx("hit");

        if (healthCurrent <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        EventBus.RaiseEnemyKilled(enemyType, transform.position);
        SpawnDrops();
        Destroy(gameObject);
    }

    private void SpawnDrops()
    {
        if (Random.value <= 0.7f && dropItemPrefab != null)
        {
            GameObject d = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            d.GetComponent<DropItem>().Setup("candy", candyReward);
        }
    }
}
```

---

## 9. PROJECTILES & DROPS

### 9.1 SoapBullet.cs
```csharp
using UnityEngine;

public class SoapBullet : MonoBehaviour
{
    public float speed = 12f;
    public Vector2 direction = Vector2.right;
    public int damage = 25;
    public float maxRange = 15f;
    private float traveled = 0f;

    private void Update()
    {
        float dist = speed * Time.deltaTime;
        transform.position += (Vector3)(direction * dist);
        traveled += dist;
        if (traveled >= maxRange) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<EnemyBase>(out var enemy))
        {
            enemy.TakeDamage(damage, transform.position);
            AudioManager.Instance.PlaySfx("hit");
            Destroy(gameObject);
        }
        else if (col.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
```

### 9.2 DropItem.cs
```csharp
using UnityEngine;

public class DropItem : MonoBehaviour
{
    public string dropType = "candy"; // candy, health, repair
    public int value = 15;
    public float magnetRadius = 2.5f;
    public float magnetSpeed = 6.0f;
    private Transform playerTransform;

    public void Setup(string type, int val)
    {
        dropType = type;
        value = val;
    }

    private void Start()
    {
        var p = FindFirstObjectByType<Player>();
        if (p != null) playerTransform = p.transform;
        Destroy(gameObject, 25f);
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist < magnetRadius)
            {
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            switch (dropType)
            {
                case "candy":
                    GameState.Instance.AddCandy(value);
                    AudioManager.Instance.PlaySfx("coin");
                    break;
                case "health":
                    col.GetComponent<Player>().Heal(value);
                    AudioManager.Instance.PlaySfx("repair");
                    break;
                case "repair":
                    col.GetComponent<Player>().AddRepairPoints(value);
                    AudioManager.Instance.PlaySfx("repair");
                    break;
            }
            Destroy(gameObject);
        }
    }
}
```

---

## 10. WAVE MANAGER & LEVEL PROGRESSION

### 10.1 WaveManager.cs
```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public Transform leftPortal;
    public Transform rightPortal;
    public Transform topPortal;
    public GameObject enemyBasePrefab;

    public LevelConfigSO[] allLevels;
    public LevelConfigSO currentLevel;

    private int currentWaveIdx = 0;
    private int activeEnemies = 0;
    private int totalEnemiesThisLevel = 0;
    private int defeatedEnemies = 0;

    private int killStreak = 0;
    private int maxKillStreak = 0;

    public event Action<int, int, bool> OnWaveStarted;
    public event Action<float> OnProgressUpdated;
    public event Action<int, int> OnKillStreakUpdated;
    public event Action<int> OnLevelCompleted;

    private void OnEnable() => EventBus.OnEnemyKilled += HandleEnemyKilled;
    private void OnDisable() => EventBus.OnEnemyKilled += HandleEnemyKilled;

    public void StartLevel(int levelId)
    {
        currentLevel = allLevels[Mathf.Clamp(levelId - 1, 0, allLevels.Length - 1)];
        currentWaveIdx = 0;
        defeatedEnemies = 0;
        killStreak = 0;
        maxKillStreak = 0;

        totalEnemiesThisLevel = 0;
        foreach (var wave in currentLevel.waves)
            foreach (var grp in wave.enemyGroups)
                totalEnemiesThisLevel += grp.count;

        StartCoroutine(RunWaveRoutine());
    }

    private IEnumerator RunWaveRoutine()
    {
        while (currentWaveIdx < currentLevel.waves.Count)
        {
            var wave = currentLevel.waves[currentWaveIdx];
            yield return new WaitForSeconds(wave.preDelay);

            OnWaveStarted?.Invoke(currentWaveIdx + 1, currentLevel.waves.Count, wave.isHugeWave);
            if (wave.isHugeWave) AudioManager.Instance.PlaySfx("alarm");

            foreach (var grp in wave.enemyGroups)
            {
                for (int i = 0; i < grp.count; i++)
                {
                    SpawnEnemy(grp.enemyType, grp.spawnLocation);
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }

            while (activeEnemies > 0) yield return null;
            currentWaveIdx++;
        }

        EvaluateVictory();
    }

    private void SpawnEnemy(string type, string location)
    {
        Vector3 pos = location switch
        {
            "left" => leftPortal.position,
            "right" => rightPortal.position,
            "top" => topPortal.position,
            _ => UnityEngine.Random.value < 0.5f ? leftPortal.position : rightPortal.position
        };

        GameObject enemy = Instantiate(enemyBasePrefab, pos, Quaternion.identity);
        enemy.GetComponent<EnemyBase>().enemyType = type;
        activeEnemies++;
    }

    private void HandleEnemyKilled(string type, Vector2 pos)
    {
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
        defeatedEnemies++;
        OnProgressUpdated?.Invoke((float)defeatedEnemies / Mathf.Max(1, totalEnemiesThisLevel));

        killStreak++;
        maxKillStreak = Mathf.Max(maxKillStreak, killStreak);

        int bonus = killStreak switch
        {
            5 => 50,
            10 => 150,
            20 => 400,
            40 => 1000,
            _ => 0
        };

        if (bonus > 0)
        {
            GameState.Instance.AddCandy(bonus);
            AudioManager.Instance.PlaySfx("streak");
        }

        OnKillStreakUpdated?.Invoke(killStreak, bonus);
    }

    private void EvaluateVictory()
    {
        int stars = 1;
        var store = FindFirstObjectByType<Minimarket>();
        if (store != null)
        {
            float hpPct = (float)store.healthCurrent / store.healthMax;
            if (hpPct >= 0.5f) stars = 2;
            if (hpPct >= 0.7f && maxKillStreak >= 5) stars = 3;
        }

        GameState.Instance.RecordLevelResult(currentLevel.levelId, stars);
        OnLevelCompleted?.Invoke(stars);
    }
}
```

---

## 11. AUDIO MANAGER (SFX & PROCEDURAL SYNTHESIS)

```csharp
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource bgmSource;
    public List<AudioSource> sfxPool = new List<AudioSource>();
    public Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < 8; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            sfxPool.Add(src);
        }
    }

    public void PlaySfx(string sfxName)
    {
        if (sfxClips.TryGetValue(sfxName, out var clip))
        {
            foreach (var src in sfxPool)
            {
                if (!src.isPlaying)
                {
                    src.volume = GameState.Instance.data.volumeSfx * GameState.Instance.data.volumeMaster;
                    src.PlayOneShot(clip);
                    return;
                }
            }
        }
    }

    public void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = GameState.Instance.data.volumeBgm * GameState.Instance.data.volumeMaster;
        bgmSource.Play();
    }

    public void StopBgm() => bgmSource?.Stop();
}
```

---

## 12. CHECKLIST MIGRASI KE UNITY

1. **Buat Unity Project:**
   - Template: **2D (URP)**.
   - Project Settings -> Physics 2D: Gravity Y = `-18` atau `-20`.
2. **Import Assets:**
   - Copy folder `assets/audio/sfx/` ke `Assets/Audio/SFX/`.
   - Copy sprite dari `assets/sprites/` dan `assets/ui/` ke `Assets/Sprites/`.
   - Set Texture Type = `Sprite (2D and UI)`, Pixels Per Unit = 100, Filter Mode = `Point (no filter)`.
3. **Instal Script C#:**
   - Buat folder `Assets/Scripts/` dan masukkan semua file C# di atas (`GameState.cs`, `Player.cs`, `EnemyBase.cs`, `WaveManager.cs`, dll.).
4. **Buat Prefabs:**
   - `Player.prefab`, `Minimarket.prefab`, `EnemyBase.prefab`, `DropItem.prefab`.
   - Defense Prefabs: `SoapCannon.prefab`, `SoapBarrelSpawner.prefab`, `Fence.prefab`, `BalloonTrap.prefab`.
   - Projectile Prefabs: `SoapBullet.prefab`, `SoapBomb.prefab`, `SoapBarrel.prefab`, `BubbleMine.prefab`.
5. **Setup Scene Utama:**
   - `MainMenu.unity`
   - `LevelSelect.unity`
   - `ShopScreen.unity`
   - `LevelBase.unity` (Arena gameplay tempat RnA Mart berada).
