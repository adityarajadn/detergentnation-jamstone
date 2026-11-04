using UnityEngine;

[CreateAssetMenu(fileName = "NewJetpackData", menuName = "Detergentnation/Jetpack Data")]
public class JetpackData : ScriptableObject
{
    public string jetpack_name;
    [TextArea(2, 4)] public string description;
    public int price = 150;
    public Sprite icon;
    public GameObject jetpack_prefab;
    public float hover_duration = 3f;
    public float speed_multiplier = 1.2f;
    public bool can_double_jump = true;
}
