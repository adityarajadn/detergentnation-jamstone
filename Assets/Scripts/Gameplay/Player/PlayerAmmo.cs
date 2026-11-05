using UnityEngine;

public class PlayerAmmo : MonoBehaviour
{
    public int ammo_in_mag = 12;
    public int ammo_carried = 64;
    public int ammo_carried_max = 64;

    [Header("Scaling")]
    [SerializeField] private int ammo_bonus_per_level = 6;

    public void Initialize(WeaponData ranged_weapon)
    {
        int ammo_level = GameManager.instance != null ? GameManager.instance.GetUpgradeLevel(GameConstants.UpgradeKeys.WeaponAmmo) : 1;
        int ammo_bonus = (ammo_level - 1) * ammo_bonus_per_level;

        if (ranged_weapon != null)
        {
            ammo_in_mag = ranged_weapon.magazine_capacity;
            ammo_carried_max = ranged_weapon.carried_ammo_base + ammo_bonus;
            ammo_carried = ammo_carried_max;
        }
    }

    public bool CanShoot()
    {
        return ammo_in_mag > 0;
    }

    public void ConsumeBullet()
    {
        if (ammo_in_mag > 0) ammo_in_mag--;
    }

    public void Reload(int mag_capacity)
    {
        if (ammo_in_mag >= mag_capacity || ammo_carried <= 0) return;

        int needed = mag_capacity - ammo_in_mag;
        int take = Mathf.Min(needed, ammo_carried);
        ammo_in_mag += take;
        ammo_carried -= take;

        NotificationToast.Show("Weapon Reloaded! (" + ammo_in_mag + "/" + ammo_carried + ")", ToastType.Info);
    }

    public void Restock()
    {
        ammo_carried = ammo_carried_max;
    }
}
