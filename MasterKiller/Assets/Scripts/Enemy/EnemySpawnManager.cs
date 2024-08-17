using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private int totalEnemies;
    private int enemiesRemaining;

    private void Start()
    {
        totalEnemies = transform.childCount;
        enemiesRemaining = totalEnemies;
        
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Enemy"))
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    public void EnemyDied(GameObject enemy)
    {
        enemiesRemaining--;
        
        if (enemiesRemaining <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnemyDied(enemy);
            }
            Destroy(gameObject);
            if (GameManager.Instance != null && GameManager.Instance.enemySpawnPrefab != null)
            {
                Instantiate(GameManager.Instance.enemySpawnPrefab, Vector3.zero, Quaternion.identity);
            }
        }
    }
}