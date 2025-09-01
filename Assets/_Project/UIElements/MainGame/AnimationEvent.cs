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

    public void Tick(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    public void FixedTick(float fixedDeltaTime)
    {
        throw new System.NotImplementedException();
    }
}
