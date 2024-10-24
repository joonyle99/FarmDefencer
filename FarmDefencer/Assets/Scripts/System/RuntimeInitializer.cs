using UnityEngine;

namespace JoonyleGameDevKit
{
    /// <summary>
    /// 런타임 초기화를 위한 클래스입니다
    /// </summary>
    public static class RuntimeInitializer
    {
        private const string BOOTSTRAPPER_PREFAB_PATH = "Prefabs/Bootstrapper";

        /// <summary>
        /// 씬이 로드되기 전에 부트스트랩을 생성합니다
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstantiateBootstrapper()
        {
            // runtime resource load
            Object resource = Resources.Load(BOOTSTRAPPER_PREFAB_PATH);

            if (resource == null)
            {
                Debug.LogError($"Failed to load resource at: {BOOTSTRAPPER_PREFAB_PATH}");
                return;
            }

            // instantiate bootstrapper
            GameObject bootstrapperGO = Object.Instantiate(resource) as GameObject;

            if (bootstrapperGO == null)
            {
                Debug.LogError($"Failed to instantiate bootstrapper");
                return;
            }

            Bootstrapper bootstrapper = bootstrapperGO.GetComponent<Bootstrapper>();
            bootstrapper.InitializeGame();
        }
    }
}
