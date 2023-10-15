using System.Collections;
using UnityEngine;

public interface IGrabber {
    void OnEatableReleased(IEatable eatable);
}
