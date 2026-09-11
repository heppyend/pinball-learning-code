using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BNRoom.Static
{
    public static class BuffManager
    {
        private class BuffData
        {
            public EnumEffectValueType type;
            public int remainTurns;
            public float parameter;
            public GameObject effectInstance;   // 存储该 BUFF 对应的特效对象

            public BuffData(EnumEffectValueType type, int turns, float param, GameObject effect)
            {
                this.type = type;
                this.remainTurns = turns;
                this.parameter = param;
                this.effectInstance = effect;
            }
        }

        private static Dictionary<GameObject, List<BuffData>> buffMap = new Dictionary<GameObject, List<BuffData>>();

        // ----- 公共 API -----

        /// <summary>
        /// 为指定对象添加一个 BUFF，并创建对应的特效
        /// </summary>
        public static void AddBuff(GameObject obj, EnumEffectValueType type, int turns, int param)
        {
            if (obj == null) return;

            // 如果持续回合为 0 或负数，直接忽略，不创建任何数据
            if (turns <= 0)
            {
                SendOnceBuffValue(obj, type, param);
                return;
            }

            // 加载特效预制件
            string effectPath = SysDefines.MARBLEEFFECTS + type + "OverTime";
            GameObject prefab = ResManager.Instance.LoadPrefab(effectPath);
            GameObject effect = null;
            if (prefab != null)
            {
                effect = ObjectPoolManager.Instance.Spawn(
                    prefab,
                    Vector3.zero,
                    Quaternion.identity,
                    obj.transform
                );
            }

            // 创建 BUFF 数据并存储
            var buffData = new BuffData(type, turns, param, effect);
            if (!buffMap.ContainsKey(obj))
                buffMap[obj] = new List<BuffData>();
            buffMap[obj].Add(buffData);
            // Debug.Log($"Added BUFF {type} to {obj.name}, turns={turns}, param={param}, effect={effect}");
        }

        /// <summary>
        /// 全局减少所有对象上所有 BUFF 的剩余回合，归零则移除并销毁特效
        /// </summary>
        public static void ReduceAllBuffs()
        {
            List<GameObject> keys = new List<GameObject>(buffMap.Keys);

            foreach (var obj in keys)
            {
                if (obj == null)
                {
                    // 若对象已销毁，清理其所有 BUFF 特效
                    DestroyAllEffectsInList(buffMap[obj]);
                    buffMap.Remove(obj);
                    continue;
                }

                var list = buffMap[obj];
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var buff = list[i];
                    buff.remainTurns--;
                    if (buff.remainTurns <= 0)
                    {
                        // 销毁特效
                        if (buff.effectInstance != null)
                            ObjectPoolManager.Instance.Unspawn(buff.effectInstance);
                        list.RemoveAt(i);
                        // Debug.Log($"BUFF {buff.type} on {obj.name} expired and effect removed.");
                    }
                }

                if (list.Count == 0)
                    buffMap.Remove(obj);
            }
        }

        /// <summary>
        /// 获取指定对象上所有 BUFF 的枚举类型和汇总后的效果参数（同类型相加）
        /// </summary>
        public static List<(EnumEffectValueType type, float parameter)> GetBuffInfo(GameObject obj)
        {
            var result = new List<(EnumEffectValueType, float)>();
            if (obj == null || !buffMap.ContainsKey(obj))
                return result;

            var sumDict = new Dictionary<EnumEffectValueType, float>();
            foreach (var buff in buffMap[obj])
            {
                if (sumDict.ContainsKey(buff.type))
                    sumDict[buff.type] += buff.parameter;
                else
                    sumDict[buff.type] = buff.parameter;
            }

            foreach (var kv in sumDict)
                result.Add((kv.Key, kv.Value));
            return result;
        }

        /// <summary>
        /// 获取指定对象上某类型所有 BUFF 的参数总和
        /// </summary>
        public static float GetBuffParameter(GameObject obj, EnumEffectValueType type)
        {
            if (obj == null || !buffMap.ContainsKey(obj))
                return 0f;

            float total = 0f;
            foreach (var buff in buffMap[obj])
                if (buff.type == type)
                    total += buff.parameter;
            return total;
        }

        /// <summary>
        /// 检查对象是否拥有某类型的 BUFF
        /// </summary>
        public static bool HasBuff(GameObject obj, EnumEffectValueType type)
        {
            if (obj == null || !buffMap.ContainsKey(obj))
                return false;
            return buffMap[obj].Exists(b => b.type == type);
        }

        /// <summary>
        /// 清除所有 BUFF 数据并销毁所有特效（场景切换时调用）
        /// </summary>
        public static void ClearAllBuffs()
        {
            buffMap.Clear();
        }

        /// <summary>
        /// 清理已被销毁的对象（移除 null 键并销毁对应特效）
        /// </summary>
        public static void Cleanup()
        {
            var toRemove = new List<GameObject>();
            foreach (var kv in buffMap)
            {
                if (kv.Key == null)
                {
                    DestroyAllEffectsInList(kv.Value);
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var key in toRemove)
                buffMap.Remove(key);
        }

        // ------ 辅助方法 ------

        private static void DestroyAllEffectsInList(List<BuffData> list)
        {
            if (list == null) return;
            foreach (var buff in list)
            {
                if (buff.effectInstance != null)
                    ObjectPoolManager.Instance.Unspawn(buff.effectInstance);
            }
            list.Clear();
        }

        private static void SendOnceBuffValue(GameObject obj, EnumEffectValueType type, int param)
        {
            switch (type)
            {
                case EnumEffectValueType.RestoreHpByPercentage:
                    var copy = new Dictionary<string, object>();
                    copy.Add("Value", param);
                    copy.Add("IsPercentage", true);
                    if (obj.CompareTag("Marble"))
                    {
                        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_BUFFADDENEMYHEALTH, null, null,copy);
                    }
                    break;
                case EnumEffectValueType.RestoreHp:
                    if (obj.CompareTag("Marble"))
                    {
                        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_SETHEALTH, null, param);
                    }
                    break;
            }
        }
    }
}