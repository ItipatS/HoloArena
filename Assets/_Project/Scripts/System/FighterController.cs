using System.Collections.Generic;
using UnityEngine;

public class FighterController : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    private List<ICharacterModule> modules = new List<ICharacterModule>();
    private bool isActive = true;
    private InputBufferModule inputBuffer = new InputBufferModule(.35f, 60);
    public InputBufferModule InputBuffer => inputBuffer;
    public Rigidbody Rigidbody => rb;
    public Animator Animator => animator;
    
    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigibody found on " + gameObject.name);
            rb = gameObject.AddComponent<Rigidbody>();
        }

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator found on " + gameObject.name);
            animator = gameObject.AddComponent<Animator>();
        }

        modules.AddRange(GetComponentsInChildren<ICharacterModule>());
        foreach (var module in modules)
        {
            module.Initialize(this);
        }
    }

    void Update()
    {
        if (!isActive) return;

        float deltaTime = Time.deltaTime;
        foreach (var module in modules)
        {
            module.Tick(deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        float fixedDeltaTime = Time.fixedDeltaTime;
        foreach (var module in modules)
        {
            module.FixedTick(fixedDeltaTime);
        }
    }

    public T GetModule<T>() where T : class, ICharacterModule
    {
        foreach (var module in modules)
        {
            if (module is T foundModule)
                return foundModule;
        }
        return null;
    }

    public void AddModule<T>() where T : MonoBehaviour, ICharacterModule
    {
        T module = gameObject.AddComponent<T>();
        module.Initialize(this);
        modules.Add(module);
    }

    // Remove a module at runtime
    public void RemoveModule<T>() where T : MonoBehaviour, ICharacterModule
    {
        var module = GetModule<T>();
        if (module != null)
        {
            modules.Remove(module);
            Destroy(module as MonoBehaviour);
        }
    }

    // Toggle module ticking (e.g., for stun or death)
    public void SetActive(bool active)
    {
        isActive = active;

        if (active)
        {
            foreach (var module in modules)
            {
                if (module is IResettable resettable)
                    resettable.ResetModule();
            }
        }
    }

    public interface IResettable
    {
        void ResetModule();
    }



}
