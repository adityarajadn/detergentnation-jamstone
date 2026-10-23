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
