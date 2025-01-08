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
        SetSurvivedCountText(_waveSystem.SurvivedCount);

        // subscribe event: target spawn count
        _waveSystem.OnTargetSpawnCountChanged -= SetTargetSpawnCountText;
        _waveSystem.OnTargetSpawnCountChanged += SetTargetSpawnCountText;

        // subscribe event: total spawn count
        _waveSystem.OnTotalSpawnCountChanged -= SetTotalSpawnCountText;
        _waveSystem.OnTotalSpawnCountChanged += SetTotalSpawnCountText;

        // subscribe event: survived count
        _waveSystem.OnSurvivedCountChanged -= SetSurvivedCountText;
        _waveSystem.OnSurvivedCountChanged += SetSurvivedCountText;
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

        // unsubscribe event: survived count
        _waveSystem.OnSurvivedCountChanged -= SetSurvivedCountText;
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
