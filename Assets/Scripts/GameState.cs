using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int candy = 10000;
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
    private static GameState _instance;
    public static GameState Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameState>();
                if (_instance == null)
                {
                    var go = new GameObject("GameState");
                    _instance = go.AddComponent<GameState>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }
    public SaveData data = new SaveData();

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "save_data.json");

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        transform.SetParent(null);
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

    public int GetUpgradeMax(string key)
    {
        if (key.StartsWith("defense_")) return 5;
        return 10;
    }

    public int GetUpgradeCost(string key)
    {
        int lvl = GetUpgradeLevel(key);
        if (lvl >= GetUpgradeMax(key)) return 999999;
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

    public bool IsItemUnlocked(string id)
    {
        if (string.IsNullOrEmpty(id) || id == "none") return true;
        return data.unlockedItems != null && data.unlockedItems.Contains(id);
    }

    public void UnlockItem(string id)
    {
        if (data.unlockedItems == null) data.unlockedItems = new List<string>();
        if (!data.unlockedItems.Contains(id))
        {
            data.unlockedItems.Add(id);
            SaveGame();
        }
    }

    public void EquipItem(string category, string id)
    {
        switch (category)
        {
            case "range": data.equippedRange = id; break;
            case "melee": data.equippedMelee = id; break;
            case "armor": data.equippedArmor = id; break;
            case "jetpack": data.equippedJetpack = id; break;
        }
        SaveGame();
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
