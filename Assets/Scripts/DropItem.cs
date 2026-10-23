using UnityEngine;

public class DropItem : MonoBehaviour
{
    public string dropType = "candy"; // candy, health, repair
    public int value = 15;
    public float magnetRadius = 2.5f;
    public float magnetSpeed = 6.0f;
    public Sprite candySprite;
    public Sprite healthSprite;
    public Sprite repairSprite;
    private Transform playerTransform;

    public void Setup(string type, int val)
    {
        dropType = type;
        value = val;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (type == "candy" && candySprite != null) sr.sprite = candySprite;
            else if (type == "health" && healthSprite != null) sr.sprite = healthSprite;
            else if (type == "repair" && repairSprite != null) sr.sprite = repairSprite;
        }
    }

    private void Start()
    {
        var p = FindAnyObjectByType<Player>();
        if (p != null) playerTransform = p.transform;
        Destroy(gameObject, 25f);
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist < magnetRadius)
            {
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            switch (dropType)
            {
                case "candy":
                    if (GameState.Instance != null) GameState.Instance.AddCandy(value);
                    AudioManager.Instance?.PlaySfx("coin");
                    break;
                case "health":
                    if (col.TryGetComponent<Player>(out var p)) p.Heal(value);
                    AudioManager.Instance?.PlaySfx("repair");
                    break;
                case "repair":
                    if (col.TryGetComponent<Player>(out var p2)) p2.AddRepairPoints(value);
                    AudioManager.Instance?.PlaySfx("repair");
                    break;
            }
            Destroy(gameObject);
        }
    }
}
