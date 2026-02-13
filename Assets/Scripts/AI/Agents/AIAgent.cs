using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    public enum AIState
    {
        Moving,
        InDanger,
        SeekCover,
        InCover,
    }

    public class AIAgent : MonoBehaviour
    {
        [Header("Agent Settings")]
        public float maxSpeed;
        public bool lockY = true;

        public enum EBehaviorType { Kinematic, Steering }
        public EBehaviorType behaviorType;

        public float health = 100f;
        public float damageAmount = 10f;

        [Header("Collsion Avoidance Settings")]
        public float raylength = 5f;
        public float rayAngle = 15f;
        public LayerMask obstacleLayerMask;
        public float avoidanceForce = 10f;

        // private Animator animator;

        [Header("Target Tracking")]
        public Transform trackedTarget;
        public Transform flockTarget;

        [Header("Events")]
        public UnityEvent<AIState> OnStateChange;
        public UnityEvent<AIAgent> agentDiedEvent;

        [Header("DEBUG: NO ASSIGNMENT")]
        public AIState currentState;
        private Cover currentCover;
        [Tooltip("The position the agent is trying to reach. Just a Vector3 presentation of tracked target.")]
        public Vector3 trackedTargetPosition;
        public Vector3 avoidanceDirection;

        #region Properties
        public Vector3 TargetPosition
        {
            get => trackedTarget != null ? trackedTarget.position : trackedTargetPosition;
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

        public void TrackTarget(Transform targetTransform)
        {
            trackedTarget = targetTransform;
        }
        
        public void UnTrackTarget()
        {
            trackedTarget = null;
        }

        public Vector3 Velocity { get; set; }

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0f)
            {
                Cover.RemoveCoverOccupant(this, currentCover);
                agentDiedEvent.Invoke(this);
            }
        }

        public void Heal(float amount)
        {
            health += amount;
            health = Mathf.Clamp(health, 0f, 100f);
        }

        #endregion

        #region Unity
        private void OnEnable()
        {
            OnStateChange.AddListener(HandleOnStateChange);           
        }

        private void OnDisable()
        {
            OnStateChange.RemoveListener(HandleOnStateChange);
        }

        private void Awake()
        {
            // Default state is always moving
            currentState = AIState.Moving;

            // animator = GetComponent<Animator>();
        }

        private void Update()
        {
            HandleTargetTracking();

            HandleMovement();

            if (currentState == AIState.Moving)
            {
                HandleCollisionAvoidance();
            }
            else if (currentState == AIState.InDanger)
            {
                HandleFindCover();
            }

            if (currentState == AIState.SeekCover) 
                ArriveCoverTarget();

            FixOrientation();

            HealthUpdate();

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

        private void FixOrientation()
        {
            if (lockY)
            {
                Vector3 pos = transform.position;
                pos.y = 0f; // MAGIC NUMBER
                transform.position = pos;
            }

            // Set the agent xz rotation to 0
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
        #endregion

        #region AI State
        public void SetState(AIState newState)
        {
            OnStateChange.Invoke(newState);
        }

        private void HandleOnStateChange(AIState newState)
        {
            if (newState == currentState)
                return;
            
            // if state changed from InCover to something else, reset cover occupancy
            if (currentState == AIState.InCover)
            {
                Cover.RemoveCoverOccupant(this, currentCover);
                currentCover = null;
            }

            currentState = newState;
        }

        // Health increase while InCover state, else lose health. InDanger when health is below 50%
        private void HealthUpdate()
        {
            if (currentState == AIState.InCover)
            {
                Heal(Time.deltaTime * damageAmount * 2f);
            }
            else
            {
                TakeDamage(Time.deltaTime * damageAmount);
                if (health < 50f && currentState == AIState.Moving)
                {
                    SetState(AIState.InDanger);
                }
            }
        }

        private void ArriveCoverTarget()
        {
            // If we are close to the target, stops and wait for squad command
            float arriveDistance = 1f;
            if (Vector3.Distance(transform.position, TargetPosition) <= arriveDistance)
            {
                Velocity = Vector3.zero;
                SetState(AIState.InCover);
            }
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
            
            // Apply avoidance force if obstacle detected
            if (leftRayHit || rightRayHit)
            {
                // Simple avoidance: steer away from the obstacle
                avoidanceDirection = Vector3.zero;
                if (leftRayHit)
                {
                    avoidanceDirection += transform.right; // steer right
                }
                if (rightRayHit)
                {
                    avoidanceDirection -= transform.right; // steer left
                }
                avoidanceDirection.Normalize();

                // Apply avoidance force
                Velocity += avoidanceForce * Time.deltaTime * avoidanceDirection;
            }
            
        }

        #endregion

        #region AI Cover Seeking

        public void HandleFindCover()
        {
            float coverRayLengthMod = 3f;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            Vector3 rayDirection = transform.forward * raylength * coverRayLengthMod;
            Vector3 leftRayDirection = Quaternion.Euler(0, -rayAngle, 0) * rayDirection;
            Vector3 rightRayDirection = Quaternion.Euler(0, rayAngle, 0) * rayDirection;


            bool leftRayHit = Physics.Raycast(rayOrigin, leftRayDirection, out RaycastHit leftHit, raylength, obstacleLayerMask);
            bool rightRayHit = Physics.Raycast(rayOrigin, rightRayDirection, out RaycastHit rightHit, raylength, obstacleLayerMask);

            // if ray hits a cover, do cover occupy logic. Else apply avoidance force
            if (CoverCollision(leftRayHit, leftHit, rightRayHit, rightHit, out Transform coverTransform, out currentCover) 
                && currentState == AIState.InDanger)
            {
                SeekCover(coverTransform);
                // maybe restore health and update UI later
                return;
            }
        }

        private bool CoverCollision(bool leftRayHit, RaycastHit leftHit, bool rightRayHit, RaycastHit rightHit, out Transform coverTransform, out Cover currentCover)
        {
            coverTransform = null;
            currentCover = null;

            if (leftRayHit)
            {
                // Debug.Log("Left ray hit: " + leftHit.collider.name);
                // check if any hit is a cover object
                if (leftHit.collider.CompareTag("Cover"))
                {
                    // Try to seek cover
                    Cover cover = leftHit.collider.GetComponent<Cover>();
                    if (cover != null && cover.IsCoverAvailable())
                    {
                        currentCover = cover;
                        return cover.TryOccupyCover(this, out coverTransform);
                    }
                }
            }

            if (rightRayHit)
            {
                // Debug.Log("Right ray hit: " + rightHit.collider.name);
                // check if any hit is a cover object
                if (rightHit.collider.CompareTag("Cover"))
                {
                    // Try to seek cover
                    Cover cover = rightHit.collider.GetComponent<Cover>();
                    if (cover != null && cover.IsCoverAvailable())
                    {
                        return cover.TryOccupyCover(this, out coverTransform);
                    }
                }
            }

            return false;
        }

        public void SeekCover(Transform coverTransform)
        {
            TrackTarget(coverTransform);
            SetState(AIState.SeekCover);
        }
        #endregion
    }


}