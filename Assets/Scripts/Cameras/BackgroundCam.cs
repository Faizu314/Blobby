using UnityEngine;

public class BackgroundCam : MonoBehaviour
{
    private Camera m_MainCam;
    private Camera m_Camera;

    private void Start() {
        m_MainCam = Camera.main;
        m_Camera = GetComponent<Camera>();
    }

    private void Update() {
        m_Camera.orthographicSize = m_MainCam.orthographicSize;
    }
}
