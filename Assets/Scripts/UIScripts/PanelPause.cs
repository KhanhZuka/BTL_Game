using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PanelPause : MonoBehaviour
{
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnPlayAgain;
    [SerializeField] private Button btnQuit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btnQuit.onClick.AddListener(OpenUIScene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OpenUIScene()
    {
        GameData.backToMapPanel = true;
        SceneManager.LoadScene("UIScene");
    }
}
