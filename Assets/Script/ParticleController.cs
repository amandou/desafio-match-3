using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public virtual void Destroy()
    {
        Destroy(gameObject);
    }
}
