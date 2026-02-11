using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private bool isFreeCamMode = false;
    [SerializeField] private float moveSpeed = 5f;

    private PlayerInput playerInput;
    private Camera _camera;
    private Vector3 moveInput;

    void OnEnable()
    {
        _camera = GetComponent<Camera>();

        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Interact"].performed += OnInteract;
        playerInput.actions["Look"].performed += OnLook;
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
    }

    void OnDisable()
    {
        playerInput.actions["Interact"].performed -= OnInteract;
        playerInput.actions["Look"].performed -= OnLook;
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isFreeCamMode)
        {
            OnInteract(default);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isFreeCamMode)
        {
            transform.position += moveSpeed * Time.deltaTime * moveInput;
        }
    }

    // Freecam mode toggle
    public void OnInteract(InputAction.CallbackContext context)
    {
        isFreeCamMode = !isFreeCamMode;
        Cursor.lockState = isFreeCamMode ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = isFreeCamMode;
    }
    
    // Look is free look cam
    public void OnLook(InputAction.CallbackContext context)
    {
        if (!isFreeCamMode) return;
        Vector2 input = context.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, input.x, Space.World);
        transform.Rotate(Vector3.right, -input.y, Space.Self);
    }

    // Movement is free fly cam
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!isFreeCamMode) return;
        Vector2 input = context.ReadValue<Vector2>();

        // Use camera full 3D orientation for movement
        Vector3 forward = _camera.transform.forward;
        Vector3 right = _camera.transform.right;

        moveInput = (forward * input.y) + (right * input.x);
    }

}
