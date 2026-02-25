using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Components
{
    public enum JumpType
    {
        None, JumpWithPrediction, JumpToWallClimb
    }

    public class PredictedJumpComponent : MonoBehaviour
    {
        [SerializeField] float jumpRange = 15f;
        [SerializeField] float minAcceptableDistance = 1f;
        [SerializeField] private float _minimumHeight = 2f;
        [SerializeField][UnityTag] private List<string> acceptableTags = new() { "JumpDestination", "Teeter" };

        private LaunchComponent _launchComponent;
        private ClimbingComponent _climbingComponent;
        private IMovement _movement;

        private void Awake()
        {
            _launchComponent = GetComponent<LaunchComponent>();
            _climbingComponent = GetComponent<ClimbingComponent>();
            _movement = GetComponent<IMovement>();
        }

        public LaunchData CalculateLaunchData(Vector3 startPoint, Vector3 targetPoint, JumpParameters parameters)
        {
            return _launchComponent.CalculateLaunchData(startPoint, targetPoint, parameters);
        }

        public JumpType GetLaunchParameters(Vector3 desiredDirection, JumpParameters jumpParameters, out LaunchData launchInfo, float maxAcceptableAngle = 90f)
        {
            launchInfo = LaunchData.Empty();

            // First: Try to get a position for simple jump
            List<Transform> simpleJumpPossibilities = GetSimpleJumpLaunchDestination(desiredDirection);
            if(simpleJumpPossibilities != null && simpleJumpPossibilities.Count > 0)
            {
                for (int i = 0; i < simpleJumpPossibilities.Count; i++)
                {
                    var launchData = _launchComponent.CalculateLaunchData(transform.position, 
                        simpleJumpPossibilities[i].position, jumpParameters, JumpResultType.Highest);
                    if (launchData.foundSolution && transform.position.y - launchData.target.y <= _minimumHeight)
                    {
                        launchInfo = launchData;
                        return JumpType.JumpWithPrediction;
                    }
                }
            }

            // Second: if first attempt failed, try to find a ledge to grab
            // get possible destinations
            var climbDestinations = _climbingComponent.GetLedgesDestinationsForJump(jumpRange, desiredDirection, maxAcceptableAngle);
            if (climbDestinations.Count > 0)
            {
                for (int i = 0; i < climbDestinations.Count; i++)
                {
                    var launch = _launchComponent.CalculateLaunchData(_climbingComponent.CastOrigin,
                                                                        climbDestinations[i], jumpParameters);
                    if (launch.foundSolution &&
                        transform.position.y - launch.target.y <= _minimumHeight)
                    {
                        launchInfo = launch;
                        return IsReachableByNormalJump(launch, jumpParameters) ? JumpType.None : JumpType.JumpToWallClimb;
                    }
                }
            }

            // Nothing were found, make a simple jump
            return JumpType.None;
        }

        private bool IsReachableByNormalJump(LaunchData launchData, JumpParameters jumpParameters)
        {
            LaunchData normalJump = CalculateLaunchData(transform.position, launchData.target, jumpParameters);

            if (!normalJump.foundSolution)
            {
                return false;
            }

            Vector3 direction = (launchData.target - transform.position).GetNormal2D();
            Vector3 cp1 = launchData.target + Vector3.up * (_movement.CapsuleRadius + 0.5f) + direction * _movement.CapsuleRadius;
            Vector3 cp2 = cp1 + Vector3.up * (_movement.CapsuleHeight - _movement.CapsuleRadius * 2);

            return Physics.OverlapCapsule(cp1, cp2, _movement.CapsuleRadius, Physics.AllLayers,
                QueryTriggerInteraction.Ignore).Length == 0;
        }

        /// <summary>
        ///  Get a destination for a simple jump
        /// </summary>
        /// <returns></returns>
        public List<Transform> GetSimpleJumpLaunchDestination(Vector3 inputDirection, float maxAcceptableAngle = 45f)
        {
            var acceptableDot = Mathf.Cos(Mathf.Deg2Rad * maxAcceptableAngle);

            // FIRST STEP: overlap around player to find a possible point of destination and add all destinations to a list
            Collider[] aroundTargets = Physics.OverlapSphere(transform.position, jumpRange, Physics.AllLayers, QueryTriggerInteraction.Collide);
            List<Collider> possibleDestinations = new List<Collider>();
            foreach (var target in aroundTargets)
            {
                Vector3 targetPosition = target.transform.position;
                targetPosition.y = transform.position.y;
                if (acceptableTags.Contains(target.tag))
                {
                    if (Vector3.Distance(transform.position, targetPosition) > minAcceptableDistance)
                        possibleDestinations.Add(target);
                }
            }

            if (possibleDestinations.Count == 0) return null;

            List<JumpData> possibleJumps = new List<JumpData>();

            // SECOND STEP: loop through all possible destination and check if it's in the direction of jump
            // if so,add it to possible jumps
            foreach (var destination in possibleDestinations)
            {
                Vector3 jumpDirection = destination.transform.position - transform.position;
                jumpDirection.y = 0;
                jumpDirection.Normalize();

                float dot = Vector3.Dot(inputDirection, jumpDirection);

                if (dot > acceptableDot)
                {
                    possibleJumps.Add(new JumpData(dot, destination.transform));
                }
            }

            if (possibleJumps.Count == 0) return null;

            possibleJumps.Sort((x, y) => y.dot.CompareTo(x.dot));
            return possibleJumps.ConvertAll(x => x.target);
        }
    }
}