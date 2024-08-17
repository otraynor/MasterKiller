using UnityEngine;

public class FirstAidPowerUp : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Heal(15);
                GameManager.Instance.RegenerateHealth(transform.position);
                Destroy(gameObject);
            }
        }
    }
}