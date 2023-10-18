using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public float MoveValue { get; private set; }


    /// <summary>
    /// Vector from cursor pos when primary was pressed and held to current cursor pos in viewport space.
    /// </summary>
    public Vector2 DragVector { get; private set; }
    public Vector2 DragVectorFixed { get; private set; }

    private InputManager.HardwareInputs m_HardwareInputs;
    private InputManager.InterfaceInputs m_InterfaceInputs;
    private Vector2 m_InitialDragPos;
    private Vector2 m_InitialDragPosFixed;

    private void Start() {
        m_HardwareInputs = InputManager.Instance.hardwareInputs;
        m_InterfaceInputs = InputManager.Instance.interfaceInputs;

        StartCoroutine(nameof(LateFixedUpdate));
    }

    private void Update() {
        GetFrameButtons();
        CacheFixedButtons();
        SetDragVector();

        MoveValue = m_InterfaceInputs.PrimaryJoyStick.x;
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
        if (!(IsPrimaryDown || IsSecondaryDown) && m_HardwareInputs.PrimaryActionDown && !EventSystem.current.IsPointerOverGameObject()) {
            DidPrimaryPress = m_HardwareInputs.CursorPos.x >= 0.5f;
            IsPrimaryDown = DidPrimaryPress;
            DidSecondaryPress = !DidPrimaryPress;
            IsSecondaryDown = !DidPrimaryPress;
        }
        else if ((IsPrimaryDown || IsSecondaryDown) && !m_HardwareInputs.PrimaryAction && !EventSystem.current.IsPointerOverGameObject()) {
            DidPrimaryRelease = IsPrimaryDown;
            DidSecondaryRelease = IsSecondaryDown;
            IsPrimaryDown = IsSecondaryDown = false;
        }
        else {
            DidPrimaryPress = DidSecondaryPress = DidPrimaryRelease = DidSecondaryRelease = false;
        }
    }

    private void CacheFixedButtons() {
        WasPrimaryPressedFixed = !WasPrimaryDownFixed && IsPrimaryDown || WasPrimaryPressedFixed;
        WasPrimaryReleasedFixed = WasPrimaryDownFixed && !IsPrimaryDown || WasPrimaryReleasedFixed;
        WasSecondaryPressedFixed = !WasSecondaryDownFixed && IsSecondaryDown || WasSecondaryPressedFixed;
        WasSecondaryReleasedFixed = WasSecondaryDownFixed && !IsSecondaryDown || WasSecondaryReleasedFixed;
        WasPrimaryDownFixed |= IsPrimaryDown;
        WasSecondaryDownFixed |= IsSecondaryDown;

        if (WasPrimaryPressedFixed || WasPrimaryDownFixed)
            WasPrimaryReleasedFixed = false;
        if (WasSecondaryPressedFixed || WasSecondaryDownFixed)
            WasSecondaryReleasedFixed = false;
    }

    private void ResetCachedButtons() {
        WasPrimaryPressedFixed = !WasPrimaryDownFixed && IsPrimaryDown;
        WasPrimaryReleasedFixed = WasPrimaryDownFixed && !m_HardwareInputs.PrimaryAction;
        WasSecondaryPressedFixed = !WasSecondaryDownFixed && IsSecondaryDown;
        WasSecondaryReleasedFixed = WasSecondaryDownFixed && !m_HardwareInputs.PrimaryAction;
        WasPrimaryDownFixed = m_HardwareInputs.PrimaryAction && IsPrimaryDown;
        WasSecondaryDownFixed = m_HardwareInputs.PrimaryAction && IsSecondaryDown;
    }

    private void SetDragVector() {
        var dragVector = Vector2.zero;

        if (DidPrimaryPress)
            m_InitialDragPos = m_HardwareInputs.CursorPos;
        else if (IsPrimaryDown)
            dragVector = m_HardwareInputs.CursorPos - m_InitialDragPos;

        dragVector.x = Mathf.Clamp(dragVector.x, -1f, 1f);
        dragVector.y = Mathf.Clamp(dragVector.y, -1f, 1f);

        DragVector = dragVector;
    }

    private void SetDragVectorFixed() {
        var dragVectorFixed = Vector2.zero;

        if (WasPrimaryPressedFixed)
            m_InitialDragPosFixed = m_HardwareInputs.CursorPos;
        else if (WasPrimaryDownFixed)
            dragVectorFixed = m_HardwareInputs.CursorPos - m_InitialDragPosFixed;

        dragVectorFixed.x = Mathf.Clamp(dragVectorFixed.x, -1f, 1f);
        dragVectorFixed.y = Mathf.Clamp(dragVectorFixed.y, -1f, 1f);

        DragVectorFixed = dragVectorFixed;
    }
}
