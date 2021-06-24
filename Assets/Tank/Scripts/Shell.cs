using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour {

	public int damage;
	public float explosionForce;
	public float explosionRadius;
	public GameObject explosionEffect;
	public float explosionTimeUp;

	private LayerMask lm;

	public void Init(LayerMask enemyLayer) {
		lm = enemyLayer;
	}

	void OnCollisionEnter() {
		GameObject explosion =  Instantiate (explosionEffect, transform.position, transform.rotation) as GameObject;
		Destroy (gameObject);
		Destroy (explosion, explosionTimeUp);

		Collider[] cols = Physics.OverlapSphere (transform.position, explosionRadius);
		if (cols.Length > 0) {
			for (int i = 0; i < cols.Length; i++) {
				Rigidbody r = cols [i].GetComponent<Rigidbody> ();
				if (r) {
					r.AddExplosionForce (explosionForce, transform.position, explosionRadius);
				}
			}
		}

		Collider[] colss = Physics.OverlapSphere (transform.position, explosionRadius, lm);
		if (colss.Length > 0) {
			for (int i = 0; i < colss.Length; i++) {
				Unit u = colss [i].GetComponent<Unit> ();
				if (u) {
					u.ApplyDamage (damage);
				}
			}
		}
	}
}
