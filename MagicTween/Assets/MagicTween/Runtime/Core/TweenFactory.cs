using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using MagicTween.Core.Components;
using Unity.Burst;
using System.Runtime.CompilerServices;

namespace MagicTween.Core
{
    using static TweenWorld;

    [BurstCompile]
    public static class TweenFactory
    {
        public static Tween<TValue, TOptions> CreateToTween<TValue, TOptions, TPlugin>(
            TweenGetter<TValue> getter, TweenSetter<TValue> setter, in TValue endValue, float duration)
            where TValue : unmanaged
            where TOptions : unmanaged, ITweenOptions
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            var archetype = ArchetypeStorageRef.GetLambdaTweenArchetype<TValue, TOptions>(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<LambdaTweenController<TValue, TPlugin>>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);
            AddPropertyAccessor<TValue, TPlugin>(entity, getter(), endValue, getter, setter);

            return new Tween<TValue, TOptions>(entity);
        }

        public static Tween<TValue, TOptions> CreateFromToTween<TValue, TOptions, TPlugin>(
            in TValue startValue, in TValue endValue, float duration, TweenSetter<TValue> setter)
            where TValue : unmanaged
            where TOptions : unmanaged, ITweenOptions
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            var archetype = ArchetypeStorageRef.GetLambdaTweenArchetype<TValue, TOptions>(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<LambdaTweenController<TValue, TPlugin>>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);
            AddPropertyAccessor<TValue, TPlugin>(entity, startValue, endValue, null, setter);

            return new Tween<TValue, TOptions>(entity);
        }

        public static Tween<TValue, TOptions> CreateToTweenNoAlloc<TObject, TValue, TOptions, TPlugin>(
            TObject target, TweenGetter<TObject, TValue> getter, TweenSetter<TObject, TValue> setter, in TValue endValue, float duration)
            where TObject : class
            where TValue : unmanaged
            where TOptions : unmanaged, ITweenOptions
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            var archetype = ArchetypeStorageRef.GetNoAllocLambdaTweenArchetype<TValue, TOptions>(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<NoAllocLambdaTweenController<TValue, TPlugin>>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);
            AddPropertyAccessorNoAlloc<TObject, TValue, TPlugin>(entity, target, getter(target), endValue, getter, setter);

            return new Tween<TValue, TOptions>(entity);
        }
        public static Tween<TValue, TOptions> CreateFromToTweenNoAlloc<TObject, TValue, TOptions, TPlugin>(
            TObject target, in TValue startValue, in TValue endValue, float duration, TweenSetter<TObject, TValue> setter)
            where TObject : class
            where TValue : unmanaged
            where TOptions : unmanaged, ITweenOptions
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            var archetype = ArchetypeStorageRef.GetNoAllocLambdaTweenArchetype<TValue, TOptions>(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<NoAllocLambdaTweenController<TValue, TPlugin>>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);
            AddPropertyAccessorNoAlloc<TObject, TValue, TPlugin>(entity, target, startValue, endValue, null, setter);

            return new Tween<TValue, TOptions>(entity);
        }

        public static Tween<TValue, PunchTweenOptions> CreatePunchTween<TValue, TPlugin>(
            TweenGetter<TValue> getter, TweenSetter<TValue> setter, in TValue strength, float duration)
            where TValue : unmanaged
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            var archetype = ArchetypeStorageRef.GetPunchLambdaTweenArchetype<TValue>(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<LambdaTweenController<TValue, TPlugin>>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);

            EntityManager.SetComponentData(entity, new TweenOptions<PunchTweenOptions>
            {
                value = new PunchTweenOptions()
                {
                    frequency = 10,
                    dampingRatio = 1f
                }
            });
            EntityManager.SetComponentData(entity, new VibrationStrength<TValue>() { value = strength });
            EntityManager.SetComponentData(entity, new TweenStartValue<TValue>() { value = getter() });
            EntityManager.SetComponentData(entity, TweenPropertyAccessorPool<TValue>.Rent(getter, setter));

            return new Tween<TValue, PunchTweenOptions>(entity);
        }

        public static Tween<TValue, PunchTweenOptions> CreatePunchTweenNoAlloc<TObject, TValue, TPlugin>(
            TObject target, TweenGetter<TObject, TValue> getter, TweenSetter<TObject, TValue> setter, TValue strength, float duration)
            where TObject : class
            where TValue : unmanaged
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            var archetype = ArchetypeStorageRef.GetNoAllocPunchLambdaTweenArchetype<TValue>(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<LambdaTweenController<TValue, TPlugin>>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);

            EntityManager.SetComponentData(entity, new TweenOptions<PunchTweenOptions>
            {
                value = new PunchTweenOptions()
                {
                    frequency = 10,
                    dampingRatio = 1f
                }
            });
            EntityManager.SetComponentData(entity, new VibrationStrength<TValue>() { value = strength });
            EntityManager.SetComponentData(entity, new TweenStartValue<TValue>() { value = getter(target) });
            EntityManager.SetComponentData(entity, TweenPropertyAccessorNoAllocPool<TValue>.Rent(
                target,
                UnsafeUtility.As<TweenGetter<TObject, TValue>, TweenGetter<object, TValue>>(ref getter),
                UnsafeUtility.As<TweenSetter<TObject, TValue>, TweenSetter<object, TValue>>(ref setter)
            ));

            return new Tween<TValue, PunchTweenOptions>(entity);
        }

        public static Tween<TValue, ShakeTweenOptions> CreateShakeTween<TValue, TPlugin>(
            TweenGetter<TValue> getter, TweenSetter<TValue> setter, in TValue strength, float duration)
            where TValue : unmanaged
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            var archetype = ArchetypeStorageRef.GetShakeLambdaTweenArchetype<TValue>(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<LambdaTweenController<TValue, TPlugin>>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);

            EntityManager.SetComponentData(entity, new TweenOptions<ShakeTweenOptions>
            {
                value = new ShakeTweenOptions()
                {
                    frequency = 10,
                    dampingRatio = 1f,
                    randomSeed = 0
                }
            });
            EntityManager.SetComponentData(entity, new VibrationStrength<TValue>() { value = strength });
            EntityManager.SetComponentData(entity, new TweenStartValue<TValue>() { value = getter() });
            EntityManager.SetComponentData(entity, TweenPropertyAccessorPool<TValue>.Rent(getter, setter));

            return new Tween<TValue, ShakeTweenOptions>(entity);
        }

        public static Tween<TValue, ShakeTweenOptions> CreateShakeTweenNoAlloc<TObject, TValue, TPlugin>(
            TObject target, TweenGetter<TObject, TValue> getter, TweenSetter<TObject, TValue> setter, TValue strength, float duration)
            where TObject : class
            where TValue : unmanaged
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            var archetype = ArchetypeStorageRef.GetNoAllocShakeLambdaTweenArchetype<TValue>(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<LambdaTweenController<TValue, TPlugin>>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);

            EntityManagerRef.SetComponentData(entity, new TweenOptions<ShakeTweenOptions>
            {
                value = new ShakeTweenOptions()
                {
                    frequency = 10,
                    dampingRatio = 1f,
                    randomSeed = 0
                }
            });
            EntityManager.SetComponentData(entity, new VibrationStrength<TValue>() { value = strength });
            EntityManager.SetComponentData(entity, new TweenStartValue<TValue>() { value = getter(target) });

            EntityManager.SetComponentData(entity, TweenPropertyAccessorNoAllocPool<TValue>.Rent(
                target,
                UnsafeUtility.As<TweenGetter<TObject, TValue>, TweenGetter<object, TValue>>(ref getter),
                UnsafeUtility.As<TweenSetter<TObject, TValue>, TweenSetter<object, TValue>>(ref setter)
            ));

            return new Tween<TValue, ShakeTweenOptions>(entity);
        }

        public static Tween<UnsafeText, StringTweenOptions> CreateStringToTween(TweenGetter<string> getter, TweenSetter<string> setter, string endValue, float duration)
        {
            var archetype = ArchetypeStorageRef.GetStringLambdaTweenArchetype(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<StringLambdaTweenController>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);

            var start = new TweenStartValue<UnsafeText>()
            {
                value = new UnsafeText(0, Allocator.Persistent)
            };

            var endValueText = new UnsafeText(System.Text.Encoding.UTF8.GetByteCount(endValue), Allocator.Persistent);
            endValueText.CopyFrom(endValue);
            var end = new TweenEndValue<UnsafeText>()
            {
                value = endValueText
            };

            var value = new TweenValue<UnsafeText>()
            {
                value = new UnsafeText(endValue.Length, Allocator.Persistent)
            };

            EntityManager.SetComponentData(entity, start);
            EntityManager.SetComponentData(entity, end);
            EntityManager.SetComponentData(entity, value);
            EntityManager.SetComponentData(entity, TweenPropertyAccessorPool<string>.Rent(getter, setter));

            return new Tween<UnsafeText, StringTweenOptions>(entity);
        }
        public static Tween<UnsafeText, StringTweenOptions> CreateStringFromToTween(TweenSetter<string> setter, string startValue, string endValue, float duration)
        {
            var archetype = ArchetypeStorageRef.GetStringLambdaTweenArchetype(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<StringLambdaTweenController>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);

            var startValueText = new UnsafeText(System.Text.Encoding.UTF8.GetByteCount(startValue), Allocator.Persistent);
            startValueText.CopyFrom(startValue);
            var start = new TweenStartValue<UnsafeText>()
            {
                value = startValueText
            };

            var endValueText = new UnsafeText(System.Text.Encoding.UTF8.GetByteCount(endValue), Allocator.Persistent);
            endValueText.CopyFrom(endValue);
            var end = new TweenEndValue<UnsafeText>()
            {
                value = endValueText
            };

            var value = new TweenValue<UnsafeText>()
            {
                value = new UnsafeText(endValue.Length, Allocator.Persistent)
            };

            EntityManager.SetComponentData(entity, start);
            EntityManager.SetComponentData(entity, end);
            EntityManager.SetComponentData(entity, value);
            EntityManager.SetComponentData(entity, TweenPropertyAccessorPool<string>.Rent(null, setter));

            return new Tween<UnsafeText, StringTweenOptions>(entity);
        }

        public unsafe static Tween<float3, PathTweenOptions> CreatePathTween(TweenGetter<float3> getter, TweenSetter<float3> setter, float3[] points, float duration)
        {
            var archetype = ArchetypeStorageRef.GetPathLambdaTweenArchetype(ref EntityManagerRef);
            var controllerId = TweenControllerContainer.GetId<LambdaTweenController<float3, PathTweenPlugin>>();

            CreateEntity(ref EntityManagerRef, archetype, duration, controllerId, out var entity);

            var buffer = EntityManager.GetBuffer<PathPoint>(entity);
            buffer.Resize(points.Length + 1, NativeArrayOptions.UninitializedMemory);

            fixed (float3* src = &points[0])
            {
                UnsafeUtility.MemCpy((float3*)buffer.AsNativeArray().GetUnsafePtr() + 1, src, points.Length * sizeof(float3));
            }

            EntityManager.SetComponentData(entity, TweenPropertyAccessorPool<float3>.Rent(getter, setter));

            return new Tween<float3, PathTweenOptions>(entity);
        }

        [BurstCompile]
        public static class ECS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Tween<TValue, TOptions> CreateFromTo<TValue, TOptions, TPlugin, TTranslator>(in Entity target, in TValue startValue, in TValue endValue, float duration)
                where TValue : unmanaged
                where TOptions : unmanaged, ITweenOptions
                where TPlugin : unmanaged, ITweenPlugin<TValue>
                where TTranslator : unmanaged, ITweenTranslatorBase<TValue>
            {
                CreateFromToCore<TValue, TOptions, TPlugin, TTranslator>(ref EntityManagerRef, ref ArchetypeStorageRef, target, startValue, endValue, duration, out var tween);
                return tween;
            }

            static void CreateFromToCore<TValue, TOptions, TPlugin, TTranslator>(ref EntityManager entityManager, ref ArchetypeStorage archetypeStorage, in Entity target, in TValue startValue, in TValue endValue, float duration, out Tween<TValue, TOptions> tween)
                where TValue : unmanaged
                where TOptions : unmanaged, ITweenOptions
                where TPlugin : unmanaged, ITweenPlugin<TValue>
                where TTranslator : unmanaged, ITweenTranslatorBase<TValue>
            {
                var archetype = archetypeStorage.GetEntityTweenArchetype<TValue, TOptions, TTranslator>(ref entityManager);
                var controllerId = TweenControllerContainer.GetId<EntityTweenController<TValue, TPlugin>>();

                CreateEntity(ref entityManager, archetype, duration, controllerId, out var entity);
                InitializeEntityTweenComponents<TValue, TTranslator>(ref entityManager, entity, startValue, endValue, target);

                entityManager.SetComponentData(entity, new TweenTranslationOptionsData(TweenTranslationOptions.FromTo));

                tween = new Tween<TValue, TOptions>(entity);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Tween<TValue, TOptions> CreateTo<TValue, TOptions, TPlugin, TTranslator>(in Entity target, in TValue endValue, float duration)
                where TValue : unmanaged
                where TOptions : unmanaged, ITweenOptions
                where TPlugin : unmanaged, ITweenPlugin<TValue>
                where TTranslator : unmanaged, ITweenTranslatorBase<TValue>
            {
                CreateToCore<TValue, TOptions, TPlugin, TTranslator>(ref EntityManagerRef, ref ArchetypeStorageRef, target, endValue, duration, out var tween);
                return tween;
            }

            static void CreateToCore<TValue, TOptions, TPlugin, TTranslator>(ref EntityManager entityManager, ref ArchetypeStorage archetypeStorage, in Entity target, in TValue endValue, float duration, out Tween<TValue, TOptions> tween)
                where TValue : unmanaged
                where TOptions : unmanaged, ITweenOptions
                where TPlugin : unmanaged, ITweenPlugin<TValue>
                where TTranslator : unmanaged, ITweenTranslatorBase<TValue>
            {
                var archetype = archetypeStorage.GetEntityTweenArchetype<TValue, TOptions, TTranslator>(ref entityManager);
                var controllerId = TweenControllerContainer.GetId<EntityTweenController<TValue, TPlugin>>();

                CreateEntity(ref entityManager, archetype, duration, controllerId, out var entity);
                InitializeEntityTweenComponents<TValue, TTranslator>(ref entityManager, entity, endValue, target);

                entityManager.SetComponentData(entity, new TweenTranslationOptionsData(TweenTranslationOptions.To));

                tween = new Tween<TValue, TOptions>(entity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tween CreateUnitTween(float duration)
        {
            CreateUnitTweenCore(ref EntityManagerRef, ref ArchetypeStorageRef, duration, out var tween);
            return tween;
        }

        static void CreateUnitTweenCore(ref EntityManager entityManager, ref ArchetypeStorage archetypeStorage, float duration, out Tween tween)
        {
            var archetype = archetypeStorage.GetUnitTweenArchetype(ref entityManager);
            var controllerId = TweenControllerContainer.GetId<UnitTweenController>();

            CreateEntity(ref entityManager, archetype, duration, controllerId, out var entity);
            tween = new Tween(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sequence CreateSequence()
        {
            CreateSequenceCore(ref EntityManagerRef, ref ArchetypeStorageRef, out var sequence);
            return sequence;
        }

        static void CreateSequenceCore(ref EntityManager entityManager, ref ArchetypeStorage archetypeStorage, out Sequence sequence)
        {
            var archetype = archetypeStorage.GetSequenceArchetype(ref entityManager);
            var controllerId = TweenControllerContainer.GetId<SequenceTweenController>();

            CreateEntity(ref entityManager, archetype, 0f, controllerId, out var entity);
            sequence = new Sequence(entity);
        }

        [BurstCompile]
        static void CreateEntity(ref EntityManager entityManager, in EntityArchetype archetype, float duration, short controllerId, out Entity entity)
        {
            entity = entityManager.CreateEntity(archetype);

            if (MagicTweenSettings.defaultAutoPlay) entityManager.SetComponentData(entity, new TweenParameterAutoPlay(true));
            if (MagicTweenSettings.defaultAutoKill) entityManager.SetComponentData(entity, new TweenParameterAutoKill(true));
            entityManager.SetComponentData(entity, new TweenParameterDuration(duration));
            entityManager.SetComponentData(entity, new TweenParameterPlaybackSpeed(1f));
            entityManager.SetComponentData(entity, new TweenParameterLoops(1));
            entityManager.SetComponentData(entity, new TweenParameterIgnoreTimeScale(MagicTweenSettings.defaultIgnoreTimeScale));
            if (MagicTweenSettings.defaultEase != Ease.Linear) entityManager.SetComponentData(entity, new TweenParameterEase(MagicTweenSettings.defaultEase));
            if (MagicTweenSettings.defaultLoopType != LoopType.Restart) entityManager.SetComponentData(entity, new TweenParameterLoopType(MagicTweenSettings.defaultLoopType));
            entityManager.SetComponentData(entity, new TweenControllerReference(controllerId));
        }

        [BurstCompile]
        static void AddStartAndEndValue<TValue>(in Entity entity, in TValue startValue, in TValue endValue)
            where TValue : unmanaged
        {
            EntityManagerRef.SetComponentData(entity, new TweenStartValue<TValue>() { value = startValue });
            EntityManagerRef.SetComponentData(entity, new TweenEndValue<TValue>() { value = endValue });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AddPropertyAccessor<TValue, TPlugin>(
            in Entity entity, in TValue startValue, in TValue endValue, TweenGetter<TValue> getter, TweenSetter<TValue> setter)
            where TValue : unmanaged
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            AddStartAndEndValue(entity, startValue, endValue);
            EntityManagerRef.SetComponentData(entity, TweenPropertyAccessorPool<TValue>.Rent(getter, setter));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AddPropertyAccessorNoAlloc<TObject, TValue, TPlugin>(
            in Entity entity, TObject target, in TValue startValue, in TValue endValue, TweenGetter<TObject, TValue> getter, TweenSetter<TObject, TValue> setter)
            where TObject : class
            where TValue : unmanaged
            where TPlugin : unmanaged, ITweenPlugin<TValue>
        {
            AddStartAndEndValue(entity, startValue, endValue);
            EntityManagerRef.SetComponentData(entity, TweenPropertyAccessorNoAllocPool<TValue>.Rent(
                target,
                UnsafeUtility.As<TweenGetter<TObject, TValue>, TweenGetter<object, TValue>>(ref getter),
                UnsafeUtility.As<TweenSetter<TObject, TValue>, TweenSetter<object, TValue>>(ref setter)
            ));
        }

        [BurstCompile]
        static void InitializeEntityTweenComponents<TValue, TTranslator>(
            ref EntityManager entityManager, in Entity entity, in TValue startValue, in TValue endValue, in Entity target)
            where TValue : unmanaged
            where TTranslator : unmanaged, ITweenTranslatorBase<TValue>
        {
            var translator = default(TTranslator);
            translator.TargetEntity = target;

            AddStartAndEndValue(entity, startValue, endValue);
            entityManager.SetComponentData(entity, translator);
        }

        [BurstCompile]
        static void InitializeEntityTweenComponents<TValue, TTranslator>(
            ref EntityManager entityManager, in Entity entity, in TValue endValue, in Entity target)
            where TValue : unmanaged
            where TTranslator : unmanaged, ITweenTranslatorBase<TValue>
        {
            var translator = default(TTranslator);
            translator.TargetEntity = target;

            entityManager.SetComponentData(entity, new TweenEndValue<TValue>() { value = endValue });
            entityManager.SetComponentData(entity, translator);
        }
    }
}