using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class LoadingPanel : MonoBehaviour
{
    RectTransform myRect;
    [SerializeField] private Image BackgroundImage;
    [SerializeField] private TextMeshProUGUI RoundText;
    Color fullCol, clearCol;
    Ease loadingEase = Ease.Linear, textFadeEase = Ease.InSine;

    public void Initialize()
    {
        fullCol = ColorBox.Inst.loadingBackgroundColor;
        clearCol = ColorBox.Inst.loadingBackgroundFadeColor;
    }


    public void SetEnable(bool startFromHalf, UnityAction startCallback, UnityAction endCallback)
    {
        gameObject.SetActive(true);
        int targetRound = DataManager.Inst.GetMainFieldRound();
        if (startFromHalf)
        {
            BackgroundImage.color = fullCol;
            HalfToEnd(targetRound, Enums.MAIN_FIGHT_LOADING_DELAY, startCallback, endCallback);
        }
        else
        {
            ToHalf(targetRound, startCallback, endCallback);
        }
    }

    void UpdateRoundText(int targetRound)
    {
        RoundText.text = $"라운드 {targetRound}";
    }

    void ToHalf(int targetRound, UnityAction startCallback, UnityAction endCallback)
    {
        UpdateRoundText(targetRound);
        BackgroundImage.DOKill();
        RoundText.DOKill();
        RoundText.DOFade(0f, 0f);
        BackgroundImage.color = clearCol;

        float duration = Enums.MAIN_FIGHT_LOADING_DURATION;
        RoundText.DOFade(1f, duration).SetEase(textFadeEase);
        BackgroundImage.DOFade(1f, duration).SetEase(loadingEase).OnComplete(() =>
        {
            HalfToEnd(targetRound, Enums.MAIN_FIGHT_LOADING_DELAY, startCallback, endCallback);
        });
    }

    void HalfToEnd(int targetRound, float startDelay, UnityAction startCallback, UnityAction endCallback)
    {
        UpdateRoundText(targetRound);
        startCallback?.Invoke();
        BackgroundImage.DOKill();
        RoundText.DOKill();
        BackgroundImage.color = fullCol;
        RoundText.DOFade(0f, Enums.MAIN_FIGHT_LOADING_DURATION)
            .SetEase(loadingEase)
            .SetDelay(startDelay);

        BackgroundImage.DOFade(0f, Enums.MAIN_FIGHT_LOADING_DURATION)
            .SetEase(loadingEase)
            .SetDelay(startDelay).OnComplete(() =>
        {
            endCallback?.Invoke();
            SetDisable();
        });
    }

    public void SetDisable()
    {
        gameObject.SetActive(false);
    }

    public void UpdateSize(float targetY)
    {
        GetRect().sizeDelta = new Vector2(0f, targetY);
    }

    public RectTransform GetRect()
    {
        if (myRect == null)
            myRect = GetComponent<RectTransform>();
        return myRect;
    }


}
