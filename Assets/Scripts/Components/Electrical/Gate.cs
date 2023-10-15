using UnityEngine;
using UnityEngine.Events;

public class Gate : MonoBehaviour
{
    [SerializeField] private BinarySource m_InputA;
    [SerializeField] private BinarySource m_InputB;

    [Space(5)]
    public UnityEvent And;
    public UnityEvent Nand;
    public UnityEvent Or;
    public UnityEvent Nor;
    public UnityEvent Xor;
    public UnityEvent Nxor;
    public UnityEvent Not;


    private void OnEnable() {
        m_InputA.OnInputChange.AddListener(OnInputChange);
        m_InputB.OnInputChange.AddListener(OnInputChange);
    }
    private void OnDisable() {
        m_InputA.OnInputChange.RemoveListener(OnInputChange);
        m_InputB.OnInputChange.RemoveListener(OnInputChange);
    }

    private void OnInputChange() {
        if (m_InputA.State || m_InputB.State)
            Or.Invoke();
        if (!(m_InputA.State || m_InputB.State))
            Nor.Invoke();
        if (m_InputA.State && m_InputB.State)
            And.Invoke();
        if (!(m_InputA.State && m_InputB.State))
            Nand.Invoke();
        if (!m_InputA.State && m_InputB.State || m_InputA.State && !m_InputB.State)
            Xor.Invoke();
        if (!(!m_InputA.State && m_InputB.State || m_InputA.State && !m_InputB.State))
            Nxor.Invoke();
        if (!m_InputA.State)
            Not.Invoke();
    }
}
