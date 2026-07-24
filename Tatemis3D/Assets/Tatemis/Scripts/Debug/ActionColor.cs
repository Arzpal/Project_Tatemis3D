using UnityEngine;

namespace Tatemis
{
    /// Debug: tiñe al personaje con un color según la acción en curso (para leer de un vistazo qué hace).
    /// Usa MaterialPropertyBlock → no instancia materiales. Vuelve al color base al terminar el flash.
    public class ActionColor : MonoBehaviour
    {
        public Renderer target;
        public Color baseColor = new Color(0.25f, 0.5f, 0.9f);

        MaterialPropertyBlock _mpb;
        float _timer;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        void Awake()
        {
            if (target == null) target = GetComponentInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();
            Apply(baseColor);
        }

        public void Flash(Color c, float duration)
        {
            Apply(c);
            _timer = duration;
        }

        void Update()
        {
            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f) Apply(baseColor);
            }
        }

        void Apply(Color c)
        {
            if (target == null || _mpb == null) return;
            target.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorId, c);
            target.SetPropertyBlock(_mpb);
        }
    }
}
