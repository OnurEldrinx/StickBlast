using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject failScreen;
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private float failScreenDelay = 1f;
    public TMP_InputField gridWidthInput;
    public TMP_InputField gridHeightInput;
    
    
    public Action<int,int> OnPlayButtonClicked;
    
    public void ShowFailScreen()
    {
        Invoke(nameof(EnableFailScreen), failScreenDelay);
    }
    
    private void EnableFailScreen()
    {
        failScreen.SetActive(true);
    }

    public async void HandlePlayButtonClicked()
    {
        try
        {
            if (gridWidthInput.text == "" || gridHeightInput.text == "") return;
            int x = int.Parse(gridWidthInput.text);
            int y = int.Parse(gridHeightInput.text);
            OnPlayButtonClicked?.Invoke(x,y);
            await Task.Delay(250);
            homeScreen.SetActive(false);
        }
        catch (Exception)
        {
            //ignored
        }
    }
}
