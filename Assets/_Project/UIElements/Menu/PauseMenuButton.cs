using UnityEngine.UI;

[System.Serializable]

public class PauseMenuButton
{
    public Button button;
    public SlideDirection slideDirection = SlideDirection.Bottom;
    public GameMode modeToSet;
    public string sceneToLoad;
}
