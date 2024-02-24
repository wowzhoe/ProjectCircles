using UnityEngine;

public abstract class PersistentMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }

        private set {}
    }
}
