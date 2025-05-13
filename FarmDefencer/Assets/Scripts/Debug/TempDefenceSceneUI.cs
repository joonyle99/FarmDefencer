using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TempDefenceSceneUI : MonoBehaviour
{
    private Button _goTycoonButton;
    private Button _spawnMonsterButton;
    private Button _coinInputButton;
    private TMP_InputField _coinInputField;
    private TMP_InputField _spawnMonsterInputField;

    private void Awake()
    {
        _spawnMonsterInputField = transform.Find("SpawnMonsterInputField").GetComponent<TMP_InputField>();
        _coinInputField = transform.Find("CoinInputField").GetComponent<TMP_InputField>();
        
        _goTycoonButton = transform.Find("GoTycoonButton").GetComponent<Button>();
        _goTycoonButton.onClick.AddListener(() => SceneManager.LoadScene(1));
        
        _spawnMonsterButton = transform.Find("SpawnMonsterButton").GetComponent<Button>();
        _spawnMonsterButton.onClick.AddListener(() =>
        {
            ResourceManager.Instance.SurvivedMonsters.Add(_spawnMonsterInputField.text);
            _spawnMonsterInputField.text = "";
        });
        
        _coinInputButton = transform.Find("CoinButton").GetComponent<Button>();
        _coinInputButton.onClick.AddListener(() =>
        {
            ResourceManager.Instance.Gold = int.Parse(_coinInputField.text);
            _spawnMonsterInputField.text = "";
        });
    }
}
