using UnityEngine;
using UnityEngine.Events;

public class Goal : MonoBehaviour
{
    public UnityEvent OnPlayerEnter;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!Util.IsInLayerMask(GameManager.Instance.GameConfiguration.PlayerPointsLayer, collision.gameObject.layer))
            return;

        OnPlayerEnter?.Invoke();
    }
}
