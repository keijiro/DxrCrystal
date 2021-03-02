using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DxrCrystal {

[ExecuteInEditMode]
sealed class FragmentGroup : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [SerializeField] Fragment.Config _fragmentConfig;
    [SerializeField] uint _instanceCount = 100;
    [SerializeField] uint _randomSeed = 1;
    [SerializeField] Mesh[] _meshes;
    [SerializeField] Material _mainMaterial;
    [SerializeField] Material _overlayMaterial;

    #endregion

    #region Private fields

    TransformAccessArray _taa;
    (Material main, Material overlay) _materials;
    float _time;

    #endregion

    #region ITimeControl implementation

    public void OnControlTimeStart() {}
    public void OnControlTimeStop() {}
    public void SetTime(double time) => _time = (float)time;

    #endregion

    #region IPropertyPreview implementation

    public void GatherProperties
      (PlayableDirector director, IPropertyCollector driver) {}

    #endregion

    #region Fragment object population

    void Prepare()
    {
        if (_taa.isCreated) return;

        _materials.main = new Material(_mainMaterial);
        _materials.main.hideFlags = HideFlags.HideAndDontSave;

        if (_overlayMaterial)
        {
            _materials.overlay = new Material(_overlayMaterial);
            _materials.overlay.hideFlags = HideFlags.HideAndDontSave;
        }

        var xforms = new Transform[_instanceCount];

        for (var i = 0u; i < _instanceCount; i++)
        {
            var mesh = _meshes[i % _meshes.Length];
            var go = ObjectFactory.CreateDoubleMeshObject
              (transform, mesh, _materials.main, _materials.overlay);
            xforms[i] = go.transform;
        }

        _taa = new TransformAccessArray(xforms);
    }

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        Prepare();

        _materials.main.SetFloat("_LocalTime", _time);
        _materials.overlay?.SetFloat("_LocalTime", _time);

        new FragmentUpdateJob(_fragmentConfig, _randomSeed, _time)
           .Schedule(_taa).Complete();
    }

    void OnDisable()
    {
        if (!_taa.isCreated) return;

        for (var i = 0; i < _taa.length; i++)
            if (Application.isPlaying)
                Destroy(_taa[i].gameObject);
            else
                DestroyImmediate(_taa[i].gameObject);

        _taa.Dispose();
    }

    #endregion
}

} // namespace DxrCrystal
