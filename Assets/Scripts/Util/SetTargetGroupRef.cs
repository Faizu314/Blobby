using Cinemachine;
using UnityEngine;

public class SetTargetGroupRef : MonoBehaviour {

    private void Awake() {
        GameManager.Instance.TargetGroup = GetComponent<CinemachineTargetGroup>();
    }
}
