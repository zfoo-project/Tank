using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameWin : MonoBehaviour {
	public GameObject boss;

	void Update ()
	{
		if (!boss)
			StartCoroutine (Timer ());
	}

	IEnumerator Timer() {
		yield return new WaitForSeconds (10);
		SceneManager.LoadScene (0);
	}
}
