using DiasGames.Debugging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiasGames.Controller
{
    public enum PlayerPrefab
    {
        CharacterController,
        Rigidbody,
        Mobile
    }
    
    public class LevelController : MonoBehaviour, ILevelController
    {
        [SerializeField] private PlayerPrefab _spawnPlayerType = PlayerPrefab.CharacterController;
        [SerializeField] private Transform _spawnPoint;
        [Header("Player prefabs")]
        [SerializeField] private GameObject _characterControllerPlayer;
        [SerializeField] private GameObject _rigidbodyPlayer;
        [SerializeField] private GameObject _mobilePlayer;
        [SerializeField] private GameObject _fixedUpdateMainCamera;
        [SerializeField] private CursorLockMode _cursorLockMode = CursorLockMode.Locked;
        [SerializeField] private bool _cursorVisible = false;

        private GameObject _currentPlayer;

        private static bool _restartingScene = false;
        private static PlayerPrefab _restartPlayerType;

        private const string charControllerCommandId = "playCharController";
        private const string rigidbodyCommandId = "playRigidbody";
        private const string mobileCommandId = "playMobile";
        
        private void Awake()
        {
            if (_restartingScene)
            {
                _restartingScene = false;
                _spawnPlayerType = _restartPlayerType;
            }

            SpawnPlayer();
            CreateDebugCommands();
            Cursor.lockState = _cursorLockMode;
            Cursor.visible = _cursorVisible;
        }

        private void OnDestroy()
        {
            DebugConsole.RemoveConsoleCommand(charControllerCommandId);
            DebugConsole.RemoveConsoleCommand(rigidbodyCommandId);
            DebugConsole.RemoveConsoleCommand(mobileCommandId);
        }

        private void CreateDebugCommands()
        {
            DebugCommand charControllerCommand = new DebugCommand(charControllerCommandId,
                "Restart Scene to play with Character Controller", charControllerCommandId, CharControllerRestart);

            DebugCommand rigidbodyCommand = new DebugCommand(rigidbodyCommandId,
                "Restart Scene to play with Rigidbody", rigidbodyCommandId, RigidbodyRestart);

            DebugCommand mobileCommand = new DebugCommand(mobileCommandId,
                "Restart Scene to play with Mobile Controls", mobileCommandId, MobileRestart);

            DebugConsole.AddConsoleCommand(charControllerCommand);
            DebugConsole.AddConsoleCommand(rigidbodyCommand);
            DebugConsole.AddConsoleCommand(mobileCommand);
        }

        private void SpawnPlayer()
        {
            Vector3 targetPosition = Vector3.zero;
            Quaternion targetRotation = Quaternion.identity;
            if (_spawnPoint != null)
            {
                targetPosition = _spawnPoint.position;
                targetRotation = _spawnPoint.rotation;
            }

            GameObject selectedPrefab = null;
            switch (_spawnPlayerType)
            {
                case PlayerPrefab.CharacterController:
                    selectedPrefab = _characterControllerPlayer;
                    break;
                case PlayerPrefab.Rigidbody:
                    selectedPrefab = _rigidbodyPlayer;
                    if (_fixedUpdateMainCamera)
                    {
                        GameObject currentCam = Camera.main.gameObject;
                        Instantiate(_fixedUpdateMainCamera);
                        Destroy(currentCam);
                    }
                    break;
                case PlayerPrefab.Mobile:
                    selectedPrefab = _mobilePlayer;
                    break;
            }

            if (selectedPrefab == null)
            {
                return;
            }

            _currentPlayer = Instantiate(selectedPrefab, targetPosition, targetRotation);
        }

        public void RestartLevel()
        {
            _restartingScene = true;
            _restartPlayerType = _spawnPlayerType;
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void SetCursorLocked(bool locked)
        {
            if (locked)
            {
                Cursor.lockState = _cursorLockMode;
                Cursor.visible = _cursorVisible;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void MobileRestart()
        {
            RestartToCharacter(PlayerPrefab.Mobile);
        }

        private void RigidbodyRestart()
        {
            RestartToCharacter(PlayerPrefab.Rigidbody);
        }

        private void CharControllerRestart()
        {
            RestartToCharacter(PlayerPrefab.CharacterController);
        }

        private void RestartToCharacter(PlayerPrefab playerPrefab)
        {
            _spawnPlayerType = playerPrefab;
            RestartLevel();
        }
        
    }
}