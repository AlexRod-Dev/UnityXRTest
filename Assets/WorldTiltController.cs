using UnityEngine;

public class WorldTiltController : MonoBehaviour
{
  
    [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float maxTiltAngle = 100f;

        private Vector3 currentEulerRotation;
        private bool bIsTilting;
   
    void Update()
    {
        //Check if ALT is held down
        bIsTilting = Input.GetKey(KeyCode.LeftAlt);

        if (bIsTilting)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            // Calculate new tilt angles
            currentEulerRotation.x += mouseY * rotationSpeed * Time.deltaTime;
            currentEulerRotation.z -= mouseX * rotationSpeed * Time.deltaTime;
            
            // Clamp tilt angles
            currentEulerRotation.x = Mathf.Clamp(currentEulerRotation.x, -maxTiltAngle, maxTiltAngle);
            currentEulerRotation.z = Mathf.Clamp(currentEulerRotation.z, -maxTiltAngle, maxTiltAngle);
            
            // Apply rotation
            transform.rotation = Quaternion.Euler(currentEulerRotation);
        }
        //Smoothly return to neutral if ALT released
        else if (transform.rotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
            currentEulerRotation = transform.rotation.eulerAngles;

            // Fix wraparound angle bug
            if (currentEulerRotation.x > 180) currentEulerRotation.x -= 360;
            if (currentEulerRotation.z > 180) currentEulerRotation.z -= 360;
        }

    }
}
