    using UnityEngine;


    public class ThirdPersonCamera : MonoBehaviour
    {
        public static ThirdPersonCamera instance;
        public Transform target;
        public Vector3 offset = new Vector3(0, 3, -6);
        
        public float sensitivity = 3f;     // độ nhạy chuột
        public float smoothTime = 0.1f;

        private Vector3 currentVelocity;
        private float yaw = 0f;
        private float pitch = 10f;         // xoay trục X (nhìn lên/xuống)
        private float pitchMin = -30f;
        private float pitchMax = 60f;

        private InputSystem inputActions;
        private Vector2 lookInput;

        private void Awake()
        {
            instance = this;
            inputActions = new InputSystem();
            inputActions.Enable();
            inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
            // Khóa con trỏ và ẩn nó
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            if (target == null) return;

            // Lấy input chuột
            yaw += lookInput.x * sensitivity;
            pitch -= lookInput.y * sensitivity;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

            // Tính hướng xoay camera
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredPosition = target.position + rotation * offset;

            // Di chuyển mượt tới vị trí camera
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }
