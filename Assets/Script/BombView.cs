using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombView : TileView
{
    public override void Destroy()
    {
        Debug.Log("Destroying bomb");
        Instantiate(particlePrefab, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(gameObject);
    }
}
