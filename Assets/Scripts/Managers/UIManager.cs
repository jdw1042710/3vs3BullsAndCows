using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

/// <summary>
/// UI 팝업, 패널 등을 생성하고 관리하는 매니저
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Transform uiRoot;

    // 현재 열려있는 UI 관리
    private Dictionary<string, GameObject> activeUI = new ();

    #region Public Methods
    /// <summary>
    /// 비동기 UI 생성 및 열기
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async UniTask<GameObject> OpenUIAsync(string key, Transform parent)
    {
        if (activeUI.ContainsKey(key)) return activeUI[key];

        var handle = StartInstantiate(key, parent);
        await handle; // Wait
        return RegisterUI(key, handle);
    }

    /// <summary>
    /// 동기 UI 생성 및 열기
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public GameObject OpenUISync(string key, Transform parent)
    {
        if (activeUI.ContainsKey(key)) return activeUI[key];

        var handle = StartInstantiate(key, parent);
        handle.WaitForCompletion(); // Wait
        return RegisterUI(key, handle);
    }

    #endregion
    /// <summary>
    /// 열려있는 UI를 닫고 메모리에서 해제
    /// </summary>
    public void CloseUI(string key)
    {
        if (activeUI.TryGetValue(key, out GameObject uiObj))
        {
            // 반드시 ReleaseInstance로 파괴해야 함 (Destroy()시, Reference Count가 꼬일 수 있음)
            bool success = Addressables.ReleaseInstance(uiObj);

            if (success)
            {
                activeUI.Remove(key);
            }
            else
            {
                Debug.LogError($"[UIManager] Failed to release UI instance: {key}");
            }
        }
        else
        {
            Debug.LogWarning($"[UIManager] Trying to close UI that is not open: {key}");
        }
    }

    #region Private Methods
    private AsyncOperationHandle<GameObject> StartInstantiate(string key, Transform parent)
    {
        // trackHandle: true로 해서 나중에 ReleaseInstance 가능하게 설정
        return Addressables.InstantiateAsync(key, uiRoot, false, true);
    }

    private GameObject RegisterUI(string key, AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var uiObj = handle.Result;
            if (!activeUI.ContainsKey(key)) activeUI.Add(key, uiObj);
            return uiObj;
        }
        else
        {
            Debug.LogError($"UI Fail: {key}");
            return null;
        }
    }
    #endregion
}