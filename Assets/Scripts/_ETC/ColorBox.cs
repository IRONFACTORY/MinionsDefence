using UnityEngine;
using TMPro;

public class ColorBox : MonoBehaviour
{
    public static ColorBox Inst;

    public Color loadingBackgroundFadeColor;
    public Color loadingBackgroundColor;
    public Color whiteClear;

    public Color stageGage_stageStartColor;
    public Color stageGage_wavingColor;
    public Color stageGage_resultColor;

    public TMP_ColorGradient Damage_Normal, Damage_Critical, DamageFixed;
    public TMP_ColorGradient ItemGradient;

    void Awake()
    {
        Inst = this;
    }

    public TMP_ColorGradient GetDamageTextGradient(DamageType dmgType)
    {
        switch (dmgType)
        {
            case DamageType.NORMAL:
                return Damage_Normal;
            case DamageType.CRITICAL:
                return Damage_Critical;
            case DamageType.FIXED:
                return DamageFixed;
            default:
                return Damage_Normal;
        }
    }

}
