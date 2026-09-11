using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BNRoom.Static
{
    /// <summary>
    /// 目标查找工具类（返回 GameObject）
    /// </summary>
    public static class TargetFinder
    {
        /// <summary>
        /// 查找目标对象
        /// </summary>
        /// <param name="selfPosition">自身位置</param>
        /// <param name="selfTag">自身标签（如 "EnemyMarbe" 或 "Marble"）</param>
        /// <param name="targetType">1=敌人，5=友军</param>
        /// <param name="subType1">1=最近，2=最远，3=全部（不排序）</param>
        /// <param name="subType2">0=无，1=1个，2=2个，3=全部</param>
        /// <returns>目标 GameObject 列表</returns>
        public static List<GameObject> FindTargets(
            Vector3 selfPosition,
            string selfTag,
            int targetType,
            int subType1,
            int subType2)
        {
            // 1. 确定目标标签
            string targetTag = GetTargetTag(selfTag, targetType);
            if (string.IsNullOrEmpty(targetTag))
                return new List<GameObject>();

            // 2. 获取所有目标对象
            GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
            if (targets.Length == 0)
                return new List<GameObject>();

            // 3. 计算距离并构建信息列表
            List<TargetInfo> infos = new List<TargetInfo>();
            foreach (var obj in targets)
            {
                float dist = Vector3.Distance(selfPosition, obj.transform.position);
                infos.Add(new TargetInfo { GameObject = obj, Distance = dist });
            }

            // 4. 排序
            if (subType1 == 1) // 最近 → 升序
                infos = infos.OrderBy(t => t.Distance).ToList();
            else if (subType1 == 2) // 最远 → 降序
                infos = infos.OrderByDescending(t => t.Distance).ToList();
            else if (subType1 == 3)
                subType2 = subType1;

            // 5. 截取数量
            int count = 0;
            if (subType2 == 0)
                return new List<GameObject>();
            else if (subType2 == 1)
                count = 1;
            else if (subType2 == 2)
                count = 2;
            else // subType2 == 3
                count = infos.Count;

            if (count > infos.Count)
                count = infos.Count;

            // 6. 提取 GameObject
            List<GameObject> result = new List<GameObject>();
            for (int i = 0; i < count; i++)
                result.Add(infos[i].GameObject);

            return result;
        }

        /// <summary>
        /// 根据自身标签和目标类型获取目标标签
        /// </summary>
        private static string GetTargetTag(string selfTag, int targetType)
        {
            if (targetType == 1) // 敌人
            {
                if (selfTag == "EnemyMarble")
                    return "Marble";
                else if (selfTag == "Marble")
                    return "EnemyMarble";
                else
                    return selfTag; // 回退
            }
            else if (targetType == 5) // 友军
            {
                return selfTag;
            }
            else
            {
                Debug.LogError($"无效的目标类型: {targetType}");
                return null;
            }
        }

        // 辅助结构体
        private struct TargetInfo
        {
            public GameObject GameObject;
            public float Distance;
        }
    }
}