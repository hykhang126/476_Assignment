using UnityEngine;

namespace AI
{
    public class AIAgent : MonoBehaviour
    {
        [Header("Agent Settings")]
        public float maxSpeed;
        public bool lockY = true;
        public bool debug;

        public enum EBehaviorType { Kinematic, Steering }
        public EBehaviorType behaviorType;

        [Header("Collsion Avoidance Settings")]
        public float raylength = 5f;
        public float rayAngle = 15f;
        public LayerMask obstacleLayerMask;

        // private Animator animator;

        [Header("Target Tracking")]
        public Transform trackedTarget;
        public Transform flockTarget;

        [Header("DEBUG: NO ASSIGNMENT")]
        [SerializeField] private AIState currentState;
        [SerializeField] private Vector3 targetPosition;

        #region Properties
        public Vector3 TargetPosition
        {
            get => trackedTarget != null ? trackedTarget.position : targetPosition;
        }
        public Vector3 TargetForward
        {
            get => trackedTarget != null ? trackedTarget.forward : Vector3.forward;
        }
        public Vector3 TargetVelocity
        {
            get
            {
                Vector3 v = Vector3.zero;
                if (trackedTarget != null)
                {
                    AIAgent targetAgent = trackedTarget.GetComponent<AIAgent>();
                    if (targetAgent != null)
                        v = targetAgent.Velocity;
                }

                return v;
            }
        }

        public Vector3 Velocity { get; set; }

        public void TrackTarget(Transform targetTransform)
        {
            trackedTarget = targetTransform;
        }
        
        public void UnTrackTarget()
        {
            trackedTarget = null;
        }
        #endregion

        #region Unity
        private void Awake()
        {
            // Default state is always moving
            currentState = AIState.Moving;

            // animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (debug)
            {
                Debug.DrawRay(transform.position, Velocity, Color.red);
                targetPosition = TargetPosition;
            }

            HandleTargetTracking();

            HandleMovement();

            HandleCollisionAvoidance();

            FixYPosition();

            // animator.SetBool("walking", Velocity.magnitude > 0);
            // animator.SetBool("running", Velocity.magnitude > maxSpeed/2);
        }
        #endregion

        #region AI Target
        private void HandleTargetTracking()
        {
            // State dependent. If Moving (flocking) then target the flock target, else target the tracked target
            if (currentState == AIState.Moving)
            {
                if (flockTarget != null)
                    TrackTarget(flockTarget);
            }
            else if (currentState == AIState.SeekCover || currentState == AIState.InCover)
            {
                // Stay on tracked target
            }
            else
            {
                if (trackedTarget != null)
                    TrackTarget(trackedTarget);
            }
        }
        #endregion

        #region AI movement
        // Movement handler method, for different AI behaviors
        private void HandleMovement()
        {
            if (behaviorType == EBehaviorType.Kinematic)
            {
                // TODO: average all kinematic behaviors attached to this object to obtain the final kinematic output and then apply it
				GetKinematicAvg(out Vector3 kinematicAvg, out Quaternion rotation);
                Velocity = kinematicAvg;
                transform.rotation = rotation;
            }
            else
            {
                // TODO: combine all steering behaviors attached to this object to obtain the final steering output and then apply it
				GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation);
                Velocity += steeringForceSum * Time.deltaTime;
                Velocity = Vector3.ClampMagnitude(Velocity, maxSpeed);
                transform.rotation *= rotation;
            }

            // apply velocity
            transform.position += Velocity * Time.deltaTime;
        }
        private void GetKinematicAvg(out Vector3 kinematicAvg, out Quaternion rotation)
        {
            kinematicAvg = Vector3.zero;
            Vector3 eulerAvg = Vector3.zero;
            AIMovement[] movements = GetComponents<AIMovement>();
            int count = 0;
            foreach (AIMovement movement in movements)
            {
                // Check if Agent should flock based on their State
                if (movement is Flocking flockAgent)
                {
                    if (currentState != AIState.Moving)
                        continue;
                }

                kinematicAvg += movement.GetKinematic(this).linear;
                eulerAvg += movement.GetKinematic(this).angular.eulerAngles;

                ++count;
            }

            if (count > 0)
            {
                kinematicAvg /= count;
                eulerAvg /= count;
                rotation = Quaternion.Euler(eulerAvg);
            }
            else
            {
                kinematicAvg = Velocity;
                rotation = transform.rotation;
            }
        }

        private void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;
            AIMovement[] movements = GetComponents<AIMovement>();
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).linear;
                rotation *= movement.GetSteering(this).angular;
            }
        }

        private void FixYPosition()
        {
            if (lockY)
            {
                Vector3 pos = transform.position;
                pos.y = 0f; // MAGIC NUMBER
                transform.position = pos;
            }
        }
        #endregion

        #region AI State
        public enum AIState
        {
            Moving,
            SeekCover,
            InCover,
        }
        public void SetState(AIState newState)
        {
            currentState = newState;
        }

        public void SeekCover(Transform coverTransform)
        {
            TrackTarget(coverTransform);
            SetState(AIState.SeekCover);
        }
        #endregion

        #region Collision Avoidance
        private void HandleCollisionAvoidance()
        {
            // Draw two rays in a v shape in the forward direction of the agent, with a small offset in the x-axis
            // if either ray hits an obstacle, apply a steering force to avoid the obstacle
            // if it hit a cover object, try to seek cover
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            Vector3 rayDirection = transform.forward * raylength;
            Vector3 leftRayDirection = Quaternion.Euler(0, -rayAngle, 0) * rayDirection;
            Vector3 rightRayDirection = Quaternion.Euler(0, rayAngle, 0) * rayDirection;


            bool leftRayHit = Physics.Raycast(rayOrigin, leftRayDirection, out RaycastHit leftHit, raylength, obstacleLayerMask);
            bool rightRayHit = Physics.Raycast(rayOrigin, rightRayDirection, out RaycastHit rightHit, raylength, obstacleLayerMask);

            if (leftRayHit)
            {
                // check if any hit is a cover object
                if (leftHit.collider.CompareTag("Cover"))
                {
                    // Try to seek cover
                    Cover cover = leftHit.collider.GetComponent<Cover>();
                    if (cover != null && cover.IsCoverAvailable())
                    {
                        if (cover.TryOccupyCover(this, out Transform coverTransform))
                        {
                            SeekCover(coverTransform);
                            return; // Exit to avoid applying avoidance force
                        }
                    }
                }
                // Debug.Log("Left ray hit: " + leftHit.collider.name);
                // Apply a steering force to the right
                Vector3 avoidanceForce = transform.right * maxSpeed;
                Velocity += avoidanceForce * Time.deltaTime;
            }

            if (rightRayHit)
            {
                // check if any hit is a cover object
                if (rightHit.collider.CompareTag("Cover"))
                {
                    // Try to seek cover
                    Cover cover = rightHit.collider.GetComponent<Cover>();
                    if (cover != null && cover.IsCoverAvailable())
                    {
                        if (cover.TryOccupyCover(this, out Transform coverTransform))
                        {
                            SeekCover(coverTransform);
                            return; // Exit to avoid applying avoidance force
                        }
                    }
                }
                // Debug.Log("Right ray hit: " + rightHit.collider.name);
                // Apply a steering force to the left
                Vector3 avoidanceForce = -transform.right * maxSpeed;
                Velocity += avoidanceForce * Time.deltaTime;
            }

            if (debug) VisualizeNavigationRays(raylength, rayAngle);
        }

        private void VisualizeNavigationRays(float length = 5.0f, float angle = 15.0f)
        {
            // draw a v-shaped ray in the forward direction of the agent, with a small offset in the x-axis
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            Vector3 direction = transform.forward * length;
            // tilt the ray direction to the left and right by 15 degrees
            Vector3 leftDirection = Quaternion.Euler(0, -angle, 0) * direction;
            Vector3 rightDirection = Quaternion.Euler(0, angle, 0) * direction;
            Debug.DrawRay(origin, leftDirection, Color.green);
            Debug.DrawRay(origin, rightDirection, Color.green); 
        }
        #endregion
    }


}