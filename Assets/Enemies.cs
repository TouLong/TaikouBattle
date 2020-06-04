using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : List<Enemy>
{
    static public int Layer = LayerMask.GetMask("Enemy");
    static public Enemies InScene = new Enemies();
    public Enemies()
    {
    }

    public void HighLight(bool enable)
    {
        ForEach(x => x.HighLight(enable));
    }
}
