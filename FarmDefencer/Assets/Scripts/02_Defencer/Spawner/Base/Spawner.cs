using UnityEngine;

public abstract class Spawner : MonoBehaviour
{
    [Header("���������������� Spawner ����������������")]
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
