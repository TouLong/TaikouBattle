using UnityEngine;
using System.Collections.Generic;
using System;

public class TextUI
{
    static public int defualtFontSize = 10;
    public static TextMesh Pop(string content, Color color, Vector3 position, int fontSize = -1, float time = 0.5f)
    {
        GameObject gameObject = new GameObject("Text Pop", typeof(TextMesh), typeof(TextPop));
        gameObject.transform.position = position;
        TextPop pop = gameObject.GetComponent<TextPop>();
        pop.Setup((fontSize > 0 ? fontSize : defualtFontSize) / 2f);
        Timer.Set(time, () => { pop.DestorySelf(); });
        return ConfigTextMesh(gameObject.GetComponent<TextMesh>(), content, color, fontSize);
    }

    public static TextMesh Stay(string content, Vector3 position, Color color, int fontSize = -1)
    {
        GameObject gameObject = new GameObject("Text Stay", typeof(TextMesh), typeof(TextStay));
        gameObject.transform.position = position;
        gameObject.GetComponent<TextStay>().Setup();
        return ConfigTextMesh(gameObject.GetComponent<TextMesh>(), content, color, fontSize);
    }
    static TextMesh ConfigTextMesh(TextMesh textMesh, string content, Color color, int fontSize)
    {
        int size = fontSize > 0 ? fontSize : defualtFontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = content;
        textMesh.characterSize = size / (float)100;
        textMesh.fontSize = size * 10;
        textMesh.color = color;
        return textMesh;
    }
    class TextPop : MonoBehaviour
    {
        float xOffset;
        float yOffset;
        Transform cam;
        float speed;
        Vector3 movement;

        public void Setup(float speed)
        {
            cam = Camera.main.transform;
            xOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
            yOffset = 1f;
            this.speed = speed;
        }

        void LateUpdate()
        {
            transform.LookAt(transform.position + cam.rotation * Vector3.forward, cam.rotation * Vector3.up);
            movement = transform.right * xOffset + transform.up * yOffset;
            transform.position += movement * speed * Time.deltaTime;
        }

        public void DestorySelf()
        {
            Destroy(gameObject);
        }
    }

    class TextStay : MonoBehaviour
    {
        Transform cam;

        public void Setup()
        {
            cam = Camera.main.transform;
        }

        void LateUpdate()
        {
            transform.LookAt(transform.position + cam.rotation * Vector3.forward, cam.rotation * Vector3.up);
        }

        public void DestorySelf()
        {
            Destroy(gameObject);
        }
    }
}

