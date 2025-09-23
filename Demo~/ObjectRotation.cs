using UnityEngine;

public class ObjectRotation : MonoBehaviour
{
    public float m_x_rotationSpeed;
    public float m_y_rotationSpeed;
    public float m_z_rotationSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(m_x_rotationSpeed * Time.deltaTime, m_y_rotationSpeed * Time.deltaTime, m_z_rotationSpeed * Time.deltaTime, Space.World);
    }
}
