using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public bool IsInvisible { get; private set; }

    public float maxHealth = 50f;
    public float currentHealth;
    public float firstAidAmount = 15f;

    public TextMeshProUGUI healthDisplay;
    public Slider invisibilitySlider;
    public GameObject invisibilityUI;

    private float invisibilityEndTime = 0f;
    
    private List<AudioSource> _audioSources = new List<AudioSource>();
    [SerializeField] private AudioClip ammo;
    [SerializeField] private AudioClip firstAid;
    [SerializeField] private AudioClip invisibility;
    [SerializeField] private AudioClip playerHit;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        currentHealth = maxHealth;
        
        if (invisibilityUI != null)
        {
            invisibilityUI.SetActive(false);
        }
        
        for (int i = 0; i < 4; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            _audioSources.Add(source);
        }
    }

    private void Update()
    {
        if (IsInvisible)
        {
            if (Time.time > invisibilityEndTime)
            {
                IsInvisible = false;
                ToggleInvisibilityUI(false);
            }
            else
            {
                float remainingTime = invisibilityEndTime - Time.time;
                if (invisibilitySlider != null)
                {
                    invisibilitySlider.value = remainingTime;
                }
            }
        }
        
        if (healthDisplay != null)
            healthDisplay.SetText(currentHealth + " / " + maxHealth);
    }

    public void ToggleInvisibility(float duration)
    {
        IsInvisible = true;
        PlaySound(invisibility);
        invisibilityEndTime = Time.time + duration;
        ToggleInvisibilityUI(true);
        if (invisibilitySlider != null)
        {
            invisibilitySlider.maxValue = duration;
            invisibilitySlider.value = duration;
        }
    }

    private void ToggleInvisibilityUI(bool state)
    {
        if (invisibilityUI != null)
        {
            invisibilityUI.SetActive(state);
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Healed! Current Health: {currentHealth}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FirstAid"))
        {
            Heal(firstAidAmount);
            PlaySound(firstAid);
            Destroy(other.gameObject);
        }
        
        if (other.CompareTag("Ammo"))
        {
            PlaySound(ammo);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy Bullet"))
        {
            TakeDamage(1);
            Debug.Log("Hit by enemy");
            PlaySound(playerHit);
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
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
            Debug.LogWarning("No available AudioSource player to play the sound.");
        }
    }
}
