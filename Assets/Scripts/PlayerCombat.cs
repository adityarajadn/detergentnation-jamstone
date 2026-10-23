using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        int ammoUp = GameState.Instance != null ? GameState.Instance.GetUpgradeLevel("range_ammo") - 1 : 0;

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

        int rateUp = GameState.Instance != null ? GameState.Instance.GetUpgradeLevel("range_fire_rate") - 1 : 0;
        fireRateDelay = Mathf.Max(0.06f, fireRateDelay - (rateUp * 0.02f));
        clipCurrent = clipMax;

        int meleeUp = GameState.Instance != null ? GameState.Instance.GetUpgradeLevel("melee_damage") - 1 : 0;
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
        Vector3 mouseScreenPos = Input.mousePosition;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            var mPos = Mouse.current.position.ReadValue();
            mouseScreenPos = new Vector3(mPos.x, mPos.y, 0f);
        }
#endif
        if (aimArm != null && Camera.main != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            Vector3 dir = (mousePos - aimArm.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            aimArm.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (player != null && player.isPlanningMode) return;

        if (fireCooldown > 0f) fireCooldown -= Time.deltaTime;
        if (meleeCooldown > 0f) meleeCooldown -= Time.deltaTime;

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f) FinishReload();
            return;
        }

        bool reloadPress = Input.GetKeyDown(KeybindManager.Reload) || Input.GetKeyDown(KeyCode.R);
        bool fireHold = Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.F);
        bool meleePress = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.V);

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.rKey.wasPressedThisFrame) reloadPress = true;
            if (kb.jKey.isPressed || kb.fKey.isPressed) fireHold = true;
            if (kb.kKey.wasPressedThisFrame || kb.vKey.wasPressedThisFrame) meleePress = true;
        }

        var ms = Mouse.current;
        if (ms != null)
        {
            if (ms.leftButton.isPressed || ms.leftButton.wasPressedThisFrame) fireHold = true;
            if (ms.rightButton.wasPressedThisFrame) meleePress = true;
        }
#endif

        if (reloadPress) StartReload();

        if (fireHold && fireCooldown <= 0f)
        {
            FireRange();
        }

        if (meleePress && meleeCooldown <= 0f)
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
        AudioManager.Instance?.PlaySfx("shoot");

        Vector3 muzPos = muzzle != null ? muzzle.position : transform.position;
        Vector2 aimDir = Vector2.right;
        if (Camera.main != null)
        {
            Vector3 mScreen = Input.mousePosition;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                var mp = Mouse.current.position.ReadValue();
                mScreen = new Vector3(mp.x, mp.y, 0f);
            }
#endif
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mScreen.x, mScreen.y, -Camera.main.transform.position.z));
            aimDir = ((Vector2)worldPos - (Vector2)muzPos).normalized;
        }

        if (aimDir.sqrMagnitude < 0.01f)
        {
            float facing = (player != null && player.visualBody != null && player.visualBody.localScale.x < 0) ? -1f : 1f;
            aimDir = new Vector2(facing, 0f);
        }

        int dmgUp = GameState.Instance != null ? GameState.Instance.GetUpgradeLevel("range_damage") - 1 : 0;

        if (currentRangeWeapon == "bubble_gun" && bubbleMinePrefab != null)
        {
            GameObject b = Instantiate(bubbleMinePrefab, muzPos, Quaternion.identity);
            var mine = b.GetComponent<BubbleMine>();
            if (mine != null) mine.damage = 35 + (dmgUp * 8);
        }
        else if (currentRangeWeapon == "water_plasma" && soapBulletPrefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                float spread = (i - 1) * 12f;
                Vector2 spreadDir = Quaternion.Euler(0, 0, spread) * aimDir;
                GameObject b = Instantiate(soapBulletPrefab, muzPos, Quaternion.Euler(0, 0, Mathf.Atan2(spreadDir.y, spreadDir.x) * Mathf.Rad2Deg));
                var bullet = b.GetComponent<SoapBullet>();
                if (bullet != null)
                {
                    bullet.direction = spreadDir;
                    bullet.speed = 10f;
                    bullet.damage = 30 + (dmgUp * 10);
                    bullet.maxRange = 8f;
                }
            }
        }
        else if (soapBulletPrefab != null)
        {
            GameObject b = Instantiate(soapBulletPrefab, muzPos, Quaternion.Euler(0, 0, Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg));
            var bullet = b.GetComponent<SoapBullet>();
            if (bullet != null)
            {
                bullet.direction = aimDir;
                bullet.damage = 25 + (dmgUp * 7);
                if (currentRangeWeapon == "water_rifle")
                {
                    bullet.speed = 17f;
                    bullet.damage = 22 + (dmgUp * 6);
                }
            }
        }
    }

    private void PerformMelee()
    {
        meleeCooldown = meleeDelay;
        AudioManager.Instance?.PlaySfx("shoot");

        Vector3 mountPos = meleeMount != null ? meleeMount.position : transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(mountPos, 1.2f, enemyLayer);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(meleeDamage, transform.position);
                AudioManager.Instance?.PlaySfx("hit");
            }
        }
    }

    public void StartReload()
    {
        if (isReloading || clipCurrent >= clipMax || carriedAmmo <= 0) return;
        isReloading = true;
        reloadTimer = reloadDuration;
        AudioManager.Instance?.PlaySfx("reload");
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
