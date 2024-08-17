using UnityEngine;
using UnityEngine.UI;

public class MainMenuQuitButton : MonoBehaviour
{
    [SerializeField] private Button quitButton;
    [SerializeField] private QuitGame quitGame;

    private void Start()
    {
        if (quitButton != null && quitGame != null)
        {
            quitButton.onClick.AddListener(quitGame.QuitMasterKiller);
        }
    }
}