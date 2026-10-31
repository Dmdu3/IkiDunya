using System.Collections.Generic;
using System.Text;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Management;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Management
{
    /// <summary>İnsan turunun ekranı: üstte kaynaklar ve aşama, solda yuvalar ve imlecin altındaki bölge, altta aşamanın paneli (rapor, görevler, kart eli, olay) ve İleri düğmesi.
    /// Bütün yazılar metin tablosundan id ile gelir.</summary>
    public class YonetimArayuzu : MonoBehaviour
    {
        [SerializeField] private InsanTuru tur;
        [SerializeField] private HaritaSecici harita;
        [SerializeField] private MetinTablosu metinler;
        [SerializeField] private GameObject kok;

        [Header("Üst ve sol")]
        [SerializeField] private Text ustBilgi;
        [SerializeField] private Text asamaBasligi;
        [SerializeField] private Text yuvaListesi;
        [SerializeField] private Text bolgeBilgisi;
        [SerializeField] private Text hedefYazisi;

        [Header("Paneller")]
        [SerializeField] private GameObject raporPaneli;
        [SerializeField] private Text raporYazisi;
        [SerializeField] private GameObject gorevPaneli;
        [SerializeField] private Transform gorevDugmeleri;
        [SerializeField] private GameObject kartPaneli;
        [SerializeField] private Transform kartDugmeleri;
        [SerializeField] private GameObject olayPaneli;
        [SerializeField] private Text olayYazisi;
        [SerializeField] private Button aDugmesi;
        [SerializeField] private Button bDugmesi;
        [SerializeField] private Button ileriDugmesi;
        [Tooltip("Görev ve kart düğmelerinin kalıbı (kapalı duran bir düğme, içinde Text).")]
        [SerializeField] private Button dugmeKalibi;

        private readonly List<Button> dugmeler = new List<Button>();
        private int sonBolge = -2;

        private MetinTablosu Tablo => metinler != null ? metinler : MetinTablosu.Yuklu;
        private string Al(string id) => Tablo != null ? Tablo.Al(id) : $"[{id}]";

        private void OnEnable()
        {
            tur.Degisti += Yenile;
            tur.Yonetim.Degisti += Yenile;
            tur.Kumsal.Degisti += Yenile;
            ileriDugmesi.onClick.AddListener(tur.Ilerle);
            aDugmesi.onClick.AddListener(() => tur.Sec(true));
            bDugmesi.onClick.AddListener(() => tur.Sec(false));
            Yenile();
        }

        private void OnDisable()
        {
            if (tur == null) return;
            tur.Degisti -= Yenile;
            tur.Yonetim.Degisti -= Yenile;
            tur.Kumsal.Degisti -= Yenile;
            ileriDugmesi.onClick.RemoveAllListeners();
            aDugmesi.onClick.RemoveAllListeners();
            bDugmesi.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            if (harita.Ustundeki == sonBolge) return;
            sonBolge = harita.Ustundeki;
            BolgeBilgisi();
        }

        private void Yenile()
        {
            bool acik = tur.SuAnki != InsanTuru.Asama.Kapali && tur.SuAnki != InsanTuru.Asama.Bitiyor;
            kok.SetActive(acik);
            if (!acik) return;

            YonetimDurumu y = tur.Yonetim;
            KumsalDurumu k = tur.Kumsal;
            ustBilgi.text = $"{Al("yonetim_tur")} {y.Tur}     {Al("yonetim_butce")} {y.Butce}     " +
                            $"{Al("yonetim_gonullu")} {y.BosGonullu(k)}/{y.Gonullu(k)}     {Al("yonetim_destek")} {y.Destek}/{y.Ayarlar.DestekEnFazla}";
            asamaBasligi.text = Al($"asama_{tur.SuAnki.ToString().ToLowerInvariant()}");

            raporPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Rapor);
            gorevPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Gonulluler);
            kartPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Kartlar);
            olayPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Olay);
            ileriDugmesi.gameObject.SetActive(tur.SuAnki != InsanTuru.Asama.Olay);

            Rapor();
            Dugmeler();
            Olay();
            YuvaListesi();
            BolgeBilgisi();

            hedefYazisi.text = tur.BeklenenHedef == Hedef.Bolge ? Al("hedef_bolge_sec")
                : tur.BeklenenHedef == Hedef.Yuva ? Al("hedef_yuva_sec") : "";
        }

        private void Rapor()
        {
            var sb = new StringBuilder();
            foreach (RaporSatiri satir in tur.Yonetim.Rapor)
                sb.AppendLine(satir.Sayi != 0 ? $"{Al(satir.Id)}: {satir.Sayi}" : Al(satir.Id));
            if (sb.Length == 0) sb.AppendLine(Al("rapor_sakin"));
            raporYazisi.text = sb.ToString();
        }

        /// <summary>Görev ya da kart düğmelerini baştan kurar.</summary>
        private void Dugmeler()
        {
            foreach (Button d in dugmeler)
                if (d != null) Destroy(d.gameObject);
            dugmeler.Clear();

            if (tur.SuAnki == InsanTuru.Asama.Gonulluler)
                foreach (GorevTanimi gorev in tur.Yonetim.Ayarlar.Gorevler)
                {
                    GorevTanimi g = gorev;
                    Dugme(gorevDugmeleri, $"{Al(g.Id)}\n<size=18>{Al(g.Id + "_aciklama")}</size>",
                        tur.GorevVerilebilir(g), tur.BekleyenGorev == g, () => tur.GorevSec(g));
                }

            if (tur.SuAnki == InsanTuru.Asama.Kartlar)
                foreach (KartTanimi kart in tur.El)
                {
                    KartTanimi k = kart;
                    Dugme(kartDugmeleri, $"{Al(k.Id)}  ({tur.Yonetim.Bedel(k)})\n<size=18>{Al(k.Id + "_aciklama")}</size>",
                        tur.KartOynanabilir(k), tur.BekleyenKart == k, () => tur.KartSec(k));
                }
        }

        private void Dugme(Transform ebeveyn, string yazi, bool acik, bool secili, UnityEngine.Events.UnityAction tikla)
        {
            Button dugme = Instantiate(dugmeKalibi, ebeveyn);
            dugme.gameObject.SetActive(true);
            dugme.GetComponentInChildren<Text>().text = yazi;
            dugme.interactable = acik;
            if (secili) dugme.GetComponent<Image>().color = new Color(1f, 0.85f, 0.4f);
            dugme.onClick.AddListener(tikla);
            dugmeler.Add(dugme);
        }

        private void Olay()
        {
            OlayTanimi olay = tur.Olay;
            if (olay == null) return;
            olayYazisi.text = Al(olay.Id);
            aDugmesi.GetComponentInChildren<Text>().text = olay.Uyari ? Al("olay_tamam") : Al(olay.Id + "_a");
            aDugmesi.interactable = tur.SecenekAcik(true);
            bDugmesi.gameObject.SetActive(!olay.Uyari);
            bDugmesi.GetComponentInChildren<Text>().text = Al(olay.Id + "_b");
            bDugmesi.interactable = tur.SecenekAcik(false);
        }

        private void YuvaListesi()
        {
            var sb = new StringBuilder();
            IReadOnlyList<Yuva> yuvalar = tur.Kumsal.Yuvalar;
            for (int i = 0; i < yuvalar.Count; i++)
            {
                Yuva y = yuvalar[i];
                sb.Append($"{Al("yuva")} {i + 1}: {Al($"bant_{y.Bant.ToString().ToLowerInvariant()}")} · {Al("yuva_yumurta")} {y.Yumurta} · {Al("yuva_hafta")} {y.CikisinaHafta}");
                if (y.Kafesli) sb.Append($" · {Al("yuva_kafesli")}");
                sb.AppendLine();
            }
            yuvaListesi.text = sb.ToString();
        }

        private void BolgeBilgisi()
        {
            int i = harita.Ustundeki;
            if (i < 0 || i >= tur.Kumsal.BolgeSayisi)
            {
                bolgeBilgisi.text = "";
                return;
            }
            Bolge b = tur.Kumsal.BolgeAl(i);
            var sb = new StringBuilder($"{Al("bolge")} {i + 1}\n");
            foreach (BolgeKatmani katman in System.Enum.GetValues(typeof(BolgeKatmani)))
                sb.AppendLine($"{Al($"katman_{katman.ToString().ToLowerInvariant()}")}: {b.Seviye(katman)}");
            foreach (BolgeYapisi yapi in System.Enum.GetValues(typeof(BolgeYapisi)))
                if (b.Var(yapi)) sb.AppendLine(Al($"yapi_{yapi.ToString().ToLowerInvariant()}"));
            bolgeBilgisi.text = sb.ToString();
        }
    }
}
