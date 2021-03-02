using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrCrystal {

//
// Fragment animation with pre-deterministic Newtonian motion
//
readonly struct Fragment
{
    #region Configuration structure

    [System.Serializable]
    public struct Config
    {
        [SerializeField] float _radius;
        [SerializeField] float _spin;
        [SerializeField] float _drag;
        [SerializeField] float _scale;

        public float Radius => _radius;
        public float Spin => _spin;
        public float Drag => _drag;
        public float Scale => _scale;
    }

    #endregion

    #region Data storage

    readonly Config _config;
    readonly XXHash _hash;
    readonly float _time;

    #endregion

    #region Constructor

    public Fragment(in Config config, uint seed, float time)
    {
        _config = config;
        _hash = new XXHash(seed);
        _time = time;
    }

    #endregion

    #region Properties

    // Current state

    public float3 Position
      => -TotalMotion * (math.exp(-_config.Drag * _time) - 1);

    public float Spin
      => -TotalSpin * (math.exp(-_config.Drag * _time) - 1);

    public quaternion Rotation
      => math.mul(InitialRotation, quaternion.AxisAngle(SpinAxis, Spin));

    public float3 Scale
      => math.float3(1, 1, 1) * (_config.Scale * _hash.Float(0.3f, 1.0f, 1));

    // Initial state

    quaternion InitialRotation
      => _hash.Rotation(3);

    float3 SpinAxis
      => _hash.Direction(4);

    // Total amount of motion

    float3 TotalMotion
      => _hash.InSphere(5) * _config.Radius;

    float TotalSpin
      => _hash.Float(6) * _config.Spin;

    #endregion
}

//
// Fragment transform update job
//
[BurstCompile]
struct FragmentUpdateJob : IJobParallelForTransform
{
    Fragment.Config _config;
    XXHash _hash;
    float _time;

    public FragmentUpdateJob(in Fragment.Config config, uint seed, float time)
    {
        _config = config;
        _hash = new XXHash(seed);
        _time = time;
    }

    public void Execute(int index, TransformAccess xform)
    {
        var frag = new Fragment(_config, _hash.UInt((uint)index), _time);
        xform.localPosition = frag.Position;
        xform.localRotation = frag.Rotation;
        xform.localScale = frag.Scale;
    }
}

} // namespace DxrCrystal
