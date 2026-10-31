using System.Collections.Generic;
using System.Text;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Management;
using UnityEngine;
using UnityEngine.InputSystem;
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
            GameInput.BaglamalarDegisti += Yenile;
            ileriDugmesi.onClick.AddListener(tur.Ilerle);
            aDugmesi.onClick.AddListener(() => tur.Sec(true));
            bDugmesi.onClick.AddListener(() => tur.Sec(false));
            Yenile();
        }

        private void OnDisable()
        {
            GameInput.BaglamalarDegisti -= Yenile;
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
            SolPanel();
        }

        private void Yenile()
        {
            bool acik = tur.SuAnki != InsanTuru.Asama.Kapali && tur.SuAnki != InsanTuru.Asama.Bitiyor;
            kok.SetActive(acik);
            if (!acik) return;

            YonetimDurumu y = tur.Yonetim;
            KumsalDurumu k = tur.Kumsal;
            var ust = new StringBuilder($"{Al("yonetim_tur")} {y.Tur}     {Al("yonetim_butce")} {y.Butce}");
            if (y.Bakim > 0) ust.Append($" (−{y.Bakim} {Al("kart_bakim")})");
            ust.Append($"     {Al("yonetim_gonullu")} {y.BosGonullu(k)}/{y.Gonullu(k)}     {Al("yonetim_destek")} {y.Destek}/{y.Ayarlar.DestekEnFazla}\n");
            foreach (Paydas p in System.Enum.GetValues(typeof(Paydas)))
                ust.Append($"{Al(PaydasId(p))} {Isaretli(y.Iliski(p))}     ");
            if (y.UyariVar) ust.Append($"<color=#ff9a3c>{Al($"uyari_{y.AktifUyari.ToString().ToLowerInvariant()}")}</color>");
            ustBilgi.text = ust.ToString();
            asamaBasligi.text = Al($"asama_{tur.SuAnki.ToString().ToLowerInvariant()}");

            raporPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Rapor);
            gorevPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Gonulluler);
            kartPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Kartlar);
            olayPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Olay);
            ileriDugmesi.gameObject.SetActive(tur.SuAnki != InsanTuru.Asama.Olay);
            IleriYazisi();

            Rapor();
            Dugmeler();
            Olay();
            YuvaListesi();
            BolgeBilgisi();
            SolPanel();

            hedefYazisi.text = tur.BeklenenHedef == Hedef.Bolge ? Al("hedef_bolge_sec")
                : tur.BeklenenHedef == Hedef.Yuva ? Al("hedef_yuva_sec") : "";
        }

        /// <summary>İleri düğmesinde tuşu da yazar (Space ya da ayarlardan atanan). Düğme açılınca MetinYazisi yazıyı tablodan yeniden kurar; bu ondan sonra çağrılır.</summary>
        private void IleriYazisi()
        {
            InputAction e = GameInput.Eylem("ZiplaDal");
            string tus = e != null ? $"  [{e.GetBindingDisplayString(0)}]" : "";
            ileriDugmesi.GetComponentInChildren<Text>().text = Al("yonetim_ileri") + tus;
        }

        /// <summary>Sol panel boşken (yuva yok, imleç haritada değil) haritayı kapatmasın.</summary>
        private void SolPanel()
        {
            bool dolu = yuvaListesi.text.Length > 0 || bolgeBilgisi.text.Length > 0;
            yuvaListesi.transform.parent.gameObject.SetActive(dolu);
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
                    Dugme(kartDugmeleri, KartYazisi(k), tur.KartOynanabilir(k), tur.BekleyenKart == k, () => tur.KartSec(k), kart: true);
                }
        }

        // Açık zeminli düğmede okunan renkler.
        private const string TurRengi = "#444444", AcilRengi = "#c0561b", ArtiRengi = "#2e7d32", EksiRengi = "#b03030";

        /// <summary>Ad ve bedel; tür, bakım, güçlü mü; açıklama; paydaş ve halk etkisi, koşulu.
        /// Satırlar boyla değil kalınlık ve renkle ayrılır: yazı düğmeye sığacak kadar küçülür (çok kartlı elde dar kalır).</summary>
        private string KartYazisi(KartTanimi k)
        {
            YonetimDurumu y = tur.Yonetim;
            string turRengi = k.Tur == KartTuru.Acil ? AcilRengi : TurRengi;
            var sb = new StringBuilder($"<b>{Al(k.Id)} ({y.Bedel(k)})</b>\n<color={turRengi}>{Al($"kart_turu_{k.Tur.ToString().ToLowerInvariant()}")}");
            if (k.Bakim > 0) sb.Append($" · {Al("kart_bakim")} {k.Bakim}");
            sb.Append("</color>");
            if (y.Guclu(k)) sb.Append($" · <color={ArtiRengi}>{Al("kart_guclu")}</color>");
            sb.Append($"\n{Al(k.Id + "_aciklama")}");

            var etki = new StringBuilder();
            HalkVePaydas(etki, k.DestekFarki, k.PaydasFarki);
            if (k.KosulVar)
            {
                string renk = y.KosulTamam(k) ? TurRengi : EksiRengi;
                etki.Append($"<color={renk}>{Al(PaydasId(k.KosulPaydas))} {Isaretli(k.KosulEnAz)} {Al("kart_ister")}</color>");
            }
            if (etki.Length > 0) sb.Append($"\n<b>{etki.ToString().TrimEnd()}</b>");
            return sb.ToString();
        }

        private void HalkVePaydas(StringBuilder etki, int destek, PaydasFarki paydas)
        {
            if (destek != 0) etki.Append(Fark(Al("kart_halk"), destek));
            foreach (Paydas p in System.Enum.GetValues(typeof(Paydas)))
                if (paydas.Al(p) != 0) etki.Append(Fark(Al(PaydasId(p)), paydas.Al(p)));
        }

        private static string Fark(string ad, int sayi) => $"<color={(sayi > 0 ? ArtiRengi : EksiRengi)}>{ad} {Isaretli(sayi)}</color>  ";
        private static string PaydasId(Paydas p) => $"paydas_{p.ToString().ToLowerInvariant()}";
        private static string Isaretli(int sayi) => sayi > 0 ? $"+{sayi}" : sayi.ToString();

        private void Dugme(Transform ebeveyn, string yazi, bool acik, bool secili, UnityEngine.Events.UnityAction tikla, bool kart = false)
        {
            Button dugme = Instantiate(dugmeKalibi, ebeveyn);
            dugme.gameObject.SetActive(true);
            Text metin = dugme.GetComponentInChildren<Text>();
            metin.text = yazi;
            if (kart)
            {
                // Kalıbın yazı boyu üst sınır; sığmazsa küçülür. Başlıklar yan yana kartlarda aynı hizada dursun diye yazı üstten başlar.
                metin.resizeTextMaxSize = metin.fontSize;
                metin.resizeTextMinSize = Mathf.Min(10, metin.fontSize);
                metin.resizeTextForBestFit = true;
                metin.alignment = TextAnchor.UpperCenter;
            }
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
            aDugmesi.GetComponentInChildren<Text>().text = olay.Uyari ? Al("olay_tamam") : SecenekYazisi(olay, true);
            aDugmesi.interactable = tur.SecenekAcik(true);
            bDugmesi.gameObject.SetActive(!olay.Uyari);
            bDugmesi.GetComponentInChildren<Text>().text = SecenekYazisi(olay, false);
            bDugmesi.interactable = tur.SecenekAcik(false);
        }

        /// <summary>Seçenek ve altında kaynak etkisi (kartlardaki gibi): bütçe, halk, paydaşlar, gelecek turun gönüllüsü. Kumsala etkisi seçenek metninde anlatılır.</summary>
        private string SecenekYazisi(OlayTanimi olay, bool a)
        {
            var etki = new StringBuilder();
            if (olay.Butce(a) != 0) etki.Append(Fark(Al("yonetim_butce"), olay.Butce(a)));
            HalkVePaydas(etki, olay.Destek(a), olay.Paydaslar(a));
            if (olay.Gonullu(a) != 0) etki.Append(Fark(Al("yonetim_gonullu"), olay.Gonullu(a)));
            string ad = Al(olay.Id + (a ? "_a" : "_b"));
            return etki.Length > 0 ? $"{ad}\n<b>{etki.ToString().TrimEnd()}</b>" : ad;
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
                float risk = tur.AvciRiski(i);
                if (risk > 0f) sb.Append($" · {Al("yuva_avci_riski")} %{Mathf.RoundToInt(risk * 100f)}");
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
            if (tur.Yonetim.Korunuyor(i)) sb.AppendLine(Al("bolge_korunuyor"));
            foreach (BolgeKatmani katman in System.Enum.GetValues(typeof(BolgeKatmani)))
                sb.AppendLine($"{Al($"katman_{katman.ToString().ToLowerInvariant()}")}: {b.Seviye(katman)}");
            foreach (BolgeYapisi yapi in System.Enum.GetValues(typeof(BolgeYapisi)))
                if (b.Var(yapi)) sb.AppendLine(Al($"yapi_{yapi.ToString().ToLowerInvariant()}"));
            bolgeBilgisi.text = sb.ToString();
        }
    }
}
