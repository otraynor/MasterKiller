using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public float health;
    public float maxHealth;
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

    public AudioSource breatheAudioSource;
    public AudioClip breatheClip;

    private List<AudioSource> _audioSources = new List<AudioSource>();
    [SerializeField] AudioClip shoot;
    [SerializeField] AudioClip beenHit;
    [SerializeField] AudioClip die;

    [SerializeField] private FloatingHealthBar healthBar;
    private AudioSource breatheSource;

    private bool isPlayerInvisible;

    private void Awake()
    {
        healthBar = GetComponentInChildren<FloatingHealthBar>();

        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        
        for (int i = 0; i < 4; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            _audioSources.Add(source);
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(agent.transform.position, out hit, 10.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        if (breatheAudioSource != null && breatheClip != null)
        {
            breatheAudioSource.clip = breatheClip;
            breatheAudioSource.loop = true;
            breatheAudioSource.pitch = Random.Range(0.85f, 1.15f);
            breatheAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Breathe AudioSource or Clip is not assigned.");
        }
    }

    private void Start()
    {
        health = maxHealth;
        healthBar.UpdateHealthBar(health, maxHealth);
    }

    private void Update()
    {
        isPlayerInvisible = PlayerController.Instance != null && PlayerController.Instance.IsInvisible;

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
        {
            Patrolling();
        }
        else if (playerInSightRange && !playerInAttackRange && !isPlayerInvisible)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange && playerInSightRange && !isPlayerInvisible)
        {
            AttackPlayer();
        }
        else if (isPlayerInvisible)
        {
            agent.SetDestination(transform.position);
        }
    }

    private void Patrolling()
    {
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
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(transform.position);
        }
        else
        {
            Debug.LogError("NavMeshAgent is not on the NavMesh or is not enabled!");
        }

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
        PlaySound(shoot);
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
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        healthBar.UpdateHealthBar(health, maxHealth);
        
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        agent.enabled = false;
        
        if (breatheSource != null)
        {
            breatheSource.Stop();
        }
        
        PlaySound(die);
        
        EnemySpawnManager spawnManager = GetComponentInParent<EnemySpawnManager>();
        if (spawnManager != null)
        {
            spawnManager.EnemyDied(gameObject);
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDied(gameObject);
        }
        
        StartCoroutine(ScaleDownAndDestroy(0.4f));
    }

    private IEnumerator ScaleDownAndDestroy(float duration)
    {
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(initialScale.x, 0f, initialScale.z);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = targetScale;
        
        Destroy(transform.parent.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player Bullet"))
        {
            TakeDamage(1);
            PlaySound(beenHit);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (var source in _audioSources)
        {
            if (!source.isPlaying)
                return source;
        }
        return null;
    }

    void PlaySound(AudioClip clip)
    {
        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {
            source.clip = clip;
            source.Play();
        }
        else
        {
            Debug.LogWarning("No available AudioSource to play the sound.");
        }
    }
}
