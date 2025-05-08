using UnityEngine;

public class Singleton <T> : MonoBehaviour where T : Singleton<T> //Singleton Generic Class
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (!_instance)
            {
                //_instance = (T)FindFirstObjectByType(typeof(T));
                _instance = (T)FindAnyObjectByType(typeof(T));
                if (!_instance)
                {
                    SetupInstance();
                }
            }
            return _instance;
        }
    }

    public virtual void Awake()
    {
        RemoveDuplicates();
    }

    private static void SetupInstance()
    {
        //_instance = (T)FindFirstObjectByType(typeof(T));
        _instance = (T)FindAnyObjectByType(typeof(T));
        if (!_instance)
        {
            GameObject obj = new GameObject
            {
                name = typeof(T).Name
            };
            _instance = obj.AddComponent<T>();
            DontDestroyOnLoad(obj);
        }
    }

    private void RemoveDuplicates()
    {
        if (!_instance)
        {
            _instance = this as T;
            DontDestroyOnLoad(_instance);
        }
        else
        {
            Debug.Log(gameObject.name + ": Duplicate Destroy");
            Destroy(gameObject);
        }
    }
}
