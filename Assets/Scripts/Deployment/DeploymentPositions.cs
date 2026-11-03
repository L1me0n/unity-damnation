using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DeploymentPositions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerObservationPrefab;
    [SerializeField] private TurnOrchestrator turnOrchestrator;
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private BattlefieldGenerator battlefieldGenerator;

    private bool isLeftUsed = false;

    private TextMeshProUGUI leftTextComponent;
    private TextMeshProUGUI rightTextComponent;
    private TextMeshProUGUI centerTextComponent;

    private Dictionary<BuildingNode, GameObject> playerObservations = new Dictionary<BuildingNode, GameObject>();

    private void OnEnable()
    {
        turnOrchestrator.ShowDeploymentPositions += ShowPositions;
        turnOrchestrator.HideDeploymentPositions += HidePositions;
    }

    private void OnDisable()
    {
        turnOrchestrator.ShowDeploymentPositions -= ShowPositions;
        turnOrchestrator.HideDeploymentPositions -= HidePositions;
    }

    private void Awake()
    {
        if (turnOrchestrator == null)
        {
            turnOrchestrator = FindObjectOfType<TurnOrchestrator>();
        }

        if (matchManager == null)
        {
            matchManager = FindObjectOfType<MatchManager>();
        }

        if (battlefieldGenerator == null)
        {
            battlefieldGenerator = FindObjectOfType<BattlefieldGenerator>();
        }
    }

    public void ShowPositions(int turn)
    {
        if (matchManager == null)
        {
            Debug.LogError("MatchManager is not assigned.");
            return;
        }

        for (int i = 1; i < matchManager.DeploymentPositions.Count; i++)
        {
            BuildingNode node = matchManager.DeploymentPositions[i];
            if (node != null)
            {
                if (playerObservations.ContainsKey(node))
                {
                    GameObject existingObservation = playerObservations[node];
                    centerTextComponent = existingObservation.transform.Find("InfoTextCenter").GetComponent<TextMeshProUGUI>();
                    

                    if (isLeftUsed)
                    {
                        centerTextComponent.transform.localPosition = new Vector3(0, 0, 0);
                        centerTextComponent.fontSize = 2.5f;
                        leftTextComponent = existingObservation.transform.Find("InfoTextLeft").GetComponent<TextMeshProUGUI>();
                        leftTextComponent.fontSize = 2.5f;
                        rightTextComponent = existingObservation.transform.Find("InfoTextRight").GetComponent<TextMeshProUGUI>();
                        rightTextComponent.gameObject.SetActive(true);
                        rightTextComponent.text = "6";
                        switch (i)
                        {
                            case 1:
                                rightTextComponent.color = Color.blue;
                                break;
                            case 2:
                                rightTextComponent.color = Color.red;
                                break;
                            default:
                                rightTextComponent.color = Color.yellow;
                                break;
                        }
                        isLeftUsed = false;
                    }
                    else
                    {
                        centerTextComponent.transform.localPosition = new Vector3(2f, 0, 0);
                        centerTextComponent.fontSize = 4f;
                        leftTextComponent = existingObservation.transform.Find("InfoTextLeft").GetComponent<TextMeshProUGUI>();
                        leftTextComponent.gameObject.SetActive(true);
                        leftTextComponent.text = "6";
                        leftTextComponent.fontSize = 4f;
                        switch (i)
                        {
                            case 1:
                                leftTextComponent.color = Color.blue;
                                break;
                            case 2:
                                leftTextComponent.color = Color.red;
                                break;
                            default:
                                leftTextComponent.color = Color.yellow;
                                break;
                        }
                        isLeftUsed = true;
                    }

                    continue;
                }

                BuildingNodePreview preview = battlefieldGenerator.Nodes[node];
                GameObject playerObservation = Instantiate(playerObservationPrefab, preview.transform.position, Quaternion.identity);
                centerTextComponent = playerObservation.transform.Find("InfoTextCenter").GetComponent<TextMeshProUGUI>();
                leftTextComponent = playerObservation.transform.Find("InfoTextLeft").GetComponent<TextMeshProUGUI>();
                rightTextComponent = playerObservation.transform.Find("InfoTextRight").GetComponent<TextMeshProUGUI>();
                leftTextComponent.gameObject.SetActive(false);
                rightTextComponent.gameObject.SetActive(false);
                centerTextComponent.text = "6";
                centerTextComponent.fontSize = 5f;
                switch (i)
                {
                    case 1:
                        centerTextComponent.color = Color.blue;
                        break;
                    case 2:
                        centerTextComponent.color = Color.red;
                        break;
                    default:
                        centerTextComponent.color = Color.yellow;
                        break;
                }

                playerObservations.Add(node, playerObservation);
            }
        }
    }

    public void HideAt(BuildingNode node)
    {
        if (playerObservations.ContainsKey(node))
        {
            GameObject observation = playerObservations[node];
            Destroy(observation);
            playerObservations.Remove(node);
        }
    }

    public void HidePositions(int turn)
    {
        foreach (GameObject observation in playerObservations.Values)
        {
            Destroy(observation);
        }
        playerObservations.Clear();
    }
}