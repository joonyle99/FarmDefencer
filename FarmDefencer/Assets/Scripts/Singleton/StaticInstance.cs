using UnityEngine;

namespace JoonyleGameDevKit
{
    /// <summary>
    /// A static instance is similar to a singleton, but instead of destroying any new instances,
    /// it overrides the current instance. This is handy for resetting the state and saves you doing it manually
    /// </summary>
    public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// ��𿡼��� ������ �� �ִ� ���� �ν��Ͻ�
        /// </summary>
        /// <remarks>
        /// ���� ������ �ƴ� ���� ������ ����Ͽ� ���ϼ��� �����մϴ�.
        /// </remarks>
        public static T Instance { get; private set; }

        protected virtual void Awake() => Instance = this as T;

        protected virtual void OnApplicationQuit()
        {
            Instance = null;
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// This converts the 'static instance' into a 'basic singleton'.
    /// It will destroy any new versions created, leaving the original instance
    /// </summary>
    public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            // �̹� �ν��Ͻ��� �����ϸ� this ��ü�� �ı��Ͽ� '�̱���'�� �����մϴ�.
            if (Instance != null)
            {
                Destroy(this.gameObject);
            }

            base.Awake();
        }
    }

    /// <summary>
    /// This will survive through scene loads.
    /// Perfect for system classes which require stateful, persistent data,
    /// audio sources where music plays through loading screens, etc
    /// </summary>
    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();

            // �ش� �̱��� ��ü�� ���� ����Ǿ �ı����� �ʵ��� �����մϴ�.
            DontDestroyOnLoad(gameObject);
        }
    }
}