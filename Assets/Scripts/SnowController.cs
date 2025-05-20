using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class SnowController : MonoBehaviour
{
    [SerializeField] private ParticleSystem snowParticles;
    [SerializeField] private ParticleSystem snowParticles2;
    [SerializeField] private Transform cityRoot;

    private bool bIsSnowing = false;

    private bool bIsTilting = false;

    private float tiltDuration = 0.5f;

    private float tiltAngle = 180f;
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!bIsSnowing)
            {
                StartCoroutine(SnowfallRoutine());
             
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (!bIsTilting)
            {
                StartCoroutine(TiltCity());
            }
           
        }
    }

    private IEnumerator SnowfallRoutine()
    {
        bIsSnowing = true;

        snowParticles.Play();
        snowParticles2.Play();

        // Wait for 3 seconds while snow accumulates
        yield return new WaitForSeconds(3f);

        // Stop emission and clear existing particles
        snowParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        snowParticles2.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // Optional: allow some time for remaining particles to fall
        yield return new WaitForSeconds(3f);

        snowParticles.Clear();
        snowParticles2.Clear();

        bIsSnowing = false;
    }

    private IEnumerator TiltCity()
    {
        bIsTilting = true;
        
        Quaternion startRotation = cityRoot.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, tiltAngle);

        float elapsed = 0f;

        while (elapsed < tiltDuration)
        {
            cityRoot.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / tiltDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        //Return to original rotation
        elapsed = 0f;
        while (elapsed < tiltDuration)
        {
            cityRoot.rotation = Quaternion.Slerp(targetRotation, startRotation, elapsed / tiltDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        cityRoot.rotation = startRotation;
        bIsTilting = false;

      
    }
}
