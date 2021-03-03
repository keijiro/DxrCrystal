using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrCrystal {

[System.Serializable]
public struct LightBarConfig
{
    [SerializeField] float _frequency;
    [SerializeField, Range(0, 1)] float _distribution;
    [SerializeField] float3 _extent;

    public float Frequency => _frequency;
    public float Distribution => _distribution;
    public float3 Extent => _extent;
}

[BurstCompile]
struct LightBarUpdateJob : IJobParallelForTransform
{
    LightBarConfig _config;
    XXHash _hash;
    float _time;

    public LightBarUpdateJob(in LightBarConfig config, uint seed, float time)
    {
        _config = config;
        _hash = new XXHash(seed);
        _time = time;
    }

    public void Execute(int index, TransformAccess xform)
    {
        // Per-element hash
        var hash = new XXHash(_hash.UInt((uint)index));

        // Total accumulated parameter
        var param = _time * _config.Frequency;
        param *= hash.Float(1 - _config.Distribution, 1, 1u);

        // Per-stride random vector
        var rand = hash.Float3((uint)param + 2u);

        // Position
        var pos = math.float3(rand.x, rand.y, math.frac(param));
        pos = (pos - 0.5f) * _config.Extent;

        // Roll (z-axis rotation)
        var roll = rand.z * math.PI * 2;

        xform.localPosition = pos;
        xform.localRotation = quaternion.AxisAngle(math.float3(0, 0, 1), roll);
    }
}

} // namespace DxrCrystal
