using UnityEngine;

public abstract class Spawner : MonoBehaviour
{
    [Header("收收收收收收收收 Spawner 收收收收收收收收")]
    [Space]

    [SerializeField] private Factory _factory;
    public Factory Factory => _factory;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn();
        }
    }

    protected abstract void Spawn();
}
