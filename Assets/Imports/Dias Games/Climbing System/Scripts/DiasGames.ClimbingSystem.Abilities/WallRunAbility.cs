using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "WallRunAbility", menuName = "Dias Games/Abilities/WallRunAbility", order = 0)]
    public class WallRunAbility : Ability
    {
        [SerializeField] private LayerMask wallRunMask;
        [SerializeField] private float wallRunSpeed = 5f;
        [SerializeField] private float smoothnessTime = 0.1f;
        [SerializeField] private float offsetFromWall = 0.3f;
        [SerializeField] private float _abilityDuration = 1.0f;
        [Header("Animation")]
        [SerializeField] private string wallRunAnimState = "Wall Run";
        [SerializeField] private string mirrorBoolParameter = "Mirror";
        [Header("Audio")]
        [SerializeField] private AudioClipContainer _wallRunVocals;

        private WallRunTrigger _wall;
        private IAudioPlayer _audioPlayer;

        // smoothness positioning
        private Vector3 _startPos, _targetPos;
        private Quaternion _startRot, _targetRot;
        private float _weight;
        private float _step;

        private Animator Animator => AnimationController.Animator;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _audioPlayer = ownerSystem.GameObject.GetComponent<IAudioPlayer>();
        }

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }
            
            return Movement.IsFalling && Movement.Velocity.y > 1f && FoundWall();
        }
        
        protected override void OnStartAbility(GameObject instigator)
        {
            _weight = 0;
            _step = _abilityDuration / smoothnessTime;
            _startPos = transform.position;
            _startRot = transform.rotation;

            _targetPos = _wall.WallContact.position + _wall.WallContact.forward * offsetFromWall;
            _targetPos.y = transform.position.y;
            _targetRot = Quaternion.LookRotation(_wall.WallMoveDirection);

            Movement.SetGravityEnabled(false);

            AnimationController.SetAnimationState(wallRunAnimState);
            Animator.SetBool(mirrorBoolParameter, _wall.IsRightMove);
            _audioPlayer.PlayVoice(_wallRunVocals);
        }

        public override void UpdateAbility(float deltaTime)
        {
            Movement.Move(_wall.WallMoveDirection * wallRunSpeed);

            if(!Mathf.Approximately(_weight, 1f))
            {
                _weight = Mathf.MoveTowards(_weight, 1f, _step * Time.deltaTime);

                Movement.SetPosition(Vector3.Lerp(_startPos, _targetPos, _weight));
                Movement.SetRotation(Quaternion.Lerp(_startRot, _targetRot, _weight));

                return;
            }

            if (Time.time - StartTime > _abilityDuration)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            transform.rotation = Quaternion.Lerp(_startRot, _targetRot, 0.25f);
            Movement.SetGravityEnabled(true);
        }

        private bool FoundWall()
        {
            float radius = Movement.CapsuleRadius;
            Vector3 p1 = transform.position + Vector3.up * radius;
            Vector3 p2 = transform.position + Vector3.up *(Movement.CapsuleHeight - radius);

            foreach(var coll in Physics.OverlapCapsule(p1,p2, radius, wallRunMask, QueryTriggerInteraction.Collide))
            {
                if (coll.TryGetComponent(out _wall))
                {
                    // is character moving through wall move direction?
                    if (Vector3.Dot(_wall.WallContact.forward, transform.forward) < 0.5f &&
                        Vector3.Dot(_wall.WallMoveDirection, transform.forward) > 0.1f)
                        return true;
                }
            }

            return false;
        }
    }
}