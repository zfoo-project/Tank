using MiniKing.Script.Procedure.Scene;
using Spring.Core;
using UnityEngine;
using UnityEngine.AI;

public class AITank : Unit
{
    public float enemySearchRange;
    public float attackRange;
    public float moveSpeed;
    public float rotateSpeed;
    public float shootCoolDown;

    private GameObject enemy;
    private float timer;
    private TankWeapon tw;
    private NavMeshAgent nma;
    private LayerMask enemyLayer;

    private int originHealth;

    void Start()
    {
        base.StartTo();
        if (enemySearchRange <= 0)
        {
            return;
        }

        enemyLayer = LayerManager.GetEnemyLayer(team);
        tw = GetComponent<TankWeapon>();
        nma = GetComponent<NavMeshAgent>();
        tw.Init(team);
        originHealth = health;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!enemy)
        {
            SearchEnemy();
            return;
        }

        float dist = Vector3.Distance(enemy.transform.position, transform.position);

        Vector3 dir = enemy.transform.position - transform.position;
        Quaternion wantedRotation = Quaternion.LookRotation(dir);
        transform.rotation = wantedRotation;

        if (dist > attackRange)
        {
            nma.SetDestination(enemy.transform.position);
            //transform.Translate (Vector3.forward * moveSpeed * Time.deltaTime);
        }
        else
        {
            nma.ResetPath();
            //transform.LookAt (player.transform.position);
            if (timer > shootCoolDown && shootCoolDown != 0)
            {
                tw.Shoot();
                SearchEnemy();
                timer = 0;
            }
        }
    }

    public void SearchEnemy()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, enemySearchRange, enemyLayer);
        if (cols.Length > 0)
        {
            enemy = cols[Random.Range(0, cols.Length)].gameObject;
        }
    }

    private void OnDestroy()
    {
        if (SpringContext.GetScanFlag())
        {
            SpringContext.GetBean<ProcedureLevel1>().addScore(originHealth);
        }
    }
}