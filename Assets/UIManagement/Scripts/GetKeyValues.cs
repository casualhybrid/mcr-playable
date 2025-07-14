using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GetKeyValues : MonoBehaviour
{
    [InfoBox("Only enable the following booleans if dealing with in game session inventory. e.g on the coins txt GameObject in HUD panel", InfoMessageType.Warning)]
    [BoxGroup("GamePlaySessionBox")] [SerializeField] private bool useSessionInventoryOnly = false;

    [BoxGroup("GamePlaySessionBox")] [SerializeField] private bool useBothPlayerAndSessionInventories = false;

    [Space]
    [Space]
    [Space]

    // [SerializeField] private UnityEvent OnEnableListener;
    [SerializeField] private GameEvent updateScore;

    public InventorySystem inventorySystemObj;
    public GamePlaySessionInventory gamePlaySessionInventory;
    public string keyString;

    private TextMeshProUGUI componentText;

    private void Awake()
    {
        componentText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        UpdateKeyValue();
        //    OnEnableListener.Invoke();

        if (!useSessionInventoryOnly || useBothPlayerAndSessionInventories)
        {
            updateScore.TheEvent.AddListener(HandlePlayerInventoryUpdated);
        }

        if (useSessionInventoryOnly || useBothPlayerAndSessionInventories)
        {
            if (gamePlaySessionInventory == null)
            {
                throw new System.Exception("GamePlaySessionInventory isn't assigned even though session inventory is being used");
            }

            gamePlaySessionInventory.OnGamePlayInventoryItemAdded.AddListener(HandlePlayerInventoryUpdated);
        }
    }

    public void GetIntKeyValue()
    {
        componentText.text = inventorySystemObj.GetIntKeyValue(keyString).ToString("00");
    }

    private void HandlePlayerInventoryUpdated(GameEvent gameEvent)
    {
        UpdateKeyValue();
    }

    private void HandlePlayerInventoryUpdated(IntegerValueItem item)
    {
        UpdateKeyValue();
    }

    private void UpdateKeyValue()
    {
        int val;

        if (useBothPlayerAndSessionInventories)
        {
            val = inventorySystemObj.GetIntKeyValue(keyString) + gamePlaySessionInventory.GetIntKeyValue(keyString);
        }
        else if (useSessionInventoryOnly)
        {
            val = gamePlaySessionInventory.GetIntKeyValue(keyString);
        }
        // Default Behaviour
        else
        {
            val = inventorySystemObj.GetIntKeyValue(keyString);
        }

        componentText.text = val.ToString("00");
    }

    private void OnDisable()
    {
        updateScore.TheEvent.RemoveListener(HandlePlayerInventoryUpdated);
        gamePlaySessionInventory?.OnGamePlayInventoryItemAdded.RemoveListener(HandlePlayerInventoryUpdated);
    }
}