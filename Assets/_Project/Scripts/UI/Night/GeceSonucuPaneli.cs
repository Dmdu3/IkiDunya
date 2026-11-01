using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Night
{
    /// <summary>Gecenin sonu: yuva gecesinde yuvanın bandı (renk ve adı) ve yumurta sayısı, kaçışta kurtulan, kaybedilen, dişi ve erkek. Başlık ve satır adları MetinYazisi ile metinler.md'den gelir.</summary>
    public class GeceSonucuPaneli : MonoBehaviour
    {
        [SerializeField] private YuvaGecesi yuvaGecesi;
        [SerializeField] private KacisGecesi kacisGecesi;
        [SerializeField] private TermalAyarlari termal;
        [SerializeField] private MetinTablosu metinler;

        [Header("Yuva gecesi")]
        [SerializeField] private GameObject yuvaPaneli;
        [Tooltip("Yuvanın sıcaklık bandı rengi.")]
        [SerializeField] private Image yuvaBandi;
        [Tooltip("Bandın adı (bant_mavi … bant_kirmizi); renk ayırt edemeyen oyuncu için.")]
        [SerializeField] private Text bantAdi;
        [SerializeField] private Text yumurtaSayisi;

        [Header("Kaçış gecesi")]
        [SerializeField] private GameObject kacisPaneli;
        [SerializeField] private Text kurtulan;
        [SerializeField] private Text kaybedilen;
        [SerializeField] private Text disi;
        [SerializeField] private Text erkek;
        [Tooltip("Kayıpların nedenleri (Sıcak kum 4 · Ağ 2 …); kayıp yoksa boş.")]
        [SerializeField] private Text kayipNedenleri;

        /// <summary>Sonuç paneli ekranda (ipucu kutusu bu sırada bekler).</summary>
        public static bool Acik { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => Acik = false;

        private void OnEnable()
        {
            if (yuvaGecesi != null)
            {
                yuvaGecesi.SonucHazir += YuvaSonucu;
                yuvaGecesi.SonucKapandi += Kapat;
            }
            if (kacisGecesi != null)
            {
                kacisGecesi.SonucHazir += KacisSonucu;
                kacisGecesi.SonucKapandi += Kapat;
            }
            Kapat();
        }

        private void OnDisable()
        {
            if (yuvaGecesi != null)
            {
                yuvaGecesi.SonucHazir -= YuvaSonucu;
                yuvaGecesi.SonucKapandi -= Kapat;
            }
            if (kacisGecesi != null)
            {
                kacisGecesi.SonucHazir -= KacisSonucu;
                kacisGecesi.SonucKapandi -= Kapat;
            }
            Acik = false;
        }

        private void YuvaSonucu(Yuva yuva)
        {
            if (yuva == null || yuvaPaneli == null) return;
            SicaklikBandi bant = yuvaGecesi.YuvaBandi(yuva);
            yuvaBandi.color = termal.Renk(bant);
            MetinTablosu tablo = metinler != null ? metinler : MetinTablosu.Yuklu;
            if (bantAdi != null && tablo != null) bantAdi.text = tablo.Al($"bant_{bant.ToString().ToLowerInvariant()}");
            yumurtaSayisi.text = yuva.Yumurta.ToString();
            yuvaPaneli.SetActive(true);
            Acik = true;
        }

        private void KacisSonucu(KacisGecesi.Sonuc sonuc)
        {
            if (kacisPaneli == null) return;
            kurtulan.text = sonuc.Kurtulan.ToString();
            kaybedilen.text = sonuc.Kaybedilen.ToString();
            disi.text = sonuc.Disi.ToString();
            erkek.text = sonuc.Erkek.ToString();
            if (kayipNedenleri != null) kayipNedenleri.text = Nedenler(sonuc.Nedenler);
            kacisPaneli.SetActive(true);
            Acik = true;
        }

        /// <summary>"Sıcak kum 4 · Ağ 2"; çoktan aza, sıfırlar yazılmaz.</summary>
        private string Nedenler(int[] sayilar)
        {
            if (sayilar == null) return "";
            MetinTablosu tablo = metinler != null ? metinler : MetinTablosu.Yuklu;
            var sira = new List<int>();
            for (int i = 0; i < sayilar.Length; i++) if (sayilar[i] > 0) sira.Add(i);
            sira.Sort((a, b) => sayilar[b].CompareTo(sayilar[a]));
            var parcalar = new List<string>();
            foreach (int i in sira)
            {
                string id = $"kayip_{((KayipNedeni)i).ToString().ToLowerInvariant()}";
                parcalar.Add($"{(tablo != null ? tablo.Al(id) : id)} {sayilar[i]}");
            }
            return string.Join("  ·  ", parcalar);
        }

        private void Kapat()
        {
            if (yuvaPaneli != null) yuvaPaneli.SetActive(false);
            if (kacisPaneli != null) kacisPaneli.SetActive(false);
            Acik = false;
        }
    }
}
