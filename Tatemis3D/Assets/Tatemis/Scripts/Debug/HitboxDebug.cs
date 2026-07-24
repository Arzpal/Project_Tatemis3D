using UnityEngine;

namespace Tatemis
{
    /// Debug visual (solo para pruebas): instancia una esfera roja translúcida donde se activa una
    /// hitbox, para ver en la Game view dónde golpea cada ataque. Pon HitboxDebug.Enabled = false
    /// (o borra este archivo y las llamadas Show) para quitarlo.
    public static class HitboxDebug
    {
        public static bool Enabled = true;

        /// Color de caja gris de cada elemento — para que un estallido se lea de un vistazo.
        /// Provisional: cuando haya VFX de verdad, esto se tira.
        public static Color ColorOf(Element e)
        {
            switch (e)
            {
                case Element.Fuego:      return new Color(1f, 0.35f, 0.05f, 0.4f);
                case Element.Agua:       return new Color(0.2f, 0.55f, 1f, 0.4f);
                case Element.Naturaleza: return new Color(0.3f, 0.9f, 0.3f, 0.4f);
                case Element.Mineral:    return new Color(0.85f, 0.7f, 0.35f, 0.45f);
                case Element.Rayo:       return new Color(0.95f, 0.9f, 0.25f, 0.45f);
                default:                 return new Color(0.8f, 0.95f, 1f, 0.35f);   // Aire
            }
        }

        static Material _mat;

        static Material Mat()
        {
            if (_mat != null) return _mat;
            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            _mat = new Material(sh);
            var red = new Color(1f, 0.15f, 0.15f, 0.35f);

            // Configurar transparencia (URP Unlit).
            if (_mat.HasProperty("_Surface")) _mat.SetFloat("_Surface", 1f);      // 1 = Transparent
            if (_mat.HasProperty("_Blend"))   _mat.SetFloat("_Blend", 0f);        // 0 = Alpha
            if (_mat.HasProperty("_SrcBlend")) _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (_mat.HasProperty("_DstBlend")) _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (_mat.HasProperty("_ZWrite"))  _mat.SetInt("_ZWrite", 0);
            _mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _mat.renderQueue = 3000;
            if (_mat.HasProperty("_BaseColor")) _mat.SetColor("_BaseColor", red);
            if (_mat.HasProperty("_Color"))     _mat.SetColor("_Color", red);
            return _mat;
        }

        /// Muestra una esfera translúcida (sin collider) en `center` con el `radius` dado, durante
        /// `duration`. `color` tiñe esa esfera concreta (un estallido elemental) sin tocar el material.
        public static void Show(Vector3 center, float radius, float duration = 0.12f, Color? color = null)
        {
            if (!Enabled) return;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "HitboxDebug";
            Object.Destroy(go.GetComponent<Collider>());   // que NO interfiera con la física
            go.transform.position = center;
            go.transform.localScale = Vector3.one * radius * 2f;   // el primitivo mide 1 de diámetro

            var rend = go.GetComponent<Renderer>();
            rend.sharedMaterial = Mat();
            if (color.HasValue)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_BaseColor", color.Value);
                mpb.SetColor("_Color", color.Value);
                rend.SetPropertyBlock(mpb);
            }
            Object.Destroy(go, duration);
        }
    }
}
