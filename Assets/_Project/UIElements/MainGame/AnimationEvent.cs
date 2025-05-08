using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour, ICharacterModule
{

    private FighterAttackModule AttackModule;

  

    public void Initialize(FighterController controller)
    {
        AttackModule = controller.GetModule<FighterAttackModule>();
    }
    
    public void ActiveHitbox()
    {
        AttackModule.ActivateHitbox();
    }

    public void DeactiveHitbox()
    {
        AttackModule.DeactivateHitbox();
    } 
     public void FixedTick()
    {
    }
    public void Tick()
    {
    }

}
