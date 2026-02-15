using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // SINGLETON PATTERN
    public static GameManager Instance;

    [Header("Cover Settings")]
    public GameObject coverPrefab;
    [Min(1)]
    public int maxCoverCount = 12;
    [Min(0)]
    public int currentCoverCount = 0;

    [Header("Broadcast Events")]
    public GenericEvent onGameStart;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void StartGame()
    {
        Debug.Log("Game Started!");
        onGameStart.Invoke();
    }

    public void PlaceCover()
    {
        if (currentCoverCount >= maxCoverCount)
        {
            Debug.LogWarning("Maximum cover count reached. Cannot place more cover.");
            return;
        }
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, LayerMask.GetMask("Ground")))
        {
            Instantiate(coverPrefab, hit.point + Vector3.up * 1f, Quaternion.identity);
            currentCoverCount++;
        }
    }

    public void RemoveCover()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Cover")))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("PlayerCover")) Destroy(hit.collider.gameObject);
            currentCoverCount--;
        }
    }
}
