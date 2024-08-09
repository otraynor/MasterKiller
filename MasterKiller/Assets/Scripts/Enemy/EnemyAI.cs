using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public float health;
    public float timeBetweenAttacks;
    public bool alreadyAttacked;
    
    public GameObject bullet;
    public Transform attackPoint;
    public float shootForce, upwardForce;
    public float spread;
    public GameObject muzzleFlash;
    
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    
    public Vector3 walkPoint;
    public bool walkPointSet;
    public float walkPointRange;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        NavMeshHit hit;
        if (NavMesh.SamplePosition(agent.transform.position, out hit, 10.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            Debug.Log("Agent placed on NavMesh at: " + hit.position);
        }
        else
        {
            Debug.LogError("Agent is NOT on the NavMesh!");
        }
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        Debug.Log($"Player in sight range: {playerInSightRange}, Player in attack range: {playerInAttackRange}");

        if (!playerInSightRange && !playerInAttackRange)
        {
            Patrolling();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange && playerInSightRange)
        {
            AttackPlayer();
        }
    }

    private void Patrolling()
    {
        Debug.Log("Patrolling");
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        Debug.Log("ChasePlayer");

        float distanceToPlayer = Vector3.Distance(agent.transform.position, player.position);

        if (distanceToPlayer > agent.stoppingDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.velocity = Vector3.Lerp(agent.velocity, Vector3.zero, Time.deltaTime * 2);
        }
    }

    private void AttackPlayer()
    {
        Debug.Log("AttackPlayer");

        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            Shoot();

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void Shoot()
    {
        Ray ray = new Ray(attackPoint.position, player.position - attackPoint.position);
        RaycastHit hit;
        
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);
        
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;
        
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);
        
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
        
        currentBullet.transform.forward = directionWithSpread.normalized;
        
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(transform.up * upwardForce, ForceMode.Impulse);
        
        Destroy(currentBullet, 2f);
        
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player Bullet"))
        {
            TakeDamage(2);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
