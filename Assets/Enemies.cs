using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : List<Enemy>
{
    static public int Layer;
    static public Enemies InScene = new Enemies();
    public Enemies()
    {
    }
    public void HighLight(bool enable)
    {
        ForEach(x => x.HighLight(enable));
    }
    public void Damage(int attackPoint)
    {
        ForEach(x => x.Damage(attackPoint));
    }
    public void Circling()
    {
        ForEach(x => x.Circling());
    }
}
