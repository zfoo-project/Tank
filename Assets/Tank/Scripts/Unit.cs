using UnityEngine;
using System.Collections;

public enum Team {
	Blue, Gold, White
}

public class Unit : MonoBehaviour {

	public int health = 100;
	public Team team;
	public GameObject deadEffect;

	private int curHealth;

	public int GetCurHealth() {
		return curHealth;
	}

	public void SetHealth(int health) {
		curHealth = health;
	}

	public void StartTo() {
		curHealth = health;
	}

	public void ApplyDamage(int damage) {
		if (curHealth > damage) {
			curHealth -= damage;
		} else {
			Destruct ();
		}
	}

	public void Destruct() {
		if (deadEffect) {
			Instantiate (deadEffect, transform.position, transform.rotation);
		}
		Destroy (gameObject);
	}
}
