using UnityEngine;
using Animancer;

public class CharacterAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private Transform playerTransform;

    [Header("Animation Clips")]
    [SerializeField] private AnimationClip flappingClip;
    [SerializeField] private AnimationClip glideClip;

    [Header("Settings")]
    [SerializeField] private float moveSpeedThreshold = 0.5f;
    [SerializeField] private float fadeDuration = 0.25f;

    private AnimationClip currentClip;
    private Vector3 previousPosition;

    private void Awake()
    {
        if (animancer == null)
        {
            animancer = GetComponent<AnimancerComponent>();
        }

        if (playerTransform != null)
        {
            previousPosition = playerTransform.localPosition;
        }
    }

    private void Update()
    {
        if (animancer == null || playerTransform == null) return;

        Vector3 currentPosition = playerTransform.localPosition;
        float speed = Vector3.Distance(currentPosition, previousPosition) / Time.deltaTime;
        previousPosition = currentPosition;

        AnimationClip targetClip = speed > moveSpeedThreshold ? flappingClip : glideClip;

        if (targetClip != null && targetClip != currentClip)
        {
            animancer.Play(targetClip, fadeDuration);
            currentClip = targetClip;
        }
    }
}
