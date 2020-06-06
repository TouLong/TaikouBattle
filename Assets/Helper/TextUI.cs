using UnityEngine;
using System.Collections.Generic;
using System;

public class TextUI
{
    public static TextMesh Pop(string content, Color color, Vector3 position, float time = 0.5f)
    {
        GameObject gameObject = new GameObject("Text Pop", typeof(TextMesh), typeof(TextPop));
        gameObject.transform.position = position;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = content;
        textMesh.fontSize = 30;
        textMesh.color = color;
        TextPop pop = gameObject.GetComponent<TextPop>();
        pop.Init(textMesh.fontSize / 3);
        Timer.Set(time, () => { pop.DestorySelf(); });
        return textMesh;
    }

    public static TextMesh Stay(string content, Vector3 position, int fontSize = 30)
    {
        GameObject gameObject = new GameObject("Text Stay", typeof(TextMesh), typeof(TextStay));
        gameObject.transform.position = position;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = content;
        textMesh.fontSize = fontSize;
        textMesh.color = Color.cyan;
        TextStay stay = gameObject.GetComponent<TextStay>();
        stay.Init();
        return textMesh;
    }

    class TextPop : MonoBehaviour
    {
        float xOffset;
        float yOffset;
        float speed;
        Transform cam;
        Vector3 movement;

        public void Init(float speed)
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
        public void Init()
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

