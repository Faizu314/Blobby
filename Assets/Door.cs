using UnityEngine;

public class Door : MonoBehaviour
{
    
    public void OnOpen() {
        Debug.Log("Opening");
        Destroy(gameObject);
    }
}
