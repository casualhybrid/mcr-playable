using System.Collections;
using UnityEngine;

public class Blinker : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private float blinkingDelay;
  //  [SerializeField] private GameEvent playerHasRevived;

    private Coroutine blinkerCorout;
    private float blinkingDuration;

   // private void OnEnable()
   // {
      //  playerHasRevived.TheEvent.AddListener(BlinkOnExitOfState);
   // }

   // private void OnDisable()
   // {
      //  playerHasRevived.TheEvent.RemoveListener(BlinkOnExitOfState);
    //}

    public void StartBlinking(float duration)
    {
        if (blinkerCorout != null)
        {
            StopCoroutine(blinkerCorout);
            blinkerCorout = null;
        }
        if (blinkerCorout == null)
        {
            blinkingDuration = duration;
            blinkerCorout = StartCoroutine(Blinking());
        }
    }

    private void Update()
    {
        blinkingDuration -= Time.deltaTime;
    }

    private IEnumerator Blinking()
    {
        GameManager.Invincible = true;
        CarSkeleton carSkeleton = PlayerSharedData.CarSkeleton;
        CharacterSkeleton characterSkeleton = PlayerSharedData.CharacterSkeleton;

        while (blinkingDuration > 0)
        {

            for (int i = 0; i < carSkeleton.GameObjectsToDisableDuringBlinking.Length; i++)
            {
                GameObject gameObj = carSkeleton.GameObjectsToDisableDuringBlinking[i];
                gameObj.SetActive(false);
            }

            DisableMeshes(carSkeleton.Meshes, false);
            DisableMeshes(characterSkeleton.Meshes, false);

            yield return new WaitForSeconds(blinkingDelay);


            for (int i = 0; i < carSkeleton.GameObjectsToDisableDuringBlinking.Length; i++)
            {
                GameObject gameObj = carSkeleton.GameObjectsToDisableDuringBlinking[i];
                gameObj.SetActive(true);
            }

            DisableMeshes(carSkeleton.Meshes, true);
            DisableMeshes(characterSkeleton.Meshes, true);

            yield return new WaitForSeconds(blinkingDelay);
        }
        GameManager.Invincible = GameManager.IsForcedInvincible;
    }

    private void DisableMeshes(Renderer[] Renderer, bool flag)
    {
        for (int i = 0; i < Renderer.Length; i++)
        {
            Renderer[i].enabled = flag;
        }
    }

    private void BlinkOnExitOfState(GameEvent gameEvent)
    {
        StartBlinking(3f);
    }
}