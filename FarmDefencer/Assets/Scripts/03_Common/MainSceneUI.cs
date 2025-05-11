using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainSceneUI : MonoBehaviour
{
    private Button _goTycoonButton;
    
    private void Awake()
    {
        _goTycoonButton = transform.Find("GoTycoonButton").GetComponent<Button>();
        _goTycoonButton.onClick.AddListener(OnTycoonButtonClickedHandler);
    }

    private void OnTycoonButtonClickedHandler()
    {
        SceneManager.LoadScene(1);
    }
}
