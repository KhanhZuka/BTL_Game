using UnityEngine;
using UnityEngine.UI;

public class BtnPause : MonoBehaviour
{
    [SerializeField] private Button btnPause;
    [SerializeField] private GameObject panelPause;

    void Awake()
    {
        if (btnPause == null)
        {
            btnPause = GetComponent<Button>();
        }
    }

    void Start()
    {
        btnPause.onClick.AddListener(OpenPanelPause);
    }

    void OpenPanelPause()
    {
        Time.timeScale = 0f;
        panelPause.SetActive(true);
    }
}