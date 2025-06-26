using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerInput : MonoBehaviour
{
    protected InputSystem inputActions;
    protected Vector2 moveInput;
    public float moveSpeed = 5f;

    private CharacterController controller;
    public Camera mainCamera;
    [HideInInspector]public Animator animator;

    protected virtual void Awake()
    {
        inputActions = new InputSystem();
    }

    protected virtual void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();

        inputActions.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
    }

    protected virtual void Update()
    {
        Move();
    }

    protected virtual void Move()
    {
        if (moveInput == Vector2.zero)
        {
            animator?.SetFloat("Speed", 0f);
            return;
        }

        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = mainCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;
        moveDir.Normalize();

        // Di chuyển bằng CharacterController
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // Gán animation
        animator?.SetFloat("Speed", 1f);

        // 🔥 CHỈ xoay khi KHÔNG ngắm
        if (moveDir != Vector3.zero && !(this is Archer archer && archer.IsAiming))
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
    }

    protected virtual void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    protected virtual void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
}
