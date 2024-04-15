using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ColorObjectManager : MonoBehaviour
{
    private int OBJECT_LAYER { get; set; }
    private int COLOR_OBJECT_LAYER { get; set; }

    private Dictionary<eActivableColor, List<ColorObject>> colorObjects = new Dictionary<eActivableColor, List<ColorObject>>();

    private void Awake()
    {
        OBJECT_LAYER = LayerMask.NameToLayer("Object");
        COLOR_OBJECT_LAYER = LayerMask.NameToLayer("ColorObject");

        ColorObject[] objects = transform.GetComponentsInChildren<ColorObject>();
        List<ColorObject>[] objectColorList = new List<ColorObject>[(int)eActivableColor.MAX_COLOR];
        for(int i =0; i < objectColorList.Length; i++)
        {
            objectColorList[i] = new List<ColorObject>();
        }

        for(int i = 0; i < objects.Length; i++)
        {
            objectColorList[(int)objects[i].ObjectColor].Add(objects[i]);
        }

        for (int i = 0; i < objectColorList.Length; i++)
        {
            colorObjects.Add((eActivableColor)i, objectColorList[i]);
        }
    }
    private void Start()
    {
        for (int i = 0; i < (int)eActivableColor.MAX_COLOR; i++)
        {
            DisableColors((eActivableColor)i);
        }
        PlayManager.Instance.ActivationColorEvent.AddListener(EnableColors);
    }

    public void EnableColors(eActivableColor color)
    {
        for(int i = 0;i < colorObjects[color].Count; i++)
        {
            colorObjects[color][i].EnableObject(color);
            colorObjects[color][i].ChangeLayer(COLOR_OBJECT_LAYER);
        }
    }
    public void DisableColors(eActivableColor color)
    {
        for (int i = 0; i < colorObjects[color].Count; i++)
        {
            colorObjects[color][i].DisableObject();
            colorObjects[color][i].ChangeLayer(OBJECT_LAYER);
        }
    }

}
