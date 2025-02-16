using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject failScreen;
    [SerializeField] private float failScreenDelay = 1f;

    public void ShowFailScreen()
    {
        Invoke(nameof(EnableFailScreen), failScreenDelay);
    }
    
    private void EnableFailScreen()
    {
        failScreen.SetActive(true);
    }
}
