using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private WaveSystem _waveSystem;

    [Space]

    [SerializeField] private TextMeshProUGUI _targetSpawnCountText;
    [SerializeField] private TextMeshProUGUI _totalSpawnCountText;
    [SerializeField] private TextMeshProUGUI _survivedCountText;

    private void OnEnable()
    {
        if (_waveSystem == null)
        {
            return;
        }

        // init all text
        SetTargetSpawnCountText(_waveSystem.TargetSpawnCount);
        SetTotalSpawnCountText(_waveSystem.TotalSpawnCount);
        SetSurvivedCountText(TowerDefenceManager.Instance.SurvivedCount);

        // subscribe event: target spawn count
        _waveSystem.OnTargetSpawnCountChanged -= SetTargetSpawnCountText;
        _waveSystem.OnTargetSpawnCountChanged += SetTargetSpawnCountText;

        // subscribe event: total spawn count
        _waveSystem.OnTotalSpawnCountChanged -= SetTotalSpawnCountText;
        _waveSystem.OnTotalSpawnCountChanged += SetTotalSpawnCountText;

        if (TowerDefenceManager.Instance == null)
        {
            return;
        }

        // subscribe event: survived count
        TowerDefenceManager.Instance.OnSurvivedCountChanged -= SetSurvivedCountText;
        TowerDefenceManager.Instance.OnSurvivedCountChanged += SetSurvivedCountText;
    }
    private void OnDisable()
    {
        if (_waveSystem == null)
        {
            return;
        }

        // unsubscribe event: target spawn count
        _waveSystem.OnTargetSpawnCountChanged -= SetTargetSpawnCountText;

        // unsubscribe event: total spawn count
        _waveSystem.OnTotalSpawnCountChanged -= SetTotalSpawnCountText;

        if (TowerDefenceManager.Instance == null)
        {
            return;
        }

        // unsubscribe event: survived count
        TowerDefenceManager.Instance.OnSurvivedCountChanged -= SetSurvivedCountText;
    }

    public void SetTargetSpawnCountText(int count)
    {
        _targetSpawnCountText.text = $"TargetSpawn: {count}";
    }
    public void SetTotalSpawnCountText(int count)
    {
        _totalSpawnCountText.text = $"TotalSpawn: {count}";
    }
    public void SetSurvivedCountText(int count)
    {
        _survivedCountText.text = $"Survived: {count}";
    }
}
