using UnityEngine;

public class AmmoPowerUp : MonoBehaviour
{
    public int ammoAmount = 40;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Projectile gun = player.GetComponentInChildren<Projectile>();
                if (gun != null)
                {
                    gun.AddAmmo(ammoAmount);
                    GameManager.Instance.RegenerateAmmo(transform.position);
                    Destroy(gameObject);
                }
            }
        }
    }
}