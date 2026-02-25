using UnityEngine;

namespace DiasGames.Debugging
{
    public class Initializer
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            DebugConsole _debugConsole =  Resources.Load<DebugConsole>("Debug Console");
            Object.Instantiate(_debugConsole);
        }
#endif
    }
}