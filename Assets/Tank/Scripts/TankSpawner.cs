using UnityEngine;
using System.Collections;

public class TankSpawner : MonoBehaviour {

	public GameObject prefab;
	public float coolDown;

	void Start () {
		StartCoroutine (Timer ());
	}

	IEnumerator Timer() {
		while (enabled) {
			yield return new WaitForSeconds (coolDown);
			Instantiate (prefab, transform.position, transform.rotation);
		}
	}
}
