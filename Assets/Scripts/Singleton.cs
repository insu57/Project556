using UnityEngine;

public class Singleton <T> : MonoBehaviour where T : Singleton<T> //Singleton Generic Class
{
    private static T _instance;
    public static T Instance //해당 컴포넌트 접근
    {
        get
        {
            if (!_instance) //없다면 새로...
            {
                _instance = (T)FindAnyObjectByType(typeof(T));
                if (!_instance)
                {
                    SetupInstance();
                }
            }
            
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        RemoveDuplicates();//중복제거
    }

    private static void SetupInstance() //인스턴스 설정
    {
        _instance = (T)FindAnyObjectByType(typeof(T));
        
        if (_instance) return; //이미 있으면 return
        var obj = new GameObject //없다면 새로 생성
        {
            name = typeof(T).Name
        };
        _instance = obj.AddComponent<T>();
        DontDestroyOnLoad(obj);
    }

    private void RemoveDuplicates()//중복제거
    {
        if (!_instance) //없으면 지정
        {
            _instance = this as T;
            DontDestroyOnLoad(_instance);//로드 시 파괴 방지
        }
        else
        {
            Debug.Log(gameObject.name + ": Duplicate Destroy");
            Destroy(gameObject);
        }
    }
}
