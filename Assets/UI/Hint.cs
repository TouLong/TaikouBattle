using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;
using System;

[ExecuteInEditMode]
public class Hint : MonoBehaviour
{
    static public Hint self;
    public GameObject shift;
    public GameObject ctrl;
    public GameObject space;
    public GameObject alt;
    public GameObject control;
    public GameObject confirm;
    public GameObject right;
    public void Awake()
    {
        if (self == null)
            self = this;
    }
}