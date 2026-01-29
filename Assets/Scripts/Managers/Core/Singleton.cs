using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance 
    { 
        get
        {
            if (instance == null)
            {
                GameObject singletonObj = new GameObject();
                instance = singletonObj.AddComponent<T>();
                singletonObj.name = typeof(T).ToString();       
            }
            return instance;
        }
    }

    [Tooltip("Scene 이동 시 파괴 여부")]
    [SerializeField] protected bool isDontDestroyOnLoad = true;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (isDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (instance != this)
        {
            // 이미 인스턴스가 존재하는데 또 생겼다면(씬 로드 등) 자신을 파괴
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
