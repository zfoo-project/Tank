using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using MiniKing.Script.Constant;
using MiniKing.Script.Procedure.Scene;
using Spring.Core;

public class GameOver : MonoBehaviour
{
	public GameObject player;
	public GameObject boss;
	public AudioSource winMusic;

	void Start() {
		winMusic = GetComponent<AudioSource> ();
	}

	void Update ()
	{
		if (!player)
			StartCoroutine (PlayerTimer ());

		if (!boss)
			StartCoroutine (BossTimer ());
	}

	IEnumerator PlayerTimer() {
		yield return new WaitForSeconds (10);
		SpringContext.GetBean<ProcedureChangeScene>().ChangeScene(SceneEnum.Menu);
	}

	IEnumerator BossTimer() {
		if (!winMusic.isPlaying)
			winMusic.Play ();
		
		yield return new WaitForSeconds (60);
		SpringContext.GetBean<ProcedureChangeScene>().ChangeScene(SceneEnum.Menu);
	}
}

