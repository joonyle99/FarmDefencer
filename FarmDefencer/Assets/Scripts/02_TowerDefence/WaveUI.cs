using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    public Wave waveSystem;

    [Space]

    public TextMeshProUGUI targetSpawnCountText;
    public TextMeshProUGUI totalSpawnCountText;
    public TextMeshProUGUI survivedCountText;

    private void OnEnable()
    {
        if (waveSystem == null)
        {
            return;
        }

        SetTargetSpawnCountText(waveSystem.TargetSpawnCount);
        SetTotalSpawnCountText(waveSystem.TotalSpawnCount);
        SetSurvivedCountText(TowerDefenceManager.Instance.SurvivedCount);

        waveSystem.OnTotalSpawnCountChanged -= SetTotalSpawnCountText;
        waveSystem.OnTotalSpawnCountChanged += SetTotalSpawnCountText;

        if (TowerDefenceManager.Instance == null)
        {
            return;
        }

        TowerDefenceManager.Instance.OnSurvivedCountChanged -= SetSurvivedCountText;
        TowerDefenceManager.Instance.OnSurvivedCountChanged += SetSurvivedCountText;
    }
    private void OnDisable()
    {
        if (waveSystem == null)
        {
            return;
        }

        waveSystem.OnTotalSpawnCountChanged -= SetTotalSpawnCountText;

        if (TowerDefenceManager.Instance == null)
        {
            return;
        }

        TowerDefenceManager.Instance.OnSurvivedCountChanged -= SetSurvivedCountText;
    }

    public void SetTargetSpawnCountText(int count)
    {
        targetSpawnCountText.text = $"TargetSpawn: {count}";
    }
    public void SetTotalSpawnCountText(int count)
    {
        totalSpawnCountText.text = $"TotalSpawn: {count}";
    }
    public void SetSurvivedCountText(int count)
    {
        survivedCountText.text = $"Survived: {count}";
    }
}
