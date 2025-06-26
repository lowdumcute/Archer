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
    private bool isAiming = false;
    public bool IsAiming => isAiming;

    private Vector3 originalAimTargetLocalPosition;
    private float aimYaw = 0f;
    private float aimPitch = 0f;
    private float sensitivity = 0.5f;


    protected override void Start()
    {
        base.Start();
        AimObj.SetActive(false);
        originalAimTargetLocalPosition = aimTarget.localPosition;
        inputActions.Player.Aim.started += ctx => StartAiming();
        inputActions.Player.Aim.canceled += ctx => StopAiming();
        inputActions.Player.Attack.performed += ctx => FireArrow();
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


    private void FireArrow()
    {
        if (!isAiming) return;

        if (arrowPrefab != null && firePoint != null)
        {
            // Ray từ giữa màn hình
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.51f, 0.51f));
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.origin + ray.direction * 100f;
            }

            // Hướng từ firePoint đến targetPoint
            Vector3 direction = (targetPoint - firePoint.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);

            // Gọi mũi tên ra
            Instantiate(VFX, firePoint.position, rotation);
            Instantiate(arrowPrefab, firePoint.position, rotation);

            // Vẽ ray để căn chỉnh trong Scene View
            Debug.DrawRay(firePoint.position, direction * 100f, Color.red, 2f); // Đường màu đỏ, tồn tại 2 giây
        }

        animator.SetTrigger("Fire");
        Debug.Log("Arrow fired!");
    }



    private void HandleAimingWithRigTarget()
    {
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        float lookX = lookInput.x * sensitivity;
        float lookY = lookInput.y * sensitivity;

        aimYaw += lookX;
        aimPitch -= lookY;

        aimPitch = Mathf.Clamp(aimPitch, -90f, 90f);

        // Nếu vượt quá 90f → xoay nhân vật và giữ aimYaw tại 90
        if (aimYaw > 90f)
        {
            float excess = aimYaw - 90f;
            aimYaw = 90f;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + excess, 0f);
        }
        // Nếu nhỏ hơn -75f → xoay nhân vật và giữ aimYaw tại -75
        else if (aimYaw < -75f)
        {
            float excess = aimYaw + 75f; // aimYaw âm
            aimYaw = -75f;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + excess, 0f);
        }

        // Tính hướng mới cho aimTarget
        Vector3 direction = Quaternion.Euler(aimPitch, aimYaw, 0f) * Vector3.forward;
        Vector3 targetPosition = transform.position + transform.rotation * direction * 2f + Vector3.up * 1.5f;
        aimTarget.position = targetPosition;
    }





}
