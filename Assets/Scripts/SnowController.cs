using System.Collections;
using UnityEngine;

public class SnowController : MonoBehaviour
{
    [SerializeField] private ParticleSystem snowParticles;
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
                snowParticles.Play();
                bIsSnowing = true;
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

        // Clear snow particles
        snowParticles.Clear();
        bIsSnowing = false;
    }
}
