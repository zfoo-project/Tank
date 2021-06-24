using UnityEngine;
using System.Collections;

public class BossWeapon : MonoBehaviour {

	public GameObject shell;
	public float shootPower;
	public Transform shootPoint;

	private AudioSource audioSource;
	private LayerMask enemyLayer;

	void Start() {
		audioSource = GetComponent<AudioSource> ();
	}

	public void Init(Team team) {
		enemyLayer = LayerManager.GetEnemyLayer (team);
	}

	public void Shoot() {
		GameObject newShell = Instantiate (shell, shootPoint.position, shootPoint.rotation) as GameObject;
		newShell.GetComponent<Shell> ().Init (enemyLayer);
		Rigidbody r = newShell.GetComponent<Rigidbody> ();
		//r.velocity = shootPoint.forward * shootPower;
		r.velocity = shootPoint.forward * shootPower;
		audioSource.Play ();
	}

	public void SetPower(float power) {
		shootPower = power;
	}
}
