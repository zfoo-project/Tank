using Spring.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILoginMono : MonoBehaviour
{
    public void LoginClick()
    {
        var gameInput = GameObject.Find("NameInput");
        var inputText = gameInput.GetComponentInChildren<Text>();
        Log.Info(inputText);
        Log.Info(inputText.text);

        SceneManager.LoadSceneAsync("Assets/Test/Unity/UI/UIGameScene.unity");
    }
}