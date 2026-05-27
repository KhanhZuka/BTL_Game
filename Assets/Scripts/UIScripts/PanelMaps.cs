using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelMaps : MonoBehaviour
{
    [SerializeField] private Button btnComeBack;
    [SerializeField] private Button btnMan1;
    [SerializeField] private Button btnMan2;
    [SerializeField] private Button btnMan3;
    [SerializeField] private Button btnMan4;

    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelMap;

    void Start()
    {
        btnComeBack.onClick.AddListener(OpenMainScreen);

        btnMan1.onClick.AddListener(OpenMapOne);
        btnMan2.onClick.AddListener(OpenMapTwo);
        btnMan3.onClick.AddListener(OpenMapThree);
        btnMan4.onClick.AddListener(OpenMapFour);
    }

    void OpenMapOne()
    {
        SceneManager.LoadScene("Map1_Scene");
    }

    void OpenMapTwo()
    {
        SceneManager.LoadScene("Map2_Scene");
    }

    void OpenMapThree()
    {
        SceneManager.LoadScene("Map3_Scene");
    }

    void OpenMapFour()
    {
        SceneManager.LoadScene("Map4_Scene");
    }

    void OpenMainScreen()
    {
        panelMap.SetActive(false);
        panelMain.SetActive(true);
    }
}