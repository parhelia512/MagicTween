using System.Collections.Generic;
using UnityEngine.Jobs;

namespace MagicTween.Core.Transforms
{
    internal static class TransformManager
    {
        static TransformAccessArray transformAccessArray;
        readonly static Dictionary<int, int> instanceIdToArrayIndex = new();
        readonly static Dictionary<int, int> arrayIndexToInstanceId = new();

        public static bool IsCreated => transformAccessArray.isCreated;

        public static ref TransformAccessArray GetAccessArrayRef()
        {
            return ref transformAccessArray;
        }

        public static void Initialize()
        {
            transformAccessArray = new TransformAccessArray(32);
            instanceIdToArrayIndex.Clear();
            arrayIndexToInstanceId.Clear();
        }

        public static void Dispose()
        {
            transformAccessArray.Dispose();
        }

        public static void Register(TweenTargetTransform target)
        {
            if (!IsCreated) return;
            if (target.isRegistered) return;
            target.isRegistered = true;
            var instanceId = target.instanceId;
            if (!instanceIdToArrayIndex.ContainsKey(instanceId))
            {
                var index = transformAccessArray.length;
                transformAccessArray.Add(target.target);
                instanceIdToArrayIndex.Add(instanceId, index);
                arrayIndexToInstanceId.Add(index, instanceId);
            }
        }

        public static void Unregister(TweenTargetTransform target)
        {
            if (!IsCreated) return;
            if (!target.isRegistered) return;
            target.isRegistered = false;
            if (instanceIdToArrayIndex.TryGetValue(target.instanceId, out var index))
            {
                if (transformAccessArray.length == 1)
                {
                    instanceIdToArrayIndex.Remove(target.instanceId);
                    arrayIndexToInstanceId.Remove(index);
                    transformAccessArray.RemoveAtSwapBack(index);
                    return;
                }

                var lastIndex = transformAccessArray.length - 1;
                var lastInstanceId = arrayIndexToInstanceId[lastIndex];

                instanceIdToArrayIndex.Remove(target.instanceId);
                arrayIndexToInstanceId.Remove(lastIndex);
                transformAccessArray.RemoveAtSwapBack(index);

                instanceIdToArrayIndex[lastInstanceId] = index;
                arrayIndexToInstanceId[index] = lastInstanceId;
            }
        }

        public static int IndexOf(int instanceId)
        {
            return instanceIdToArrayIndex[instanceId];
        }
    }

}