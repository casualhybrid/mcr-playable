using UnityEngine;
using FMOD.Studio;
using System.Collections.Generic;

public class BaseAudioPlayer : MonoBehaviour
{
    [HideInInspector] protected List<string> allBanks = new List<string>();
    [HideInInspector] protected List<string> allClipEvents = new List<string>();

    private FMOD.Studio.System studioSystem;
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        allBanks = FMODUnity.Settings.Instance.Banks;
        studioSystem = FMODUnity.RuntimeManager.StudioSystem;
        foreach (string obj in allBanks)
        {
            string objPath = "bank:/" + obj;
            Bank bankObj;
            studioSystem.getBank(objPath, out bankObj);

            EventDescription[] allEvents;
            bankObj.getEventList(out allEvents);
            //UnityEngine.Console.Log("Bank = " + objPath + " || GameplayBank = " + allEvents.Length);

            foreach (EventDescription eventDec in allEvents)
            {
                string path;
                eventDec.getPath(out path);
                allClipEvents.Add(path);
                //print("Events = " + path);
            }
        }

    }
}
