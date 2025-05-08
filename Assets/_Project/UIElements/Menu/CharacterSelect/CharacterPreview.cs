using UnityEngine;

public class CharacterPreview : MonoBehaviour
{
    private GameObject currentModel;
    private Animator currentAnimator;

    public Transform previewAnchor; // assign this in inspector

    public void Display(GameObject characterPrefab)
    {
        Clear();

        if (characterPrefab == null) return;

        currentModel = Instantiate(characterPrefab, previewAnchor);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        currentAnimator = currentModel.GetComponent<Animator>();
        if (currentAnimator != null)
        {
            currentAnimator.Play("Idle"); // assumes you have an "Idle" anim
        }
    }

    public void Clear()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
        }
    }
}
