using UnityEngine;

public class CeilingLava : MonoBehaviour
{
    public GameObject lavaDrop;

    public void DropLava()
    {
        lavaDrop.SetActive(true);
    }
}