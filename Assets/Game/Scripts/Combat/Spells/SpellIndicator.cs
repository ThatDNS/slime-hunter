using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SpellIndicator : MonoBehaviour
{
    public float castRange;
    public float areaOfEffect;
    public bool Active { get; set; }

    public abstract Vector3 GetTarget { get; }

    public abstract void ToggleReady(bool ready);

    public abstract void ShowIndicator(SpellSO spellSO);

    public abstract void HideIndicator();
}
