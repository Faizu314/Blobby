using System.Collections;
using UnityEngine;

public class PlayerInputs : MonoBehaviour {

    public bool IsPrimaryDown { get; private set; }
    public bool DidPrimaryPress { get; private set; }
    public bool DidPrimaryRelease { get; private set; }
    public bool IsSecondaryDown { get; private set; }
    public bool DidSecondaryPress { get; private set; }
    public bool DidSecondaryRelease { get; private set; }

    /// <summary>
    /// Did user press primary in any frame before this fixed update.
    /// </summary>
    public bool WasPrimaryDownFixed { get; private set; }
    /// <summary>
    /// Did primary begin pressing before this fixed update. Not mutually exclusive with was primary released fixed in case user presses and releases before fixed update.
    /// </summary>
    public bool WasPrimaryPressedFixed { get; private set;  }
    /// <summary>
    /// Did primary begin being in a released state before this fixed update. Not mutually exclusive with was primary pressed fixed in case user presses and releases before fixed update.
    /// </summary>
    public bool WasPrimaryReleasedFixed { get; private set;  }
    /// <summary>
    /// Did user press secondary in any frame before this fixed update.
    /// </summary>
    public bool WasSecondaryDownFixed { get; private set; }
    /// <summary>
    /// Did secondary begin pressing before this fixed update. Not mutually exclusive with was secondary released fixed in case user presses and releases before fixed update.
    /// </summary>
    public bool WasSecondaryPressedFixed { get; private set; }
    /// <summary>
    /// Did secondary begin being in a released state before this fixed update. Not mutually exclusive with was secondary pressed fixed in case user presses and releases before fixed update.
    /// </summary>
    public bool WasSecondaryReleasedFixed { get; private set; }


    /// <summary>
    /// Vector from cursor pos when primary was pressed and held to current cursor pos in viewport space.
    /// </summary>
    public Vector2 DragVector { get; private set; }
    public Vector2 DragVectorFixed { get; private set; }

    private InputManager.GlobalInputs m_Inputs;
    private Vector2 m_InitialDragPos;
    private Vector2 m_InitialDragPosFixed;

    private void Start() {
        m_Inputs = InputManager.Instance.globalInputs;

        StartCoroutine(nameof(LateFixedUpdate));
    }

    private void Update() {
        GetFrameButtons();
        CacheFixedButtons();
        SetDragVector();
    }

    private void FixedUpdate() {
        SetDragVectorFixed();
    }

    private IEnumerator LateFixedUpdate() {
        while (true) {
            yield return new WaitForFixedUpdate();

            ResetCachedButtons();
        }
    }

    private void GetFrameButtons() {
        DidPrimaryPress = !IsPrimaryDown && m_Inputs.PrimaryAction;
        DidPrimaryRelease = IsPrimaryDown && !m_Inputs.PrimaryAction;
        DidSecondaryPress = !IsSecondaryDown && m_Inputs.SecondaryAction;
        DidSecondaryRelease = IsSecondaryDown && !m_Inputs.SecondaryAction;
        IsPrimaryDown = m_Inputs.PrimaryAction;
        IsSecondaryDown = m_Inputs.SecondaryAction;
    }

    private void CacheFixedButtons() {
        WasPrimaryPressedFixed = !WasPrimaryDownFixed && m_Inputs.PrimaryAction || WasPrimaryPressedFixed;
        WasPrimaryReleasedFixed = WasPrimaryDownFixed && !m_Inputs.PrimaryAction || WasPrimaryReleasedFixed;
        WasSecondaryPressedFixed = !WasSecondaryDownFixed && m_Inputs.SecondaryAction || WasSecondaryPressedFixed;
        WasSecondaryReleasedFixed = WasSecondaryDownFixed && !m_Inputs.SecondaryAction || WasSecondaryReleasedFixed;
        WasPrimaryDownFixed |= m_Inputs.PrimaryAction;
        WasSecondaryDownFixed |= m_Inputs.SecondaryAction;

        if (WasPrimaryPressedFixed || WasPrimaryDownFixed)
            WasPrimaryReleasedFixed = false;
        if (WasSecondaryPressedFixed || WasSecondaryDownFixed)
            WasSecondaryReleasedFixed = false;
    }

    private void ResetCachedButtons() {
        WasPrimaryPressedFixed = !WasPrimaryDownFixed && m_Inputs.PrimaryAction;
        WasPrimaryReleasedFixed = WasPrimaryDownFixed && !m_Inputs.PrimaryAction;
        WasSecondaryPressedFixed = !WasSecondaryDownFixed && m_Inputs.SecondaryAction;
        WasSecondaryReleasedFixed = WasSecondaryDownFixed && !m_Inputs.SecondaryAction;
        WasPrimaryDownFixed = m_Inputs.PrimaryAction;
        WasSecondaryDownFixed = m_Inputs.SecondaryAction;
    }

    private void SetDragVector() {
        var dragVector = Vector2.zero;

        if (DidPrimaryPress)
            m_InitialDragPos = m_Inputs.CursorPos;
        else if (IsPrimaryDown)
            dragVector = m_Inputs.CursorPos - m_InitialDragPos;

        dragVector.x = Mathf.Clamp(dragVector.x, -1f, 1f);
        dragVector.y = Mathf.Clamp(dragVector.y, -1f, 1f);

        DragVector = dragVector;
    }

    private void SetDragVectorFixed() {
        var dragVectorFixed = Vector2.zero;

        if (WasPrimaryPressedFixed)
            m_InitialDragPosFixed = m_Inputs.CursorPos;
        else if (WasPrimaryDownFixed)
            dragVectorFixed = m_Inputs.CursorPos - m_InitialDragPosFixed;

        dragVectorFixed.x = Mathf.Clamp(dragVectorFixed.x, -1f, 1f);
        dragVectorFixed.y = Mathf.Clamp(dragVectorFixed.y, -1f, 1f);

        DragVectorFixed = dragVectorFixed;
    }
}
