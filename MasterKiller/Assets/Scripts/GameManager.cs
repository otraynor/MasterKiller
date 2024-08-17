using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public static int enemiesKilled = 0;
    public int enemyWave = 1;
    public int totalKills = 0;

    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] public GameObject enemySpawnPrefab;
    
    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private TextMeshProUGUI enemiesLeft;
    [SerializeField] private TextMeshProUGUI wave;
    [SerializeField] private TextMeshProUGUI enemiesDefeated;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private MouseLook mouseLookCam;
    [SerializeField] private MouseLook mouseLookPlayer;
    
    [SerializeField] private QuitGame quitGame;
    [SerializeField] private CursorLockHide cursorLockHide;

    private Vector3[] powerUpSpawnPoints = new Vector3[]
    {
        new Vector3(-95, 1, 75),
        new Vector3(97, 1, 58),
        new Vector3(-110, 1, -103),
        new Vector3(105, 1, -80)
    };

    private Vector3[] ammoSpawnPoints = new Vector3[]
    {
        new Vector3(-95, 1, 75),
        new Vector3(97, 1, 58),
        new Vector3(-110, 1, -118),
        new Vector3(105, 1, -81),
        new Vector3(-79, 1, -19),
        new Vector3(-12, 1, 111),
        new Vector3(111, 1, 106),
        new Vector3(143, 1, 35),
        new Vector3(2, 1, 2),
    };

    private Vector3[] healthSpawnPoints = new Vector3[]
    {
        new Vector3(-75, 1, -19),
        new Vector3(-9, 1, 120),
        new Vector3(100, 1, 110),
        new Vector3(138, 1, 32),
        new Vector3(145, 1, -67),
        new Vector3(89, 1, -142),
        new Vector3(-17, 1, -141),
        new Vector3(-51, 1, -87),
        new Vector3(-53, 1, -13),
        new Vector3(44, 1, 16)
    };

    private List<GameObject> activeEnemies = new List<GameObject>();
    private const int totalEnemies = 10;
    private bool waveInProgress = false;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (Vector3 spawnPoint in powerUpSpawnPoints)
        {
            StartCoroutine(SpawnObjectAtInterval(powerUpPrefab, spawnPoint, 60f));
        }

        foreach (Vector3 spawnPoint in ammoSpawnPoints)
        {
            StartCoroutine(SpawnObjectAtInterval(ammoPrefab, spawnPoint, 60f));
        }

        foreach (Vector3 spawnPoint in healthSpawnPoints)
        {
            StartCoroutine(SpawnObjectAtInterval(healthPrefab, spawnPoint, 60f));
        }
        
        StartCoroutine(SpawnEnemyWave());
        
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            quitGame.QuitMasterKiller();
        }
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            EnablePlayerControls(true);
            EnableMouseLook(true);
            cursorLockHide.enabled = true;
            pauseMenu.SetActive(false);
            isPaused = false;
        }
        else
        {
            Time.timeScale = 0f;
            EnablePlayerControls(false);
            EnableMouseLook(false);
            cursorLockHide.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            pauseMenu.SetActive(true);
            isPaused = true;
        }
    }

    private void EnablePlayerControls(bool enable)
    {
        if (playerController != null)
        {
            playerController.enabled = enable;
        }
    }

    private void EnableMouseLook(bool enable)
    {
        if (mouseLookCam != null)
        {
            mouseLookCam.enabled = enable;
        }

        if (mouseLookPlayer != null)
        {
            mouseLookPlayer.enabled = enable;
        }
    }

    private IEnumerator SpawnObjectAtInterval(GameObject prefab, Vector3 spawnPoint, float interval)
    {
        while (true)
        {
            GameObject spawnedObject = Instantiate(prefab, spawnPoint, Quaternion.identity);
            yield return new WaitForSeconds(interval);
            Destroy(spawnedObject);
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator SpawnEnemyWave()
    {
        if (waveInProgress)
        {
            yield break;
        }

        waveInProgress = true;
        
        GameObject oldEnemySpawn = GameObject.Find("EnemySpawn");
        if (oldEnemySpawn != null)
        {
            Destroy(oldEnemySpawn);
        }
        
        Instantiate(enemySpawnPrefab, Vector3.zero, Quaternion.identity);
        
        if (enemiesKilled >= totalEnemies)
        {
            enemyWave++;
            enemiesKilled = 0;
            Debug.Log("Enemy Wave: " + enemyWave);
        }
        
        UpdateUI();
        
        yield return new WaitForSeconds(5f);

        waveInProgress = false;
    }

    public void RegeneratePowerUp(Vector3 position)
    {
        StartCoroutine(RegenerateObjectCoroutine(powerUpPrefab, position));
    }

    public void RegenerateAmmo(Vector3 position)
    {
        StartCoroutine(RegenerateObjectCoroutine(ammoPrefab, position));
    }

    public void RegenerateHealth(Vector3 position)
    {
        StartCoroutine(RegenerateObjectCoroutine(healthPrefab, position));
    }

    private IEnumerator RegenerateObjectCoroutine(GameObject prefab, Vector3 position)
    {
        yield return new WaitForSeconds(60f);
        Instantiate(prefab, position, Quaternion.identity);
    }
    
    public void EnemyDied(GameObject enemy)
    {
        enemiesKilled++;
        activeEnemies.Remove(enemy);
        UpdateUI();
        
        if (activeEnemies.Count == 0)
        {
            StartCoroutine(SpawnEnemyWave());
        }
    }

    public void RegisterEnemy(GameObject enemy)
    {
        activeEnemies.Add(enemy);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (enemiesLeft != null)
        {
            enemiesLeft.text = "ENEMIES LEFT: " + (totalEnemies - enemiesKilled);
        }
        if (wave != null)
        {
            wave.text = "WAVE: " + enemyWave;
        }
        if (enemiesDefeated != null)
        {
            enemiesDefeated.text = "KILLS: " + enemiesKilled;
        }
    }
}
