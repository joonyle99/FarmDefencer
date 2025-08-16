using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타워 디펜스에서 사용되는 UI를 제어한다
/// </summary>
public class DefenceUIController : MonoBehaviour
{
    [SerializeField] private ProcessUI _processUI;
    public ProcessUI ProcessUI => _processUI;
    [SerializeField] private BuildUI _buildUI;
    public BuildUI BuildUI => _buildUI;
    [SerializeField] private UpgradeUI _upgradeUI;
    public UpgradeUI UpgradeUI => _upgradeUI;

    [Space]

    [SerializeField] private Image _loadingUI;
    public Image LoadingUI => _loadingUI;
}
