using UnityEngine;

public class UIElement<T>
    where T : Object
{
    private T component;
    public T Component => component;

    public UIElement(string tag, GameObject gameObject)
    {
        var tags = gameObject.GetComponentsInChildren<UITag>();

        foreach (var uiTag in tags)
        {
            if (uiTag.Tag != tag)
                continue;


            component = uiTag.GetComponent<T>();

            if (component == null)
                Debug.LogError("component is not found");

            break;
        }
    }

}