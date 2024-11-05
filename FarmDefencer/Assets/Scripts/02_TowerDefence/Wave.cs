using UnityEngine;

public class Wave : MonoBehaviour
{
    //[Header("收收收收收收收收 Wave 收收收收收收收收")]
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
