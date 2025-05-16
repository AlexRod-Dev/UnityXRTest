using UnityEngine;

public class EditorStyleCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float boostMultiplier = 3f;
    public float mouseSensitivity = 3f;
    public float scrollSpeed = 5f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Input.GetMouseButton(1)) // Right mouse button held
        {
            // Mouse look
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);

            // Movement speed with optional shift boost
            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f);
            Vector3 move = new Vector3();

            // WASD movement (relative to camera)
            if (Input.GetKey(KeyCode.W)) move += transform.forward;
            if (Input.GetKey(KeyCode.S)) move -= transform.forward;
            if (Input.GetKey(KeyCode.A)) move -= transform.right;
            if (Input.GetKey(KeyCode.D)) move += transform.right;

            // Q/E for up/down
            if (Input.GetKey(KeyCode.Q)) move -= transform.up;
            if (Input.GetKey(KeyCode.E)) move += transform.up;

            transform.position += move.normalized * speed * Time.deltaTime;

            // Zoom with scroll wheel (move forward/back)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                transform.position += transform.forward * scroll * scrollSpeed * 100f * Time.deltaTime;
            }
        }
    }
}