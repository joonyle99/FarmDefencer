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
    private static Dictionary<string, object> _cache = new();

    public static async Task<T> LoadAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var obj))
        {
            return obj as T;
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        var result = await handle.Task;

        // successfully loaded resource
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _cache[key] = result;
            return result;
        }
        else
        {
            Debug.LogError($"Failed to load resource with key: {key}");
            return null;
        }
    }
    public static T Get<T>(string key) where T : class
    {
        return _cache.TryGetValue(key, out var obj) ? obj as T : null;
    }
    public static void Clear()
    {
        foreach (var resource in _cache.Values)
        {
            Addressables.Release(resource);
        }

        _cache.Clear();
    }
    public static void PrintAll()
    {
        foreach (var key in _cache.Keys)
        {
            Debug.Log($"<color=lightblue>Resource Key: {key}, Type: {_cache[key].GetType().Name}</color>");
        }
    }
}

public static class AssetCache
{
    private static Dictionary<string, GameObject> _cache = new();

    public static async Task<T> LoadAsync<T>(string key) where T : Component
    {
        if (_cache.TryGetValue(key, out var obj))
        {
            return obj.GetComponent<T>();
        }

        var handle = Addressables.LoadAssetAsync<GameObject>(key);
        var result = await handle.Task;

        // successfully loaded asset (prefab)
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _cache[key] = result;
            return result.GetComponent<T>();
        }
        else
        {
            Debug.LogError($"Failed to load resource with key: {key}");
            return null;
        }
    }
    public static T Get<T>(string key) where T : Component
    {
        if (_cache.TryGetValue(key, out var obj))
        {
            return obj.GetComponent<T>();
        }

        return null;
    }
    public static void Clear()
    {
        foreach (var asset in _cache.Values)
        {
            Addressables.Release(asset);
        }

        _cache.Clear();
    }
    public static void PrintAll()
    {
        foreach (var key in _cache.Keys)
        {
            Debug.Log($"<color=yellow>Asset Key: {key}, Type: {_cache[key].GetType().Name}</color>");
        }
    }
}

/// <summary>
/// LoadingScene에 존재하며, 게임 시작 시 필요한 리소스를 미리 로드하는 매니저.
/// </summary>
/// <remarks>
/// 현재는 디펜스의 리소스만 로드하지만, 추후에는 타이쿤의 리소스도 로드할 수 있도록 한다.
/// </remarks>
public class LoadingManager : MonoBehaviour
{
    private async void Start()
    {
        //// cache 초기화
        //ResourceCache.Clear();
        //AssetCache.Clear();

        //////////////////////////////////////////////

        var mapEntry = MapManager.Instance.CurrentMap;
        var mapCode = mapEntry.MapCode;

        var tasks = new List<Task>();

        // 스프라이트
        tasks.Add(ResourceCache.LoadAsync<Sprite>($"{mapCode}_departure"));
        tasks.Add(ResourceCache.LoadAsync<Sprite>($"{mapCode}_arrival"));

        // 비디오
        tasks.Add(ResourceCache.LoadAsync<VideoClip>($"{mapCode}_video"));

        // 몬스터 스파인
        foreach (var monster in mapEntry.Monsters)
        {
            var lowerCode = monster.MonsterData.Name.ToLower();
            tasks.Add(ResourceCache.LoadAsync<Material>($"monster_{lowerCode}_Material"));
            tasks.Add(ResourceCache.LoadAsync<SkeletonDataAsset>($"monster_{lowerCode}_SkeletonData"));
        }

        // 프리팹
        tasks.Add(AssetCache.LoadAsync<DamageText>("DamageText"));
        tasks.Add(AssetCache.LoadAsync<ChainLightning>("ChainLightning"));

        // 모든 리소스를 병렬 로드
        await Task.WhenAll(tasks);

        // 최소 로딩 시간 보장 (예: 2초)
        var minDelay = Task.Delay(2000);
        await Task.WhenAll(minDelay);

        SceneLoadContext.OnSceneChanged?.Invoke();
        SceneManager.LoadScene(SceneLoadContext.NextSceneName);
    }
}
