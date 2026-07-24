using UnityEngine;

namespace Tatemis
{
    /// VFX de impacto placeholder (T1.6): un "pop" en el punto de golpe — un destello que crece de
    /// golpe y se desvanece, del color de la efectividad (misma tabla que el número y el flash). Es
    /// caja gris a propósito; cuando haya partículas de verdad, se sustituye sin tocar a quien llama.
    /// El tamaño y la vida escalan con el peso del golpe, como todo lo demás del feedback.
    ///
    /// Se dibuja con dos primitivas sin collider que se animan y auto-destruyen — nada que cablear.
    public class ImpactVFX : MonoBehaviour
    {
        public static bool Enabled = true;

        // Afinado en playtest (2026-07-22): la primera pasada (0.14-0.26 s · 0.5-1.5 u) era casi
        // imperceptible — contra una cápsula de 2 u de alto, medio metro de destello no se ve.
        const float MinLife = 0.26f, MaxLife = 0.5f;
        const float MinSize = 1.3f, MaxSize = 3.2f;

        float _life, _age, _size;
        Transform _core;      // núcleo brillante
        Transform _ring;      // anillo que se expande
        MaterialPropertyBlock _mpb;
        Renderer _coreR, _ringR;
        Color _color;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public static void Spawn(Vector3 point, float multiplier, float weight01)
        {
            if (!Enabled) return;
            weight01 = Mathf.Clamp01(weight01);
            var go = new GameObject("~ImpactVFX");
            go.transform.position = point;
            var fx = go.AddComponent<ImpactVFX>();
            fx.Init(HitFlash.ColorFor(multiplier), weight01);
        }

        void Init(Color color, float weight01)
        {
            _color = color;
            _life = Mathf.Lerp(MinLife, MaxLife, weight01);
            _size = Mathf.Lerp(MinSize, MaxSize, weight01);
            _mpb = new MaterialPropertyBlock();

            _core = MakePiece(PrimitiveType.Sphere);
            _coreR = _core.GetComponent<Renderer>();
            _ring = MakePiece(PrimitiveType.Sphere);
            _ringR = _ring.GetComponent<Renderer>();
        }

        Transform MakePiece(PrimitiveType type)
        {
            var p = GameObject.CreatePrimitive(type);
            Destroy(p.GetComponent<Collider>());
            p.transform.SetParent(transform, false);
            p.GetComponent<Renderer>().sharedMaterial = VfxMaterial();
            return p.transform;
        }

        void Update()
        {
            _age += Time.unscaledDeltaTime;   // sobrevive al hitstop (que congela Time.time)
            float t = _age / _life;
            if (t >= 1f) { Destroy(gameObject); return; }

            // Núcleo: aparece grande y se encoge; anillo: parte pequeño y se expande. Los dos se apagan.
            // El anillo usa una curva desacelerada (sqrt): se abre de golpe y frena — un golpe seco,
            // no una expansión lineal de burbuja.
            float ringT = Mathf.Sqrt(t);
            _core.localScale = Vector3.one * (_size * Mathf.Lerp(1f, 0.1f, t));
            _ring.localScale = Vector3.one * (_size * Mathf.Lerp(0.3f, 2.2f, ringT));

            Paint(_coreR, new Color(_color.r, _color.g, _color.b, (1f - t) * (1f - t) * 1.1f));
            Paint(_ringR, new Color(_color.r, _color.g, _color.b, (1f - ringT) * 0.55f));
        }

        void Paint(Renderer r, Color c)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorId, c);
            _mpb.SetColor("_Color", c);
            r.SetPropertyBlock(_mpb);
        }

        static Material _mat;
        static Material VfxMaterial()
        {
            if (_mat != null) return _mat;
            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            _mat = new Material(sh);
            if (_mat.HasProperty("_Surface")) _mat.SetFloat("_Surface", 1f);   // Transparent
            if (_mat.HasProperty("_SrcBlend")) _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (_mat.HasProperty("_DstBlend")) _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);   // aditivo = brilla
            if (_mat.HasProperty("_ZWrite")) _mat.SetInt("_ZWrite", 0);
            _mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _mat.renderQueue = 3100;
            return _mat;
        }
    }
}
