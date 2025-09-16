using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public enum CostType { STAMINA = 0, GOLD, DIA }
public enum AnimationType { IDLE = 0, WALK, ATTACK, DIE };
public enum UnitType { HERO = 0, MINION };
public enum IngameItemType { GAME_GOLD = 0, };

public enum DamageType { NORMAL = 0, CRITICAL, FIXED, };

public enum TeamType { A, B, C, D };

/// <summary>
/// GUILD_HALL = 모든 관리
/// MINE_FIELD = 채석장 (스코어링 깊게 가는것)
/// WOOD_FIELD = 내가 뿌린대로 획득하는곳
/// FIELD = 그냥 필드
/// </summary>

public class Enums
{
    public static readonly Dictionary<CostType, int> COST_MAX = new Dictionary<CostType, int>
    {
        { CostType.STAMINA, 10 },
        { CostType.GOLD, 0 },
        { CostType.DIA, 0 },
    };

    public static List<Vector2> eightDirections = new List<Vector2>()
    {
        new Vector2(0f, 1f).normalized,
        new Vector2(1f, 1f).normalized,
        new Vector2(1f, 0f).normalized,
        new Vector2(1f, -1f).normalized,
        new Vector2(0f, -1f).normalized,
        new Vector2(-1f, -1f).normalized,
        new Vector2(-1f, 0f).normalized,
        new Vector2(-1f, 1f).normalized,
    };
    public static void InitializeOnStart(RectTransform target, RectTransform parent)
    {
        target.SetParent(parent);
        target.anchoredPosition3D = Vector3.zero;
        target.localScale = Vector3.one;
    }

    public static long LongRandom(long min, long max)
    {
        if (min == 0 && max == 0)
            return 0;

        return RandomExtentions.NextLong(new System.Random(), min, max);
    }


    public static Vector2 DirFromAngle(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    public static readonly long[] COST_START = new long[] { 0, 0, 0, 0, 0 };

    /// <summary>
    /// 웨이브 시간들
    /// </summary>

    public const int START_TOWER_CARD = 3;
    public const int TOWER_PURCHASE_COST_START = 100;
    public const int TOWER_PURCHASE_COST_ADDS = 50;

    public const int MINION_DEFAULT_EXP = 10;
    public const int MINION_BOSS_EXP = 10;

    public const int MINION_DEFAULT_GOLD = 10000;
    public const int MINION_BOSS_GOLD = 100;

    public const int INGAME_HERO_LEVEL_MAX = 20;

    public const float GAGE_SPD = 6f;

    public const float STAGE_WAVE_TIME = 10f;
    public const float STAGE_FIRST_DELAY_TIME = 2.5f;


    public const int ITEM_COUNT_MAX = 999999;
    public const float UI_ANIMATION_TIME_DEFAULT = 0.5f;
    public const float MAIN_FIGHT_LOADING_DURATION = .5f;
    public const float MAIN_FIGHT_LOADING_DELAY = 1f;

    public const float GAME_END_Y = 0f;
    public const float GAME_END_Y_VISIBLE = -2f;

    public const string ANI_IDLE = "Idle", ANI_WALK = "Walk", ANI_ATTACK = "Attack", ANI_SKILL = "Skill";

    public static bool Approximately(long amount, long compare) => amount == compare;
    public static bool Approximately(float amount, float compare) => Mathf.Approximately(amount, compare);

    public static float BackgroundAlpha = 0.825f;

    public static Vector2 GetTargetHoldPos(Vector2 spawnPos)
    {
        Vector2 viewportPosition = MainCamera.Inst.GetCamera().WorldToViewportPoint(spawnPos);
        RectTransform canvasRect = MyCanvas.Inst.GetCanvasRect();
        Vector2 canvasPosition = new Vector2(
            viewportPosition.x * canvasRect.sizeDelta.x,
            viewportPosition.y * canvasRect.sizeDelta.y
        );
        return canvasPosition;
    }

    public static Vector2 RectTransformToScreenPos(RectTransform targetRect)
    {
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        Vector3[] corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);
        Vector3 worldPoint = (corners[0] + corners[2]) / 2;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(UICamera.Inst.GetCamera(), worldPoint);

        float scaleFactor = canvas.scaleFactor;
        screenPoint /= scaleFactor;
        return screenPoint;
    }

}


public static class RandomExtentions
{
    //returns a uniformly random ulong between ulong.Min inclusive and ulong.Max inclusive
    public static ulong NextULong(this System.Random rng)
    {
        byte[] buf = new byte[8];
        rng.NextBytes(buf);
        return BitConverter.ToUInt64(buf, 0);
    }

    //returns a uniformly random ulong between ulong.Min and Max without modulo bias
    public static ulong NextULong(this System.Random rng, ulong max, bool inclusiveUpperBound = false)
    {
        return rng.NextULong(ulong.MinValue, max, inclusiveUpperBound);
    }

    //returns a uniformly random ulong between Min and Max without modulo bias
    public static ulong NextULong(this System.Random rng, ulong min, ulong max, bool inclusiveUpperBound = false)
    {
        ulong range = max - min;

        if (inclusiveUpperBound)
        {
            if (range == ulong.MaxValue)
                return rng.NextULong();
            range++;
        }
        if (range <= 0)
            throw new ArgumentOutOfRangeException("Max must be greater than min when inclusiveUpperBound is false, and greater than or equal to when true", "max");
        ulong limit = ulong.MaxValue - ulong.MaxValue % range;
        ulong r;
        do
        {
            r = rng.NextULong();
        } while (r > limit);
        return r % range + min;
    }

    //returns a uniformly random long between long.Min inclusive and long.Max inclusive
    public static long NextLong(this System.Random rng)
    {
        byte[] buf = new byte[8];
        rng.NextBytes(buf);
        return BitConverter.ToInt64(buf, 0);
    }

    //returns a uniformly random long between long.Min and Max without modulo bias
    public static long NextLong(this System.Random rng, long max, bool inclusiveUpperBound = false)
    {
        return rng.NextLong(long.MinValue, max, inclusiveUpperBound);
    }

    //returns a uniformly random long between Min and Max without modulo bias
    public static long NextLong(this System.Random rng, long min, long max, bool inclusiveUpperBound = false)
    {
        ulong range = (ulong)(max - min);

        if (inclusiveUpperBound)
        {
            if (range == ulong.MaxValue)
                return rng.NextLong();
            range++;
        }
        if (range <= 0)
            throw new ArgumentOutOfRangeException("Max must be greater than min when inclusiveUpperBound is false, and greater than or equal to when true", "max");
        ulong limit = ulong.MaxValue - ulong.MaxValue % range;
        ulong r;
        do
        {
            r = rng.NextULong();
        } while (r > limit);
        return (long)(r % range + (ulong)min);
    }
}


public static class ArrayExtensions
{
    public static T[] ResizeArrayToEnumLength<T, TEnum>(this T[] array, Action<bool> callback = null)
        where TEnum : Enum
    {
        int enumLength = Enum.GetValues(typeof(TEnum)).Length;

        if (array.Length >= enumLength)
        {
            callback?.Invoke(false);
            return array;
        }

        Debug.Log($"Resizing array from {array.Length} to {enumLength}");

        var newArray = new T[enumLength];
        Array.Copy(array, newArray, array.Length);

        bool itemCreated = false;
        for (int i = array.Length; i < enumLength; i++)
        {
            newArray[i] = (T)CreateNewItem<T, TEnum>((TEnum)(object)i);
            Debug.Log($"New Item Created: {(TEnum)(object)i}");
            itemCreated = true;
        }

        callback?.Invoke(itemCreated);
        return newArray;
    }

    private static object CreateNewItem<T, TEnum>(TEnum enumValue)
        where TEnum : Enum
    {
        object item;
        var itemType = typeof(T);

        if (itemType.IsValueType)
        {
            if (itemType == typeof(int))
                item = Convert.ChangeType((int)(object)enumValue, itemType);
            else
                item = Activator.CreateInstance(itemType);
        }
        else
        {
            var constructor = itemType.GetConstructor(new[] { typeof(TEnum) });

            if (constructor != null)
            {
                item = Activator.CreateInstance(itemType, enumValue);
            }
            else
            {
                item = Activator.CreateInstance(itemType);
                var setIndexMethod = itemType.GetMethod("SetIndex");
                if (setIndexMethod != null)
                {
                    setIndexMethod.Invoke(item, new object[] { enumValue });
                }
            }
        }

        return item;
    }
}


public static class EnumExtensions
{
    public static int Length<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum)).Length;
    }

}