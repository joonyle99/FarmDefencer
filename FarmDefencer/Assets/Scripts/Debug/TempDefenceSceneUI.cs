using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TempDefenceSceneUI : MonoBehaviour
{
    private Button _goTycoonButton;

    private void Awake()
    {
        _goTycoonButton = transform.Find("GoTycoonButton").GetComponent<Button>();
        _goTycoonButton.onClick.AddListener(OnGoTycoonButtonPressed);
    }

    private void OnGoTycoonButtonPressed()
    {
        SceneManager.LoadScene(1);
    }
}
