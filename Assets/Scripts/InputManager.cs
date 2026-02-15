using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager: MonoBehaviour
{
    PlayerInput playerInput;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        // Enable playr input Player map
        playerInput.SwitchCurrentActionMap("Player");

        playerInput.actions["Attack"].performed += StartGame;
        playerInput.actions["Interact"].performed += PlaceCover;
        playerInput.actions["Remove"].performed += RemoveCover;
    }

    void StartGame(InputAction.CallbackContext context)
    {
        GameManager.Instance.StartGame();
    }

    void PlaceCover(InputAction.CallbackContext context)
    {
        GameManager.Instance.PlaceCover();
    }

    void RemoveCover(InputAction.CallbackContext context)
    {
        GameManager.Instance.RemoveCover();
    }
}
