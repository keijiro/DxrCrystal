using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrCrystal {

readonly struct Fragment
{
    #region Configuration structure

    [System.Serializable]
    public struct Config
    {
        [SerializeField] float _radius;
        [SerializeField] float _speed;
        [SerializeField] float _spin;
        [SerializeField] float _scale;

        public float Radius => _radius;
        public float Speed => _speed;
        public float Spin => _spin;
        public float Scale => _scale;
    }

    #endregion

    #region Data storage

    readonly XXHash _hash;
    readonly float3 _position;
    readonly quaternion _rotation;

    #endregion

    #region Public accessors

    public float3 Position => _position;
    public quaternion Rotation => _rotation;

    public float3 GetScale(in Config config)
      => math.float3(1, 1, 1) * (config.Scale * _hash.Float(0.3f, 1.0f, 6));

    #endregion

    #region Private properties and methods

    static float3 GetInitialPosition(in XXHash hash, in Config config)
      => hash.InSphere(1) * config.Radius;

    static quaternion GetInitialRotation(in XXHash hash)
      => hash.Rotation(2);

    float3 GetVelocity(in Config config)
      => math.normalize(GetInitialPosition(_hash, config)) *
           _hash.Float(0.1f, 1.0f, 3) * config.Speed;

    float3 GetDeltaPosition(in Config config, float dt)
      => GetVelocity(config) * dt;

    float3 RotationAxis
      => _hash.Direction(4);

    float GetAngularVelocity(in Config config)
      => _hash.Float(5) * config.Spin;

    quaternion GetDeltaRotation(in Config config, float dt)
      => quaternion.AxisAngle(RotationAxis, GetAngularVelocity(config) * dt);

    #endregion

    #region Factory methods and constructor

    public static Fragment InitialState(uint seed, in Config config)
    {
        var hash = new XXHash(seed);
        return new Fragment(hash,
                            GetInitialPosition(hash, config),
                            GetInitialRotation(hash));
    }

    public Fragment(in XXHash hash, float3 position, quaternion rotation)
    {
        _hash = hash;
        _position = position;
        _rotation = rotation;
    }

    #endregion

    #region Frame advance method

    public Fragment NextFrame(in Config config, float dt)
      => new Fragment(_hash,
                      _position + GetDeltaPosition(config, dt),
                      math.mul(_rotation, GetDeltaRotation(config, dt)));

    #endregion
}

[BurstCompile]
struct FragmentUpdateJob : IJobParallelForTransform
{
    NativeArray<Fragment> _rocks;
    Fragment.Config _config;
    float _dt;

    public FragmentUpdateJob(NativeArray<Fragment> rocks, in Fragment.Config config, float dt)
    {
        _rocks = rocks;
        _config = config;
        _dt = dt;
    }

    public void Execute(int index, TransformAccess xform)
    {
        var rock = _rocks[index].NextFrame(_config, _dt);
        xform.localPosition = rock.Position;
        xform.localRotation = rock.Rotation;
        xform.localScale = rock.GetScale(_config);
        _rocks[index] = rock;
    }
}

} // namespace DxrCrystal
