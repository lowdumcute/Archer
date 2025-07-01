using UnityEngine;

public class PiercingArrowSkill : SkillBehaviour
{
    public override void UseSkill( Transform firePoint,GameObject target)
    {
        base.UseSkill(firePoint, target); // Gọi hiệu ứng VFX nếu có ở target

        // Tính hướng từ người chơi tới mục tiêu
        Vector3 direction = (target.transform.position - firePoint.position).normalized;
        GameObject arrow = Instantiate(gameObject, firePoint.position, Quaternion.LookRotation(direction));

        // Truyền dữ liệu skill vào đạn
        PiercingArrow arrowScript = arrow.GetComponent<PiercingArrow>();
        if (arrowScript != null)
        {
            arrowScript.Init(skillData, direction);
        }
    }
}
