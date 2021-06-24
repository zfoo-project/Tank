using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuUserInterface : MonoBehaviour {

	public void LoadGame() {
		SceneManager.LoadScene ("Level1");
	}
}
