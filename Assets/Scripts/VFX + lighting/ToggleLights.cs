using Unity.VisualScripting;
using UnityEngine;

public class ToggleLights : MonoBehaviour
{
    private GameObject interiorLights;
    private GameObject exteriorLights;

    void Awake()
    {
       interiorLights = transform.Find("Interior lights").gameObject;
       exteriorLights = transform.Find("Exterior lights").gameObject;
    }

    public void toggleLights()
    {
        exteriorLights.SetActive(!exteriorLights.activeSelf);
        interiorLights.SetActive(!interiorLights.activeSelf);
    }
}
