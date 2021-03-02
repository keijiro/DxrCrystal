using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrCrystal {

sealed class Scatter : MonoBehaviour
{
    [SerializeField] float _radius = 10;
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

        var template = new GameObject
          ("Instance", typeof(MeshFilter), typeof(MeshRenderer));

        for (var i = 0u; i < _instanceCount; i++)
        {
            var rock = Rock.InitialState(hash.UInt(seed++), _radius);
            var go = Instantiate(template, rock.Position, rock.Rotation, parent);

            var mesh = _meshes[i % _meshes.Length];
            var material = hash.Float(seed++) < 0.1f ?
              _emissiveMaterial : _reflectiveMaterial;

            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;

            rocks[i] = rock;
            xforms[i] = go.transform;
        }

        Destroy(template);

        _rocks = new NativeArray<Rock>(rocks, Allocator.Persistent);
        _taa = new TransformAccessArray(xforms);
    }

    void Update()
      => new RockUpdateJob(_rocks, Time.deltaTime).Schedule(_taa).Complete();

    void OnDestroy()
    {
        for (var i = 0; i < _taa.length; i++) Destroy(_taa[i].gameObject);

        _rocks.Dispose();
        _taa.Dispose();
    }
}

} // namespace DxrCrystal
