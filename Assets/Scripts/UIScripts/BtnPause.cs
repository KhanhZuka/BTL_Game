using UnityEngine;
using UnityEngine.UI;
public class BtnPause : MonoBehaviour
{
    [SerializeField] private Button btnPause;
    [SerializeField] private GameObject panelPause;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        btnPause.onClick.AddListener(OpenPanelPause);
    }

    void OpenPanelPause()
    {
        panelPause.SetActive(true);
    }


}
