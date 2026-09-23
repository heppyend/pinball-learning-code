using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Helper
{

    public Helper()
    {
        CapsuleCollider capsuleCollider = new();
        Grid Grid= new();
        Tilemap Tilemap = new();
        TilemapRenderer TilemapRenderer=new();
        AudioListener AudioListener = new();
        PolygonCollider2D PolygonCollider2D = new();
        SpriteMask SpriteMask = new();
    }

}
