using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.XR;

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

        [Header("In Danger Settings")]
        public float health = 100f;
        public float damageAmount = 10f;
        public float inDangerSpeedMod = 2f;
        public float inDanderRayRangeMod = 5f;

        [Header("Collsion Avoidance Settings")]
        public float raylength = 2f;
        public float rayAngle = 15f;
        public LayerMask obstacleLayerMask;
        public float avoidanceForce = 20f;

        [Header("Target Tracking")]
        public Transform trackedTarget;
        public Transform flockTarget;
        public bool usePathFinding;
        public Pathfinder pathfinder;

        [Header("DEBUG: NO ASSIGNMENT")]
        public AIState currentState;
        public Cover currentCover;
        public Transform currentCoverTarget;
        public Vector3 avoidanceDirection;
        public List<GridGraphNode> currentPath;
        public bool hasInit = false;
        
        [Header("Broadcast Events")]
        public UnityEvent<AIAgent> agentDiedEvent;

        public Vector3 Velocity { get; set; }
        
        #region AI Target

        public Vector3 TargetPosition
        {
            get => trackedTarget != null ? trackedTarget.position : transform.position;
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
                    if (trackedTarget.TryGetComponent<AIAgent>(out var targetAgent))
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

        #endregion

        #region Unity

        public void Initialize()
        {
            currentState = AIState.Moving;

            if (usePathFinding && pathfinder == null)
            {
                pathfinder = FindFirstObjectByType<Pathfinder>();
                if (pathfinder == null)
                {
                    Debug.LogError("Pathfinding component not found in the scene. Please add a Pathfinding component to the scene or disable usePathFinding.");
                    usePathFinding = false;
                }
            }

            hasInit = true;
        }

        private void Start()
        {
            if (hasInit) return;
            Initialize();

            if (usePathFinding && pathfinder != null)
            {
                GeneratePathToTarget();
            }
        }

        private void Update()
        {
            HandleMovement();

            if (currentState == AIState.Moving)
            {
                // HandleCollisionAvoidance();
            }
            
            if (currentState == AIState.InDanger)
            {
                HandleFindCoverSphere();
            }

            if (currentState == AIState.SeekCover)
            {
                ArriveCoverTarget();
            }
            
            if (currentState == AIState.InCover)
            {    
                WaitToMove();
            }

            HealthUpdate();
        }
        #endregion

        #region AI Pathfinding

        public void GeneratePathToTarget()
        {
            GeneratePathToTarget(flockTarget);
        }

        private void GeneratePathToTarget(Transform target)
        {
            if (usePathFinding && pathfinder != null && target != null)
            {
                currentPath = pathfinder.GetAstarPathFromTransforms(transform, target);
                if (TryGetComponent<Pathfinding>(out var pathfindingComponent))
                {
                    pathfindingComponent.NewPathGenerated();
                }
            }
        }

        #endregion

        #region AI Movement
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
            // If in danger, apply a modifier to the velocity to make the agent move faster
            float speedMod = 1f;
            if (currentState == AIState.InDanger || currentState == AIState.SeekCover)
            {
                speedMod = inDangerSpeedMod;
            }
            else if (currentState == AIState.InCover)
            {
                speedMod = 0f;
            }
            transform.position += speedMod * Time.deltaTime * Velocity;
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

        #endregion

        #region AI State
        public void SetState(AIState newState)
        {
            if (newState == currentState)
                return;

            // if state changed from InCover to something else, reset cover occupancy
            if (currentState == AIState.InCover)
            {
                if (currentCover != null)
                {
                    currentCover.RemoveCoverOccupant(this);
                    currentCover = null;
                }
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
        
        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0f)
            {
                GameManager.Instance.Announce($"{this.name} has made a blunder.");
                if (currentCover != null)
                {
                    currentCover.RemoveCoverOccupant(this);
                    currentCover = null;
                }
                agentDiedEvent.Invoke(this);
            }
        }

        public void Heal(float amount)
        {
            health += amount * 2f;
            health = Mathf.Clamp(health, 0f, 100f);
        }

        private void ArriveCoverTarget()
        {
            // If we are close to the cover, stops and wait for squad command
            float arriveDistance = 0.5f;

            // Make sure agent is not behind the cover target. If so, return
            Vector3 toCoverTarget = currentCoverTarget.position - transform.position;
            Physics.Raycast(transform.position + Vector3.up * 0.1f, toCoverTarget.normalized, out RaycastHit hit, toCoverTarget.magnitude, LayerMask.GetMask("Cover"));
            if (hit.collider != null)
            {
                return;
            }
            else if (Vector3.Distance(TargetPosition, currentCoverTarget.position) < 2f)
            {
                TrackTarget(currentCoverTarget);
            }

            if (Vector3.Distance(transform.position, TargetPosition) <= arriveDistance)
            {
                Velocity = Vector3.zero;
                SetState(AIState.InCover);
            }
        }

        private void WaitToMove()
        {
            UnTrackTarget();
            // Stay in cover until health is fully restored, then switch to moving state
            if (health >= 100f)
            {
                SetState(AIState.Moving);
                GeneratePathToTarget();
            }
        }

        #endregion

        #region AI Collision
        private void HandleCollisionAvoidance()
        {
            // Draw two rays in a v shape in the forward direction of the agent, with a small offset in the x-axis
            // if either ray hits an obstacle, apply a steering force to avoid the obstacle
            // if it hit a cover object, try to seek cover
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            Vector3 rayDirection = inDanderRayRangeMod * raylength * transform.forward;
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
                else if (rightRayHit)
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

        public void HandleFindCoverSphere()
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            Collider[] hitColliders = new Collider[50];
            float rayLengthMod = (currentState == AIState.InDanger) ? inDanderRayRangeMod : 1f;

            // Cast a sphere around the agent to find nearby cover objects
            Physics.OverlapSphereNonAlloc(rayOrigin, raylength * rayLengthMod, hitColliders, obstacleLayerMask);

            // Remove null colliders from the array
            hitColliders = hitColliders.Where(collider => collider != null).ToArray();

            // Sorted the hit colliders by distance to the agent
            System.Array.Sort(hitColliders, (a, b) => 
                Vector3.Distance(rayOrigin, a.transform.position).CompareTo(Vector3.Distance(rayOrigin, b.transform.position)));
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Cover") || hitCollider.CompareTag("PlayerCover"))
                {
                    Cover cover = hitCollider.GetComponent<Cover>();
                    if (cover != null && cover.IsCoverAvailable())
                    {
                        if (cover.TryOccupyCover(this, out Transform coverTransform))
                        {
                            currentCover = cover;
                            currentCoverTarget = coverTransform;
                            SeekCover(coverTransform);
                            return;
                        }
                    }
                }
            }
        }

        public void SeekCover(Transform coverTransform)
        {
            SetState(AIState.SeekCover);
            if (usePathFinding && pathfinder != null)
            {
                GeneratePathToTarget(coverTransform);
            }
            else
            {
                TrackTarget(coverTransform);
            }
        }
        #endregion
    }


}