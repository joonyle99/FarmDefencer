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
}

public static class AssetCache
{
    private static Dictionary<string, object> _cache = new();

    public static async Task<T> LoadAsync<T>(string key) where T : Component
    {
        if (_cache.TryGetValue(key, out var obj))
        {
            return obj as T;
        }

        var handle = Addressables.LoadAssetAsync<GameObject>(key);
        var prefab = await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var result = prefab.GetComponent<T>();

            if (result == null)
            {
                Debug.LogError($"Component {typeof(T).Name} not found in prefab with key: {key}");
                return null;
            }

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
}

public class LoadingManager : MonoBehaviour
{
    private async void Start()
    {
        var mapEntry = MapManager.Instance.CurrentMap;
        var mapCode = mapEntry.MapCode;

        // 미리 필요한 리소스/에셋 로드
        // TODO: 이후에 ScriptableOjbect로 관리할 것 (만약 씬마다 다르게 가져가고 싶다면)
        await ResourceCache.LoadAsync<Sprite>($"{mapCode}_departure");
        await ResourceCache.LoadAsync<Sprite>($"{mapCode}_arrival");
        await ResourceCache.LoadAsync<VideoClip>($"{mapCode}_video");
        for (int i = 0; i < mapEntry.Monsters.Length; i++)
        {
            var monsterCode = mapEntry.Monsters[i].MonsterData.Name;
            var lowerCode = monsterCode.ToLower();
            await ResourceCache.LoadAsync<Material>($"monster_{lowerCode}_Material");
            await ResourceCache.LoadAsync<SkeletonDataAsset>($"monster_{lowerCode}_SkeletonData");
        }
        await AssetCache.LoadAsync<DamageText>("DamageText");
        await AssetCache.LoadAsync<ChainLightning>("ChainLightning");

        // 대기 (기본 2초)
        await Task.Delay(2000);

        SceneLoadContext.OnSceneChanged?.Invoke();
        SceneManager.LoadScene(SceneLoadContext.NextSceneName);
    }
}
