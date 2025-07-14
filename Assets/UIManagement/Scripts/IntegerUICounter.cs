using TMPro;
using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GetKeyValues))]
public class IntegerUICounter : MonoBehaviour
{
    [SerializeField] private GameEvent updateUICounterEvent, updateUIEvent;

    private GetKeyValues getKeyComponenet;
    private TextMeshProUGUI componentText;
    private int initVal, targetVal;
    private GameObject soundHandler;

    private void Awake()
    {
        componentText = GetComponent<TextMeshProUGUI>();
        getKeyComponenet = GetComponent<GetKeyValues>();
    }

    private void OnEnable()
    {
        updateUICounterEvent.TheEvent.AddListener(UpdateUICounterEvent);
        updateUIEvent.TheEvent.AddListener(UpdateUIEvent);
    }

    void UpdateUICounterEvent(GameEvent theEvent)
    {
        UnityEngine.Console.Log("UI Counter Event Func Run...." + getKeyComponenet.inventorySystemObj.GetIntKeyValue(getKeyComponenet.keyString));
        initVal = getKeyComponenet.inventorySystemObj.GetIntKeyValue(getKeyComponenet.keyString);
    }

    void UpdateUIEvent(GameEvent theEvent)
    {
        targetVal = getKeyComponenet.inventorySystemObj.GetIntKeyValue(getKeyComponenet.keyString);
        //componentText.text = inventorySystemObj.GetIntKeyValue(keyString).ToString("00");
        if (initVal != targetVal)
            ScoreIncrementor(initVal, targetVal, 3f);
    }

    private IEnumerator ScoreIncrementor(int initialValue, int targetValue, float duration)
    {
        int start = initialValue, tempNum = 0;
        for (float timer = 0; timer < duration; timer += Time.unscaledDeltaTime)
        {
            float progress = timer / duration;
            initialValue = (int)Mathf.Lerp(start, targetValue, progress);
            componentText.text = initialValue.ToString();
            
            if(tempNum != initialValue)
                GetComponent<AudioPlayer>().ShootAudioEvent();
            tempNum = initialValue;
            yield return new WaitForSecondsRealtime(TimeSpan.FromMilliseconds(timer).Seconds);

           // await Task.Delay(TimeSpan.FromMilliseconds(timer));//Task.Delay(TimeSpan.FromSeconds(0.25f));
            print("Integer Item Value");
        }

        initialValue = targetValue;
        initVal = targetVal;
        componentText.text = initialValue.ToString();
    }

    private void OnDisable()
    {
        updateUICounterEvent.TheEvent.RemoveListener(UpdateUICounterEvent);
        updateUIEvent.TheEvent.RemoveListener(UpdateUIEvent);
    }
}
