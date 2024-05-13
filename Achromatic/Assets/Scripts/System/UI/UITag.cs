using UnityEngine;
using UnityEngine.EventSystems;

public class UITag : MonoBehaviour
{
    [SerializeField] private string tag;

    public string Tag
    {
        get
        {
            if (!string.IsNullOrEmpty(tag))
                return tag;

            return this.gameObject.name;
        }
    }
}