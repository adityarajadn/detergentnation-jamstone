using UnityEngine;

[CreateAssetMenu(fileName = "NewArmorData", menuName = "Detergentnation/Armor Data")]
public class ArmorData : ScriptableObject
{
    public string armor_name;
    [TextArea(2, 4)] public string description;
    public int price = 100;
    public Sprite icon;
    public GameObject armor_prefab;
    public float hp_bonus_percent = 0f;
    public float damage_reduction_percent = 0f;
}
