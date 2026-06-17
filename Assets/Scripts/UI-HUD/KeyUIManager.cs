using UnityEngine;

public class KeyUIManager : MonoBehaviour
{
    public static KeyUIManager Instance;
    public GameObject keyIcon;
    void Awake()
    {
        Instance = this;
    }
    public void ShowKey()
    {
        keyIcon.SetActive(true);
    }
    public void HideKey()
    {
        keyIcon.SetActive(false);
    }
}
