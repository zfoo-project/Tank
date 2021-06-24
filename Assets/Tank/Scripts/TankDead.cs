using UnityEngine;
using System.Collections;

public class TankDead : MonoBehaviour {
	void Start() {
		float x = Random.Range (-10, 10);
		float y = Random.Range (0, 10);
		float z = Random.Range (-10, 10);

		Rigidbody r = gameObject.GetComponent<Rigidbody> ();
		r.velocity = gameObject.transform.TransformVector(x, y, z);
	}
}
