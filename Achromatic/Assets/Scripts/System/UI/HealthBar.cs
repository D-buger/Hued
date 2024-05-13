using UnityEngine;
using UnityEngine.UI;

using Image = UnityEngine.UI.Image;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Texture2D emptyHeart;
    [SerializeField]
    private Texture2D fullHeart;

    private Texture2D halfHeart;

    private Image image;
    private Texture2D texture;
    private Rect frameRect;

    private int ratio;
    private int maxShowSprite;
    private int textureWidth;
    private int textureHeight;

    private void Awake()
    {
        texture = new Texture2D(0, 0);
        texture.filterMode = FilterMode.Point;

        MakeHalfHeart();

        frameRect = GetComponent<RectTransform>().rect;
        ratio = (int)(frameRect.width / frameRect.height);

        maxShowSprite = ratio;
        textureWidth = emptyHeart.width * ratio;
        textureHeight = emptyHeart.height;

        image = gameObject.GetComponent<Image>();
    }

    private void MakeHalfHeart()
    {
        halfHeart = new Texture2D(emptyHeart.width, emptyHeart.height);
        halfHeart.filterMode = FilterMode.Point;

        halfHeart.SetPixels(0, 0, halfHeart.width / 2, halfHeart.height,
            fullHeart.GetPixels(0, 0, fullHeart.width / 2, fullHeart.height));

        halfHeart.SetPixels(halfHeart.width / 2, 0, halfHeart.width / 2, halfHeart.height,
            emptyHeart.GetPixels(emptyHeart.width / 2, 0, emptyHeart.width / 2, emptyHeart.height));

        halfHeart.Apply();
    }

    public void SizeChange(int maxHp, int nowHp)
    {
        int HP = (int)(maxHp * 0.5f);

        maxShowSprite = (textureWidth / emptyHeart.width) * (textureHeight / emptyHeart.height);
        textureWidth = maxShowSprite > HP ?
            textureWidth :
            emptyHeart.width * HP;
        textureHeight = maxShowSprite > HP ?
            textureHeight :
            emptyHeart.height +
            emptyHeart.height * (HP <= ratio ? 0 : HP - ratio) / ratio;

        if (texture == null)
            texture = new Texture2D(textureWidth, textureHeight);
        else
            texture.Reinitialize(textureWidth, textureHeight);

        Color[] color = texture.GetPixels();
        for (int i = 0; i < color.Length; i++)
            color[i] = new Color(0, 0, 0, 0);
        texture.SetPixels(color);

        Vector2 pivot = new Vector2(
            texture.width / 2,
            texture.height / 2);
        Rect rect = new Rect(0, 0, texture.width, texture.height);

        Sprite lifeSprite = Sprite.Create(texture, rect, pivot);
        image.sprite = lifeSprite;

        SetInsideGraphic(maxHp, nowHp);
    }

    public void SetInsideGraphic(int maxHp, int nowHp)
    {
        int width = 0, height = 0;
        for (int i = 0; i < maxHp - 1; i += 2)
        {
            width = (i / 2) % (int)(textureWidth / fullHeart.width) * fullHeart.width;
            height += fullHeart.height * (width == 0 ? 1 : 0);

            if (i < nowHp - 1)
            {
                texture.SetPixels(width, texture.height - height,
                    fullHeart.width, fullHeart.height, fullHeart.GetPixels());
            }
            else if (nowHp % 2 != 0 && i < nowHp)
            {
                texture.SetPixels(width, texture.height - height,
                    halfHeart.width, halfHeart.height, halfHeart.GetPixels());
            }
            else if (i >= nowHp)
            {
                texture.SetPixels(width, texture.height - height,
                    emptyHeart.width, emptyHeart.height, emptyHeart.GetPixels());
            }
        }

        texture.Apply();
    }
}