using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThiccTapeman.Utils
{
    public class TickSystem
    {
        public static TickSystem instance;

        public class OnTickEventArgs : EventArgs
        {
            public int tick;
        }

        public event EventHandler<OnTickEventArgs> OnTick;

        private const float TICK_TIMER_MAX = .01f;

        private GameObject tickSystemGameObject;
        private int tick;

        public static TickSystem GetInstance()
        {
            if (instance != null) return instance;

            instance = new TickSystem();
            instance.Setup();

            return instance;
        }

        private void Setup()
        {
            tickSystemGameObject = new GameObject("TickSystem");
            tickSystemGameObject.AddComponent<TickSystemObject>();
        }

        public static int GetTick()
        {
            return GetInstance().tick;
        }


        private class TickSystemObject : MonoBehaviour
        {
            private float tickTimer;

            private void Awake()
            {
                instance.tick = 0;
            }

            private void Update()
            {
                if (instance == null) GetInstance();

                tickTimer += Time.deltaTime;
                if (tickTimer >= TICK_TIMER_MAX)
                {
                    tickTimer -= TICK_TIMER_MAX;

                    instance.tick++;
                    if (instance.OnTick != null) instance.OnTick(this, new OnTickEventArgs { tick = instance.tick });
                }
            }

        }
    }


}
