using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static int selected_level = 1;
    public int candy_total = 1000;
    public int highest_unlocked_level = 1;
    public int[] level_stars = new int[5];

    public string equipped_ranged_weapon = GameConstants.EquipmentNames.DefaultRanged;
    public string equipped_melee_weapon = GameConstants.EquipmentNames.DefaultMelee;
    public string equipped_armor = GameConstants.EquipmentNames.None;
    public string equipped_jetpack = GameConstants.EquipmentNames.None;
    public string equipped_consumable_1 = GameConstants.EquipmentNames.DefaultConsumable1;
    public string equipped_consumable_2 = GameConstants.EquipmentNames.DefaultConsumable2;
    public WeaponType active_weapon_type = WeaponType.Ranged;

    public UnityEvent<int> on_candy_changed;
    public UnityEvent<string, bool> on_equipment_changed;
    public UnityEvent<string> on_armor_changed;
    public UnityEvent<string> on_jetpack_changed;
    public UnityEvent<string, int> on_consumable_equipped;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoLoad()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("GameManager (AutoLoad)");
            instance = go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    public void AddCandy(int amount)
    {
        candy_total += amount;
        SaveData();
        if (on_candy_changed != null)
        {
            on_candy_changed.Invoke(candy_total);
        }
    }

    public bool SpendCandy(int amount)
    {
        if (candy_total < amount) return false;
        candy_total -= amount;
        SaveData();
        if (on_candy_changed != null)
        {
            on_candy_changed.Invoke(candy_total);
        }
        AudioManager.PlaySFX(GameConstants.AudioKeys.Purchase);
        return true;
    }

    public bool IsEquipmentOwned(string item_name, int price)
    {
        if (price == 0) return true;
        return PlayerPrefs.GetInt(GameConstants.PrefKeys.OwnedEquipmentPrefix + item_name, 0) == 1;
    }

    public void SetEquipmentOwned(string item_name)
    {
        PlayerPrefs.SetInt(GameConstants.PrefKeys.OwnedEquipmentPrefix + item_name, 1);
        PlayerPrefs.Save();
    }

    public void EquipWeapon(string item_name, bool is_ranged)
    {
        if (is_ranged)
        {
            equipped_ranged_weapon = item_name;
            PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedRangedWeapon, item_name);
        }
        else
        {
            equipped_melee_weapon = item_name;
            PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedMeleeWeapon, item_name);
        }
        active_weapon_type = is_ranged ? WeaponType.Ranged : WeaponType.Melee;
        PlayerPrefs.SetInt(GameConstants.PrefKeys.ActiveWeaponType, (int)active_weapon_type);
        PlayerPrefs.Save();
        if (on_equipment_changed != null)
        {
            on_equipment_changed.Invoke(item_name, is_ranged);
        }
    }

    public void EquipArmor(string item_name)
    {
        equipped_armor = item_name;
        PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedArmor, item_name);
        PlayerPrefs.Save();
        if (on_armor_changed != null)
        {
            on_armor_changed.Invoke(item_name);
        }
    }

    public void EquipJetpack(string item_name)
    {
        equipped_jetpack = item_name;
        PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedJetpack, item_name);
        PlayerPrefs.Save();
        if (on_jetpack_changed != null)
        {
            on_jetpack_changed.Invoke(item_name);
        }
    }

    public void EquipConsumable(string item_name, int slot)
    {
        if (slot == 1)
        {
            if (equipped_consumable_1 == item_name)
            {
                equipped_consumable_1 = GameConstants.EquipmentNames.None;
            }
            else
            {
                if (equipped_consumable_2 == item_name)
                {
                    equipped_consumable_2 = GameConstants.EquipmentNames.None;
                    PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedConsumable2, equipped_consumable_2);
                }
                equipped_consumable_1 = item_name;
            }
            PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedConsumable1, equipped_consumable_1);
        }
        else if (slot == 2)
        {
            if (equipped_consumable_2 == item_name)
            {
                equipped_consumable_2 = GameConstants.EquipmentNames.None;
            }
            else
            {
                if (equipped_consumable_1 == item_name)
                {
                    equipped_consumable_1 = GameConstants.EquipmentNames.None;
                    PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedConsumable1, equipped_consumable_1);
                }
                equipped_consumable_2 = item_name;
            }
            PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedConsumable2, equipped_consumable_2);
        }
        PlayerPrefs.Save();
        if (on_consumable_equipped != null)
        {
            on_consumable_equipped.Invoke(item_name, slot);
        }
    }

    public int GetUpgradeLevel(string upgrade_id)
    {
        return PlayerPrefs.GetInt(GameConstants.PrefKeys.UpgradePrefix + upgrade_id, 1);
    }

    public void SetUpgradeLevel(string upgrade_id, int level)
    {
        PlayerPrefs.SetInt(GameConstants.PrefKeys.UpgradePrefix + upgrade_id, level);
        PlayerPrefs.Save();
    }

    public int GetConsumableCount(string item_name)
    {
        return PlayerPrefs.GetInt(GameConstants.PrefKeys.ConsumablePrefix + item_name, 0);
    }

    public void SetConsumableCount(string item_name, int count)
    {
        PlayerPrefs.SetInt(GameConstants.PrefKeys.ConsumablePrefix + item_name, count);
        PlayerPrefs.Save();
    }

    public void CompleteLevel(int level_number, int stars_earned)
    {
        int index = level_number - 1;
        if (index >= 0 && index < level_stars.Length)
        {
            if (stars_earned > level_stars[index])
            {
                level_stars[index] = stars_earned;
            }
        }

        if (level_number >= highest_unlocked_level && highest_unlocked_level < 5)
        {
            highest_unlocked_level = level_number + 1;
        }

        SaveData();
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt(GameConstants.PrefKeys.CandyTotal, candy_total);
        PlayerPrefs.SetInt(GameConstants.PrefKeys.UnlockedLevel, highest_unlocked_level);
        for (int i = 0; i < level_stars.Length; i++)
        {
            PlayerPrefs.SetInt(GameConstants.PrefKeys.LevelStarsPrefix + i, level_stars[i]);
        }
        PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedRangedWeapon, equipped_ranged_weapon);
        PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedMeleeWeapon, equipped_melee_weapon);
        PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedArmor, equipped_armor);
        PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedJetpack, equipped_jetpack);
        PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedConsumable1, equipped_consumable_1);
        PlayerPrefs.SetString(GameConstants.PrefKeys.EquippedConsumable2, equipped_consumable_2);
        PlayerPrefs.SetInt(GameConstants.PrefKeys.ActiveWeaponType, (int)active_weapon_type);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        candy_total = PlayerPrefs.GetInt(GameConstants.PrefKeys.CandyTotal, 0);
        highest_unlocked_level = PlayerPrefs.GetInt(GameConstants.PrefKeys.UnlockedLevel, 1);
        for (int i = 0; i < level_stars.Length; i++)
        {
            level_stars[i] = PlayerPrefs.GetInt(GameConstants.PrefKeys.LevelStarsPrefix + i, 0);
        }
        equipped_ranged_weapon = PlayerPrefs.GetString(GameConstants.PrefKeys.EquippedRangedWeapon, GameConstants.EquipmentNames.DefaultRanged);
        equipped_melee_weapon = PlayerPrefs.GetString(GameConstants.PrefKeys.EquippedMeleeWeapon, GameConstants.EquipmentNames.DefaultMelee);
        equipped_armor = PlayerPrefs.GetString(GameConstants.PrefKeys.EquippedArmor, GameConstants.EquipmentNames.None);
        equipped_jetpack = PlayerPrefs.GetString(GameConstants.PrefKeys.EquippedJetpack, GameConstants.EquipmentNames.None);
        equipped_consumable_1 = PlayerPrefs.GetString(GameConstants.PrefKeys.EquippedConsumable1, GameConstants.EquipmentNames.DefaultConsumable1);
        equipped_consumable_2 = PlayerPrefs.GetString(GameConstants.PrefKeys.EquippedConsumable2, GameConstants.EquipmentNames.DefaultConsumable2);
        active_weapon_type = (WeaponType)PlayerPrefs.GetInt(GameConstants.PrefKeys.ActiveWeaponType, (int)WeaponType.Ranged);
    }
}
