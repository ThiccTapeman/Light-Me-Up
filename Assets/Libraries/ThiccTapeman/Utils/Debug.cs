using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThiccTapeman.Utils.Debug
{
    public class DebugDrawer
    {
        private static DebugDrawer instance;

        private List<Line> lines;

        private GameObject gameObject;

        public static DebugDrawer GetInstance()
        {
            if(instance == null) instance = new DebugDrawer();

            return instance;
        }

        public DebugDrawer()
        {
            Setup();
        }

        private void Setup()
        {
            lines = new List<Line>();

            gameObject = new GameObject("DebugDrawer");
            gameObject.AddComponent<DebugDrawerMonoBehaviour>();
        }

        public void AddLine(Line line)
        {
            if (lines.Contains(line)) return;

            lines.Add(line);
        }

        public void RemoveLine(Line line)
        {
            if(lines.Contains(line)) lines.Remove(line);
        }

        private class DebugDrawerMonoBehaviour : MonoBehaviour
        {
            public DebugDrawer debugDrawer;

            private void OnDrawGizmos()
            {
                foreach(Line line in GetInstance().lines)
                {
                    Gizmos.color = line.color;
                    Gizmos.DrawLine(line.from, line.to);   
                }
            }
        }

        public class Line
        {
            public Vector3 from;
            public Vector3 to;

            public Color color;

            public Line(Vector3 from, Vector3 to, Color color)
            {
                this.from = from;
                this.to = to;
                this.color = color;
            }
        }
    }
}
