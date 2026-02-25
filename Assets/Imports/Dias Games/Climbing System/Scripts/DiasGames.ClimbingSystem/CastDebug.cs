using System.Collections.Generic;
using DiasGames.Debugging;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DiasGames.ClimbingSystem
{
    public class CastDebug : MonoBehaviour
    {
        private static CastDebug _instance;

        private bool _drawGizmos = true;
        private float _defaultDuration = 0.0f;

        private readonly Queue<Sphere> sphereGizmosQueue = new Queue<Sphere>();
        private readonly Queue<Label> labelGizmos = new Queue<Label>();
        private readonly Queue<Capsule> capsuleGizmosQueue = new Queue<Capsule>();

        public static CastDebug Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("Cast Debugger");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<CastDebug>();
                }

                return _instance;
            }
        }

        private void SetDebugEnabled(bool enable)
        {
            _drawGizmos = enable;
        }

        private void SetDebugDuration(float duration)
        {
            _defaultDuration = duration;
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
#if UNITY_EDITOR
            RegisterConsoleCommands();
#endif
        }

        private static void RegisterConsoleCommands()
        {
            string commandId = "showCastDebug";
            var show_cast = new DebugCommandBool(commandId,
                "Draw gizmos for climbing casts", $"{commandId} <true/false>",
                (x) => CastDebug.Instance.SetDebugEnabled(x));

            commandId = "setCastDebugDuration";
            var debug_duration = new DebugCommandFloat(commandId,
                "Set the duration for which the cast gizmo will persist", $"{commandId} <duration>",
                (x) => CastDebug.Instance.SetDebugDuration(x));

            DebugConsole.AddConsoleCommand(show_cast);
            DebugConsole.AddConsoleCommand(debug_duration);
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
            {
                return;
            }
            
            int count = sphereGizmosQueue.Count;
            for (int i = 0; i < count; i++)
            {
                Sphere sphere = sphereGizmosQueue.Dequeue();

                Gizmos.color = sphere.color;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);

                if (sphere.remainingTime > 0)
                {
                    sphere.remainingTime -= Time.deltaTime;
                    sphereGizmosQueue.Enqueue(sphere);
                }
            }

            count = capsuleGizmosQueue.Count;
            for (int i = 0; i < count; i++)
            {
                Capsule capsule = capsuleGizmosQueue.Dequeue();

                Gizmos.color = capsule.color;
                Gizmos.DrawWireSphere(capsule.bot, capsule.radius);
                Gizmos.DrawWireSphere(capsule.top, capsule.radius);
                
                for (int j = 0; j < 4; j++)
                {
                    float x = j == 0 ? 1 : j == 2 ? -1 : 0;
                    float y = j == 1 ? 1 : j == 3 ? -1 : 0;

                    Vector3 offset = new Vector3(x, 0, y) * capsule.radius;
                    Vector3 start = capsule.bot + offset;
                    Vector3 end = capsule.top + offset;

                    Gizmos.DrawLine(start, end);
                }

                if (capsule.remainingTime > 0)
                {
                    capsule.remainingTime -= Time.deltaTime;
                    capsuleGizmosQueue.Enqueue(capsule);
                }
            }

#if UNITY_EDITOR
            count = labelGizmos.Count;
            for (int i = 0; i < count; i++)
            {
                Label label = labelGizmos.Dequeue();

                var style = new GUIStyle();
                style.normal.textColor = label.color;
                style.fontSize = 20;

                Handles.Label(label.center, label.label, style);
                if (label.remainingTime > 0)
                {
                    label.remainingTime -= Time.deltaTime;
                    labelGizmos.Enqueue(label);
                }
            }
#endif
        }

        public void DrawSphere(Vector3 center, float radius, Color color, float duration = 0)
        {
            if (!_drawGizmos)
            {
                return;
            }

            Sphere sphere = new Sphere();
            sphere.center = center;
            sphere.radius = radius;
            sphere.color = color;
            sphere.remainingTime = duration == 0 ? _defaultDuration : duration;

            sphereGizmosQueue.Enqueue(sphere);
        }

        public void DrawCapsule(Vector3 p1, Vector3 p2, float radius, Color color, float duration = 0)
        {
            if (!_drawGizmos)
            {
                return;
            }

            Capsule capsule = new Capsule();
            capsule.bot = p1;
            capsule.top = p2;
            capsule.radius = radius;
            capsule.color = color;
            capsule.remainingTime = duration == 0 ? _defaultDuration : duration;

            capsuleGizmosQueue.Enqueue(capsule);
        }

        public void DrawLabel(string text, Vector3 position, Color color, float duration = 0)
        {
            if (!_drawGizmos)
            {
                return;
            }
 
            Label label = new Label();
            label.center = position;
            label.label = text;
            label.color = color;
            label.remainingTime = duration == 0 ? _defaultDuration : duration;

            labelGizmos.Enqueue(label);
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color, float duration=0)
        {
            if (!_drawGizmos)
            {
                return;
            }

            Debug.DrawLine(start, end, color, duration == 0 ? _defaultDuration : duration);
        }
    }


    public class Label
    {
        public string label;
        public Vector3 center;
        public Color color;
        public float remainingTime;
    }

    public class Sphere
    {
        public Vector3 center;
        public float radius;
        public Color color;
        public float remainingTime;
    }
    public class Capsule
    {
        public Vector3 bot;
        public Vector3 top;
        public float radius;
        public Color color;
        public float remainingTime;
    }
}