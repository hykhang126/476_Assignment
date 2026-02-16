using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using AI;

public class GameManager : MonoBehaviour
{
    // SINGLETON PATTERN
    public static GameManager Instance;

    public bool hasGameStarted = false;

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

    [Header("UI settings")]
    public TMP_Text coverCountText;
    public TMP_Text gameStateText;
    public TMP_Text pointsText;
    public int playerPoints = 0;

    [Header("Broadcast Events")]
    public GenericEvent onGameStart;
    public GenericEvent onFlockRelease;
    public GenericEvent onFlockRelease1;
    public GenericEvent onFlockRelease2;
    public GenericEvent onFlockRelease3;
    
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

        if (coverCountText == null || gameStateText == null || pointsText == null)
        {
            Debug.LogError("One or more UI Text references are not assigned in the GameManager.");
        }

        currentCoverCount = 0;
        playerPoints = 0;
        coverCountText.text = $"Cover: {currentCoverCount}/{maxCoverCount}";
        gameStateText.text = $"Place {maxCoverCount} covers!";
        pointsText.text = $"Points: {playerPoints}";
    }

    public void StartGame()
    {
        if (hasGameStarted) return;
        Debug.Log("Game Started!");
        hasGameStarted = true;
        onGameStart.Invoke();

        StartCoroutine(ReleaseFlockCoroutine(10f));

        gameStateText.text = "Game Started! Flock 1 released!";
    }

    public void PlaceCover()
    {
        if (hasGameStarted) return;

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
            coverCountText.text = $"Cover: {currentCoverCount}/{maxCoverCount}";
        }
    }

    public void RemoveCover()
    {
        if (hasGameStarted) return;

        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Cover")))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("PlayerCover")) Destroy(hit.collider.gameObject);
            currentCoverCount--;
            coverCountText.text = $"Cover: {currentCoverCount}/{maxCoverCount}";
        }
    }

    private System.Collections.IEnumerator ReleaseFlockCoroutine(float delayBetweenFlocks = 8f)
    {
        onFlockRelease.Invoke();

        yield return new WaitForSeconds(delayBetweenFlocks);
        onFlockRelease1.Invoke();
        gameStateText.text = "Flock 2 released!";

        yield return new WaitForSeconds(delayBetweenFlocks);
        onFlockRelease2.Invoke();
        gameStateText.text = "Flock 3 released!";

        yield return new WaitForSeconds(delayBetweenFlocks);
        onFlockRelease3.Invoke();
        gameStateText.text = "Flock 4 released!";
    }

    public void IncreasePlayerPoints(int amount)
    {
        playerPoints += amount;
        pointsText.text = $"Points: {playerPoints}";
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IncreasePlayerPoints(10);
            if (other.TryGetComponent<AIAgent>(out var agent))
            {
                agent.agentDiedEvent.Invoke(agent);
            }
        }
    }
}
