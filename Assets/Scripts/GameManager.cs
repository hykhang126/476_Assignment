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

    [Header("Flock Settings")]
    public Flock flock1;
    public Flock flock2;
    public Flock flock3;
    public Flock flock4;

    [Header("Broadcast Events")]
    public GenericEvent onGameStart;
    public GenericEvent onFlockRelease;

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

        if (coverPrefab == null)
        {
            Debug.LogError("Cover prefab is not assigned in the GameManager.");
        }

        if (flock1 == null || flock2 == null || flock3 == null || flock4 == null)
        {
            Debug.LogError("One or more Flock references are not assigned in the GameManager.");
        }
    }

    public void StartGame()
    {
        Debug.Log("Game Started!");
        onGameStart.Invoke();
        StartFlockSpawner();
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

    private void StartFlockSpawner()
    {
        StartCoroutine(ReleaseFlockCoroutine());
    }

    private System.Collections.IEnumerator ReleaseFlockCoroutine(float delayBetweenFlocks = 8f)
    {
        yield return new WaitForSeconds(delayBetweenFlocks);
        flock1.Initialize();
        onFlockRelease.Invoke();
        yield return new WaitForSeconds(delayBetweenFlocks);
        flock2.Initialize();
        onFlockRelease.Invoke();
        yield return new WaitForSeconds(delayBetweenFlocks);
        flock3.Initialize();
        onFlockRelease.Invoke();
        yield return new WaitForSeconds(delayBetweenFlocks);
        flock4.Initialize();
        onFlockRelease.Invoke();

        while (flock1.swarm.Count > 0 || flock2.swarm.Count > 0 || flock3.swarm.Count > 0 || flock4.swarm.Count > 0)
        {
            yield return new WaitForSeconds(delayBetweenFlocks);
            onFlockRelease.Invoke();
        }
    }
}
