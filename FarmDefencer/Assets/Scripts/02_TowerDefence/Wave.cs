using UnityEngine;

public class Wave : MonoBehaviour
{
    //[Header("���������������� Wave ����������������")]
    //[Space]

    // [SerializeField] private Factory _factory;
    // public Factory Factory => _factory;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn();
        }
    }

    protected void Spawn()
    {
        /*
        Monster monster = Factory.GetProduct<Monster>();
        monster.SetOriginFactory(Factory);
        monster.transform.position = transform.position;
        */
    }
}
