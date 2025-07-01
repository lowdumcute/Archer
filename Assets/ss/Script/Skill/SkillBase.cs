using UnityEngine;

public enum SkillType { Attack, Buff, Heal }
public enum StatusEffectType { None, Burn, Poison, Stun }

[CreateAssetMenu(menuName = "Skills/New Skill")]
public class SkillBase : ScriptableObject
{
    public string skillName;
    public SkillType skillType;
    public GameObject visualEffectPrefab;

    [Header("Stats")]
    public float power; // Damage, Heal hoặc Buff power
    public StatusEffectType statusEffect;
    public float statusDuration;
}