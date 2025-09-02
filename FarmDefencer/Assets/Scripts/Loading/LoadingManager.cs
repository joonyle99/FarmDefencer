using Spine.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public static class ResourceCache
{
    private static Dictionary<string, AsyncOperationHandle> _cachedHandles = new();

    public static async Task<T> LoadAsync<T>(string key) where T : class
    {
        if (_cachedHandles.TryGetValue(key, out var _handle))
        {
            return _handle.Result as T;
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _cachedHandles[key] = handle;
            return handle.Result;
        }
        else
        {
            Debug.LogError($"Failed to load resource with key: {key}");
            return null;
        }
    }
    public static T Get<T>(string key) where T : class
    {
        if (_cachedHandles.TryGetValue(key, out var handle))
        {
            if (handle.Result is T result)
            {
                return result;
            }
            else
            {
                Debug.LogError($"{key} is not of type {typeof(T).Name}, but {handle.Result?.GetType().Name}");
                return null;
            }
        }

        return null;
    }
    public static void Clear()
    {
        foreach (var handle in _cachedHandles.Values)
        {
            Addressables.Release(handle);
        }

        _cachedHandles.Clear();
    }
    public static void PrintAll()
    {
        foreach (var kvp in _cachedHandles)
        {
            var key = kvp.Key;
            var value = kvp.Value;
            var type = value.Result != null ? value.Result.GetType().Name : "null";

            Debug.Log($"<color=lightblue>Resource Key: {kvp.Key}, Type: {type}</color>");
        }
    }
}

public static class AssetCache
{
    private static Dictionary<string, AsyncOperationHandle<GameObject>> _cachedHandles = new();

    public static async Task<T> LoadAsync<T>(string key) where T : Component
    {
        if (_cachedHandles.TryGetValue(key, out var _handle))
        {
            return _handle.Result != null ? _handle.Result.GetComponent<T>() : null;
        }

        var handle = Addressables.LoadAssetAsync<GameObject>(key);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _cachedHandles[key] = handle;
            return handle.Result != null ? handle.Result.GetComponent<T>() : null;
        }
        else
        {
            Debug.LogError($"Failed to load prefab with key: {key}");
            return null;
        }
    }
    public static T Get<T>(string key) where T : Component
    {
        if (_cachedHandles.TryGetValue(key, out var handle))
        {
            return handle.Result != null ? handle.Result.GetComponent<T>() : null;
        }

        return null;
    }
    public static void Clear()
    {
        foreach (var handle in _cachedHandles.Values)
        {
            Addressables.Release(handle);
        }
        _cachedHandles.Clear();
    }
    public static void PrintAll()
    {
        foreach (var kvp in _cachedHandles)
        {
            var key = kvp.Key;
            var value = kvp.Value;
            var type = value.Result != null ? value.Result.GetType().Name : "null";
            Debug.Log($"<color=lightgreen>Prefab Key: {key}, Type: {type}</color>");
        }
    }
}

/// <summary>
/// LoadingScene에서 게임 시작 시 필요한 리소스를 미리 로드하는 매니저.
/// </summary>
/// <remarks>
/// 현재는 디펜스의 리소스만 로드하지만, 추후에는 타이쿤의 리소스도 로드할 수 있도록 한다.
/// </remarks>
public class LoadingManager : MonoBehaviour
{
    private async void Start()
    {
        ResourceCache.Clear();
        AssetCache.Clear();

        var mapEntry = MapManager.Instance.CurrentMap;
        var mapCode = mapEntry.MapCode;
        //var mapResources = mapEntry.Resources;

        {
            var tasks = new List<Task>();

            // 스프라이트
            tasks.Add(ResourceCache.LoadAsync<Sprite>($"{mapCode}_departure"));
            tasks.Add(ResourceCache.LoadAsync<Sprite>($"{mapCode}_arrival"));

            // 비디오
            tasks.Add(ResourceCache.LoadAsync<VideoClip>($"{mapCode}_video"));

            // 몬스터 (스파인)
            foreach (var monster in mapEntry.Monsters)
            {
                var lowerCode = monster.MonsterData.Name.ToLower();
                tasks.Add(ResourceCache.LoadAsync<Material>($"monster_{lowerCode}_Material"));
                tasks.Add(ResourceCache.LoadAsync<SkeletonDataAsset>($"monster_{lowerCode}_SkeletonData"));
            }

            // 프리팹
            tasks.Add(AssetCache.LoadAsync<DamageText>("DamageText"));
            tasks.Add(AssetCache.LoadAsync<ChainLightning>("ChainLightning"));

            // TODO 이후에 SO 방식으로 개선하면 좋을것같음

            //foreach (var resource in mapResources)
            //{
            //    var key = string.Empty;

            //    switch (resource.temp)
            //    {
            //        case Temp.None:
            //            key = resource.format;
            //            break;
            //        case Temp.MapCode:
            //            key = resource.format.Replace("{MapCode}", mapCode);
            //            break;
            //        case Temp.MonterCode:
            //            // TODO
            //            break;
            //        default:
            //            key = resource.format;
            //            break;
            //    }

            //    switch (resource.kind)
            //    {
            //        case ResourceKind.Sprite:
            //            tasks.Add(ResourceCache.LoadAsync<Sprite>(key));
            //            break;
            //        case ResourceKind.Video:
            //            tasks.Add(ResourceCache.LoadAsync<VideoClip>(key));
            //            break;
            //        case ResourceKind.Material:
            //            tasks.Add(ResourceCache.LoadAsync<Material>(key));
            //            break;
            //        case ResourceKind.SkeletonData:
            //            tasks.Add(ResourceCache.LoadAsync<SkeletonDataAsset>(key));
            //            break;
            //        case ResourceKind.Prefab:
            //            tasks.Add(AssetCache.LoadAsync<Component>(key));
            //            break;
            //    }
            //}

            // 모든 리소스를 병렬 로드
            await Task.WhenAll(tasks);

            // 최소 로딩 시간 보장 (예: 1.5초)
            var minDelay = Task.Delay(1500);
            await Task.WhenAll(minDelay);
        }

        SceneLoadContext.OnSceneChanged?.Invoke();
        SceneManager.LoadScene(SceneLoadContext.NextSceneName);
    }
}
