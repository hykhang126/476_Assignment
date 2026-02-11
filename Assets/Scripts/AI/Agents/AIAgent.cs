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

        [Header("DEBUG: NO ASSIGNMENT")]
        [SerializeField] private Vector3 targetPosition;
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

        private void Awake()
        {
            // animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (debug)
                Debug.DrawRay(transform.position, Velocity, Color.red);

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

            // animator.SetBool("walking", Velocity.magnitude > 0);
            // animator.SetBool("running", Velocity.magnitude > maxSpeed/2);
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
    }
}