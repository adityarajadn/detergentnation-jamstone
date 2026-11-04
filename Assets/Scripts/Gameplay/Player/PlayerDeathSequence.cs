using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Death animation: disables combat, flings equipment off the player, plays the death trigger,
/// then invokes the completion callback. Static helper run as a coroutine by PlayerController.
/// </summary>
public static class PlayerDeathSequence
{
    private const float FlingDuration = 0.65f;
    private const float TotalDuration = 1.1f;

    public static IEnumerator Run(
        PlayerController owner,
        Rigidbody2D rb,
        Animator animator,
        ArmorBase active_armor,
        Transform armor_mount,
        JetpackBase active_jetpack,
        Transform jetpack_mount,
        Transform hand_mount,
        Action on_complete)
    {
        // Disable weapons aiming & combat
        PlayerWeaponAim pwa = owner.GetComponent<PlayerWeaponAim>();
        if (pwa != null) pwa.enabled = false;
        PlayerCombat pc = owner.GetComponent<PlayerCombat>();
        if (pc != null) pc.enabled = false;

        // Stop player physical movement
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Collect equipment to fling off
        List<Transform> items_to_drop = new List<Transform>();

        if (active_armor != null) items_to_drop.Add(active_armor.transform);
        else if (armor_mount != null && armor_mount.childCount > 0) items_to_drop.Add(armor_mount.GetChild(0));

        if (active_jetpack != null) items_to_drop.Add(active_jetpack.transform);
        else if (jetpack_mount != null && jetpack_mount.childCount > 0) items_to_drop.Add(jetpack_mount.GetChild(0));

        if (hand_mount != null)
        {
            hand_mount.gameObject.SetActive(true);
            items_to_drop.Add(hand_mount);
        }

        // Fling each item using DOTween
        float ground_y = owner.transform.position.y - 0.45f;
        for (int i = 0; i < items_to_drop.Count; i++)
        {
            Transform item = items_to_drop[i];
            if (item == null) continue;

            item.SetParent(null, true);

            float dir_x = (i % 2 == 0 ? 1f : -1f) * UnityEngine.Random.Range(1.2f, 2.2f);
            Vector3 drop_target = new Vector3(item.position.x + dir_x, ground_y, item.position.z);

            // Pop up arc and rotate towards ground
            item.DOJump(drop_target, jumpPower: 1.2f, numJumps: 1, duration: FlingDuration)
                .SetEase(Ease.OutQuad);
            item.DORotate(new Vector3(0f, 0f, UnityEngine.Random.Range(-90f, 90f)), FlingDuration)
                .SetEase(Ease.OutQuad);
        }

        // Trigger Death animation
        if (animator != null)
        {
            animator.SetTrigger("death");
        }

        // Wait for fling and death animation to complete
        yield return new WaitForSeconds(TotalDuration);

        on_complete?.Invoke();
    }
}
