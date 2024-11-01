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
        /// 어디에서나 접근할 수 있는 정적 인스턴스
        /// </summary>
        /// <remarks>
        /// 전역 변수가 아닌 정적 변수를 사용하여 유일성을 보장합니다.
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
            // 이미 인스턴스가 존재하면 this 객체를 파괴하여 '싱글톤'을 보장합니다.
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

            // 해당 싱글톤 객체를 씬이 변경되어도 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);
        }
    }
}