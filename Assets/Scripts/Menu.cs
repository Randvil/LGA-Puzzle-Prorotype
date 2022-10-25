using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenuCanvas;

    [SerializeField]
    private GameObject victoryMenuCanvas;

    [SerializeField]
    private GameObject pauseMenuCanvas;

    [SerializeField]
    private TMP_Dropdown levelDropdown;

    [SerializeField]
    private TextMeshProUGUI moveCounter;

    public List<string> TextOptions { get; set; } = new();
    public List<TMP_Dropdown.OptionData> Options { get; set; } = new();

    private void Start()
    {
        GameManager.LoadPlaygroundsEvent.AddListener(OnPlaygroundsLoad);
        GameManager.WinEvent.AddListener(OnWin);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!mainMenuCanvas.activeSelf && !victoryMenuCanvas.activeSelf) ToggleMenu(pauseMenuCanvas);
        }
    }

    private void ToggleMenu(GameObject menuCanvas)
    {
        menuCanvas.SetActive(!menuCanvas.activeSelf);
    }

    private void OnPlaygroundsLoad(List<string> playgroundNames)
    {
        foreach (string playgroundName in playgroundNames)
        {
            levelDropdown.options.Add(new TMP_Dropdown.OptionData(playgroundName));
        }
        levelDropdown.captionText.text = "Select Level";
    }

    public void OnSelectLevel(int levelNumber)
    {
        GameManager.Instance.SelectLevel(levelNumber);
    }

    public void OnStartLevel()
    {
        if (levelDropdown.value != -1) ToggleMenu(mainMenuCanvas);
    }

    public void OnWin(int moves)
    {
        pauseMenuCanvas.SetActive(false);
        victoryMenuCanvas.SetActive(true);
        moveCounter.text = $"Moves: {moves}";
    }

    public void OnNextLevel()
    {
        levelDropdown.value += 1;
        GameManager.Instance.SelectLevel(levelDropdown.value);
        victoryMenuCanvas.SetActive(false);
    }

    public void OnMainMenu()
    {
        GameManager.Instance.SelectLevel(levelDropdown.value);
        victoryMenuCanvas.SetActive(false);
        pauseMenuCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }

    public void OnResume()
    {
        pauseMenuCanvas.SetActive(false);
    }

    public void OnExitGame()
    {
        Application.Quit();
    }

}
