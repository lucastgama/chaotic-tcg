using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // Centro do tabuleiro
    [SerializeField] private float distance = 10.0f; // Distância inicial da câmera

    [Header("Orbit Settings")]
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float minVerticalAngle = -80.0f;
    [SerializeField] private float maxVerticalAngle = 80.0f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 20f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private Vector3 currentRotation;
    private Vector3 smoothVelocity = Vector3.zero;

    private void Start()
    {
        // Inicializa os ângulos baseado na rotação inicial da câmera
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;

        // Garante que o cursor fique visível
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void LateUpdate()
    {
        // Reset camera to top view when K is pressed
        if (Input.GetKeyDown(KeyCode.K))
        {
            rotationX = 0;
            rotationY = 90;
        }

        // Verifica se o botão direito do mouse está pressionado
        if (Input.GetMouseButton(1)) // 1 = botão direito
        {
            // Calcula a rotação baseada no movimento do mouse
            rotationX += Input.GetAxis("Mouse X") * rotationSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * rotationSpeed;
            
            // Limita o ângulo vertical
            rotationY = Mathf.Clamp(rotationY, minVerticalAngle, maxVerticalAngle);
        }

        // Handle zoom with mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // Converte os ângulos em quaternion
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        
        // Calcula a posição da câmera
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        // Aplica a rotação e posição à câmera
        transform.rotation = rotation;
        transform.position = position;
    }

    // Método para definir o alvo da câmera
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Método para ajustar a distância da câmera
    public void SetDistance(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }
}