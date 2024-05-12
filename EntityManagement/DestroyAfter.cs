using UnityEngine;

namespace RingLib.EntityManagement;

internal class DestroyAfter : MonoBehaviour
{
    public float Seconds;
    private void Update()
    {
        Seconds -= Time.deltaTime;
        if (Seconds <= 0)
        {
            Destroy(gameObject);
        }
    }
}
