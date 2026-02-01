using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour
{
    private UIDocument _document;

    private Button _startButton;
    private Button _exitButton;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();

        _startButton = _document.rootVisualElement.Q("PlayButton") as Button;
        _startButton.RegisterCallback<ClickEvent>(OnPlayGameClick);

        _exitButton = _document.rootVisualElement.Q("ExitButton") as Button;
        _exitButton.RegisterCallback<ClickEvent>(OnExitButtonClick);
    }

    private void OnEnable()
    {
        _document = GetComponent<UIDocument>();

        _startButton = _document.rootVisualElement.Q("StartButton") as Button;
        _startButton.RegisterCallback<ClickEvent>(OnPlayGameClick);

        _exitButton = _document.rootVisualElement.Q("ExitButton") as Button;
        _exitButton.RegisterCallback<ClickEvent>(OnExitButtonClick);
    }

    private void OnExitButtonClick(ClickEvent evt)
    {
        Debug.Log("EXIT BUTTON PRESSED!");
        Application.Quit();
    }

    private void OnPlayGameClick(ClickEvent evt)
    {
        SceneManager.LoadScene("MainScene");
    }
}