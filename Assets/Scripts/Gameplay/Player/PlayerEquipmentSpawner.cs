using UnityEngine;

/// <summary>
/// Spawns the equipped armor / jetpack prefab onto the player's mount points.
/// Static helper used by PlayerController (no scene component).
/// </summary>
public static class PlayerEquipmentSpawner
{
    public static bool TrySpawnArmor(string equipped_name, GameObject[] prefabs, Transform mount, Health health, out ArmorBase armor)
    {
        armor = null;
        if (string.IsNullOrEmpty(equipped_name) || equipped_name == GameConstants.EquipmentNames.None) return false;

        GameObject armor_prefab = FindPrefabByName(prefabs, equipped_name);
        if (armor_prefab == null || mount == null) return false;

        GameObject armor_obj = Object.Instantiate(armor_prefab, mount);
        armor_obj.transform.localPosition = Vector3.zero;
        armor_obj.transform.localRotation = Quaternion.identity;
        armor = armor_obj.GetComponent<ArmorBase>();

        if (health != null) health.SetArmor(armor);
        return true;
    }

    public static bool TrySpawnJetpack(string equipped_name, GameObject[] prefabs, Transform mount, out JetpackBase jetpack)
    {
        jetpack = null;
        if (string.IsNullOrEmpty(equipped_name) || equipped_name == GameConstants.EquipmentNames.None) return false;

        GameObject jetpack_prefab = FindPrefabByName(prefabs, equipped_name);
        if (jetpack_prefab == null || mount == null) return false;

        GameObject jetpack_obj = Object.Instantiate(jetpack_prefab, mount);
        jetpack_obj.transform.localPosition = Vector3.zero;
        jetpack_obj.transform.localRotation = Quaternion.identity;
        jetpack = jetpack_obj.GetComponent<JetpackBase>();
        return true;
    }

    private static GameObject FindPrefabByName(GameObject[] list, string match_name)
    {
        if (list == null) return null;
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == null) continue;
            string n = list[i].name.Replace("Armor_", "").Replace("Jetpack_", "").Replace("_", " ");
            if (n.Equals(match_name, System.StringComparison.OrdinalIgnoreCase) || list[i].name.Contains(match_name))
            {
                return list[i];
            }
            ArmorBase ab = list[i].GetComponent<ArmorBase>();
            if (ab != null && ab.armor_name == match_name) return list[i];
            JetpackBase jb = list[i].GetComponent<JetpackBase>();
            if (jb != null && jb.jetpack_name == match_name) return list[i];
        }
        return null;
    }
}
