using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class ParticleSystemConfig
{
    [HideInInspector]
    public string ParticleName;

    public GameObject ParticlePrefab;
    public Transform PositionTransform;
    public Vector3 PositionVector;
    public Vector3 Rotation;
    public Vector3 Scale = Vector3.one;
    public float Duration = 2f;
    public float Delay = 1f;
    public bool Loop = false;

    [Header("Layer Settings")]
    public string SortingLayerName = "Default"; 
    public int OrderInLayer = 0;
}

public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner Instance { get; private set; }


    private HouseGameEntryPoint _entryPoint;
    [SerializeField] private Button homeButton;


    [Header("Item Sets by Size")]
    public List<GameObject> smallItems;
    public List<GameObject> mediumItems;
    public List<GameObject> largeItems;

    [Header("Spawn Positions")]
    public Transform smallSpawnPosition;
    public Transform mediumSpawnPosition;
    public Transform largeSpawnPosition;

    [Header("Game Settings")]
    public GameObject finishPanel;
    public float finishDelay = 1f;

    private int currentPlacedCount = 0;
    private int currentSpawnedCount = 0;
    private int totalItemCount;

    public List<ParticleSystemConfig> ParticleConfigs;

    private void Awake()
    {
        homeButton.onClick.AddListener(FinishOnButton);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        totalItemCount = smallItems.Count + mediumItems.Count + largeItems.Count;
        SpawnNextBatch();
    }
    private void FinishOnButton()
    {
        _entryPoint.InvokeGameFinished();
    }
    private void SpawnNextBatch()
    {
        currentSpawnedCount = 0;

        if (SpawnItem(smallItems, smallSpawnPosition)) currentSpawnedCount++;
        if (SpawnItem(mediumItems, mediumSpawnPosition)) currentSpawnedCount++;
        if (SpawnItem(largeItems, largeSpawnPosition)) currentSpawnedCount++;

        if (currentSpawnedCount == 0)
        {
            SetFinishForPackage();
           // StartCoroutine(FinishGame());
        }
    }

    private bool SpawnItem(List<GameObject> itemList, Transform spawnPosition)
    {
        if (itemList.Count > 0 && spawnPosition != null)
        {
            Instantiate(itemList[0], spawnPosition.position, Quaternion.identity);
            itemList.RemoveAt(0);
            return true;
        }
        return false;
    }

    void InstantiateParticles()
    {
        foreach (var particleConfig in ParticleConfigs)
        {
            Vector3 position;

            if (particleConfig.PositionTransform != null)
            {
                position = particleConfig.PositionTransform.position;
            }
            else if (particleConfig.PositionVector != Vector3.zero)
            {
                position = particleConfig.PositionVector;
            }
            else
            {
                Debug.LogError("პარტიკლის პოზიციები არარის შევსებული, შეავსე ან ტრანსფორნით ან ვექტორით");
                Debug.LogError("ვექტორით შევსებისას არ გამოიყენო ტრანსფორმი, მხოლოდ ტრანსფორმი იმუშავებს, შეავსე მხოლოდ ერთი");

                continue;
            }

            StartCoroutine(InstantiateParticleWithDelay(particleConfig, position));
        }
    }

    IEnumerator InstantiateParticleWithDelay(ParticleSystemConfig particleConfig, Vector3 position)
    {
        yield return new WaitForSeconds(particleConfig.Delay);

        GameObject particle = Instantiate(particleConfig.ParticlePrefab, position, Quaternion.Euler(particleConfig.Rotation));
        particle.transform.localScale = particleConfig.Scale;

        var particleSystem = particle.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            var mainModule = particleSystem.main;
            mainModule.loop = particleConfig.Loop;

            var particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
            {
                particleRenderer.sortingLayerName = particleConfig.SortingLayerName;
                particleRenderer.sortingOrder = particleConfig.OrderInLayer;
            }
        }

        if (!particleConfig.Loop)
        {
            Destroy(particle, particleConfig.Duration);
        }
    }

    public void ObjectPlaced()
    {
        currentPlacedCount++;

        if (currentPlacedCount >= currentSpawnedCount)
        {
            currentPlacedCount = 0;
            SpawnNextBatch();
        }
    }

    private IEnumerator FinishGame()
    {
        yield return new WaitForSeconds(finishDelay);
        InstantiateParticles();
        if (finishPanel != null)
            finishPanel.SetActive(true);
    }

   

    public void SetEntryPoint(HouseGameEntryPoint entryPoint)
    {
        _entryPoint = entryPoint;
    }

    private void SetFinishForPackage()
    {
        StartCoroutine(FinishAfterFireWorks());
    }

    private IEnumerator FinishAfterFireWorks()
    {
        yield return new WaitForSecondsRealtime(5f);
        _entryPoint.InvokeGameFinished();
    }
}
