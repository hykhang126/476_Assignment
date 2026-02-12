using UnityEngine;

namespace Utilities
{
    
// execute in edit mode so that the visualization is visible in the Scene view when the game is not running
[ExecuteInEditMode]
public class VisualizeNavigation : MonoBehaviour
{
    [SerializeField] bool visualizeNavigationRays = true;
    [SerializeField] float rayLength = 5.0f;

    // Update is called once per frame
    void Update()
    {
        if (visualizeNavigationRays) VisualizeNavigationRays();
    }

    private void VisualizeNavigationRays()
    {
        // draw a v-shaped ray in the forward direction of the agent, with a small offset in the x-axis
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Vector3 direction = transform.forward * rayLength;
        // tilt the ray direction to the left and right by 15 degrees
        Vector3 leftDirection = Quaternion.Euler(0, -15, 0) * direction;
        Vector3 rightDirection = Quaternion.Euler(0, 15, 0) * direction;
        Debug.DrawRay(origin, leftDirection, Color.green);
        Debug.DrawRay(origin, rightDirection, Color.green); 
    }
}

}