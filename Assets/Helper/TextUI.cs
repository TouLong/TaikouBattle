using UnityEngine;
using System.Collections.Generic;
using System;

public class TextUI
{
    static public int defualtFontSize = 10;
    static public float defualtPopTime = 0.5f;
    public static TextMesh Pop<T>(T content, Color color, Vector3 position, int fontSize = -1, float time = 0f)
    {
        GameObject gameObject = new GameObject("Text Pop", typeof(TextMesh), typeof(TextPop));
        gameObject.transform.position = position;
        TextPop pop = gameObject.GetComponent<TextPop>();
        pop.Setup((fontSize > 0 ? fontSize : defualtFontSize) / 2f);
        DelayEvent.Create(time > 0 ? time : defualtPopTime, pop.DestorySelf);
        return ConfigTextMesh(gameObject.GetComponent<TextMesh>(), content, color, fontSize);
    }

    public static TextMesh Stay<T>(T content, Vector3 position, Color color, int fontSize = -1)
    {
        GameObject gameObject = new GameObject("Text Stay", typeof(TextMesh), typeof(LookatCamera));
        gameObject.transform.position = position;
        return ConfigTextMesh(gameObject.GetComponent<TextMesh>(), content, color, fontSize);
    }
    static TextMesh ConfigTextMesh<T>(TextMesh textMesh, T content, Color color, int fontSize)
    {
        int size = fontSize > 0 ? fontSize : defualtFontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = content.ToString();
        textMesh.characterSize = size / (float)100;
        textMesh.fontSize = size * 10;
        textMesh.color = color;
        return textMesh;
    }
    class TextPop : MonoBehaviour
    {
        float xOffset;
        float yOffset;
        float speed;

        public void Setup(float speed)
        {
            xOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
            yOffset = 1f;
            this.speed = speed;
        }

        void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
            transform.position += (transform.right * xOffset + transform.up * yOffset) * speed * Time.deltaTime;
        }

        public void DestorySelf()
        {
            Destroy(gameObject);
        }
    }

}

