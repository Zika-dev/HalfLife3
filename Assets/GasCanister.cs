using UnityEngine;
using System.Collections;

public class GasCanister : MonoBehaviour
{
    public float gasReleaseRate = 5f;
    public float maxSpeed = 20f;
    public float accelerationFactor = 1.5f;
    public float destructionSpeedThreshold = 10f;
    public GameObject gasOutlet;
    public GameObject Player;
    public float explosionRadius;
    public float explosionForce;
    public Transform gasOutletTransform;

    public ParticleSystem gasParticles;
    private ParticleSystem.ShapeModule shapeModule;
    private ParticleSystem.EmissionModule emissionModule;
    public float maxAngle = 60f;
    public float minAngle = 15f;

    public GameObject explosionEffectPrefab;
    public GameObject replacementPrefab;

    private bool isReleasingGas = false;
    private float currentSpeed = 0f;
    private Rigidbody2D rb;
    private Rigidbody2D playerRb2;
    private DamageBehavior damageController;

    void Start()
    {

        SetupParticleGradient();

        playerRb2 = Player.GetComponent<Rigidbody2D>();
        damageController = Player.GetComponent<DamageBehavior>();
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;

        if (gasParticles == null)
        {
            gasParticles = GetComponentInChildren<ParticleSystem>();
        }
        if (gasParticles != null)
        {
            shapeModule = gasParticles.shape;
            emissionModule = gasParticles.emission;
            gasParticles.Stop();
        }
    }

    void Update()
    {
        if (isReleasingGas)
        {
            currentSpeed += Time.deltaTime * gasReleaseRate;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            Vector2 forceDirection = -gasOutletTransform.up;
            rb.AddForce(forceDirection * currentSpeed * accelerationFactor);
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);

            if (gasParticles != null)
            {
                float speedRatio = rb.linearVelocity.magnitude / maxSpeed;

                float emissionRate = Mathf.Lerp(1000, 2000, speedRatio);
                emissionModule.rateOverTime = emissionRate;

                var mainModule = gasParticles.main;
                mainModule.startLifetime = Mathf.Lerp(2f, 4f, speedRatio);  
                mainModule.startSpeed = Mathf.Lerp(5f, 10f, speedRatio);   

                var sizeOverLifetime = gasParticles.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0)); 

                shapeModule.angle = Mathf.Lerp(minAngle, 15f, speedRatio);

                gasParticles.transform.position = gasOutletTransform.position;
                gasParticles.transform.rotation = gasOutletTransform.rotation;
            }
        }
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude > destructionSpeedThreshold)
        {
            SelfDestruct();
        }
        else
        {
            StartReleasingGas();
        }
    }

    private void StartReleasingGas()
    {
        isReleasingGas = true;
        if (gasParticles != null)
        {
            gasParticles.Play();
        }
        Debug.Log("Gas release started!");
    }

    private void StopReleasingGas()
    {
        isReleasingGas = false;
        currentSpeed = 0f;
        if (gasParticles != null)
        {
            gasParticles.Stop();
        }
        Debug.Log("Gas release stopped!");
    }

    private void SelfDestruct()
    {
        Debug.Log("About to explode");
        CreateExplosion();
        InstantiateExplosionEffect();

        if (replacementPrefab != null)
        {
            GameObject splinteredCanister = Instantiate(replacementPrefab, transform.position, transform.rotation);

            Rigidbody2D[] childRigidbodies = splinteredCanister.GetComponentsInChildren<Rigidbody2D>();

            foreach (Rigidbody2D childRb in childRigidbodies)
            {
                childRb.transform.SetParent(null);

                Vector2 explosionDir = (childRb.transform.position - transform.position).normalized;
                float distance = Vector2.Distance(childRb.transform.position, transform.position);
                float explosionStrength = Mathf.Lerp(explosionForce, explosionForce / 2, distance / explosionRadius);

                childRb.AddForce((explosionDir * explosionStrength) / 2, ForceMode2D.Impulse);

                childRb.AddTorque(Random.Range(-10f, 10f), ForceMode2D.Impulse);
            }

            Destroy(splinteredCanister, 0.1f);
        }

        Destroy(gameObject);
    }




    private void CreateExplosion()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject struckObject = collider.gameObject;
            Debug.Log("Struck object: " + struckObject.name);

            Rigidbody2D hitRb = struckObject.GetComponent<Rigidbody2D>();
            if (hitRb != null && struckObject != gameObject)
            {
                Vector2 direction = (struckObject.transform.position - transform.position).normalized;
                hitRb.AddForce(direction * explosionForce);
            }
            else if (struckObject.name == "ClipPreventor")
            {
                Vector2 direction = (Player.transform.position - transform.position).normalized;
                playerRb2.AddForce(direction * explosionForce);
                damageController.health -= 1;
            }
        }
    }

    private void InstantiateExplosionEffect()
    {
        if (explosionEffectPrefab != null)
        {
            GameObject explosionInstance = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem mainExplosion = explosionInstance.GetComponent<ParticleSystem>();

            if (mainExplosion != null)
            {
                var main = mainExplosion.main;
                main.startSpeedMultiplier = 1.5f;
                main.startSizeMultiplier = 1.2f;

                var emission = mainExplosion.emission;
                var burst = emission.GetBurst(0);
                burst.count = 750;
                emission.SetBurst(0, burst);

                Light explosionLight = explosionInstance.GetComponent<Light>();
                if (explosionLight != null)
                {
                    explosionLight.intensity = 8f;
                    explosionLight.range = 15f;
                }

                mainExplosion.Play();

                float totalDuration = main.duration + main.startLifetime.constantMax;
                Destroy(explosionInstance, totalDuration);
            }
        }
    }


    private void SetupParticleGradient()
    {
        if (gasParticles != null)
        {
            var colorOverLifetime = gasParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );

            colorOverLifetime.color = gradient;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
