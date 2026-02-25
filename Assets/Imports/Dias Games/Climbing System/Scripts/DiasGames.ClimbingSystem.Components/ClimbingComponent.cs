using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Components
{
    public enum LedgeSelectionRule
    {
        Furthest, Closest
    }

    public struct ClimbCastData
    {
        public RaycastHit ForwardHit { get; set; }
        public RaycastHit TopHit { get; set; }
        public GameObject Ledge { get; set; }

        public bool HasLedge => Ledge != null;

        public ClimbCastData(RaycastHit forwardHit, RaycastHit topHit)
        {
            ForwardHit = forwardHit;
            TopHit = topHit;
            Ledge = topHit.collider.gameObject;
        }
    }
    
    public class ClimbingComponent : MonoBehaviour
    {
        private const float JumpBackHorizontalInterpDist = 1.5f;
        private const float LerpSpeed = 5.0f;

        [SerializeField] private LayerMask _climbableObjectMask;
        [SerializeField] private LayerMask _blockCharacterMask;
        [SerializeField] private Vector3 _braceOffsetFromLedge;
        [SerializeField] private Vector3 _hangOffsetFromLedge;
        [SerializeField][UnityTag] private string _ledgeTag = "Ledge";
        [SerializeField][UnityTag] private string _ladderTag = "Ladder";
        [SerializeField] private float _capsuleRadiusOnClimbing = 0.2f;

        [Header("Physics Casting")] 
        [SerializeField] private Transform _climbingCastOrigin;
        [SerializeField] private float _capsuleCastRadius = 0.25f;
        [SerializeField] private float _capsuleCastHeight = 1.0f;
        [SerializeField] private int _topIterations = 20;
        [SerializeField] private int _jumpCastIterations = 10;
        [SerializeField] private float _overlapRadius = 0.5f;
        [SerializeField] private float _maxForwardCastDistance = 1f;
        [SerializeField] private float _maxAngleAccpetance = 90.0f;
        [Header("Shimmy")] 
        [SerializeField] private float _shimmyEdgeLimit = 0.5f;
        [SerializeField] private int _shimmyEdgeDetectionIterations = 10;
        [SerializeField] private float _shimmyAngleAcceptance = 30;
        [SerializeField] private AudioClipContainer _shimmyClips;
        [Header("Corner")] 
        [SerializeField] private float _cornerOutDepth = 1.0f;

        [Header("Wall for feet detection")] 
        [SerializeField] private float _feetWallDetectionMinHeight = 0.1f;
        [SerializeField] private float _feetWallDetectionMaxHeight = 0.5f;

        private GameObject _blockedLedge;
        private float _blockDuration;
        private float _lastTimeBlocked;
        private const float DefaultBlockDuration = 0.1f;

        public GameObject CurrentLedge => _currentCastData.Ledge;
        public ClimbCastData CurrentCastData => _currentCastData;
        public Transform LedgeChild => _ledgeChildAux;
        public float CapsuleRadiusOnClimbing => _capsuleRadiusOnClimbing;
        public LayerMask ClimbableMask => _climbableObjectMask;
        public LayerMask BlockMask => _blockCharacterMask;
        public bool IsHanging => _hangingClimb;
        public Vector3 CurrentOffset => _hangingClimb ? _hangOffsetFromLedge : _braceOffsetFromLedge;
        public Vector3 CastOrigin => _climbingCastOrigin.position;

        private IMovement _movement;
        private Transform _ledgeChildAux;
        private Animator _animator;
        private bool _hangingClimb;
        private float _hangingWeight;
        private ClimbCastData _currentCastData;
        private int _hangWeightHash;
        private IAudioPlayer _audioPlayer;

        private void Awake()
        {
            _movement = GetComponent<IMovement>();
            _animator = GetComponent<Animator>();
            _audioPlayer = GetComponent<IAudioPlayer>();
            _ledgeChildAux = new GameObject("Ledge Child Aux (Don't destroy it!!)").transform;
            _hangingWeight = 0.0f;
            _hangWeightHash = Animator.StringToHash("HangWeight");
        }

        private void Update()
        {
            if (CurrentLedge != null)
            {
                _hangingClimb = !HasWallForFeet(GetCharacterPositionOnLedge(_currentCastData), -_currentCastData.ForwardHit.normal);
            }
            else
            {
                _hangingClimb = false;
            }

            _hangingWeight = Mathf.Lerp(_hangingWeight, IsHanging ? 1.0f : 0.0f, LerpSpeed * Time.deltaTime);
            _animator.SetFloat(_hangWeightHash, _hangingWeight);
        }

        public void SetCurrentLedge(ClimbCastData castData)
        {
            _currentCastData = castData;
        }

        public void BlockLedgeTemporaly(GameObject ledge, float duration = DefaultBlockDuration)
        {
            _blockedLedge = ledge;
            _blockDuration = duration;
            _lastTimeBlocked = Time.time;
        }

        private bool IsLedgeBlocked(GameObject ledge)
        {
            if (_blockedLedge == ledge)
            {
                return Time.time - _lastTimeBlocked < _blockDuration;
            }

            return false;
        }

        public bool HasCurrentLedge(out ClimbCastData result, string ledgeTag = "",
            bool isAlreadyClimbing = true, bool debug = false)
        {
            List<string> tags = new List<string>(1);
            if (!string.IsNullOrEmpty(ledgeTag))
            {
                tags.Add(ledgeTag);
            }

            result = new ClimbCastData();
            bool found = FindLedge(_climbingCastOrigin.position, _capsuleCastHeight,
                transform.forward, _maxForwardCastDistance, out List<ClimbCastData> results, tags,isAlreadyClimbing, debug: debug);

            if (found)
            {
                result = results[0];
            }

            return found;
        }

        public bool FindLedge(out ClimbCastData result)
        {
            result = new ClimbCastData();
            bool hasNearClimbable = Physics.CheckSphere(_climbingCastOrigin.position, _overlapRadius, _climbableObjectMask,
                QueryTriggerInteraction.Collide);

            if (!hasNearClimbable)
            {
                return false;
            }

            float angleStep = 360.0f / 20;
            List<ClimbCastData> allResults = new List<ClimbCastData>(20);
            for (int i = 0; i < 20; i++)
            {
                float currentAngleRad = i * angleStep * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(currentAngleRad), 0, Mathf.Sin(currentAngleRad)).normalized;
                bool found = FindLedge(_climbingCastOrigin.position, _capsuleCastHeight,
                    direction, _overlapRadius, out List<ClimbCastData> results,new List<string>(), allowCurrentLedge:false);

                if (!found)
                {
                    continue;
                }

                foreach (ClimbCastData castResult in results)
                {
                    float dot = Vector3.Dot(transform.forward, -castResult.ForwardHit.normal);
                    if (dot > Mathf.Cos(_maxAngleAccpetance * Mathf.Deg2Rad))
                    {
                        allResults.Add(castResult);
                    }
                }
            }
            
            allResults.Sort(SortCastResults);
            if (allResults.Count > 0)
            {
                result = allResults[0];
            }

            return result.HasLedge;
        }

        public bool FindLedge(Vector3 castOrigin, float capsuleHeight, Vector3 castDirection, float castDistance, 
            out List<ClimbCastData> results, List<string> ledgeTags, bool isAlreadyClimbing=false, 
            bool allowCurrentLedge = true, bool checkFreeToClimb=true, bool debug=false)
        {
            results = new List<ClimbCastData>();
            if (ledgeTags.Count == 0)
            {
                ledgeTags.Add(_ledgeTag);
                ledgeTags.Add("Untagged");
            }

            List<RaycastHit> forwardResults = GetForwardHits(_climbableObjectMask, castOrigin, capsuleHeight, 
                                                                castDirection, castDistance, ledgeTags, allowCurrentLedge: allowCurrentLedge, debug: debug);

            results = GetTopHits(forwardResults, _climbableObjectMask, capsuleHeight, isAlreadyClimbing, checkFreeToClimb, debug);

            return results.Count > 0;
        }

        public bool FindLedgeOnJumpDirection(Vector3 inputDirection, float maxJumpDistance,
            out ClimbCastData castData, LedgeSelectionRule selectionRule = LedgeSelectionRule.Furthest)
        {
            const float backOffset = 1.5f;
            Transform charTransform = transform;
            Vector3 charRight = charTransform.right;

            float jumpDistance = IsHanging ? maxJumpDistance * 0.5f : maxJumpDistance;
            float step = jumpDistance / _jumpCastIterations;
            float sideDirection = 0;
            float rightDot = Vector3.Dot(inputDirection, charRight);
            if (!Mathf.Approximately(rightDot, 0.0f))
            {
               sideDirection = rightDot > 0 ? 1.0f : -1.0f;
            }

            Vector3 defaultCastOrigin = _climbingCastOrigin.position - charTransform.forward * backOffset;
            List<ClimbCastData> possibleResults = new List<ClimbCastData>(20);
            int iterations = sideDirection != 0 ? _jumpCastIterations : 0;
            for (int i = 0; i <= iterations; i++)
            {
                Vector3 castOrigin = defaultCastOrigin + charTransform.right * (sideDirection * step * i);
                if (FindLedge(castOrigin, jumpDistance * 2.0f, charTransform.forward,
                        backOffset + _maxForwardCastDistance * 2.0f, out List<ClimbCastData> castResults,new List<string>()))
                {
                    possibleResults.AddRange(castResults);
                }
                if (FindLadder(castOrigin, jumpDistance * 2.0f, charTransform.forward,
                        backOffset + _maxForwardCastDistance * 2.0f, out ClimbCastData ladderResult, _ladderTag))
                {
                    possibleResults.Add(ladderResult);
                }
            }

            if (sideDirection != 0)
            {
                if (FindLedge(_climbingCastOrigin.position, jumpDistance * 2.0f, charRight * sideDirection,
                        backOffset + _maxForwardCastDistance * 2.0f, out List<ClimbCastData> castResults,new List<string>()))
                {
                    possibleResults.AddRange(castResults);
                }
            }

            Vector3 adjustedInputDirection = charRight * Vector3.Dot(inputDirection, charRight) +
                                             Vector3.up * Vector3.Dot(inputDirection, charTransform.forward);

            castData = GetBestCastData(possibleResults, adjustedInputDirection, 
                jumpDistance, Mathf.Cos(91.0f * Mathf.Deg2Rad),selectionRule);

            return castData.HasLedge;
        }

        public bool FindBackLedges(float maxHeight, float maxDistance, out List<ClimbCastData> castResults)
        {
            castResults = new List<ClimbCastData>(_jumpCastIterations);

            Vector3 origin = _climbingCastOrigin.position + transform.right * JumpBackHorizontalInterpDist * 0.5f;
            float step = JumpBackHorizontalInterpDist / _jumpCastIterations;
            for (int i = 0; i <= _jumpCastIterations; i++)
            {
                Vector3 start = origin - transform.right * (step * i);
                if (FindLedge(start, maxHeight, -transform.forward, maxDistance,
                        out List<ClimbCastData> results,new List<string>(), allowCurrentLedge: false))
                {
                    foreach (ClimbCastData result in results)
                    {
                        float dot = Vector3.Dot(result.ForwardHit.normal, transform.forward);
                        if (dot >= 0)
                        {
                            castResults.Add(result);
                        }
                    }
                }
            }

            return castResults.Count > 0;
        }
        
        public bool FindLadder(Vector3 castOrigin, float capsuleHeight, Vector3 castDirection, float castDistance, 
            out ClimbCastData result, string ladderTag, bool checkSides=false)
        {
            result = new ClimbCastData();
            List<string> tags = new List<string> { ladderTag };
            
            List<RaycastHit> forwardResults = GetForwardHits(_climbableObjectMask, castOrigin, capsuleHeight, 
                castDirection, castDistance, tags);

            if (forwardResults.Count == 0 && checkSides)
            {
               forwardResults = GetForwardHits(_climbableObjectMask, castOrigin, capsuleHeight, 
                    transform.right, castDistance, tags);

               if (forwardResults.Count == 0)
               {
                   forwardResults = GetForwardHits(_climbableObjectMask, castOrigin, capsuleHeight, 
                       -transform.right, castDistance, tags);
               }
            }
            
            foreach (RaycastHit fwdHit in forwardResults)
            {
                if (!fwdHit.collider.CompareTag(ladderTag))
                {
                    continue;
                }
                
                if (fwdHit.collider.TryGetComponent(out ILedge ledge))
                {
                    Transform closestPoint = ledge.GetClosestPoint(_climbingCastOrigin.position);

                    RaycastHit ladderHit = fwdHit;
                    ladderHit.point = closestPoint.position;
                    ladderHit.normal = closestPoint.forward;

                    ClimbCastData ladderResult = new ClimbCastData(ladderHit, ladderHit);
                    result = ladderResult;
                    return true;
                }
            }

            return false;
        }
        
        public bool FindLadder(out ClimbCastData result, string ladderTag)
        {
            return FindLadder(_climbingCastOrigin.position, _capsuleCastHeight, transform.forward, 
                _maxForwardCastDistance, out result, ladderTag, true);
        }

        public bool CanShimmy(float rightDirection)
        {
            if (Mathf.Approximately(rightDirection, 0.0f))
            {
                return true;
            }

            int unitDirection = rightDirection > 0 ? 1 : -1;
            Vector3 castOrigin = _climbingCastOrigin.position;
            float step = _shimmyEdgeLimit / _shimmyEdgeDetectionIterations;
            for (int i = 0; i < _shimmyEdgeDetectionIterations; i++)
            {
                Vector3 origin = castOrigin + transform.right * (unitDirection * step * i);
                if (FindLedge(origin, _capsuleCastHeight, transform.forward, _maxForwardCastDistance,
                        out List<ClimbCastData> castData,new List<string>(), true))
                {
                    ClimbCastData firstResult = castData[0];
                    if (firstResult.Ledge == CurrentLedge)
                    {
                        continue;
                    }

                    Quaternion targetRotation = GetCharacterRotationOnLedge(firstResult);
                    float dot = Quaternion.Dot(transform.rotation, targetRotation);
                    bool validShimmy = Mathf.Acos(dot) <= _shimmyAngleAcceptance * Mathf.Deg2Rad;
                    if (!validShimmy)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public bool FindCorner(Vector3 castDirection, out ClimbCastData castData)
        {
            castData = new ClimbCastData();

            // Corner In
            if (FindLedge(_climbingCastOrigin.position, _capsuleCastHeight, castDirection,
                    _maxForwardCastDistance, out List<ClimbCastData> cornerInResults, new List<string>()))
            {
                ClimbCastData firstResult = cornerInResults[0];
                float cornerDot = Vector3.Dot(transform.forward, firstResult.ForwardHit.normal);
                if (cornerDot >= -0.1f && cornerDot <= 0.1f)
                {
                    castData = cornerInResults[0];
                    return true;
                }
            }

            // Corner Out
            float acceptableDot = Mathf.Cos(30.0f * Mathf.Deg2Rad);
            Vector3 origin = _climbingCastOrigin.position + castDirection * (_maxForwardCastDistance + _shimmyEdgeLimit);
            origin += transform.forward * _cornerOutDepth;
            
            // Check for walls in the corner
            Vector3 capsulePoint01 = transform.position + Vector3.up * _movement.CapsuleRadius;
            Vector3 capsulePoint02 = capsulePoint01 + Vector3.up * (_movement.CapsuleHeight - _movement.CapsuleRadius * 2);

            if (Physics.CapsuleCast(capsulePoint01, capsulePoint02, _movement.CapsuleRadius,
                    castDirection, _maxForwardCastDistance, _blockCharacterMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }
            
            if (FindLedge(origin, _capsuleCastHeight, -castDirection, _maxForwardCastDistance + _shimmyEdgeLimit,
                    out List<ClimbCastData> cornerOutResults,new List<string>()))
            {
                ClimbCastData firstResult = cornerOutResults[0];
                if (firstResult.Ledge != CurrentLedge)
                {
                    float cornerDot = Vector3.Dot(transform.forward, firstResult.ForwardHit.normal);
                    if (cornerDot >= -acceptableDot && cornerDot <= acceptableDot)
                    {
                        castData = cornerOutResults[0];
                        return true;
                    }
                }
            }

            return false;
        }
        
        public bool CanClimbUp()
        {
            Vector3 startSphere = _climbingCastOrigin.position + transform.forward * (CurrentOffset.z + 1f) + Vector3.up * 2;
            CastDebug.Instance.DrawSphere(startSphere, 0.3f,  Color.yellow, 1f);

            if (Physics.SphereCast(startSphere, _movement.CapsuleRadius, Vector3.down, 
                    out RaycastHit hit, 2f, _movement.GroundLayerMask, QueryTriggerInteraction.Ignore))
            {
                CastDebug.Instance.DrawSphere(hit.point + Vector3.up * _movement.CapsuleRadius, _movement.CapsuleRadius,
                    Color.yellow, 1f);

                return !Physics.Raycast(hit.point, Vector3.up, 1f, _movement.GroundLayerMask, QueryTriggerInteraction.Ignore);
            }

            return false;
        }

        public ClimbCastData GetBestCastData(List<ClimbCastData> castDatas, Vector3 adjustedInputDirection, 
            float maxJumpDistance, float minAcceptableDot, LedgeSelectionRule selectionRule)
        {
            const float minLedgeDistance = 0.5f;
            Transform charTransform = transform;

            ClimbCastData bestResult = new ClimbCastData();
            float bestFactor = 0;
            foreach (ClimbCastData castData in castDatas)
            {
                Vector3 targetPosition = GetCharacterPositionOnLedge(castData);
                float distance = Vector3.Distance(targetPosition, charTransform.position);
                if (distance > maxJumpDistance)
                {
                    continue;
                }

                if (castData.Ledge == CurrentLedge && distance < minLedgeDistance)
                {
                    continue;
                }

                Vector3 directionToTarget = (targetPosition - charTransform.position).normalized;
                float dot = Vector3.Dot(adjustedInputDirection, directionToTarget);
                if (dot <= minAcceptableDot)
                {
                    continue;
                }

                float factor;
                if (selectionRule == LedgeSelectionRule.Closest)
                {
                    factor = dot / distance;
                }
                else
                {
                    factor = distance * dot + dot * 2.0f;
                }
                
                if (factor > bestFactor)
                {
                    bestFactor = factor;
                    bestResult = castData;
                }
            }

            return bestResult;
        }

        public List<RaycastHit> GetForwardHits(LayerMask climbableMask, Vector3 startCapsuleCenter, float capsuleHeight,
            Vector3 direction, float maxDistance, List<string> ledgeTags, float capsuleRadius = -1.0f, bool allowCurrentLedge = true, bool debug=false)
        {
            if (capsuleRadius < 0)
            {
                capsuleRadius = _capsuleCastRadius;
            }

            Vector3 capsulePoint01 = startCapsuleCenter + Vector3.up * (capsuleHeight * 0.5f - capsuleRadius) -
                                     direction * capsuleRadius;
            Vector3 capsulePoint02 = startCapsuleCenter + Vector3.down * (capsuleHeight * 0.5f - capsuleRadius) -
                                     direction * capsuleRadius;

            RaycastHit[] results = new RaycastHit[10];
            int collectionSize = Physics.CapsuleCastNonAlloc(capsulePoint01, capsulePoint02, capsuleRadius, direction,
                results, maxDistance, climbableMask, QueryTriggerInteraction.Collide);

            List<RaycastHit> forwardHits = new List<RaycastHit>(collectionSize);
            if (debug)
            {
                CastDebug.Instance.DrawCapsule(capsulePoint01, capsulePoint02, capsuleRadius, Color.yellow);
            }

            for (int i = 0; i < collectionSize; i++)
            {
                if (IsLedgeBlocked(results[i].collider.gameObject))
                {
                    continue;
                }
                
                if (!allowCurrentLedge && results[i].collider.gameObject == CurrentLedge)
                {
                    continue;
                }

                if (!ledgeTags.Contains(results[i].collider.tag))
                {
                   continue;
                }
                
                forwardHits.Add(results[i]);
                Vector3 deltaHit = direction * results[i].distance;
                if (debug)
                {
                    CastDebug.Instance.DrawCapsule(capsulePoint01 + deltaHit, 
                        capsulePoint02 + deltaHit, capsuleRadius, Color.green);
                }
            }

            if (forwardHits.Count == 0 && debug)
            {
                CastDebug.Instance.DrawCapsule(capsulePoint01 + direction * maxDistance,
                    capsulePoint02 + direction * maxDistance, capsuleRadius, Color.red);
            }
            
            return forwardHits;
        }

        public List<ClimbCastData> GetTopHits(List<RaycastHit> forwardResults, LayerMask climbableMask,
            float castHeight, bool isAlreadyClimbing = false, bool checkFreeToClimb=true, bool debug=false)
        {
            if (forwardResults.Count == 0)
            {
                return new List<ClimbCastData>();
            }

            List<ClimbCastData> resultDatas = new List<ClimbCastData>(forwardResults.Count);
            float step = _maxForwardCastDistance / _topIterations;
            for (var f = 0; f < forwardResults.Count; f++)
            {
                var fwdHit = forwardResults[f];
                if (fwdHit.distance == 0)
                {
                    continue;
                }

                Vector3 direction = isAlreadyClimbing ? transform.forward : -fwdHit.normal.GetNormal2D();
                Vector3 startCast = fwdHit.point + fwdHit.normal * (_maxForwardCastDistance * 0.5f);
                if (isAlreadyClimbing)
                {
                    startCast.y = CastOrigin.y + castHeight * 0.5f;
                }
                else
                {
                    startCast.y += castHeight - 0.1f;
                }

                for (int i = 0; i <= _topIterations; i++)
                {
                    Vector3 start = startCast + direction * (step * i);

                    RaycastHit[] topResults = new RaycastHit[10];
                    int collectionSize = Physics.RaycastNonAlloc(start, Vector3.down, topResults,
                        castHeight, climbableMask, QueryTriggerInteraction.Collide);


                    if (collectionSize == 0)
                    {
                        if (debug)
                        {
                            CastDebug.Instance.DrawLine(start, start + Vector3.down * castHeight, Color.red);
                        }

                        continue;
                    }

                    for (int j = 0; j < collectionSize; j++)
                    {
                        RaycastHit currentTopHit = topResults[j];
                        if (currentTopHit.collider != fwdHit.collider)
                        {
                            continue;
                        }

                        if (!isAlreadyClimbing)
                        {
                            if (fwdHit.collider.TryGetComponent(out ILedge ledge))
                            {
                                Transform closestPoint = ledge.GetClosestPoint(currentTopHit.point);
                                fwdHit.point = closestPoint.position;
                                Vector3 normal = closestPoint.forward;
                                normal.y = 0; // avoid weird behaviors in inclined ledges
                                fwdHit.normal = normal;
                                currentTopHit.point = fwdHit.point;
                            }
                        }
                        
                        float dot = Vector3.Dot(currentTopHit.normal, transform.forward);
                        const float acceptableDot = -0.35f;
                        if (dot < acceptableDot)
                        {
                            continue;
                        }

                        ClimbCastData result = new ClimbCastData(fwdHit, currentTopHit);
                        if (checkFreeToClimb && !IsFreeToClimb(result, _capsuleRadiusOnClimbing))
                        {
                            continue;
                        }

                        resultDatas.Add(result);
                        j = collectionSize; // breaks this internal loop
                        i = _topIterations+1; // breaks the external loop

                        if (debug)
                        {
                            CastDebug.Instance.DrawLine(start, currentTopHit.point, Color.green);
                        }
                    }
                }
            }

            return resultDatas;
        }

        public void SetCharacterTransformOnLedge(ClimbCastData climbCastData)
        {
            Vector3 position = GetCharacterPositionOnLedge(climbCastData);
            Quaternion rotation = GetCharacterRotationOnLedge(climbCastData);

            _movement.SetPosition(position);
            _movement.SetRotation(rotation);
        }

        public Vector3 GetCharacterPositionOnLedge(ClimbCastData climbCastData)
        {
            Vector3 position = climbCastData.ForwardHit.point +
                               climbCastData.ForwardHit.normal * CurrentOffset.z;
            position.y = climbCastData.TopHit.point.y + CurrentOffset.y;

            return position;
        }

        public Quaternion GetCharacterRotationOnLedge(ClimbCastData climbCastData)
        {
            Quaternion rotation = Quaternion.LookRotation(-climbCastData.ForwardHit.normal.GetNormal2D());
            return rotation;
        }

        private int SortCastResults(ClimbCastData x, ClimbCastData y)
        {
            if (x.TopHit.point.y >= y.TopHit.point.y)
            {
                return 1;
            }

            return -1;
        }
        
        public bool IsFreeToClimb(ClimbCastData castData, float capsuleRadius, float capsuleHeight = 1.8f)
        {
            Vector3 targetPosition = GetCharacterPositionOnLedge(castData);

            Vector3 cp1 = targetPosition + Vector3.up * capsuleRadius;
            Vector3 cp2 = targetPosition + Vector3.up * (capsuleHeight - capsuleRadius);

            bool free = !Physics.CheckCapsule(cp1, cp2, capsuleRadius,
                _blockCharacterMask, QueryTriggerInteraction.Ignore);

            if (!free)
            {
                CastDebug.Instance.DrawCapsule(cp1, cp2, capsuleRadius, Color.red);
            }

            return free;
        }

        public bool IsFreeToClimb(Transform targetPoint, float capsuleRadius, float capsuleHeight = 1.8f)
        {
            ClimbCastData castData = new()
            {
                ForwardHit = new RaycastHit { normal = targetPoint.forward, point = targetPoint.position },
                TopHit = new RaycastHit { normal = Vector3.up, point = targetPoint.position }
            };

            return IsFreeToClimb(castData, capsuleRadius, capsuleHeight);
        }

        public List<Vector3> GetLedgesDestinationsForJump(float maxLedgesDistance, Vector3 desiredDirection, float maxAcceptableAngle = 90f)
        {
            float allowedDot = Mathf.Cos(Mathf.Deg2Rad * maxAcceptableAngle);
            List<ILedge> ledges = GetCloseLedges(maxLedgesDistance);

            if (ledges.Count == 0) return new List<Vector3>();

            List<JumpData> possiblePoints = new List<JumpData>();
            foreach (ILedge ledge in ledges)
            {
                List<Transform> points = ledge.GrabPoints;

                foreach (Transform point in points)
                {
                    // avoid calculate movement for ledges that are climbable in oposite direction
                    if (Vector3.Dot(-point.forward, desiredDirection) < allowedDot) continue;

                    Vector3 direction = (point.position - CastOrigin).GetNormal2D();

                    // dot for direction of movement and desired input direction
                    float dot = Vector3.Dot(direction, desiredDirection);

                    // dot product to avoid climb ledge in the same direction of movement
                    float pointDot = Vector3.Dot(direction, point.forward);

                    if (dot > 0.65f && pointDot < 0.05f)
                    {
                        if (IsFreeToClimb(point, _capsuleRadiusOnClimbing))
                        {
                            possiblePoints.Add(new JumpData(dot, point));
                        }
                    }
                }
            }

            possiblePoints.Sort((x, y) => y.GetSortFactor().CompareTo(x.GetSortFactor()));

            return possiblePoints.ConvertAll(x => x.target.position);
        }

        private List<ILedge> GetCloseLedges(float maxRange)
        {
            List<ILedge> ledges = new List<ILedge>();
            foreach(var coll in Physics.OverlapSphere(CastOrigin, maxRange, _climbableObjectMask, QueryTriggerInteraction.Collide))
            {
                if(coll.TryGetComponent(out ILedge ledge))
                {
                    ledges.Add(ledge);
                }
            }

            return ledges;
        }

        private bool HasWallForFeet(Vector3 origin, Vector3 direction)
        {
            const float capsuleRadius = 0.1f;
            Vector3 cp1 = origin + Vector3.up * (_feetWallDetectionMinHeight + capsuleRadius); 
            Vector3 cp2 = origin + Vector3.up * (_feetWallDetectionMaxHeight - capsuleRadius);
            LayerMask masks = _climbableObjectMask | _blockCharacterMask;
            
            CastDebug.Instance.DrawCapsule(cp1, cp2, capsuleRadius, Color.white);
            CastDebug.Instance.DrawCapsule(cp1 + direction * (CurrentOffset.z + 0.2f), 
                cp2 + direction * (CurrentOffset.z + 0.2f),
                capsuleRadius, Color.white);
            
            if (Physics.CapsuleCast(cp1, cp2, capsuleRadius, direction,
                    CurrentOffset.z + 0.2f, masks, QueryTriggerInteraction.Collide))
            {
                return true;
            }
            
            return false;
        }
        
        #region Animation Events

        // Called by shimmy animations event
        private void PlayShimmy()
        {
            _audioPlayer.PlayEffect(_shimmyClips);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            if (_climbingCastOrigin == null)
            {
                return;
            }

            Color color = Color.blue;
            if (Application.isPlaying)
            {
                CastDebug.Instance.DrawSphere(_climbingCastOrigin.position, _overlapRadius, color);
            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(_climbingCastOrigin.position, _overlapRadius);
            }
        }
    }
}