using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

/// <summary>
/// 일반적인 에셋(ScriptableObject, Texture, Audio, Prefab 원본 등)을 로드하는 매니저
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    // 캐싱 및 Release(메모리 해제)용 딕셔너리
    private Dictionary<string, AsyncOperationHandle> loadedHandles = new ();

    /// <summary>
    /// 에셋 비동기 로드 (캐싱 로직 적용)
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string key) where T : Object
    {
        // 이미 로드된 리소스인지 확인
        if (loadedHandles.ContainsKey(key))
        {
            return loadedHandles[key].Result as T;
        }


        var handle = Addressables.LoadAssetAsync<T>(key);

        await handle.Task;

        // 성공 시 딕셔너리에 핸들 저장
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedHandles.Add(key, handle);
            return handle.Result;
        }
        else
        {
            Debug.LogError($"[ResourceManager] Failed to load asset: {key}");
            return null;
        }
    }

    /// <summary>
    /// 에셋을 메모리에서 해제
    /// </summary>
    public void UnloadAsset(string key)
    {
        if (loadedHandles.TryGetValue(key, out AsyncOperationHandle handle))
        {
            Addressables.Release(handle); // 실제 메모리 해제
            loadedHandles.Remove(key);   // 관리 목록에서 제거
        }
    }

    /// <summary>
    /// 관리 중인 모든 에셋 해제 (e.g. 씬 전환 시)
    /// </summary>
    public void UnloadAll()
    {
        foreach (var handle in loadedHandles.Values)
        {
            Addressables.Release(handle);
        }
        loadedHandles.Clear();
    }
}