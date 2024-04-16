using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi : MonoBehaviour
{
    public Vector2Int imageDim;
    public int regionAmount;

    private SpriteRenderer renderer;

    private Texture2D image;
    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        image = renderer.sprite.texture;
        renderer.sprite = Sprite.Create(GetDiagram(), new Rect(0, 0, imageDim.x, imageDim.y), Vector2.one * 0.5f);
    }
    Texture2D GetDiagram()
    {
        Vector2Int[] centroids = new Vector2Int[regionAmount];
        for(int i = 0; i < regionAmount; i++)
        {
            centroids[i] = new Vector2Int(Random.Range(0, imageDim.x), Random.Range(0, imageDim.y));
        }
        Color[] pixelColors = new Color[imageDim.x * imageDim.y];
        for(int x = 0; x < imageDim.x; x++)
        {
            for(int y = 0; y < imageDim.y; y++)
            {
                int index = x * imageDim.x + y;
                int centroidIndex = GetClosestCentroidIndex(new Vector2Int(x, y), centroids);
                pixelColors[index] = image.GetPixel(centroids[centroidIndex].x, centroids[centroidIndex].y);
            }
        }
        return GetImageFromColorArray(pixelColors);
    }

    int GetClosestCentroidIndex(Vector2Int pixelpos, Vector2Int[] centroids)
    {
        float smallestDst = float.MaxValue;
        int index = 0;
        for(int i = 0; i < centroids.Length; i++)
        {
            if(Vector2.Distance(pixelpos, centroids[i]) < smallestDst)
            {
                smallestDst = Vector2.Distance(pixelpos, centroids[i]);
                index = i;
            }
        }
        return index;
    }

    Texture2D GetImageFromColorArray(Color[] pixelColors)
    {
        Texture2D tex = new Texture2D(imageDim.x, imageDim.y);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixelColors);
        tex.Apply();
        return tex;
    }
}
