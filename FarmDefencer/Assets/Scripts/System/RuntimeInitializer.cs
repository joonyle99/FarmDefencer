using UnityEngine;

namespace JoonyleGameDevKit
{
    /// <summary>
    /// ��Ÿ�� �ʱ�ȭ�� ���� Ŭ�����Դϴ�
    /// </summary>
    public static class RuntimeInitializer
    {
        private const string BOOTSTRAPPER_PREFAB_PATH = "Prefabs/Bootstrapper";

        /// <summary>
        /// ���� �ε�Ǳ� ���� ��Ʈ��Ʈ���� �����մϴ�
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
