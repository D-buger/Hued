using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SecretRoom : MonoBehaviour
{
    [SerializeField]
    private float fadeTime = 1f;

    private Coroutine fadeCoroutine = null;

    private SpriteRenderer renderer;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    IEnumerator Fade(float end)
    {
        float start = renderer.color.a;
        Color color = renderer.color;
        float a = 0;
        while (true)
        {
            a += Time.deltaTime / fadeTime;

            color.a = Mathf.Lerp(start, end, a);
            renderer.color = color;

            if (a > 1)
            {
                break;
            }

            yield return null;

        }
        color.a = end;
        renderer.color = color;
        fadeCoroutine = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            fadeCoroutine = StartCoroutine(Fade(0f));
        }    
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            fadeCoroutine = StartCoroutine(Fade(1f));
        }
    }

}
