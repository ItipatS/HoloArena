using System;
using UnityEngine;

public static class HitEventManager
{
    // Event that passes the hitStop duration.
    public static event Action<GameObject,int> OnCharacterHit;

    public static void CharacterHit(GameObject hitCharacter,int hitStop)
    {
        Debug.LogWarning(hitCharacter + "got Hit");
        OnCharacterHit?.Invoke(hitCharacter, hitStop);
    }
}
