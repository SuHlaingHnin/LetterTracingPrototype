#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MagneticScrollView
{
    /// <summary>
    /// Safe operations for Object / Component creation and destruction with Undo registretion. The Undo works only in editor.
    /// </summary>
    public static class SafeOperations
    {
        /// <summary>
        /// Creates new Game Object with Undo registration.
        /// </summary>
        /// <param name="name">The starting name.</param>
        public static GameObject NewGameObject (string name)
        {
            GameObject go = NewGameObject (name, null, new System.Type [0]);
            return go;
        }

        /// <summary>
        /// Creates new Game Object with Undo registration.
        /// </summary>
        /// <param name="name">The starting name.</param>
        /// <param name="parent">The parent transform to use.</param>
        public static GameObject NewGameObject (string name, Transform parent)
        {
            GameObject go = NewGameObject (name, parent, null);
            return go;
        }

        /// <summary>
        /// Creates new Game Object with Undo registration.
        /// </summary>
        /// <param name="name">The starting name.</param>
        /// <param name="components">A list of components to add to the GameObject on creation.</param>
        public static GameObject NewGameObject (string name, params System.Type [] components)
        {
            GameObject go = NewGameObject (name, null, components);
            return go;
        }

        /// <summary>
        /// Creates new Game Object with Undo registration.
        /// </summary>
        /// <param name="name">The name that the GameObject is created with.</param>
        /// <param name="parent">The parent transform to use.</param>
        /// <param name="components">A list of components to add to the GameObject on creation.</param>
        public static GameObject NewGameObject (string name, Transform parent, params System.Type [] components)
        {
            GameObject go;
            if (components != null)
                go = new GameObject (name, components);
            else
                go = new GameObject (name);

            go.transform.SetParent (parent, false);
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo (go, "New Object created");
#endif
            return go;
        }

        /// <summary>
        /// Instantiate the given prefab with Undo registration.
        /// </summary>
        /// <param name="target">Prefab GameObject to be instantiated.</param>
        /// <returns></returns>
        public static GameObject Instantiate (GameObject target)
        {
            GameObject go;
#if UNITY_EDITOR
            go = PrefabUtility.InstantiatePrefab (target) as GameObject;
            Undo.RegisterCreatedObjectUndo (go, "New Object Instantiated");
#else
            go = Object.Instantiate (target);
#endif
            return go;
        }

        /// <summary>
        /// Instantiate the given prefab with Undo registration.
        /// </summary>
        /// <param name="target">Prefab GameObject to be instantiated.</param>
        /// <param name="parent">The parent Transform to use.</param>
        /// <returns></returns>
        public static GameObject Instantiate (GameObject target, Transform parent)
        {
            GameObject go = Instantiate (target);
            go.transform.SetParent (parent, false);
            return go;
        }

        /// <summary>
        /// Destroys the given object with Undo registration.
        /// </summary>
        /// <param name="obj">The object to be destroied.</param>
        public static void Destroy (Object obj)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate (obj);
#else
            Object.Destroy (obj);
#endif
        }

        // SAFE EXTENSION METHOD

        /// <summary>
        /// Adds a new component of a generic type to the game object with undo registration.
        /// </summary>
        /// <returns>Returns the component added.</returns>
        public static T SafeAddComponent<T> (this GameObject gameObject) where T : Component
        {
            T comp;
#if UNITY_EDITOR
            comp = Undo.AddComponent<T> (gameObject);
#else
            comp = gameObject.AddComponent<T> ();
#endif
            return comp;
        }
    }
}