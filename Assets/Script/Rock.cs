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

    #endregion

    #region Public accessors

    public float3 Position => _position;
    public quaternion Rotation => _rotation;

    #endregion

    #region Private properties and methods

    float3 Velocity => _hash.InSphere(3) * 0.5f;

    float3 Axis => _hash.Direction(4);

    float AngularAxis => _hash.Float(5);

    quaternion DeltaRotation(float dt)
      => quaternion.AxisAngle(Axis, AngularAxis * dt);

    #endregion

    #region Factory methods and constructor

    public static Rock InitialState(uint seed, float radius)
      => InitialState(new XXHash(seed), radius);

    public static Rock InitialState(in XXHash hash, float radius)
      => new Rock(hash, hash.InSphere(1) * radius, hash.Rotation(2));

    public Rock(in XXHash hash, float3 position, quaternion rotation)
    {
        _hash = hash;
        _position = position;
        _rotation = rotation;
    }

    #endregion

    #region Frame advance function

    public Rock NextFrame(float dt)
      => new Rock(_hash,
                  _position + Velocity * dt,
                  math.mul(_rotation, DeltaRotation(dt)));

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
