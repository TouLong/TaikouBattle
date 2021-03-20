using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    public void Setup(Weapon weapon)
    {
        Mesh sectorMesh = GeoGenerator.SectorPlane(weapon.angle, weapon.farLength, weapon.nearLength, 0);
        GetComponent<MeshFilter>().mesh = sectorMesh;
    }
}