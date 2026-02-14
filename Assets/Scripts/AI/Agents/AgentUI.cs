using UnityEngine;
using UnityEngine.UI;

using AI;
using TMPro;

[RequireComponent(typeof(AIAgent))]
public class AgentUI : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text agentStatusText;
    [SerializeField] private TMP_Text agentNameText;
    [SerializeField] private AIAgent agent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (healthBar == null)
        {
            Debug.LogWarning("AgentUI: No health bar assigned. Finding one in children.");
            healthBar = GetComponentInChildren<Slider>();
        }

        if (agent == null)        
        {
            Debug.LogWarning("AgentUI: No AIAgent found in hierarchy. Grabbing from the same GameObject.");
            agent = GetComponent<AIAgent>();
        }

        if (agentNameText != null)
        {
            agentNameText.text = agent.gameObject.name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (agent != null)
        {
            healthBar.value = agent.health / 100f;
            agentStatusText.text = agent.currentState.ToString();
        }
    }
}
