using UnityEngine;

namespace AI
{
    public class AIAgent : MonoBehaviour
    {
        public float maxSpeed;
        public bool lockY = true;
        public bool debug;

        public enum EBehaviorType { Kinematic, Steering }
        public EBehaviorType behaviorType;

        // private Animator animator;

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
            HandleStateChange();

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
        public void SetState(AIState newState)
        {
            currentState = newState;
        }

        private void HandleStateChange()
        {
            // TODO: handle any necessary logic when changing states, such as clearing targets, resetting timers, etc.
        }
        #endregion
    }

    public enum AIState
    {
        Moving,
        SeekCover,
        InCover,
    }
}