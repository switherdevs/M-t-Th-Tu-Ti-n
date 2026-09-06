using System.Net.NetworkInformation;
using UnityEngine;

public class Mo_UI : MonoBehaviour
{
    [SerializeField] private GameObject UiGameOb;
    void Start()
    {
        UiGameOb.gameObject.SetActive(false);
    }

    // Update is called once per frame
    public void Bat_UI()
    {
        UiGameOb.gameObject.SetActive(true);
    }
}
