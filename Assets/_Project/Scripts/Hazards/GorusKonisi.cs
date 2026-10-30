using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Yerde görünen görüş konisi (insan, tilki) ve koninin içinde mi sorusu.</summary>
    public static class GorusKonisi
    {
        private const int Dilim = 16;

        /// <summary>Sahibinin önüne, yere yassı bir koni çizer.</summary>
        public static void Ciz(Transform sahip, float menzil, float aci, Material malzeme)
        {
            var koni = new GameObject("GorusKonisi", typeof(MeshFilter), typeof(MeshRenderer));
            koni.transform.SetParent(sahip, false);
            koni.transform.localPosition = Vector3.up * 0.03f;

            var koseler = new Vector3[Dilim + 2];
            var ucgenler = new int[Dilim * 3];
            float yariAci = aci / 2f;
            for (int i = 0; i <= Dilim; i++)
            {
                float a = Mathf.Deg2Rad * Mathf.Lerp(-yariAci, yariAci, (float)i / Dilim);
                koseler[i + 1] = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * menzil;
            }
            for (int i = 0; i < Dilim; i++)
            {
                ucgenler[i * 3] = 0;
                ucgenler[i * 3 + 1] = i + 1;
                ucgenler[i * 3 + 2] = i + 2;
            }

            var mesh = new Mesh { name = "GorusKonisi", vertices = koseler, triangles = ucgenler };
            mesh.RecalculateNormals();
            koni.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = koni.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = malzeme;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        /// <summary>Hedef, sahibin önündeki koninin içinde ve araya engel girmeden görünüyor mu.</summary>
        public static bool Goruyor(Transform sahip, Vector3 hedef, float menzil, float aci, float gozYuksekligi, int engelMaskesi)
        {
            Vector3 duz = hedef - sahip.position;
            duz.y = 0f;
            if (duz.magnitude > menzil || Vector3.Angle(sahip.forward, duz) > aci / 2f) return false;
            Vector3 goz = sahip.position + Vector3.up * gozYuksekligi;
            return !Physics.Linecast(goz, hedef, engelMaskesi, QueryTriggerInteraction.Ignore);
        }
    }
}
