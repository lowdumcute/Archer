using UnityEngine;

public abstract class SkillBehaviour : MonoBehaviour
{
    public SkillBase skillData;

    public virtual void UseSkill(Animator animator, GameObject target)
    {
        // Play animation
        if (skillData.animationClip != null)
            animator.Play(skillData.animationClip.name);

        // Spawn VFX
        if (skillData.visualEffectPrefab != null)
            Instantiate(skillData.visualEffectPrefab, target.transform.position, Quaternion.identity);
    }
}
