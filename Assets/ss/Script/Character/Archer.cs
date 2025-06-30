using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Archer : CharacterControllerInput
{
    [Header("Bullet")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public GameObject VFX;

    [Header("Camera")]
    public Transform cameraAimPoint;
    public Transform originalCameraParent;
    [Header("Aim")]
    [SerializeField]private GameObject AimObj;
    public Transform aimTarget; // target để xoay bằng chuột
    [SerializeField] GameObject Dot;
    private bool isAiming = false;
    public bool IsAiming => isAiming;

    private Vector3 originalAimTargetLocalPosition;
    private float aimYaw = 0f;
    private float aimPitch = 0f;
    [SerializeField]private float sensitivity = 0.5f;
    [SerializeField]private SkillEffectBehaviour until;


    protected override void Start()
    {
        Dot.SetActive(false);
        base.Start();
        AimObj.SetActive(false);
        originalAimTargetLocalPosition = aimTarget.localPosition;
        inputActions.Player.Aim.started += ctx => StartAiming();
        inputActions.Player.Aim.canceled += ctx => StopAiming();
        inputActions.Player.Attack.performed += ctx => FireArrow();
        inputActions.Player.Ulti.performed += ctx => Ultimate();
    }

    protected override void Update()
    {
        base.Update();

        if (isAiming)
        {
            HandleAimingWithRigTarget();
        }
    }

    private void StartAiming()
    {
        AimObj.SetActive(true);
        isAiming = true;

        if (animator == null)
        {
            Debug.LogError("Animator is NULL!");
            return;
        }
        Dot.SetActive(true);
        // ✅ Thêm dòng này để xoay nhân vật theo camera
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0f;
        if (cameraForward != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(cameraForward);

        // Disable camera script
        ThirdPersonCamera.instance.enabled = false;

        // Gắn camera vào vị trí ngắm
        mainCamera.transform.SetParent(cameraAimPoint);
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;

        animator.SetBool("IsAiming", true);

        Debug.Log("Aiming...");
    }

    private void StopAiming()
    {
        Dot.SetActive(false);
        AimObj.SetActive(false);
        isAiming = false;

        // Restore camera
        mainCamera.transform.SetParent(originalCameraParent);
        ThirdPersonCamera.instance.enabled = true;

        animator.SetBool("IsAiming", false);
        Debug.Log("Stopped Aiming.");

        // Đưa aimTarget về vị trí ban đầu
        aimTarget.localPosition = originalAimTargetLocalPosition;
    }

    private void Ultimate()
    {
            Debug.Log("Using Skill: " + until.skillData.skillName);
            until.UseSkill(GetNearestEnemy().gameObject);
    }

    private void FireArrow()
    {
        Vector3 direction;      // hướng bắn
        Quaternion rotation;    // quay của mũi tên

        if (isAiming)
        {
            /* NGUYÊN LOGIC CŨ
            – bắn theo tâm màn hình
            */
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.51f, 0.51f));
            Vector3 targetPoint = ray.origin + ray.direction * 100f;

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                targetPoint = hit.point;

            direction = (targetPoint - firePoint.position).normalized;
            rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            // 🔸 KHÔNG AIM → tự chọn enemy gần nhất
            Transform enemy = GetNearestEnemy();
            if (enemy == null) return;             // Không có địch thì thôi

            // Xoay nhân vật chỉ theo trục Y
            Vector3 flatDir = enemy.position - transform.position;
            flatDir.y = 0f;
            if (flatDir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(flatDir);
                // Quay thêm 90 độ quanh trục Y (tùy model quay thiếu hay dư)
                transform.rotation = lookRotation * Quaternion.Euler(0f, 90f, 0f);
            }

            // Hướng bắn thẳng vào thân địch (nâng nhẹ Y cho tự nhiên)
            direction = (enemy.position + Vector3.up * 1.2f - firePoint.position).normalized;
            rotation = Quaternion.LookRotation(direction);
        }

        // Gọi VFX + Arrow
        Instantiate(VFX, firePoint.position, rotation);
        Instantiate(arrowPrefab, firePoint.position, rotation);

        animator.SetTrigger("Fire");
        Debug.DrawRay(firePoint.position, direction * 30f, Color.red, 1.5f);
    }


    // Tùy ý giới hạn tầm dò nếu muốn (không bắt buộc)
    private Transform GetNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minSqr = Mathf.Infinity;
        Vector3 myPos = transform.position;

        foreach (GameObject e in enemies)
        {
            float sqr = (e.transform.position - myPos).sqrMagnitude;
            if (sqr < minSqr)
            {
                minSqr = sqr;
                nearest = e.transform;
            }
        }
        return nearest;
    }
    private void HandleAimingWithRigTarget()
    {
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        float lookX = lookInput.x * sensitivity;
        float lookY = lookInput.y * sensitivity;

        aimYaw += lookX;
        aimPitch -= lookY;

        aimPitch = Mathf.Clamp(aimPitch, -90f, 90f);

        float maxRight = 0f;
        float maxLeft = -40f;

        if (aimYaw > maxRight)
        {
            float excess = aimYaw - maxRight;
            aimYaw = maxRight;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + excess, 0f);
        }
        else if (aimYaw < maxLeft)
        {
            float excess = aimYaw - maxLeft; // khác chỗ này!
            aimYaw = maxLeft;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + excess, 0f);
        }


        // Cập nhật vị trí Rig Target
        Vector3 direction = Quaternion.Euler(aimPitch, aimYaw, 0f) * Vector3.forward;
        Vector3 targetPosition = transform.position + transform.rotation * direction * 2f + Vector3.up * 1.5f;
        aimTarget.position = targetPosition;
    }

}
