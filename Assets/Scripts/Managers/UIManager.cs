using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// UI 팝업, 패널 등을 생성하고 관리하는 매니저
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Transform uiRoot; // Canvas Transform 연결 필요

    // 현재 열려있는 UI 관리
    private Dictionary<string, GameObject> activeUI = new ();

    /// <summary>
    /// UI 로드 및 인스턴스화
    /// </summary>
    public async Task<GameObject> OpenUIAsync(string key)
    {
        // 이미 열려있는지 확인 (캐싱)
        if (activeUI.ContainsKey(key))
        {
            Debug.LogWarning($"[UIManager] UI is already open: {key}");
            return activeUI[key];
        }

        // 로드 + Instantiate 동시에 수행(trackHandle: true -> ReleaseInstance로 쉽게 해제 가능)
        var handle = Addressables.InstantiateAsync(key, uiRoot, false, true);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject uiObj = handle.Result;
            // 관리 목록에 추가
            activeUI.Add(key, uiObj);

            // 필요 시 여기서 생성된 UI의 초기화 작업 수행(e.g. RectTransform 초기화, SortOrder 설정)
            return uiObj;
        }
        else
        {
            Debug.LogError($"[UIManager] Failed to open UI: {key}");
            return null;
        }
    }

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
}