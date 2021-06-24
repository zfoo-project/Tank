using UnityEngine;
using System.Collections;

public class BossAI : Unit {

	public float enemySearchRange;
	public float attackRange;
	public float moveSpeed;
	public float rotateSpeed;
	public float shootCoolDown;

	private AudioSource[] BGM;
	private GameObject enemy;
	private float timer;
	private BossWeapon tw;
	private UnityEngine.AI.NavMeshAgent nma;
	private LayerMask enemyLayer;

	void Start() {
		base.StartTo ();
		if (enemySearchRange == 0)
			return;
		enemyLayer = LayerManager.GetEnemyLayer (team);
		tw = GetComponent<BossWeapon> ();
		nma = GetComponent<UnityEngine.AI.NavMeshAgent> ();
		tw.Init (team);
		BGM = GetComponents<AudioSource>();
	}

	void Update () {
		timer += Time.deltaTime;

		if (!enemy) {
			SearchEnemy ();
			return;
		}
		if (!BGM [1].isPlaying) {
			BGM [1].Play ();
		}

		float dist = Vector3.Distance (enemy.transform.position, transform.position);

		Vector3 dir = enemy.transform.position - transform.position;
		Quaternion wantedRotation = Quaternion.LookRotation (dir);
		transform.rotation = wantedRotation;

		if (dist > attackRange) {
			nma.SetDestination (enemy.transform.position);
			//transform.Translate (Vector3.forward * moveSpeed * Time.deltaTime);
		} else {
			nma.ResetPath ();
			//transform.LookAt (player.transform.position);
			if (timer > shootCoolDown && shootCoolDown != 0) {
				shootCoolDown = (float)(Random.Range (1, 10) * 0.1);
				tw.SetPower (dist * 1.2f);
				tw.Shoot ();
				SearchEnemy ();
				timer = 0;
			}
		}
	}

	public void SearchEnemy() {
		Collider[] cols = Physics.OverlapSphere (transform.position, enemySearchRange, enemyLayer);
		if (cols.Length > 0) {
			enemy = cols [Random.Range (0, cols.Length)].gameObject;
		}
	}
}
