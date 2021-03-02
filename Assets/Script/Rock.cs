using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrCrystal {

readonly struct Rock
{
    #region Data storage

    readonly XXHash _hash;
    readonly float3 _position;
    readonly quaternion _rotation;
    readonly float _speed;

    #endregion

    #region Public accessors

    public float3 Position => _position;
    public quaternion Rotation => _rotation;

    #endregion

    #region Private properties and methods

    float3 Velocity => _hash.InSphere(3) * _speed;

    float3 Axis => _hash.Direction(4);

    float AngularAxis => _hash.Float(5);

    quaternion DeltaRotation(float dt)
      => quaternion.AxisAngle(Axis, AngularAxis * dt);

    #endregion

    #region Factory methods and constructor

    public static Rock InitialState(uint seed, float radius, float speed)
      => InitialState(new XXHash(seed), radius, speed);

    public static Rock InitialState(in XXHash hash, float radius, float speed)
      => new Rock(hash, hash.InSphere(1) * radius, hash.Rotation(2), speed);

    public Rock(in XXHash hash, float3 position, quaternion rotation, float speed)
    {
        _hash = hash;
        _position = position;
        _rotation = rotation;
        _speed = speed;
    }

    #endregion

    #region Frame advance function

    public Rock NextFrame(float dt)
      => new Rock(_hash,
                  _position + Velocity * dt,
                  math.mul(_rotation, DeltaRotation(dt)),
                  _speed);

    #endregion
}

struct RockUpdateJob : IJobParallelForTransform
{
    NativeArray<Rock> _rocks;
    float _dt;

    public RockUpdateJob(NativeArray<Rock> rocks, float dt)
    {
        _rocks = rocks;
        _dt = dt;
    }

    public void Execute(int index, TransformAccess xform)
    {
        var rock = _rocks[index].NextFrame(_dt);
        xform.localPosition = rock.Position;
        xform.localRotation = rock.Rotation;
        _rocks[index] = rock;
    }
}

} // namespace DxrCrystal
