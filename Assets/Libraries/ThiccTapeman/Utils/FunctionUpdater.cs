using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThiccTapeman.Utils
{
    public class FunctionUpdater
    {
        private class MonoBehaviourHook : MonoBehaviour
        {

            public Action OnUpdate;

            private void Update()
            {
                if (OnUpdate != null) OnUpdate();
            }

        }

        private static List<FunctionUpdater> updaterList;
        private static GameObject initGameObject;

        private static void InitIfNeeded()
        {
            if (initGameObject == null)
            {
                initGameObject = new GameObject("function_updater_global");
                updaterList = new List<FunctionUpdater>();
            }
        }

        // ---------------------------------------------------- //
        // Functions to create the function updater             //
        // ---------------------------------------------------- //
        #region Create
        public static FunctionUpdater Create(Action updateFunc)
        {
            return Create(() => { updateFunc(); return false; }, "", true, false);
        }

        public static FunctionUpdater Create(Action updateFunc, string functionName)
        {
            return Create(() => { updateFunc(); return false; }, functionName, true, false);
        }

        public static FunctionUpdater Create(Func<bool> updateFunc)
        {
            return Create(updateFunc, "", true, false);
        }

        public static FunctionUpdater Create(Func<bool> updateFunc, string functionName)
        {
            return Create(updateFunc, functionName, true, false);
        }

        public static FunctionUpdater Create(Func<bool> updateFunc, string functionName, bool active)
        {
            return Create(updateFunc, functionName, active, false);
        }

        public static FunctionUpdater Create(Func<bool> updateFunc, string functionName, bool active, bool stopAllWithSameName)
        {
            InitIfNeeded();

            if (stopAllWithSameName)
            {
                StopAllUpdatersWithName(functionName);
            }

            GameObject gameObject = new GameObject("FunctionUpdater Object " + functionName, typeof(MonoBehaviourHook));
            FunctionUpdater functionUpdater = new FunctionUpdater(gameObject, updateFunc, functionName, active);
            gameObject.GetComponent<MonoBehaviourHook>().OnUpdate = functionUpdater.Update;

            updaterList.Add(functionUpdater);
            return functionUpdater;
        }
        #endregion

        // ---------------------------------------------------- //
        // Functions to remove the function updater             //
        // ---------------------------------------------------- //
        #region Remove
        private static void RemoveUpdater(FunctionUpdater funcUpdater)
        {
            InitIfNeeded();
            updaterList.Remove(funcUpdater);
        }

        public static void DestroyUpdater(FunctionUpdater funcUpdater)
        {
            InitIfNeeded();
            if (funcUpdater != null)
            {
                funcUpdater.DestroySelf();
            }
        }
        #endregion

        // ---------------------------------------------------- //
        // Functions to remove by name                          //
        // ---------------------------------------------------- //
        #region Stop
        public static void StopUpdaterWithName(string functionName)
        {
            InitIfNeeded();
            for (int i = 0; i < updaterList.Count; i++)
            {
                if (updaterList[i].functionName == functionName)
                {
                    updaterList[i].DestroySelf();
                    return;
                }
            }
        }

        public static void StopAllUpdatersWithName(string functionName)
        {
            InitIfNeeded();
            for (int i = 0; i < updaterList.Count; i++)
            {
                if (updaterList[i].functionName == functionName)
                {
                    updaterList[i].DestroySelf();
                    i--;
                }
            }
        }
        #endregion

        // ---------------------------------------------------- //
        // The FunctionUpdater class                            //
        // ---------------------------------------------------- //
        #region SelfClass
        private GameObject gameObject;
        private string functionName;
        private bool active;
        private Func<bool> updateFunc; // Destroy Updater if return true;

        public FunctionUpdater(GameObject gameObject, Func<bool> updateFunc, string functionName, bool active)
        {
            this.gameObject = gameObject;
            this.updateFunc = updateFunc;
            this.functionName = functionName;
            this.active = active;
        }

        public void Pause()
        {
            active = false;
        }

        public void Resume()
        {
            active = true;
        }

        private void Update()
        {
            if (!active) return;
            if (updateFunc())
            {
                DestroySelf();
            }
        }

        public void DestroySelf()
        {
            RemoveUpdater(this);
            if (gameObject != null)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }
        #endregion
    }
}
