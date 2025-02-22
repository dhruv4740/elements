using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PrefabCreator : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;

    // Element prefabs
    [Header("Element Prefabs")]
    [SerializeField] private GameObject aluminiumPrefab;
    [SerializeField] private GameObject calciumPrefab;
    [SerializeField] private GameObject chlorinePrefab;
    [SerializeField] private GameObject hydrogenPrefab;
    [SerializeField] private GameObject magnesiumPrefab;
    [SerializeField] private GameObject oxygenPrefab;
    [SerializeField] private GameObject sodiumPrefab;
    [SerializeField] private GameObject sulfurPrefab;
    [SerializeField] private GameObject zincPrefab;
    [SerializeField] private GameObject ironPrefab;
    [SerializeField] private GameObject mercuryPrefab;
    [SerializeField] private GameObject potassiumPrefab;

    // Compound prefabs
    [Header("Compound Prefabs")]
    [SerializeField] private GameObject H2OPrefab;

    [SerializeField] private Vector3 prefabOffset;

    private Dictionary<string, GameObject> prefabDictionary;
    private Dictionary<string, GameObject> instantiatedObjects;
    private Dictionary<string, List<string>> compoundElements;

    private void Awake()
    {
        InitializeDictionaries();
    }

    void InitializeDictionaries()
    {
        prefabDictionary = new Dictionary<string, GameObject>
        {
            // Elements
            { "aluminium", aluminiumPrefab },
            { "calcium", calciumPrefab },
            { "chlorine", chlorinePrefab },
            { "hydrogen", hydrogenPrefab },
            { "magnesium", magnesiumPrefab },
            { "oxygen", oxygenPrefab },
            { "sodium", sodiumPrefab },
            { "sulfur", sulfurPrefab },
            { "zinc", zincPrefab },
            { "iron", ironPrefab },
            { "mercury", mercuryPrefab },
            { "potassium", potassiumPrefab },

            // Compound - only water is kept
            { "H2O", H2OPrefab }
        };

        compoundElements = new Dictionary<string, List<string>>
        {
            // Only water compound remains
            { "H2O", new List<string> { "hydrogen", "oxygen" } }
        };

        instantiatedObjects = new Dictionary<string, GameObject>();
    }

    private void OnEnable() => imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    private void OnDisable() => imageManager.trackedImagesChanged -= OnTrackedImagesChanged;

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs obj)
    {
        HandleAddedImages(obj.added);
        HandleUpdatedImages(obj.updated);
        HandleRemovedImages(obj.removed);
        CheckForCompounds();
    }

    void HandleAddedImages(List<ARTrackedImage> addedImages)
    {
        foreach (var image in addedImages)
        {
            HandleImageDetection(image);
        }
    }

    void HandleUpdatedImages(List<ARTrackedImage> updatedImages)
    {
        foreach (var image in updatedImages)
        {
            if (image.trackingState == TrackingState.Tracking)
                HandleImageDetection(image);
            else
                RemoveObject(image.referenceImage.name);
        }
    }

    void HandleRemovedImages(List<ARTrackedImage> removedImages)
    {
        foreach (var image in removedImages)
        {
            RemoveObject(image.referenceImage.name);
        }
    }

    private void HandleImageDetection(ARTrackedImage image)
    {
        string imageName = image.referenceImage.name;
        
        if (ShouldInstantiateElement(imageName))
        {
            InstantiateElement(image, imageName);
        }
    }

    bool ShouldInstantiateElement(string imageName)
    {
        return prefabDictionary.ContainsKey(imageName) && 
              !compoundElements.ContainsKey(imageName) && 
              !instantiatedObjects.ContainsKey(imageName);
    }

    void InstantiateElement(ARTrackedImage image, string imageName)
    {
        GameObject element = Instantiate(
            prefabDictionary[imageName], 
            image.transform.position + prefabOffset, 
            image.transform.rotation
        );
        instantiatedObjects[imageName] = element;
    }

    void CheckForCompounds()
    {
        foreach (var compound in compoundElements)
        {
            if (CanFormCompound(compound.Key, compound.Value))
            {
                FormCompound(compound.Key, compound.Value);
            }
        }
    }

    bool CanFormCompound(string compoundName, List<string> requiredElements)
    {
        if (instantiatedObjects.ContainsKey(compoundName)) return false;

        foreach (string element in requiredElements)
        {
            if (!instantiatedObjects.ContainsKey(element)) return false;
        }
        return true;
    }

    void FormCompound(string compoundName, List<string> elements)
    {
        // Destroy elements
        foreach (string element in elements)
        {
            DestroyElement(element);
        }

        // Spawn compound at average position
        Vector3 avgPosition = CalculateAveragePosition(elements);
        GameObject compound = Instantiate(
            prefabDictionary[compoundName],
            avgPosition + prefabOffset,
            Quaternion.identity
        );
        instantiatedObjects[compoundName] = compound;
    }

    Vector3 CalculateAveragePosition(List<string> elements)
    {
        Vector3 sum = Vector3.zero;
        foreach (string element in elements)
        {
            if (instantiatedObjects.TryGetValue(element, out GameObject obj))
            {
                sum += obj.transform.position;
            }
        }
        return sum / elements.Count;
    }

    private void DestroyElement(string elementName)
    {
        if (instantiatedObjects.TryGetValue(elementName, out GameObject element))
        {
            Destroy(element);
            instantiatedObjects.Remove(elementName);
        }
    }

    private void RemoveObject(string imageName)
    {
        if (instantiatedObjects.TryGetValue(imageName, out GameObject obj))
        {
            Destroy(obj);
            instantiatedObjects.Remove(imageName);
        }
    }
}