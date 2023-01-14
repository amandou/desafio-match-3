using System;
using UnityEngine;
using UnityEngine.UI;

public class TileView : MonoBehaviour
{
    private Outline _outline;
    [field: SerializeField] protected GameObject particlePrefab;

    private void Start()
    {
        _outline = GetComponent<Outline>();
    }
    public void ToggleOutline()
    {
        if (_outline != null)
        {
            _outline.enabled = !_outline.enabled;
        }
    }

    public virtual void Destroy()
    {
        for (int i = 0; i < 10; ++i)
        {
            var xOffset = UnityEngine.Random.Range(-30, 30) + gameObject.transform.position.x;
            var yOffset = UnityEngine.Random.Range(-30, 30) + gameObject.transform.position.y;
            Instantiate(particlePrefab, new Vector3(xOffset, yOffset, gameObject.transform.position.z), gameObject.transform.rotation);
        }
        Destroy(gameObject);
    }
}