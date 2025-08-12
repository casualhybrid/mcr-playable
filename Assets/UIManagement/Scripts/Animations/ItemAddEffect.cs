using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemAddEffect : SerializedMonoBehaviour
{
    public event Action<ItemAddEffect> OnEffectHasEnded;

    [SerializeField] private UnityEvent OnItemReachedInventory;
    [SerializeField] private Dictionary<string, Sprite> itemsSprites;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Gradient gradientNormalTrailRenderer;
    [SerializeField] private Gradient gradientHighAlphaTrailRenderer;
    [SerializeField] private RectTransform trailRectT;
    [SerializeField] private Image itemImage;
    [SerializeField] private int noOfItems = 6;
    [SerializeField] private float delayBetweenInstantiations = .3f;
    [SerializeField] private float timeXToReachTargetForItem = 1f;
    [SerializeField] private float timeYToReachTargetForItem = 1f;
    [SerializeField] private Ease easeForXMovement = Ease.Linear;
    [SerializeField] private Ease easeForYMovement = Ease.Linear;
    [SerializeField] private float minItemScale = 1;
    [SerializeField] private float maxItemScale = 1;

    private readonly List<RectTransform> availableItemImagesRTtoUse = new List<RectTransform>();

    private int numberOfItemsToSpawn;
    private Vector2 startingPos, targetPos;
    private InventoryItemAnimData animData;
   // private Vector2 trailInitialAnchoredPos;
    private Sprite itemSpriteToUse;
    

    //private void Awake()
    //{
    //    trailInitialAnchoredPos = trailRectT.anchoredPosition;
    //}

    public void Initialzie(Vector2 startingPos, Vector2 endingPos, string nameOfItem, int noOfItems = -1, bool useHighAlphaGradient = false)
    {

        trailRectT.anchoredPosition = /*trailInitialAnchoredPos*/ startingPos;

        animData = InventoryPositions.GetAnimDataForItem(nameOfItem);

        trailRenderer.colorGradient = useHighAlphaGradient ? gradientHighAlphaTrailRenderer : gradientNormalTrailRenderer;

        trailRenderer.Clear();

        itemSpriteToUse = GetSpriteForThisItem(nameOfItem);
        numberOfItemsToSpawn = noOfItems == -1 ? this.noOfItems : noOfItems;

        this.targetPos = endingPos;
        this.startingPos = startingPos;

        StartCoroutine(CreateBurstOfCoins());
    }

    private Sprite GetSpriteForThisItem(string itemName)
    {
        Sprite sprite;
        itemsSprites.TryGetValue(itemName, out sprite);

        if (sprite == null)
            throw new System.Exception($"Failed to find sprite for item {itemName} while trying to show item add effect");

        return sprite;
    }

    private float CalculateTimeForAnim()
    {
        float timeForCompleteMove = (timeXToReachTargetForItem + timeYToReachTargetForItem) * 0.5f;
        float totalDelay = numberOfItemsToSpawn <= 1 ? 0 : (numberOfItemsToSpawn - 1) * delayBetweenInstantiations;
        float totalTime = timeForCompleteMove + totalDelay;
        return totalTime - (totalTime * .2f);
    }

    private IEnumerator CreateBurstOfCoins()
    {
        float timeForAnim = CalculateTimeForAnim();
        trailRenderer.time = timeForAnim;

        float x = trailRectT.anchoredPosition.x;
        float y = trailRectT.anchoredPosition.y;

        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        sequence.Join(DOTween.To(() => x, (pos) => { x = pos; }, targetPos.x, timeXToReachTargetForItem).SetEase(easeForXMovement).SetUpdate(true));
        sequence.Join(DOTween.To(() => y, (pos) => { y = pos; }, targetPos.y, timeYToReachTargetForItem).SetEase(easeForYMovement).SetUpdate(true));

        sequence.onUpdate = () =>
        {
            trailRectT.anchoredPosition = new Vector2(x, y);
        };

        Coroutine coroutine = null;

        for (int i = 0; i < numberOfItemsToSpawn; i++)
        {
            RectTransform item;

            if (availableItemImagesRTtoUse.Count > 0)
            {
                int lastIndex = availableItemImagesRTtoUse.Count - 1;
                item = availableItemImagesRTtoUse[lastIndex];
                availableItemImagesRTtoUse.RemoveBySwap(lastIndex);
            }
            else
            {
                item = Instantiate(itemImage.rectTransform, itemImage.transform.parent);
            }

            item.GetComponent<Image>().sprite = itemSpriteToUse;
            item.anchoredPosition = this.startingPos;
            item.localScale = Vector2.one * minItemScale;
            item.gameObject.SetActive(true);
            coroutine = StartCoroutine(MoveItemToTarget(item));

            yield return new WaitForSecondsRealtime(delayBetweenInstantiations);
        }

        if (coroutine != null)
        {
            yield return coroutine;
        }

        this.gameObject.SetActive(false);
        OnEffectHasEnded?.Invoke(this);

       // Destroy(this.gameObject);
    }

    private IEnumerator MoveItemToTarget(RectTransform rt)
    {
        float timeForCompleteMove = (timeXToReachTargetForItem + timeYToReachTargetForItem) * 0.5f;

        float x = rt.anchoredPosition.x;
        float y = rt.anchoredPosition.y;

        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        sequence.Join(DOTween.To(() => x, (pos) => { x = pos; }, targetPos.x, timeXToReachTargetForItem).SetEase(easeForXMovement).SetUpdate(true));
        sequence.Join(DOTween.To(() => y, (pos) => { y = pos; }, targetPos.y, timeYToReachTargetForItem).SetEase(easeForYMovement).SetUpdate(true));

        Sequence scaleSequence = DOTween.Sequence().SetUpdate(true);
        scaleSequence.Append(rt.DOScale(maxItemScale, timeForCompleteMove * .5f).SetUpdate(true));
        scaleSequence.Append(rt.DOScale(minItemScale, timeForCompleteMove * .5f).SetUpdate(true));

        sequence.Join(scaleSequence);

        sequence.onUpdate = () =>
        {
            rt.anchoredPosition = new Vector2(x, y);
        };

        yield return sequence.WaitForCompletion();

        if (animData.isValid && animData.animations != null)
        {
            for (int i = 0; i < animData.animations.Length; i++)
            {
                animData.animations[i].DORestart();
            }
        }

        rt.gameObject.SetActive(false);
        availableItemImagesRTtoUse.Add(rt);

        OnItemReachedInventory.Invoke();
    }
}