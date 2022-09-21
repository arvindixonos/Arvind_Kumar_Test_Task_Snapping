using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    /// <summary>
    /// Templated singleton class.
    /// </summary>
    /// <typeparam name="T">class template.</typeparam>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        instance = obj.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// If the instance is null then assign this instance as the first instance else destroy the new instance.
        /// </summary>
        public virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                //DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
