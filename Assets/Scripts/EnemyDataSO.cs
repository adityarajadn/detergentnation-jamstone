using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "DetergentNation/EnemyData")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyType = "jellien";
    public string attackType = "Melee";
    public string category = "Normal";
    public int healthMax = 60;
    public float moveSpeed = 1.8f;
    public int attackDamage = 10;
    public float attackDelay = 1.0f;
    public float attackRange = 0.8f;
    public bool isFlying = false;
    public int candyReward = 15;
    public Color bodyColor = new Color(0.2f, 0.8f, 0.5f, 0.9f);
    public Sprite enemySprite;
}
