using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrCrystal {

sealed class Scatter : MonoBehaviour
{
    [SerializeField] float _radius = 10;
    [SerializeField] float _speed = 1;
    [SerializeField] uint _instanceCount = 100;
    [SerializeField] uint _randomSeed = 1;
    [SerializeField] Mesh[] _meshes;
    [SerializeField] Material _reflectiveMaterial;
    [SerializeField] Material _emissiveMaterial;

    NativeArray<Rock> _rocks;
    TransformAccessArray _taa;

    void Start()
    {
        var hash = new XXHash(_randomSeed);
        var seed = 1u;
        var parent = transform;

        var rocks = new Rock[_instanceCount];
        var xforms = new Transform[_instanceCount];

        for (var i = 0u; i < _instanceCount; i++)
        {
            var mesh = _meshes[i % _meshes.Length];
            var go = ObjectFactory.CreateDoubleMeshObject
              ("Rock", mesh, _reflectiveMaterial, _emissiveMaterial);

            rocks[i] = Rock.InitialState(hash.UInt(seed++), _radius, _speed);
            xforms[i] = go.transform;
        }

        _rocks = new NativeArray<Rock>(rocks, Allocator.Persistent);
        _taa = new TransformAccessArray(xforms);
    }

    void Update()
      => new RockUpdateJob(_rocks, Time.deltaTime).Schedule(_taa).Complete();

    void OnDestroy()
    {
        _rocks.Dispose();
        _taa.Dispose();
    }
}

} // namespace DxrCrystal
