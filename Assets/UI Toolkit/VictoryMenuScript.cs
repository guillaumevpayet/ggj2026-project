using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class VictoryMenuEvents : MonoBehaviour
{
    private UIDocument _document;

    private Button _retryButton;
    private Button _menuButton;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();

        _retryButton = _document.rootVisualElement.Q("ReplayButton") as Button;
        _retryButton.RegisterCallback<ClickEvent>(OnReplayGameClick);

        _menuButton = _document.rootVisualElement.Q("MenuButton") as Button;
        _menuButton.RegisterCallback<ClickEvent>(OnMenuButtonClick);
    }

    private void OnEnable()
    {
        _document = GetComponent<UIDocument>();

        _retryButton = _document.rootVisualElement.Q("ReplayButton") as Button;
        _retryButton.RegisterCallback<ClickEvent>(OnReplayGameClick);

        _menuButton = _document.rootVisualElement.Q("MenuButton") as Button;
        _menuButton.RegisterCallback<ClickEvent>(OnMenuButtonClick);
    }


    private void OnDisable()
    {
        _retryButton.UnregisterCallback<ClickEvent>(OnReplayGameClick);
        _menuButton.UnregisterCallback<ClickEvent>(OnMenuButtonClick);
    }

    private void OnMenuButtonClick(ClickEvent evt)
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnReplayGameClick(ClickEvent evt)
    {
        SceneManager.LoadScene("MainScene");
    }

}