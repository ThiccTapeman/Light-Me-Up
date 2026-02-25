using UnityEngine;

namespace DiasGames.Components
{
    public class CameraController : MonoBehaviour
    {
        private const float InputThreshold = 0.01f;
        
        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        [SerializeField] private GameObject _cinemachineCameraTarget;
        
        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float _topClamp = 70.0f;
        
        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] private float _bottomClamp = -30.0f;
        
        [Tooltip("Speed of camera turn by Controller")]
        [SerializeField] private Vector2 _controllerCameraTurnSpeed = new Vector2(200.0f, 200.0f);
        
        [Tooltip("Speed of camera turn by Mouse")]
        [SerializeField] private Vector2 _mouseCameraTurnSpeed = new Vector2(0.1f, 0.1f);

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        private void Awake()
        {
            _cinemachineTargetYaw = _cinemachineCameraTarget.transform.eulerAngles.y;
        }

        public void CameraRotation(Vector2 lookInput, float deltaTime)
        {
            if (lookInput.sqrMagnitude >= InputThreshold)
            {
                _cinemachineTargetYaw += lookInput.x * _controllerCameraTurnSpeed.x * deltaTime;
                _cinemachineTargetPitch += lookInput.y * _controllerCameraTurnSpeed.y * deltaTime;
            }

            ApplyRotation();
        }

        public void CameraRotationByMouse(Vector2 lookInput)
        {
            if (lookInput.sqrMagnitude >= InputThreshold)
            {
                _cinemachineTargetYaw += lookInput.x * _mouseCameraTurnSpeed.x;
                _cinemachineTargetPitch += lookInput.y * _mouseCameraTurnSpeed.y;
            }

            ApplyRotation();
        }

        private void ApplyRotation()
        {
            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

            // Cinemachine will follow this target
            _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, 
                _cinemachineTargetYaw, 0.0f);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}