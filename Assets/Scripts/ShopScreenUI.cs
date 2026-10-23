using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopScreenUI : MonoBehaviour
{
    [Header("Top Bar")]
    public Text candyText;
    public Button backButton;

    [Header("Tab Headers")]
    public Button tabEquipmentBtn;
    public Button tabConsumablesBtn;
    public Button tabUpgradesBtn;
    public GameObject equipmentContent;
    public GameObject consumablesContent;
    public GameObject upgradesContent;

    [Header("Ferry Preview")]
    public Image previewBase;
    public Image previewHat;
    public Image previewChest;
    public Image previewJetpack;
    public Image previewRange;
    public Image previewMelee;

    [Header("Loadout Summary")]
    public Text rangeSummaryText;
    public Text meleeSummaryText;
    public Text armorSummaryText;
    public Text jetpackSummaryText;

    [Header("Sprites")]
    public Sprite sprWaterPistol;
    public Sprite sprWaterRifle;
    public Sprite sprWaterPlasma;
    public Sprite sprBubbleGun;
    public Sprite sprBubbleStick;
    public Sprite sprSoapHammer;
    public Sprite sprLoofahMace;
    public Sprite sprCurseSword;
    public Sprite sprWorkerHat;
    public Sprite sprNumberOne;
    public Sprite sprOctosoap;
    public Sprite sprJoyride;
    public Sprite sprVengeance;

    [Header("Consumable Icons")]
    public Sprite iconSoapBomb;
    public Sprite iconFirstAid;
    public Sprite iconEnergyDrink;
    public Sprite iconSoapBucket;
    public Sprite iconToolsBox;
    public Sprite iconSonarBatarang;

    [Header("Container Transforms")]
    public Transform equipmentGridParent;
    public Transform consumablesGridParent;
    public Transform upgradesListParent;

    [Serializable]
    public class EquipItemDef
    {
        public string id;
        public string category; // range, melee, armor, jetpack
        public string title;
        public string desc;
        public int cost;
    }

    [Serializable]
    public class ConsumableDef
    {
        public string id;
        public string title;
        public string desc;
        public int cost;
        public Sprite icon;
    }

    [Serializable]
    public class UpgradeDef
    {
        public string key;
        public string title;
        public int maxLevel;
    }

    private List<EquipItemDef> equipmentList = new List<EquipItemDef>();
    private List<ConsumableDef> consumableList = new List<ConsumableDef>();
    private List<UpgradeDef> upgradeList = new List<UpgradeDef>();

    private void Awake()
    {
        InitDefinitions();
    }

    private void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

        if (tabEquipmentBtn != null) tabEquipmentBtn.onClick.AddListener(() => SwitchTab(0));
        if (tabConsumablesBtn != null) tabConsumablesBtn.onClick.AddListener(() => SwitchTab(1));
        if (tabUpgradesBtn != null) tabUpgradesBtn.onClick.AddListener(() => SwitchTab(2));

        SwitchTab(0);
        RefreshAll();
    }

    private void Update()
    {
        if (candyText != null && GameState.Instance != null)
        {
            candyText.text = $"CANDY: {GameState.Instance.data.candy}";
        }
    }

    private void InitDefinitions()
    {
        equipmentList = new List<EquipItemDef>
        {
            new EquipItemDef { id = "water_pistol", category = "range", title = "Water Pistol", desc = "Standard starter soap water gun (12 ammo)", cost = 0 },
            new EquipItemDef { id = "water_rifle", category = "range", title = "Water Rifle", desc = "Rapid-fire semi auto soap rifle (30 ammo)", cost = 140 },
            new EquipItemDef { id = "water_plasma", category = "range", title = "Water Plasma", desc = "High pressure short-range burst (4 ammo)", cost = 220 },
            new EquipItemDef { id = "bubble_gun", category = "range", title = "Bubble Gun", desc = "Fires floating deterrent bubble mines (50 ammo)", cost = 140 },
            new EquipItemDef { id = "bubble_stick", category = "melee", title = "Bubble Stick", desc = "Starter bubble wand with contact damage", cost = 0 },
            new EquipItemDef { id = "soap_hammer", category = "melee", title = "Soap Hammer", desc = "Heavy scrub brush tipped with solid soap bars", cost = 100 },
            new EquipItemDef { id = "loofah_mace", category = "melee", title = "Loofah Mace", desc = "Spiked soapy lather ball on a chain", cost = 200 },
            new EquipItemDef { id = "imelda", category = "melee", title = "Curse Sword Imelda", desc = "Legendary soap blade that vaporizes Foodonians", cost = 400 },
            new EquipItemDef { id = "worker_hat", category = "armor", title = "Worker's Hat", desc = "+1 Armor rating", cost = 75 },
            new EquipItemDef { id = "number_one", category = "armor", title = "I am Number One", desc = "+2 Armor, pops defensive bubbles when damaged", cost = 160 },
            new EquipItemDef { id = "octosoap", category = "armor", title = "Octosoap", desc = "+3 Armor, passively flings soap bars", cost = 250 },
            new EquipItemDef { id = "joyride", category = "jetpack", title = "Joyride Jetpack", desc = "Double jump and enhanced vertical leap", cost = 180 },
            new EquipItemDef { id = "vengeance", category = "jetpack", title = "Vengeance Jetpack", desc = "Double jump and glide slow fall", cost = 220 }
        };

        consumableList = new List<ConsumableDef>
        {
            new ConsumableDef { id = "soap_bomb", title = "Soap Bomb", desc = "Throwable explosive detergent can", cost = 100, icon = iconSoapBomb },
            new ConsumableDef { id = "first_aid", title = "First Aid Kit", desc = "Restores +50 Ferry HP", cost = 25, icon = iconFirstAid },
            new ConsumableDef { id = "energy_drink", title = "Energy Drink", desc = "Temporary +45% movement speed", cost = 10, icon = iconEnergyDrink },
            new ConsumableDef { id = "soap_bucket", title = "Soap Water Bucket", desc = "Absolute protection for next 3 attacks", cost = 75, icon = iconSoapBucket },
            new ConsumableDef { id = "tools_box", title = "Tools Box", desc = "Grants +30 Repair Points", cost = 25, icon = iconToolsBox },
            new ConsumableDef { id = "sonar_batarang", title = "Sonar Batarang", desc = "Stuns all active Foodonians for 10s", cost = 100, icon = iconSonarBatarang }
        };

        upgradeList = new List<UpgradeDef>
        {
            new UpgradeDef { key = "range_damage", title = "Range Weapon Damage", maxLevel = 10 },
            new UpgradeDef { key = "range_fire_rate", title = "Range Fire Rate", maxLevel = 10 },
            new UpgradeDef { key = "range_ammo", title = "Range Clip Size", maxLevel = 10 },
            new UpgradeDef { key = "melee_damage", title = "Melee Weapon Damage", maxLevel = 10 },
            new UpgradeDef { key = "defense_damage", title = "Defense Damage", maxLevel = 5 },
            new UpgradeDef { key = "defense_ammo", title = "Defense Ammo", maxLevel = 5 },
            new UpgradeDef { key = "defense_duration", title = "Defense Durability", maxLevel = 5 }
        };
    }

    public void SwitchTab(int index)
    {
        if (equipmentContent != null) equipmentContent.SetActive(index == 0);
        if (consumablesContent != null) consumablesContent.SetActive(index == 1);
        if (upgradesContent != null) upgradesContent.SetActive(index == 2);

        SetTabVisual(tabEquipmentBtn, index == 0);
        SetTabVisual(tabConsumablesBtn, index == 1);
        SetTabVisual(tabUpgradesBtn, index == 2);
    }

    private void SetTabVisual(Button btn, bool active)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = active ? new Color(0.12f, 0.16f, 0.24f, 1f) : new Color(0.04f, 0.07f, 0.12f, 0.8f);
        }
        var txt = btn.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.color = active ? Color.white : new Color(0.58f, 0.64f, 0.72f);
        }
    }

    public void RefreshAll()
    {
        if (equipmentList == null || equipmentList.Count == 0) InitDefinitions();
        RefreshPreview();
        RefreshEquipmentGrid();
        RefreshConsumablesGrid();
        RefreshUpgradesList();
    }

    public void RefreshPreview()
    {
        if (GameState.Instance == null) return;
        var d = GameState.Instance.data;

        // 1. Range Weapon
        string rName = "Water Pistol";
        Sprite rSpr = sprWaterPistol;
        switch (d.equippedRange)
        {
            case "water_rifle": rName = "Water Rifle"; rSpr = sprWaterRifle; break;
            case "water_plasma": rName = "Water Plasma"; rSpr = sprWaterPlasma; break;
            case "bubble_gun": rName = "Bubble Gun"; rSpr = sprBubbleGun; break;
            default: rName = "Water Pistol"; rSpr = sprWaterPistol; break;
        }
        if (previewRange != null)
        {
            previewRange.sprite = rSpr;
            previewRange.gameObject.SetActive(rSpr != null);
        }
        if (rangeSummaryText != null) rangeSummaryText.text = $"Range: {rName}";

        // 2. Melee Weapon
        string mName = "Bubble Stick";
        Sprite mSpr = sprBubbleStick;
        switch (d.equippedMelee)
        {
            case "soap_hammer": mName = "Soap Hammer"; mSpr = sprSoapHammer; break;
            case "loofah_mace": mName = "Loofah Mace"; mSpr = sprLoofahMace; break;
            case "imelda": mName = "Curse Sword Imelda"; mSpr = sprCurseSword; break;
            default: mName = "Bubble Stick"; mSpr = sprBubbleStick; break;
        }
        if (previewMelee != null)
        {
            previewMelee.sprite = mSpr;
            previewMelee.gameObject.SetActive(mSpr != null);
        }
        if (meleeSummaryText != null) meleeSummaryText.text = $"Melee: {mName}";

        // 3. Armor
        string aName = "None";
        if (previewHat != null) previewHat.gameObject.SetActive(false);
        if (previewChest != null) previewChest.gameObject.SetActive(false);

        switch (d.equippedArmor)
        {
            case "worker_hat":
                aName = "Worker's Hat";
                if (previewHat != null)
                {
                    previewHat.sprite = sprWorkerHat;
                    previewHat.gameObject.SetActive(true);
                }
                break;
            case "number_one":
                aName = "I am Number One";
                if (previewChest != null)
                {
                    previewChest.sprite = sprNumberOne;
                    previewChest.gameObject.SetActive(true);
                }
                break;
            case "octosoap":
                aName = "Octosoap";
                if (previewChest != null)
                {
                    previewChest.sprite = sprOctosoap;
                    previewChest.gameObject.SetActive(true);
                }
                break;
            default:
                aName = "None";
                break;
        }
        if (armorSummaryText != null) armorSummaryText.text = $"Armor: {aName}";

        // 4. Jetpack
        string jName = "None";
        if (previewJetpack != null) previewJetpack.gameObject.SetActive(false);

        switch (d.equippedJetpack)
        {
            case "joyride":
                jName = "Joyride Jetpack";
                if (previewJetpack != null)
                {
                    previewJetpack.sprite = sprJoyride;
                    previewJetpack.gameObject.SetActive(true);
                }
                break;
            case "vengeance":
                jName = "Vengeance Jetpack";
                if (previewJetpack != null)
                {
                    previewJetpack.sprite = sprVengeance;
                    previewJetpack.gameObject.SetActive(true);
                }
                break;
            default:
                jName = "None";
                break;
        }
        if (jetpackSummaryText != null) jetpackSummaryText.text = $"Jetpack: {jName}";
    }

    public void RefreshEquipmentGrid()
    {
        if (equipmentGridParent == null || GameState.Instance == null) return;

        int count = equipmentGridParent.childCount;
        for (int i = 0; i < equipmentList.Count && i < count; i++)
        {
            var def = equipmentList[i];
            Transform card = equipmentGridParent.GetChild(i);
            UpdateEquipmentCard(card, def);
        }
    }

    private void UpdateEquipmentCard(Transform card, EquipItemDef def)
    {
        var d = GameState.Instance.data;
        bool isEquipped = false;
        switch (def.category)
        {
            case "range": isEquipped = (d.equippedRange == def.id); break;
            case "melee": isEquipped = (d.equippedMelee == def.id); break;
            case "armor": isEquipped = (d.equippedArmor == def.id); break;
            case "jetpack": isEquipped = (d.equippedJetpack == def.id); break;
        }

        bool isUnlocked = GameState.Instance.IsItemUnlocked(def.id) || def.cost == 0;

        Button actionBtn = card.Find("ActionBtn")?.GetComponent<Button>();
        Text btnText = actionBtn?.GetComponentInChildren<Text>();
        Image btnImg = actionBtn?.GetComponent<Image>();

        if (actionBtn != null && btnText != null && btnImg != null)
        {
            actionBtn.onClick.RemoveAllListeners();

            if (isEquipped)
            {
                btnText.text = "EQUIPPED";
                btnText.color = new Color(0.35f, 0.42f, 0.52f);
                btnImg.color = new Color(0.08f, 0.11f, 0.17f, 0.9f);
                actionBtn.interactable = false;
            }
            else if (isUnlocked)
            {
                btnText.text = "EQUIP";
                btnText.color = Color.white;
                btnImg.color = new Color(0.01f, 0.52f, 0.78f);
                actionBtn.interactable = true;
                actionBtn.onClick.AddListener(() =>
                {
                    GameState.Instance.EquipItem(def.category, def.id);
                    RefreshAll();
                });
            }
            else
            {
                btnText.text = $"BUY ({def.cost} CANDY)";
                btnText.color = Color.white;
                btnImg.color = new Color(0.01f, 0.41f, 0.63f);
                actionBtn.interactable = true;
                actionBtn.onClick.AddListener(() =>
                {
                    if (GameState.Instance.SpendCandy(def.cost))
                    {
                        GameState.Instance.UnlockItem(def.id);
                        GameState.Instance.EquipItem(def.category, def.id);
                        RefreshAll();
                    }
                });
            }
        }
    }

    public void RefreshConsumablesGrid()
    {
        if (consumablesGridParent == null || GameState.Instance == null) return;

        int count = consumablesGridParent.childCount;
        for (int i = 0; i < consumableList.Count && i < count; i++)
        {
            var def = consumableList[i];
            Transform card = consumablesGridParent.GetChild(i);
            int qty = GameState.Instance.GetConsumableQty(def.id);

            Text titleText = card.Find("Title")?.GetComponent<Text>();
            if (titleText != null) titleText.text = $"{def.title} (x{qty})";

            Button buyBtn = card.Find("ActionBtn")?.GetComponent<Button>();
            if (buyBtn != null)
            {
                buyBtn.onClick.RemoveAllListeners();
                buyBtn.onClick.AddListener(() =>
                {
                    if (GameState.Instance.SpendCandy(def.cost))
                    {
                        GameState.Instance.ModifyConsumableQty(def.id, 1);
                        RefreshAll();
                    }
                });
            }
        }
    }

    public void RefreshUpgradesList()
    {
        if (upgradesListParent == null || GameState.Instance == null) return;

        int count = upgradesListParent.childCount;
        for (int i = 0; i < upgradeList.Count && i < count; i++)
        {
            var def = upgradeList[i];
            Transform row = upgradesListParent.GetChild(i);
            int curLvl = GameState.Instance.GetUpgradeLevel(def.key);
            int cost = GameState.Instance.GetUpgradeCost(def.key);
            bool isMax = curLvl >= def.maxLevel;

            Text label = row.Find("Label")?.GetComponent<Text>();
            if (label != null) label.text = $"{def.title}: Lvl {curLvl} / {def.maxLevel}";

            Button btn = row.Find("UpgradeBtn")?.GetComponent<Button>();
            Text btnTxt = btn?.GetComponentInChildren<Text>();
            if (btn != null && btnTxt != null)
            {
                btn.onClick.RemoveAllListeners();
                if (isMax)
                {
                    btnTxt.text = "MAX";
                    btn.interactable = false;
                }
                else
                {
                    btnTxt.text = $"UPGRADE ({cost} CANDY)";
                    btn.interactable = true;
                    btn.onClick.AddListener(() =>
                    {
                        if (GameState.Instance.BuyUpgrade(def.key))
                        {
                            RefreshAll();
                        }
                    });
                }
            }
        }
    }

    public void OnBackClicked() => SceneManager.LoadScene("MainMenu");
}
