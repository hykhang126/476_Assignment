using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GenericEvent", menuName = "Scriptable Objects/GenericEvent")]
public class GenericEvent : ScriptableObject
{
    public UnityEvent onEventRaised;

    public void Invoke()
    {
        onEventRaised.Invoke();
    }
}
