using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTrackingScript : MonoBehaviour
{
    [SerializeField]
    private GameObject[] placeablePrefabs;

    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();
    private ARTrackedImageManager arTrackedImageManager;

    private Camera arCamera;

    private void Awake()
    {
        arCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();

        foreach (GameObject prefab in placeablePrefabs)
        {
            GameObject newPrefab = Instantiate(prefab, Vector3.zero, prefab.transform.rotation);
            newPrefab.name = prefab.name;
            spawnedPrefabs.Add(prefab.name, newPrefab);
        }
    }

    public void OnEnable()
    {
        arTrackedImageManager.trackedImagesChanged += ImageChanged;
    }

    public void OnDisable()
    {
        arTrackedImageManager.trackedImagesChanged += ImageChanged;
    }


    public void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            spawnedPrefabs[trackedImage.name].SetActive(false);
        }

    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;
        Vector3 position = trackedImage.transform.position;

        GameObject prefab = spawnedPrefabs[name];
        prefab.transform.position = position;

        //face camera
        Vector3 camForward = arCamera.transform.forward;
        Quaternion rot = prefab.transform.rotation;
        rot = Quaternion.Euler(rot.eulerAngles.x, arCamera.transform.rotation.eulerAngles.y, rot.eulerAngles.z);
        prefab.transform.rotation = rot;

        prefab.SetActive(true);

        foreach (GameObject go in spawnedPrefabs.Values)
        {
            if(go.name != name)
            {
                go.SetActive(false);
            }
        }
    }

}
