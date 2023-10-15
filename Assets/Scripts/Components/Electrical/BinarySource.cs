using UnityEngine;
using UnityEngine.Events;

public class BinarySource : MonoBehaviour
{
    public UnityEvent OnInputChange;

    public bool State => m_State;
    private bool m_State = false;

    public void SetState(bool state) {
        if (m_State != state)
            OnInputChange.Invoke();

        m_State = state;
    }
}
