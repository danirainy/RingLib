using UnityEngine;

namespace RingLib.EntityManagement;

internal class DeactivateOnStart : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
    }
}
