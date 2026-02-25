using UnityEngine;

namespace DiasGames
{
    public interface IMovement
    {
        Vector3 Velocity { get; }
        public float CurrentMoveSpeed { get; }
        bool IsFalling { get; }
        float MaxSpeed { get; }
        float Gravity { get; }
        bool GravityEnabled { get; }
        float CapsuleHeight  { get; }
        float CapsuleRadius { get; }
        LayerMask GroundLayerMask { get; }
        event System.Action OnLanded;
        event System.Action OnStartFalling;
        void AddMoveInput(Vector2 moveInput);
        void SetMaxMoveSpeed(float newMaxMoveSpeed);
        void Jump(float jumpPower);
        void MoveByInput();
        void Move(Vector3 velocity);
        void StopMovement();
        Quaternion GetRotationFromDirection(Vector3 direction);
        Vector3 GetWorldDirectionInput();
        void EnableCollision();
        void DisableCollision();
        void SetCapsuleSize(float newHeight, float newRadius);
        void ResetCapsuleSize();
        void SetGravityEnabled(bool gravityEnabled);
        void RotateToMovementDirection(float rotationControl = 1f);
        void RotateToViewDirection();
        void SetPosition(Vector3 position);
        void SetRotation(Quaternion rotation);
    }
}