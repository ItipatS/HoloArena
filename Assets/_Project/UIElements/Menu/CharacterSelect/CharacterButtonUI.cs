using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public GameObject lockOverlay;

    private CharacterData characterData;
    private CharacterSelectManager selectManager;

    public void Init(CharacterData data, CharacterSelectManager manager)
    {
        characterData = data;
        selectManager = manager;

        icon.sprite = characterData.characterIcon;
        lockOverlay.SetActive(!characterData.isUnlocked);

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        selectManager.PreviewCharacter(characterData); // just preview
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selectManager.PreviewCharacter(selectManager.GetCurrentSelectedCharacter());
    }
}
