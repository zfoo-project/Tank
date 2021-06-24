using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UserInterface : MonoBehaviour {

	public Unit player;
	public Image healthBar;
	public Text healthLabel;

	public Unit boss;
	public Image foreRed;
	public Text bossText;

	void Start() {
		StartCoroutine (Timer ());
	}

	IEnumerator Timer() {
		while (enabled) {
			yield return new WaitForSeconds (1);
			if (player.GetCurHealth () < 100 && player.GetCurHealth () > 0) {
				player.SetHealth (player.GetCurHealth () + 1);
			}
		}
	}

	void Update () {
		if (player) {
			healthBar.fillAmount = (float)player.GetCurHealth () / player.health;
			healthLabel.text = player.GetCurHealth ().ToString ();
		} else {
			healthBar.fillAmount = 0;
			healthLabel.text = "0";
		}

		if (boss) {
			foreRed.fillAmount = (float)boss.GetCurHealth () / boss.health;
			bossText.text = boss.GetCurHealth ().ToString () + "/2000";
		} else {
			foreRed.fillAmount = 0;
			bossText.text = "0/2000";
		}
	}
}
