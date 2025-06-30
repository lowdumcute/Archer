using UnityEngine;

public abstract class SkillBehaviour : MonoBehaviour
{
    public SkillBase skillData;

    public virtual void UseSkill(GameObject target)
    {

        // Spawn VFX
        if (skillData.visualEffectPrefab != null)
            Instantiate(skillData.visualEffectPrefab, target.transform.position, Quaternion.identity);
    }
}
