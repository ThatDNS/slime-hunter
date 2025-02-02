using UnityEngine;
using UnityEngine.UIElements;

public class TabbedMenu : Menu
{
    protected TabbedMenuController controller;

    private void OnEnable()
    {
        UIDocument menu = GetComponent<UIDocument>();
        VisualElement root = menu.rootVisualElement;

        controller = new(root);
        controller.RegisterTabCallbacks();
    }

    public virtual void SwitchTab(string tabName)
    {
        controller.SwitchTab(tabName);
    }
}