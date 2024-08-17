using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public float invisibilityDuration = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = PlayerController.Instance;
            if (playerController != null)
            {
                playerController.ToggleInvisibility(invisibilityDuration);
                GameManager.Instance.RegeneratePowerUp(transform.position);
                Destroy(gameObject);
            }
        }
    }
}