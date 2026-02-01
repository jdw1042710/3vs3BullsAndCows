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

    #region Public Methods
    /// <summary>
    /// 비동기 로드 (캐싱 로직 적용)
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string key) where T : Object
    {
        var handle = GetOrStartOperation<T>(key);

        await handle;

        return GetResultAndCache(key, handle);
    }

    /// <summary>
    /// 동기 로드 (캐싱 로직 적용)
    /// </summary>
    public T LoadAssetSync<T>(string key) where T : Object
    {
        var handle = GetOrStartOperation<T>(key);

        // 로드가 안 끝났을 때만 WaitForCompletion 호출 (성능 최적화)
        if (!handle.IsDone)
        {
            handle.WaitForCompletion();
        }

        return GetResultAndCache(key, handle);
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
    #endregion

    #region Private Methods
    /// <summary>
    /// 캐시를 확인하고 핸들을 반환하거나, 없으면 새로 로드를 시작합니다.
    /// </summary>
    private AsyncOperationHandle<T> GetOrStartOperation<T>(string key)
    {
        if (loadedHandles.TryGetValue(key, out AsyncOperationHandle handle))
        {
            // 이미 있는 핸들을 T 타입으로 변환해서 반환
            return handle.Convert<T>();
        }

        // 없으면 새로 로드 시작
        return Addressables.LoadAssetAsync<T>(key);
    }

    /// <summary>
    /// 로드가 끝난 핸들의 성공 여부를 체크하고 캐싱
    /// </summary>
    private T GetResultAndCache<T>(string key, AsyncOperationHandle<T> handle) where T : Object
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // 딕셔너리에 없으면 추가 (중복 방지)
            if (!loadedHandles.ContainsKey(key))
            {
                loadedHandles.Add(key, handle);
            }
            return handle.Result;
        }
        else
        {
            Debug.LogError($"[ResourceManager] Failed to load: {key}");
            // 실패한 핸들은 즉시 해제
            Addressables.Release(handle);
            return null;
        }
    }
    #endregion
}