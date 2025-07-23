using System.Collections;
using UnityEngine;

public sealed class DefenceSceneOpenContext : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(DoStartDefence());
    }

    private IEnumerator DoStartDefence()
    {
        yield return new WaitForSeconds(1);
        
        MapManager.Instance.InvokeOnMapChanged();
        GameStateManager.Instance.ChangeState(GameState.Build);
        Destroy(gameObject);
    }
}
