using UnityEngine;
using UnityEngine.UI;
public class PanelInstruct : MonoBehaviour
{
    [SerializeField] private Button btnX;

    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelInstruct;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btnX.onClick.AddListener(OpenInstruction);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OpenInstruction()
    {
        panelInstruct.SetActive(false);
        panelMain.SetActive(true);     
    }
}
