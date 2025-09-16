using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using UnityEngine.Events;

public enum TextAniType { NONE = 0, BIGGER_YELLOW, BIGGER_GREEN, BIGGER_RED, SMALLER_RED, SMALLER_YELLOW, }
public enum TextColorType { YELLOW = 0, BLUE, PURPLE, CYAN, RED, GREEN, WHITE, ORANGE, GRAY, }

public class TextHelper : MonoBehaviour
{
    public static TextHelper Inst;
    public Color[] TextColors;

    string numField_Billion;
    string numField_Million;
    string numField_TenThousand;
    string numField_Thousand;

    private void Awake()
    {
        Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // numField_Billion = TranslateDataBox.Inst.GetExtraTextStrings(ExtraTextType.NUM_BILLION);
        // numField_Million = TranslateDataBox.Inst.GetExtraTextStrings(ExtraTextType.NUM_MILLION);
        // numField_TenThousand = TranslateDataBox.Inst.GetExtraTextStrings(ExtraTextType.NUM_TEN_THOUSAND);
        // numField_Thousand = TranslateDataBox.Inst.GetExtraTextStrings(ExtraTextType.NUM_THOUSAND);
    }

    public string StringFormatColorReplace(int index, string str, TextColorType targetColor, string frontStr = "")
    {
        string temp = str;
        if (index == 0)
            return temp.Replace(frontStr + "{0}", "<color=#" + ColorTOHTML(targetColor) + ">" + frontStr + "{0}" + "</color>");
        else if (index == 1)
            return temp.Replace(frontStr + "{1}", "<color=#" + ColorTOHTML(targetColor) + ">" + frontStr + "{1}" + "</color>");
        else if (index == 2)
            return temp.Replace(frontStr + "{2}", "<color=#" + ColorTOHTML(targetColor) + ">" + frontStr + "{2}" + "</color>");
        else if (index == 3)
            return temp.Replace(frontStr + "{3}", "<color=#" + ColorTOHTML(targetColor) + ">" + frontStr + "{3}" + "</color>");
        return temp;
    }

    // public string GetPowerStringBig(int power, float fontSize)
    // {
    //     var languageType = GameManager.Inst.GetCurrentLanagueType();

    //     if (languageType == LanguageType.KOR || languageType == LanguageType.CHN_SIMPLE ||
    //         languageType == LanguageType.CHN_TRADI || languageType == LanguageType.JPN)
    //         return FormatKoreanNumber(power, fontSize);
    //     else
    //         return FormatWesternNumber(power, fontSize);
    // }

    private string FormatKoreanNumber(long num, float fontSize)
    {
        long billions = num / 100000000; // 억 단위
        long tenThousands = (num % 100000000) / 10000; // 만 단위
        long thousands = num % 10000; // 천 단위 이하
        string result = "";
        if (billions > 0)
            result += billions + $"<size={fontSize}>{numField_Billion}</size>";
        if (tenThousands > 0)
            result += tenThousands + $"<size={fontSize}>{numField_TenThousand}</size>";
        if (thousands > 0 || result == "")
            result += thousands;
        return result.Trim(); // 불필요한 공백 제거
    }

    public string DateTimeToGoodFormat(DateTime time)
    {
        return time.ToString("yyyy/MM/dd HH:mm");    // 2024/11/16 14:30
    }

    private string FormatWesternNumber(long num, float fontSize)
    {
        long billions = num / 1000000000;
        long millions = (num % 1000000000) / 1000000;
        long thousands = (num % 1000000) / 1000;
        long remainder = num % 1000; // 천 미만

        string result = "";

        // 각 단위가 0이 아닐 때만 추가
        if (billions >= 1)
            result += $"{billions}<size={fontSize}>{numField_Billion}</size>";
        if (millions >= 1)
            result += $"{millions}<size={fontSize}>{numField_Million}</size>";
        if (thousands >= 1)
            result += $"{thousands}<size={fontSize}>{numField_Thousand}</size>";
        if (remainder > 0 || result == "")
            result += $"{remainder}";
        return result.Trim(); // 불필요한 공백 제거
    }

    public string GetHTMLString(TextColorType targetColor, string centerStr)
    {
        return "<color=#" + ColorTOHTML(targetColor) + ">" + centerStr + "</color>";
    }

    string ColorTOHTML(TextColorType targetColor)
    {
        return ColorUtility.ToHtmlStringRGBA(TextColors[(int)targetColor]);
    }

    public string GetRomeNumber(int target)
    {
        switch (target)
        {
            case 0: return "0";
            case 1: return "I";
            case 2: return "II";
            case 3: return "III";
            case 4: return "IV";
            case 5: return "V";
            case 6: return "VI";
            case 7: return "VII";
            case 8: return "VIII";
            case 9: return "IX";
            case 10: return "X";
        }
        return "";
    }

    // public static string GetNameOrInfo(string[] target)
    // {
    //     if (target == null)
    //         return "ERROR";

    //     if (GameManager.Inst == null)
    //     {
    //         return target[(int)LanguageType.KOR];
    //     }

    //     int targetIndex = (int)GameManager.Inst.GetCurrentLanagueType();
    //     if (targetIndex >= target.Length)
    //         return "";
    //     return target[targetIndex];
    // }

    public void DoTextAnimation(TextMeshProUGUI targetText, string targetStr, float aniT = 0.5f)
    {
        targetText.DOKill();
        targetText.text = "";
        targetText.DOText(targetStr, aniT, true, ScrambleMode.None).SetEase(Ease.InOutSine);
    }

    public void DoAnimateText(TextMeshProUGUI targetText, string targetStr, float spd, UnityAction? onComplete)
    {
        targetText.DOKill();
        targetText.text = targetStr;
        targetText.maxVisibleCharacters = 0;

        DOTween.To(() => targetText.maxVisibleCharacters, x => targetText.maxVisibleCharacters = x, targetStr.Length, spd)
            .SetEase(Ease.InSine).OnComplete(() => onComplete?.Invoke()).SetId(targetText);
    }

    public void DoAnimateTextLoop(TextMeshProUGUI targetText, string targetStr, float spd)
    {
        targetText.DOKill();
        targetText.text = targetStr;
        targetText.maxVisibleCharacters = 0;

        DOTween.To(() => targetText.maxVisibleCharacters, x => targetText.maxVisibleCharacters = x, targetStr.Length, spd)
            .SetEase(Ease.InSine).SetId(targetText).SetLoops(-1, LoopType.Yoyo);
    }

    public void SetRankIndexText(TextMeshProUGUI textObj, bool isTopBox, int targetRank)
    {
        textObj.text = isTopBox ? $"<size={textObj.fontSize * 0.7f}>Rank</size>.{targetRank}" : GetRankText(targetRank, textObj.fontSize);
    }

    float rankTextSizeOffset = 0.75f;
    private string GetRankText(int rank, float fontSize = -1)
    {
        if (rank == -1) return "--";
        if (rank < 0) return rank.ToString();
        if (rank >= 11) return $"{rank}.";

        switch (rank)
        {
            case 1: return $"1<size={fontSize * rankTextSizeOffset}>st</size>";
            case 2: return $"2<size={fontSize * rankTextSizeOffset}>nd</size>";
            case 3: return $"3<size={fontSize * rankTextSizeOffset}>rd</size>";
            default:
                return $"{rank}<size={fontSize * rankTextSizeOffset}>th</size>";
        }
    }

    public void SetPercentageText(TextMeshProUGUI targetText, float amount)
    {
        targetText.text = GetPercentageString(amount);
    }

    public string GetPercentageString(float amount)
    {
        return $"{ToStringWithoutZero(amount * 100f)}%";
    }

    public void SetTimerText(TextMeshProUGUI targetText, DateTime dateTime, bool isLong)
    {
        targetText.text = GetTimerString(dateTime, isLong);
    }

    public string GetTimerString(DateTime dateTime, bool isLong)
    {
        if (isLong)
            return dateTime.ToString("yyyy-MM-dd HH:mm");
        else
            return dateTime.ToString("MM-dd HH:mm");
    }

    public void SetTimerText(TextMeshProUGUI targetText, int Sec, bool onlyMinute)
    {
        targetText.text = GetTimerString(Sec, onlyMinute);
    }

    public void SetTimerText(TextMeshProUGUI targetText, TimeSpan timeSpan, bool onlyMinute)
    {
        targetText.text = onlyMinute ? timeSpan.ToString(@"mm\:ss") : timeSpan.ToString(@"hh\:mm\:ss");
    }

    public string GetTimerLadderTimerString(float seconds) => GetTimerLadderTimerString(TimeSpan.FromSeconds(seconds));
    public string GetTimerLadderTimerString(TimeSpan t) => t.ToString(@"ss\.ff");

    public void SetTimerText(TextMeshProUGUI targetText, float Sec, bool onlyMinute)
    {
        targetText.text = GetTimerString(Sec, onlyMinute);
    }

    public void SetEXPProgress(Image targetImage, float per)
    {
        targetImage.fillAmount = per;
    }

    public void SetEXPProgress(SlicedFilledImage targetImage, float per)
    {
        targetImage.fillAmount = per;
    }

    public void SetEXPText(TextMeshProUGUI targetText, int exp, int target)
    {
        int slashSize = Mathf.RoundToInt(targetText.fontSize * 0.8f);
        targetText.text = string.Format("{0}<size={1}>/</size>{2}", exp, slashSize, target);
    }

    public void SetClassLevelText(TextMeshProUGUI targetText, int level)
    {
        targetText.text = string.Format("Clv.{0}", level);
    }

    public void SetCountText(TextMeshProUGUI targetText, int count)
    {
        targetText.text = string.Format("x{0}", count);
    }

    public void SetPlusText(TextMeshProUGUI target, int targetLevel)
    {
        if (targetLevel == -1)
        {
            target.text = "";
            return;
        }
        float frontSize = target.fontSize * 0.8f;
        string from = "<size={0}>+</size>{1}";
        target.text = string.Format(from, frontSize, targetLevel);
    }

    public void SetLevelText(TextMeshProUGUI targetText, int currentLevel)
    {
        float frontSize = targetText.fontSize * 0.8f;
        string from = GetLevelString();
        targetText.text = string.Format(from, frontSize, currentLevel);
    }
    public void SetLevelText(TextMeshProUGUI targetText, long currentLevel)
    {
        SetLevelText(targetText, (int)currentLevel);
    }

    public void SetForceLevelText(TextMeshProUGUI targetText, int forceLevel)
    {
        if (forceLevel == 0)
        {
            targetText.text = "+0";
            return;
        }
        targetText.text = $"+{forceLevel}";
    }

    public string GetLevelString(bool skipSize = false)
    {
        if (skipSize)
            return "Lv.{0}";
        return "<size={0}>Lv.</size>{1}";
    }

    // public void StartNumberingAni(TextMeshProUGUI targetText, float duration, TextAniType aniType)
    // {
    //     Ease ease = Ease.InSine;
    //     RectTransform rect = targetText.rectTransform;
    //     rect.DOKill();
    //     targetText.DOKill();

    //     float aniT = duration / 2f;

    //     rect.localScale = Vector3.one;
    //     Vector3 ts = new Vector3(1f, 1.1f, 1f);
    //     rect.DOScale(ts, aniT).SetLoops(2, LoopType.Yoyo).SetEase(ease);

    //     if (aniType == TextAniType.NONE)
    //         return;

    //     Color targetCol = ColorBox.Inst.GetTextAniColor(aniType);
    //     targetText.DOColor(targetCol, aniT).SetLoops(2, LoopType.Yoyo).SetEase(ease);
    // }

    public string GetTimerString(float timer, bool onlyMinute)
    {
        if (Enums.Approximately(timer, -1))
            return "-- : -- : --";
        TimeSpan t = TimeSpan.FromSeconds(timer);
        return onlyMinute ? t.ToString(@"mm\:ss") : t.ToString(@"hh\:mm\:ss");
    }

    // public void NumberingText(TextMeshProUGUI targetText, string stringForm, float start, float target,
    // float duration, Action callback, TextAniType aniType = TextAniType.NONE)
    // {
    //     if (string.IsNullOrEmpty(stringForm))
    //         stringForm = "{0}";

    //     float startNum = start;
    //     float targetNum = target;
    //     StartNumberingAni(targetText, duration, aniType);
    //     DOTween.To(() => startNum, x => startNum = x, targetNum, duration).SetEase(Ease.Linear).OnUpdate(() =>
    //     {
    //         targetText.text = string.Format(stringForm, ToStringNonedecimal(startNum));
    //     }).OnComplete(() =>
    //     {
    //         if (callback != null)
    //             callback();
    //     }).SetId(targetText);
    // }

    public void SetAbiStrings(TextMeshProUGUI targetText, long minDamage, long maxDamage)
    {
        string min = LongConvertString(minDamage);
        string max = LongConvertString(maxDamage);
        targetText.text = string.Format("{0}~{1}", min, max);
    }

    public void TweeningNumber(TextMeshProUGUI targetText, int startNum, int endNum, float duration, float delay, UnityAction callback)
    {
        targetText.text = $"{startNum}";
        DOTween.To(() => startNum, x =>
        {
            startNum = x;
            targetText.text = $"{startNum}";
        }, endNum, duration)
        .SetDelay(delay)
        .SetId(targetText)
        .OnComplete(() =>
        {
            callback?.Invoke();
        });
    }

    public void TweeningPercentage(TextMeshProUGUI targetText, float startPer, float endPer, float duration, float delay, UnityAction callback)
    {
        // 초기 텍스트 설정
        targetText.text = GetPercentageString(startPer);

        // DOTween으로 퍼센트 애니메이션
        DOTween.To(() => startPer, x =>
        {
            startPer = x; // 현재 값을 업데이트
            targetText.text = GetPercentageString(startPer);
        }, endPer, duration)
        .SetDelay(delay)          // 딜레이 설정
        .SetId(targetText)        // 텍스트 객체를 ID로 설정
        .OnComplete(() =>
        {
            callback?.Invoke();   // 애니메이션 완료 시 콜백 호출
        });
    }

    public void TweeningTimerLadderDungeonTimer(TextMeshProUGUI targetText, float startnum, float endnum, float duration, float delay, UnityAction callback)
    {
        float numStart = startnum;
        float numEnd = endnum;

        // 초기 텍스트 설정
        targetText.text = GetTimerLadderTimerString(numStart);

        // DOTween으로 퍼센트 애니메이션
        DOTween.To(() => numStart, x =>
        {
            numStart = x; // 현재 값을 업데이트
            targetText.text = GetTimerLadderTimerString(numStart);
        }, numEnd, duration)
        .SetDelay(delay)          // 딜레이 설정
        .SetId(targetText)        // 텍스트 객체를 ID로 설정
        .OnComplete(() =>
        {
            callback?.Invoke();   // 애니메이션 완료 시 콜백 호출
        });
    }

    // public void SetAbiStrings(TextMeshProUGUI targetText, UnitStatsType statsType, long amount)
    // {
    //     targetText.text = string.Format("{0}", LongConvertString(amount));
    // }

    // public void SetAbiStrings(TextMeshProUGUI targetText, UnitStatsType statsType, float amount)
    // {
    //     targetText.text = GetAbiStrings(statsType, amount);
    // }

    // public string GetAbiStrings(UnitStatsType statsType, long amount)
    // {
    //     return string.Format("{0}", LongConvertString(amount));
    // }

    // public string GetAbiStrings(UnitStatsType statsType, float amount)
    // {
    //     if (Enums.IsPerType(statsType))
    //         return string.Format("{0}%", ToStringWithoutZero(amount * 100f));
    //     else
    //         return string.Format("{0}", ToStringNonedecimal(amount));
    // }


    // public void SetTierText(TextMeshProUGUI targetText, ItemTier tier)
    // {
    //     targetText.text = TranslateDataBox.Inst.GetItemTierString(tier);
    //     targetText.color = ColorBox.Inst.GetTierColor(tier);
    // }

    // public void SetTierText(TextMeshProUGUI targetText, EquipmentTier tier)
    // {
    //     targetText.text = TranslateDataBox.Inst.GetItemTierString(tier);
    //     targetText.color = ColorBox.Inst.GetTierColor(tier);
    // }

    int[] SplitCount = { 4, 7, 10, 13, 16, 19 };
    string[] unitStr = { "K", "M", "A", "B", "C", "D" };
    int lessCount = 2;

    public string LongConvertString(long target)
    {
        return target.ToString("N0");
        // string toStr = target.ToString();
        // int splitLevel = -1;

        // for (int i = 0; i < SplitCount.Length - 1; i++)
        // {
        //     if (SplitCount[i] > toStr.Length)
        //         continue;
        //     splitLevel++;
        // }

        // if (splitLevel == -1)
        //     return toStr;

        // int charLength = toStr.Length;
        // int split = SplitCount[splitLevel] - 1;
        // string targetStr = unitStr[splitLevel];

        // int toUnitCount = charLength - split;
        // string front = toStr.Remove(toUnitCount, split);
        // string back = toStr.Substring(charLength - split, lessCount);

        // if (back.EndsWith("0"))
        //     back = back.Substring(0, back.Length - 1);
        // return string.Format("{0}.{1}{2}", front, back, targetStr);
    }

    // public void SetLeagueNameText(TextMeshProUGUI targetText, LadderTier targetTier)
    // {
    //     if (targetTier == LadderTier.NONE)
    //         targetText.text = TranslateDataBox.Inst.GetLadderTierNames(targetTier);
    //     else
    //         targetText.text = TranslateDataBox.Inst.GetLadderTierNames(targetTier) + " " + TranslateDataBox.Inst.GetExtraTextStrings(ExtraTextType.LEAGUE);
    //     targetText.colorGradientPreset = ColorBox.Inst.GetLadderTierGradients(targetTier);
    // }

    public string ToStringWithoutZero(float value)
    {
        if (Enums.Approximately(value, 0f))
            return "0.0";

        // 숫자를 지정된 소수 자리수로 반올림하여 문자열로 변환
        string formattedValue = value.ToString("F2");

        // 소수점 이하 부분이 없으면 ".0"을 추가
        if (!formattedValue.Contains("."))
            return formattedValue + ".0";

        // 소수점 이하 두 번째 자리가 0이면 제거
        string[] parts = formattedValue.Split('.');
        if (parts[1].Length > 1 && parts[1][1] == '0')
            return parts[0] + "." + parts[1][0];

        return formattedValue;
    }
    public string ToStringNonedecimal(float value) => value.ToString("N0");

}
