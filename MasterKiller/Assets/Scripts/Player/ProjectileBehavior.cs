using UnityEngine;
using TMPro;

public class Projectile : MonoBehaviour
{
    public GameObject bullet;

    public float shootForce, upwardForce;
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    public int maxBullets = 160;
    private int bulletsLeft, bulletsShot, totalBullets;

    public Rigidbody playerRb;
    public float recoilForce;

    bool shooting, readyToShoot, reloading;

    public Camera fpsCam;
    public Transform attackPoint;

    public TextMeshProUGUI ammunitionDisplay;

    public bool allowInvoke = true;

    private AudioSource[] _sources;
    [SerializeField] AudioClip shoot;
    [SerializeField] AudioClip reload;

    private void Awake()
    {
        bulletsLeft = magazineSize;
        totalBullets = maxBullets - magazineSize;
        readyToShoot = true;
        
        _sources = GetComponents<AudioSource>();
    }

    private void Update()
    {
        MyInput();

        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + totalBullets / bulletsPerTap);
    }

    private void MyInput()
    {
        if (Time.timeScale == 0f) return;

        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
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
        currentBullet.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * upwardForce, ForceMode.Impulse);

        Destroy(currentBullet, 2f);

        currentBullet.AddComponent<BulletCollision>();
        
        PlaySound(shoot);

        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;

            playerRb.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        PlaySound(reload);
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        int bulletsToReload = Mathf.Min(magazineSize, totalBullets);
        bulletsLeft = bulletsToReload;
        totalBullets -= bulletsToReload;
        reloading = false;
    }

    private class BulletCollision : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        foreach (AudioSource source in _sources)
        {
            if (!source.isPlaying)
            {
                source.clip = clip;
                source.Play();
                break;
            }
        }
    }

    public void AddAmmo(int ammoAmount)
    {
        totalBullets = Mathf.Min(totalBullets + ammoAmount, maxBullets - magazineSize);
    }
}
