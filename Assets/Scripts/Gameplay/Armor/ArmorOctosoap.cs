using System.Collections;
using UnityEngine;

public class ArmorOctosoap : ArmorBase
{
    [SerializeField] private GameObject soap_projectile_prefab;
    [SerializeField] private float throw_interval = 15f;
    [SerializeField] private int soap_damage = 30;

    private Coroutine throw_coroutine;

    private void Reset()
    {
        armor_name = GameConstants.EquipmentNames.ArmorOctosoap;
        armor_rating = 3;
    }

    protected override void Awake()
    {
        base.Awake();
        armor_name = GameConstants.EquipmentNames.ArmorOctosoap;
        armor_rating = 3;
    }

    private void OnEnable()
    {
        throw_coroutine = StartCoroutine(AutoThrowRoutine());
    }

    private void OnDisable()
    {
        if (throw_coroutine != null)
        {
            StopCoroutine(throw_coroutine);
            throw_coroutine = null;
        }
    }

    private IEnumerator AutoThrowRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(throw_interval);
            ThrowSoap();
        }
    }

    private void ThrowSoap()
    {
        GameObject prefab = soap_projectile_prefab != null ? soap_projectile_prefab : Resources.Load<GameObject>("Prefabs/Projectiles/SoapBomb");
        if (prefab == null) return;

        Vector2 throw_dir = Vector2.right;
        Transform player = transform.root;
        Transform visuals = player != null ? player.Find("Visuals") : null;
        if ((visuals != null && visuals.localScale.x < 0) || (player != null && player.localScale.x < 0))
        {
            throw_dir = Vector2.left;
        }

        GameObject soap = Instantiate(prefab, transform.position, Quaternion.identity);
        SoapBombProjectile sbp = soap.GetComponent<SoapBombProjectile>();
        if (sbp != null)
        {
            sbp.damage = soap_damage;
            sbp.Launch(throw_dir * 7f + Vector2.up * 4f);
        }
        else
        {
            Rigidbody2D rb = soap.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = throw_dir * 7f + Vector2.up * 4f;
        }
    }
}
