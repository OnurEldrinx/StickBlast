using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject failScreen;

    public void ShowFailScreen()
    {
        failScreen.SetActive(true);
    }
}
