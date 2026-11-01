using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kumda iz: kaplumbağa yürüdükçe arkasında yüzgeç izleri kalır ve yavaşça silinir. İzler sahnenin kökünde, sınırlı sayıda (en eskisi yeniden kullanılır).</summary>
    public class IzBirakici : MonoBehaviour
    {
        private static readonly int RenkId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Material malzeme;
        [Tooltip("Bu kadar yürüyünce bir iz (metre).")]
        [SerializeField] private float aralik = 0.35f;
        [Tooltip("İzin eni ve boyu (metre); kaplumbağanın boyuna göre.")]
        [SerializeField] private Vector2 boy = new Vector2(0.5f, 0.25f);
        [SerializeField] private int enFazla = 40;
        [Tooltip("İzin silinme süresi (sn).")]
        [SerializeField] private float omur = 25f;
        [Tooltip("Sağ ve sol iz arasındaki yarı mesafe (metre).")]
        [SerializeField] private float yanAcikligi = 0.2f;

        private Transform[] izler;
        private Renderer[] gorunusler;
        private float[] yaslar;
        private MaterialPropertyBlock blok;
        private Color taban;
        private Vector3 oncekiYer;
        private int sira;
        private bool sag;

        private void Awake()
        {
            blok = new MaterialPropertyBlock();
            taban = malzeme != null ? malzeme.GetColor(RenkId) : new Color(0f, 0f, 0f, 0.3f);
        }

        private void OnEnable() => oncekiYer = transform.position;

        private void OnDestroy()
        {
            if (izler == null) return;
            foreach (Transform iz in izler)
                if (iz != null) Destroy(iz.gameObject);
        }

        private void Update()
        {
            if (malzeme == null) return;
            if (izler == null) Kur();

            for (int i = 0; i < izler.Length; i++)
            {
                if (!izler[i].gameObject.activeSelf) continue;
                yaslar[i] += Time.deltaTime;
                if (yaslar[i] >= omur)
                {
                    izler[i].gameObject.SetActive(false);
                    continue;
                }
                Color c = taban;
                c.a *= 1f - yaslar[i] / omur;
                blok.SetColor(RenkId, c);
                gorunusler[i].SetPropertyBlock(blok);
            }

            Vector3 fark = transform.position - oncekiYer;
            fark.y = 0f;
            if (fark.magnitude < aralik) return;
            oncekiYer = transform.position;
            Birak(fark.normalized);
        }

        private void Kur()
        {
            izler = new Transform[enFazla];
            gorunusler = new Renderer[enFazla];
            yaslar = new float[enFazla];
            for (int i = 0; i < enFazla; i++)
            {
                GameObject iz = GameObject.CreatePrimitive(PrimitiveType.Quad);
                iz.name = "Iz";
                Destroy(iz.GetComponent<Collider>());
                gorunusler[i] = iz.GetComponent<Renderer>();
                gorunusler[i].sharedMaterial = malzeme;
                gorunusler[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                iz.transform.localScale = new Vector3(boy.x, boy.y, 1f);
                iz.SetActive(false);
                izler[i] = iz.transform;
            }
        }

        /// <summary>Sırayla sağ ve sol yüzgeç izi; kumun üstüne yatık.</summary>
        private void Birak(Vector3 yon)
        {
            Vector3 yan = Vector3.Cross(Vector3.up, yon) * (sag ? yanAcikligi : -yanAcikligi);
            sag = !sag;
            Vector3 yer = Zemin.Oturt(transform.position + yan) + Vector3.up * 0.015f;
            Transform iz = izler[sira];
            iz.SetPositionAndRotation(yer, Quaternion.LookRotation(Vector3.down, yon));
            iz.gameObject.SetActive(true);
            yaslar[sira] = 0f;
            sira = (sira + 1) % izler.Length;
        }
    }
}
