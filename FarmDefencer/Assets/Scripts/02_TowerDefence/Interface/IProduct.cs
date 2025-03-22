using UnityEngine;

public interface IProduct
{
    public Factory OriginFactory { get; set; }
    public void SetOriginFactory(Factory originFactory);

    public GameObject GameObject { get; }
    public Transform Transform { get; }
}