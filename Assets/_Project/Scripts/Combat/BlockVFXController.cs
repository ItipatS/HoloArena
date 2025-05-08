using UnityEngine;

public class BlockVFXController : MonoBehaviour,ICharacterModule
{
    public Material blockMaterial {get; private set;}

    public void Initialize(FighterController controller)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Clone the material so changes are instance-specific.
            blockMaterial = rend.material;
        }
    }
    public void FixedTick()
    {
    }

    public void Tick()
    {
    }

}
