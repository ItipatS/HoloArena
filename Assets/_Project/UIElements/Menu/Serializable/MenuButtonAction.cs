using UnityEngine.UI;

[System.Serializable]
public class MenuButtonAction
{
    public Button button;
    public SlideDirection slideDirection = SlideDirection.Bottom;
    public GameMode modeToSet;
    public string sceneToLoad;
}
