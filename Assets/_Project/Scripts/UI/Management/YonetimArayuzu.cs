using System.Collections.Generic;
using System.Globalization;
using System.Text;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Management;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

        private YonetimDurumu yonetim;
        private KumsalDurumu kumsal;

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
        private readonly Dictionary<string, Sprite> kartGorselleri = new Dictionary<string, Sprite>();
        private GameObject kartBilgiPaneli;
        private Text kartBilgiYazisi;
        private Text kartBilgiTuru, kartBilgiAdi, kartBilgiEk;
        private Text haftaMetni, butceMetni, gonulluMetni, destekMetni, iliskiMetni;
        private Text haftaEtiketi, butceEtiketi, gonulluEtiketi, destekEtiketi;
        private GameObject hedefSeridi;
        private Text solBaslik;
        private RectTransform yuvaGorunumu, yuvaIcerigi, bolgeKutusu;
        private readonly List<GameObject> yuvaKartlari = new List<GameObject>();
        private readonly List<Text> asamaSeritleri = new List<Text>();
        private Button hedefVazgec;
        private Sprite panelSprite;
        private Texture2D panelDokusu;
        private int sonBolge = -2;

        private MetinTablosu Tablo => metinler != null ? metinler : MetinTablosu.Yuklu;
        private string Al(string id) => Tablo != null ? Tablo.Al(id) : $"[{id}]";
        private string Etiket(string id)
        {
            string metin = Al(id);
            return string.IsNullOrEmpty(metin) ? metin : char.ToUpper(metin[0], CultureInfo.GetCultureInfo("tr-TR")) + metin.Substring(1);
        }

        private void OnEnable()
        {
            GorunumuKur();
            // Yönetim ve kumsal durumu asset'tir, sahneler arasında yaşar: kapanırken çıkabilmek için saklanır.
            yonetim = tur.Yonetim;
            kumsal = tur.Kumsal;
            tur.Degisti += Yenile;
            yonetim.Degisti += Yenile;
            kumsal.Degisti += Yenile;
            GameInput.BaglamalarDegisti += Yenile;
            ileriDugmesi.onClick.AddListener(tur.Ilerle);
            DugmeSesi.Bagla(aDugmesi, bDugmesi);
            aDugmesi.onClick.AddListener(() => tur.Sec(true));
            bDugmesi.onClick.AddListener(() => tur.Sec(false));
            Yenile();
        }

        private void OnDisable()
        {
            GameInput.BaglamalarDegisti -= Yenile;
            // Sahne kapanırken tur bizden önce silinmiş olabilir; asset'lerden yine de çıkılır, yoksa silinmiş ekran güncellenmeye çalışır.
            if (yonetim != null) yonetim.Degisti -= Yenile;
            if (kumsal != null) kumsal.Degisti -= Yenile;
            if (tur != null) tur.Degisti -= Yenile;
            if (ileriDugmesi != null) ileriDugmesi.onClick.RemoveAllListeners();
            if (aDugmesi != null) aDugmesi.onClick.RemoveAllListeners();
            if (bDugmesi != null) bDugmesi.onClick.RemoveAllListeners();
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
            // Sahne kapanırken gelen son bildirim: ekran ya da tur silinmişse dokunma.
            if (this == null || kok == null || tur == null) return;
            bool acik = tur.SuAnki != InsanTuru.Asama.Kapali && tur.SuAnki != InsanTuru.Asama.Bitiyor;
            kok.SetActive(acik);
            if (!acik) return;
            if (kartBilgiPaneli != null) kartBilgiPaneli.SetActive(false);

            YonetimDurumu y = tur.Yonetim;
            KumsalDurumu k = tur.Kumsal;
            haftaEtiketi.text = Al("yonetim_tur");
            haftaMetni.text = $"<b>{y.Tur}</b>";
            butceEtiketi.text = Al("yonetim_butce");
            butceMetni.text = y.Bakim > 0 ? $"<b>{y.Butce}</b> (−{y.Bakim})" : $"<b>{y.Butce}</b>";
            gonulluEtiketi.text = Al("yonetim_gonullu");
            gonulluMetni.text = $"<b>{y.BosGonullu(k)}/{y.Gonullu(k)}</b>";
            destekEtiketi.text = Al("yonetim_destek");
            destekMetni.text = $"<b>{y.Destek}/{y.Ayarlar.DestekEnFazla}</b>";
            var ust = new StringBuilder();
            foreach (Paydas p in System.Enum.GetValues(typeof(Paydas)))
                ust.Append($"{Al(PaydasId(p))} {Isaretli(y.Iliski(p))}  ");
            if (y.UyariVar) ust.Append($"\n<color=#b45121>{Al($"uyari_{y.AktifUyari.ToString().ToLowerInvariant()}")}</color>");
            iliskiMetni.text = ust.ToString();
            asamaBasligi.text = Al($"asama_{tur.SuAnki.ToString().ToLowerInvariant()}");
            var serit = new StringBuilder();
            foreach (InsanTuru.Asama asama in new[] { InsanTuru.Asama.Rapor, InsanTuru.Asama.Gonulluler, InsanTuru.Asama.Kartlar, InsanTuru.Asama.Olay })
            {
                string ad = Al($"asama_{asama.ToString().ToLowerInvariant()}");
                serit.Append(asama == tur.SuAnki ? $"<color=#246F7D><b>{ad}</b></color>" : $"<color=#71827A>{ad}</color>");
                serit.Append("    ");
            }
            foreach (Text t in asamaSeritleri) t.text = serit.ToString();

            raporPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Rapor);
            bool hedefSeciliyor = tur.BeklenenHedef != Hedef.Yok;
            gorevPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Gonulluler && !hedefSeciliyor);
            kartPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Kartlar && !hedefSeciliyor);
            olayPaneli.SetActive(tur.SuAnki == InsanTuru.Asama.Olay);
            ileriDugmesi.gameObject.SetActive(tur.SuAnki != InsanTuru.Asama.Olay && !hedefSeciliyor);
            if (hedefSeridi != null) hedefSeridi.SetActive(hedefSeciliyor);
            IleriYazisi();

            Rapor();
            if (!hedefSeciliyor) Dugmeler();
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
            ileriDugmesi.GetComponentInChildren<Text>(true).text = Al("yonetim_ileri") + tus;
        }

        /// <summary>Sol panel boşken (yuva yok, imleç haritada değil) haritayı kapatmasın. Boyu içeriğe göre, üst çubukla alttaki panelin arasına sığacak kadar.</summary>
        private void SolPanel()
        {
            bool yuvaVar = yuvaKartlari.Count > 0;
            bool bolgeVar = bolgeBilgisi.text.Length > 0;
            bool dolu = yuvaVar || bolgeVar;
            yuvaListesi.transform.parent.gameObject.SetActive(dolu);
            if (!dolu) return;
            solBaslik.text = yuvaVar ? Al("yuva") : $"{Al("bolge")} {harita.Ustundeki + 1}";
            int bolgeSatiri = bolgeVar ? bolgeBilgisi.text.TrimEnd().Split('\n').Length : 0;
            float yuvaAlani = yuvaKartlari.Count * 116f;
            float bolgeAlani = bolgeVar ? (bolgeSatiri * 30f + 40f) : 0f;
            float ust = UstBosluk + UstYukseklik + Aralik;
            float bos = ((RectTransform)kok.transform).rect.height - ust - AltPanelUstu() - Aralik;
            float yukseklik = Mathf.Clamp(SolBaslikYuksekligi + SolAltPay + yuvaAlani + bolgeAlani, 160f, Mathf.Max(160f, bos));
            Yerles((RectTransform)yuvaListesi.transform.parent, new Vector2(0f, 1f), new Vector2(Kenar, -ust), new Vector2(SolGenislik, yukseklik));
            Kenarlik((RectTransform)solBaslik.transform, new Vector2(0f, 1f), Vector2.one, 22f, -SolBaslikYuksekligi + 6f, 22f, 8f);
            float govde = yukseklik - SolBaslikYuksekligi - SolAltPay;
            if (yuvaVar && bolgeVar)
            {
                float bolgeBoyu = govde * bolgeAlani / (yuvaAlani + bolgeAlani);
                Kenarlik(yuvaGorunumu, Vector2.zero, Vector2.one, 20f, SolAltPay + bolgeBoyu + 5f, 20f, SolBaslikYuksekligi);
                Kenarlik(bolgeKutusu, Vector2.zero, new Vector2(1f, 0f), 20f, SolAltPay, 20f, -(SolAltPay + bolgeBoyu - 5f));
            }
            else
            {
                Kenarlik(yuvaGorunumu, Vector2.zero, Vector2.one, 20f, SolAltPay, 20f, SolBaslikYuksekligi);
                Kenarlik(bolgeKutusu, Vector2.zero, Vector2.one, 20f, SolAltPay, 20f, SolBaslikYuksekligi);
            }
            yuvaGorunumu.gameObject.SetActive(yuvaVar);
            bolgeKutusu.gameObject.SetActive(bolgeVar);
        }

        /// <summary>Alttaki açık panelin (ya da hedef şeridinin) üst kenarı, tuvalin altından.</summary>
        private float AltPanelUstu()
        {
            if (hedefSeridi != null && hedefSeridi.activeSelf) return AltBosluk + HedefYuksekligi;
            if (kartPaneli.activeSelf) return AltBosluk + KartPaneliYuksekligi;
            return AltBosluk + AltPaneliYuksekligi;
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
                    GorevDugmesi(g);
                }

            if (tur.SuAnki == InsanTuru.Asama.Kartlar)
                foreach (KartTanimi kart in tur.El)
                {
                    KartTanimi k = kart;
                    KartDugmesi(k);
                }
        }

        // Mevcut sahnedeki bağlantıları koruyarak yönetim arayüzünü kart serisinin paletine uyarlar.
        private void GorunumuKur()
        {
            if (kartBilgiPaneli != null || kok == null || dugmeKalibi == null) return;
            Color kagit = new Color32(250, 247, 240, 245);
            Color yazi = new Color32(24, 56, 70, 255);
            Color turkuaz = new Color32(36, 111, 125, 255);
            panelDokusu = YuvarlakDoku();
            panelSprite = Sprite.Create(panelDokusu, new Rect(0, 0, 32, 32), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(10, 10, 10, 10));
            foreach (Text t in new[] { ustBilgi, asamaBasligi, yuvaListesi, bolgeBilgisi, raporYazisi, olayYazisi })
                if (t != null) t.color = yazi;
            foreach (GameObject panel in new[] { ustBilgi.transform.parent.gameObject, yuvaListesi.transform.parent.gameObject, raporPaneli, gorevPaneli, kartPaneli, olayPaneli })
                if (panel != null && panel.TryGetComponent(out Image zemin)) Yuvarlak(zemin, kagit);

            // Yerleşim tuval biriminde (1920x1080'e göre): yazı boyutu ayarı tuvali büyütünce paneller de yazıyla birlikte büyür.
            // En büyük ayarda tuval 1280x720'ye iner; üst çubuk, sol panel ve alt panel buna da sığacak boyda.
            Kenarlik((RectTransform)ustBilgi.transform.parent, new Vector2(0f, 1f), Vector2.one, Kenar, -(UstBosluk + UstYukseklik), Kenar, UstBosluk);
            Transform ustPanel = ustBilgi.transform.parent;
            ustBilgi.gameObject.SetActive(false);
            haftaMetni = KaynakHucresi(ustPanel, .01f, .125f, out haftaEtiketi);
            butceMetni = KaynakHucresi(ustPanel, .13f, .245f, out butceEtiketi);
            gonulluMetni = KaynakHucresi(ustPanel, .25f, .365f, out gonulluEtiketi);
            destekMetni = KaynakHucresi(ustPanel, .37f, .50f, out destekEtiketi);
            BilgiZemini(ustPanel, .51f, .845f);
            BilgiZemini(ustPanel, .85f, .99f, new Color32(224, 237, 226, 255));
            iliskiMetni = KartMetni("Iliskiler", ustPanel, new Vector2(.52f, .12f), new Vector2(.835f, .88f), 23, TextAnchor.MiddleLeft);
            Konum((RectTransform)asamaBasligi.transform, .855f, .12f, .985f, .88f);
            asamaBasligi.color = turkuaz; asamaBasligi.fontStyle = FontStyle.Bold; ArayuzOlcegi.Boyut(asamaBasligi, 25);
            asamaBasligi.alignment = TextAnchor.MiddleCenter;
            foreach (GameObject panel in new[] { raporPaneli, gorevPaneli, olayPaneli })
                AltSerit((RectTransform)panel.transform, AltPaneliYuksekligi);
            AltSerit((RectTransform)kartPaneli.transform, KartPaneliYuksekligi);
            PanelBasligi(raporPaneli, "asama_rapor");
            PanelBasligi(gorevPaneli, "asama_gonulluler");
            PanelBasligi(kartPaneli, "asama_kartlar");
            PanelBasligi(olayPaneli, "asama_olay");
            Kenarlik((RectTransform)gorevDugmeleri, Vector2.zero, Vector2.one, 16f, 14f, 16f, PanelBasligiYuksekligi);
            Kenarlik((RectTransform)kartDugmeleri, Vector2.zero, Vector2.one, 16f, 14f, 16f, PanelBasligiYuksekligi);
            Kenarlik((RectTransform)raporYazisi.transform, Vector2.zero, Vector2.one, 32f, 16f, 32f, PanelBasligiYuksekligi + 4f);
            Kenarlik((RectTransform)olayYazisi.transform, Vector2.zero, Vector2.one, 32f, 136f, 32f, PanelBasligiYuksekligi + 4f);
            Kenarlik((RectTransform)aDugmesi.transform, Vector2.zero, new Vector2(.5f, 0f), 24f, 16f, 8f, -126f);
            Kenarlik((RectTransform)bDugmesi.transform, new Vector2(.5f, 0f), new Vector2(1f, 0f), 8f, 16f, 24f, -126f);
            Yerles((RectTransform)ileriDugmesi.transform, new Vector2(1f, 0f), new Vector2(-Kenar, AltBosluk), new Vector2(IleriGenisligi, IleriYuksekligi));
            Yuvarlak(ileriDugmesi.GetComponent<Image>(), turkuaz);
            ileriDugmesi.GetComponentInChildren<Text>().color = Color.white;
            foreach (Button secenek in new[] { aDugmesi, bDugmesi })
                Yuvarlak(secenek.GetComponent<Image>(), new Color32(241, 237, 228, 255));
            Transform sol = yuvaListesi.transform.parent;
            solBaslik = KartMetni("SolBaslik", sol, new Vector2(.06f, .86f), new Vector2(.94f, .98f), 27, TextAnchor.MiddleLeft);
            solBaslik.fontStyle = FontStyle.Bold; solBaslik.color = turkuaz;
            yuvaListesi.gameObject.SetActive(false);
            YuvaPaneliKur(sol);
            ArayuzOlcegi.Boyut(bolgeBilgisi, 24);
            ArayuzOlcegi.Boyut(raporYazisi, 26); ArayuzOlcegi.Boyut(olayYazisi, 26);

            // Hedef seçerken alt panel gizlenir; yerine istem ve Geri düğmesi alttaki şeritte.
            hedefSeridi = new GameObject("HedefSeridi", typeof(RectTransform), typeof(Image));
            hedefSeridi.layer = LayerMask.NameToLayer("UI");
            hedefSeridi.transform.SetParent(kok.transform, false);
            Yerles((RectTransform)hedefSeridi.transform, new Vector2(.5f, 0f), new Vector2(0f, AltBosluk), new Vector2(HedefGenisligi, HedefYuksekligi));
            Yuvarlak(hedefSeridi.GetComponent<Image>(), kagit);
            hedefYazisi.color = yazi;
            hedefYazisi.transform.SetParent(hedefSeridi.transform, false);
            Kenarlik((RectTransform)hedefYazisi.transform, Vector2.zero, Vector2.one, 24f, 8f, 230f, 8f);
            ArayuzOlcegi.Boyut(hedefYazisi, 28);
            hedefSeridi.SetActive(false);

            hedefVazgec = Instantiate(dugmeKalibi, hedefSeridi.transform);
            hedefVazgec.name = "HedefVazgec";
            hedefVazgec.gameObject.SetActive(true);
            Kenarlik((RectTransform)hedefVazgec.transform, new Vector2(1f, 0f), Vector2.one, -210f, 10f, 12f, 10f);
            Yuvarlak(hedefVazgec.GetComponent<Image>(), new Color32(241, 237, 228, 255));
            hedefVazgec.GetComponentInChildren<Text>(true).text = Al("menu_geri");
            DugmeSesi.Bagla(hedefVazgec);
            hedefVazgec.onClick.AddListener(() =>
            {
                if (tur.BekleyenKart != null) tur.KartSec(tur.BekleyenKart);
                else if (tur.BekleyenGorev != null) tur.GorevSec(tur.BekleyenGorev);
            });

            kartBilgiPaneli = new GameObject("KartBilgiPaneli", typeof(RectTransform), typeof(Image));
            kartBilgiPaneli.layer = LayerMask.NameToLayer("UI");
            kartBilgiPaneli.transform.SetParent(kok.transform, false);
            var arka = kartBilgiPaneli.GetComponent<Image>(); Yuvarlak(arka, kagit); arka.raycastTarget = false;
            kartBilgiTuru = KartMetni("Tur", kartBilgiPaneli.transform, new Vector2(.07f, .80f), new Vector2(.93f, .94f), 20, TextAnchor.MiddleLeft);
            kartBilgiTuru.color = turkuaz; kartBilgiTuru.fontStyle = FontStyle.Bold;
            kartBilgiAdi = KartMetni("Ad", kartBilgiPaneli.transform, new Vector2(.07f, .62f), new Vector2(.93f, .80f), 29, TextAnchor.MiddleLeft);
            kartBilgiAdi.fontStyle = FontStyle.Bold;
            kartBilgiYazisi = KartMetni("Bilgi", kartBilgiPaneli.transform, new Vector2(.07f, .27f), new Vector2(.93f, .60f), 24, TextAnchor.UpperLeft);
            kartBilgiEk = KartMetni("Etki", kartBilgiPaneli.transform, new Vector2(.07f, .05f), new Vector2(.93f, .25f), 21, TextAnchor.UpperLeft);
            kartBilgiPaneli.SetActive(false);
        }

        private void PanelBasligi(GameObject panel, string id)
        {
            Text baslik = KartMetni("Baslik", panel.transform, new Vector2(0f, 1f), new Vector2(.4f, 1f), 27, TextAnchor.MiddleLeft);
            Kenarlik((RectTransform)baslik.transform, new Vector2(0f, 1f), new Vector2(.4f, 1f), 24f, -PanelBasligiYuksekligi + 4f, 0f, 6f);
            baslik.fontStyle = FontStyle.Bold; baslik.text = Al(id);
            Text asamalar = KartMetni("Asamalar", panel.transform, new Vector2(.4f, 1f), Vector2.one, 20, TextAnchor.MiddleRight);
            Kenarlik((RectTransform)asamalar.transform, new Vector2(.4f, 1f), Vector2.one, 12f, -PanelBasligiYuksekligi + 4f, 24f, 6f);
            asamaSeritleri.Add(asamalar);
        }

        private void BilgiZemini(Transform ebeveyn, float sol, float sag, Color32? renk = null)
        {
            var nesne = new GameObject("KaynakZemini", typeof(RectTransform), typeof(Image));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            nesne.transform.SetAsFirstSibling();
            Konum((RectTransform)nesne.transform, sol, .1f, sag, .9f);
            var resim = nesne.GetComponent<Image>();
            Yuvarlak(resim, renk ?? new Color32(239, 235, 224, 255));
            resim.raycastTarget = false;
        }

        /// <summary>Üst çubukta bir kaynak: zemin, üstte küçük adı, altta değeri.</summary>
        private Text KaynakHucresi(Transform ust, float sol, float sag, out Text etiket)
        {
            BilgiZemini(ust, sol, sag);
            etiket = KartMetni("Etiket", ust, new Vector2(sol + .007f, .5f), new Vector2(sag - .004f, .86f), 21, TextAnchor.LowerLeft);
            etiket.color = new Color32(62, 92, 89, 255);
            return KartMetni("Deger", ust, new Vector2(sol + .007f, .14f), new Vector2(sag - .004f, .52f), 28, TextAnchor.UpperLeft);
        }

        private void YuvaPaneliKur(Transform sol)
        {
            var viewport = new GameObject("YuvaListesiGorunumu", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            viewport.layer = LayerMask.NameToLayer("UI"); viewport.transform.SetParent(sol, false);
            yuvaGorunumu = (RectTransform)viewport.transform;
            viewport.GetComponent<Image>().color = Color.clear;

            var icerik = new GameObject("YuvaKartlari", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            icerik.layer = LayerMask.NameToLayer("UI"); icerik.transform.SetParent(viewport.transform, false);
            yuvaIcerigi = (RectTransform)icerik.transform;
            yuvaIcerigi.anchorMin = new Vector2(0, 1); yuvaIcerigi.anchorMax = new Vector2(1, 1);
            yuvaIcerigi.pivot = new Vector2(.5f, 1); yuvaIcerigi.anchoredPosition = Vector2.zero;
            yuvaIcerigi.sizeDelta = Vector2.zero;
            var siralama = icerik.GetComponent<VerticalLayoutGroup>();
            siralama.spacing = 8; siralama.childControlWidth = true; siralama.childControlHeight = true;
            siralama.childForceExpandWidth = true; siralama.childForceExpandHeight = false;
            icerik.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var kaydir = viewport.GetComponent<ScrollRect>();
            kaydir.viewport = yuvaGorunumu; kaydir.content = yuvaIcerigi;
            kaydir.horizontal = false; kaydir.vertical = true; kaydir.scrollSensitivity = 40;
            kaydir.movementType = ScrollRect.MovementType.Clamped;

            var bolge = new GameObject("BolgeKutusu", typeof(RectTransform), typeof(Image));
            bolge.layer = LayerMask.NameToLayer("UI"); bolge.transform.SetParent(sol, false);
            bolgeKutusu = (RectTransform)bolge.transform;
            var zemin = bolge.GetComponent<Image>(); Yuvarlak(zemin, new Color32(241, 237, 227, 255)); zemin.raycastTarget = false;
            bolgeBilgisi.transform.SetParent(bolge.transform, false);
            Kenarlik((RectTransform)bolgeBilgisi.transform, Vector2.zero, Vector2.one, 14f, 8f, 14f, 8f);
        }

        private static void Konum(RectTransform r, float sol, float alt, float sag, float ust)
        {
            r.anchorMin = new Vector2(sol, alt); r.anchorMax = new Vector2(sag, ust);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        // Yerleşim ölçüleri (tuval birimi; 1920x1080 referans, en büyük yazı boyutunda tuval 1280x720).
        private const float Kenar = 16f, Aralik = 12f;
        private const float UstBosluk = 10f, UstYukseklik = 92f;
        private const float AltBosluk = 14f, AltPaneliYuksekligi = 270f, KartPaneliYuksekligi = 360f, PanelBasligiYuksekligi = 58f;
        private const float IleriGenisligi = 210f, IleriYuksekligi = 76f;
        private const float SolGenislik = 400f, SolBaslikYuksekligi = 60f, SolAltPay = 14f;
        private const float HedefGenisligi = 860f, HedefYuksekligi = 80f;
        // Sol panel ve kart bilgisi ortadaki ipucu kutusuna (IpucuPaneli, iki yanda 440 birim boş bırakır) değmeyecek enlikte.
        private const float BilgiGenisligi = 400f, BilgiYuksekligi = 420f;

        /// <summary>Tek noktaya çapalı, sabit boyutlu kutu (pivot çapayla aynı köşede).</summary>
        private static void Yerles(RectTransform r, Vector2 capa, Vector2 konum, Vector2 boyut)
        {
            r.anchorMin = r.anchorMax = r.pivot = capa;
            r.anchoredPosition = konum; r.sizeDelta = boyut;
        }

        /// <summary>İki çapa arasında, kenarlardan içe boşluklu kutu (sag ve ust içe doğru pozitif).</summary>
        private static void Kenarlik(RectTransform r, Vector2 capaAlt, Vector2 capaUst, float sol, float alt, float sag, float ust)
        {
            r.anchorMin = capaAlt; r.anchorMax = capaUst; r.pivot = new Vector2(.5f, .5f);
            r.offsetMin = new Vector2(sol, alt); r.offsetMax = new Vector2(-sag, -ust);
        }

        /// <summary>Alttaki panel: ekran boyunca, sağda İleri düğmesine yer bırakarak.</summary>
        private static void AltSerit(RectTransform r, float yukseklik) =>
            Kenarlik(r, Vector2.zero, new Vector2(1f, 0f), Kenar, AltBosluk, Kenar + IleriGenisligi + Aralik, -(AltBosluk + yukseklik));

        private static Texture2D YuvarlakDoku()
        {
            var doku = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            doku.name = "YonetimPanelSekli"; doku.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                float dx = Mathf.Max(0, Mathf.Abs(x - 15.5f) - 6f);
                float dy = Mathf.Max(0, Mathf.Abs(y - 15.5f) - 6f);
                float alpha = Mathf.Clamp01(10f - Mathf.Sqrt(dx * dx + dy * dy));
                doku.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
            doku.Apply(); return doku;
        }

        private void Yuvarlak(Image resim, Color renk)
        {
            resim.sprite = panelSprite; resim.type = Image.Type.Sliced; resim.color = renk;
        }

        private Text KartMetni(string ad, Transform ebeveyn, Vector2 alt, Vector2 ust, int boyut, TextAnchor hiza)
        {
            var nesne = new GameObject(ad, typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            var r = (RectTransform)nesne.transform; r.anchorMin = alt; r.anchorMax = ust;
            r.offsetMin = r.offsetMax = Vector2.zero;
            var t = nesne.GetComponent<Text>();
            t.font = dugmeKalibi.GetComponentInChildren<Text>(true).font; ArayuzOlcegi.Boyut(t, boyut);
            t.color = new Color32(24, 56, 70, 255); t.alignment = hiza; t.raycastTarget = false;
            t.supportRichText = true;
            return t;
        }

        private Sprite KartGorseli(string id)
        {
            if (kartGorselleri.TryGetValue(id, out Sprite sprite)) return sprite;
            Texture2D doku = Resources.Load<Texture2D>($"KartDioramalari/{id}");
            if (doku == null) return null;
            sprite = Sprite.Create(doku, new Rect(0, 0, doku.width, doku.height), new Vector2(.5f, .5f));
            kartGorselleri.Add(id, sprite);
            return sprite;
        }

        private static string GorevGorseli(GorevEtkisi etki)
        {
            switch (etki)
            {
                case GorevEtkisi.Temizlik: return "kart_temizlik";
                case GorevEtkisi.Devriye: return "kart_gozcu_kulubesi";
                case GorevEtkisi.Kafes: return "kart_kulucka";
                case GorevEtkisi.GolgeSulama: return "kart_golgelik";
                case GorevEtkisi.Bilgilendirme: return "kart_plaj_tabela";
                default: return null;
            }
        }

        private void GorevDugmesi(GorevTanimi gorev)
        {
            Button dugme = Instantiate(dugmeKalibi, gorevDugmeleri);
            dugme.gameObject.SetActive(true);
            dugme.GetComponentInChildren<Text>().gameObject.SetActive(false);
            Yuvarlak(dugme.GetComponent<Image>(), new Color32(241, 237, 227, 255));
            dugme.interactable = tur.GorevVerilebilir(gorev);

            var nesne = new GameObject("GorevGorseli", typeof(RectTransform), typeof(Image));
            nesne.layer = LayerMask.NameToLayer("UI"); nesne.transform.SetParent(dugme.transform, false);
            Konum((RectTransform)nesne.transform, .02f, .23f, .30f, .77f);
            var resim = nesne.GetComponent<Image>(); resim.raycastTarget = false; resim.preserveAspect = true;
            string id = GorevGorseli(gorev.Etki);
            resim.sprite = id != null ? KartGorseli(id) : null; resim.enabled = resim.sprite != null;

            Text baslik = KartMetni("GorevAdi", dugme.transform, new Vector2(.30f, .67f), new Vector2(.97f, .96f), 26, TextAnchor.LowerLeft);
            baslik.fontStyle = FontStyle.Bold; baslik.text = Al(gorev.Id);
            Text aciklama = KartMetni("GorevAciklama", dugme.transform, new Vector2(.30f, .06f), new Vector2(.97f, .66f), 24, TextAnchor.UpperLeft);
            aciklama.text = Al(gorev.Id + "_aciklama");
            DugmeSesi.Bagla(dugme);
            dugme.onClick.AddListener(() => tur.GorevSec(gorev));
            dugmeler.Add(dugme);
        }

        private void KartDugmesi(KartTanimi kart)
        {
            Button dugme = Instantiate(dugmeKalibi, kartDugmeleri);
            dugme.gameObject.SetActive(true);
            var zemin = dugme.GetComponent<Image>();
            Yuvarlak(zemin, tur.BekleyenKart == kart ? new Color32(222, 235, 207, 255) : new Color32(250, 247, 240, 255));
            bool oynanabilir = tur.KartOynanabilir(kart);
            dugme.interactable = oynanabilir;
            Text eski = dugme.GetComponentInChildren<Text>(); eski.gameObject.SetActive(false);

            var ust = new GameObject("Tur", typeof(RectTransform), typeof(Image));
            ust.layer = LayerMask.NameToLayer("UI"); ust.transform.SetParent(dugme.transform, false);
            var ur = (RectTransform)ust.transform; ur.anchorMin = new Vector2(0, .86f); ur.anchorMax = Vector2.one; ur.offsetMin = ur.offsetMax = Vector2.zero;
            var bant = ust.GetComponent<Image>(); bant.color = new Color32(36, 111, 125, 255); bant.raycastTarget = false;
            Text turYazisi = KartMetni("TurYazisi", ust.transform, new Vector2(.05f, 0), new Vector2(.95f, 1), 21, TextAnchor.MiddleLeft);
            turYazisi.color = Color.white;
            turYazisi.text = Al($"kart_turu_{kart.Tur.ToString().ToLowerInvariant()}");

            var gorsel = new GameObject("Diorama", typeof(RectTransform), typeof(Image));
            gorsel.layer = LayerMask.NameToLayer("UI"); gorsel.transform.SetParent(dugme.transform, false);
            var gr = (RectTransform)gorsel.transform; gr.anchorMin = new Vector2(.02f, .33f); gr.anchorMax = new Vector2(.98f, .86f); gr.offsetMin = gr.offsetMax = Vector2.zero;
            var resim = gorsel.GetComponent<Image>(); resim.raycastTarget = false; resim.preserveAspect = true;
            Sprite sprite = KartGorseli(kart.Id);
            resim.sprite = sprite; resim.enabled = sprite != null;
            if (!oynanabilir) resim.color = new Color(1, 1, 1, .5f);

            Text ad = KartMetni("Ad", dugme.transform, new Vector2(.06f, .17f), new Vector2(.94f, .33f), 24, TextAnchor.MiddleLeft);
            ad.fontStyle = FontStyle.Bold; ad.text = Al(kart.Id);
            Text maliyet = KartMetni("Maliyet", dugme.transform, new Vector2(.06f, .01f), new Vector2(.94f, .17f), 24, TextAnchor.MiddleLeft);
            maliyet.text = $"{Al("yonetim_butce")} {tur.Yonetim.Bedel(kart)}  ·  {Al("kart_bakim")} {kart.Bakim}";
            dugme.onClick.AddListener(() => tur.KartSec(kart));
            var olay = dugme.gameObject.AddComponent<EventTrigger>();
            void Bagla(EventTriggerType tip, UnityEngine.Events.UnityAction<BaseEventData> islem)
            {
                var girdi = new EventTrigger.Entry { eventID = tip }; girdi.callback.AddListener(islem); olay.triggers.Add(girdi);
            }
            Bagla(EventTriggerType.PointerEnter, _ => KartBilgisiGoster(kart));
            Bagla(EventTriggerType.Select, _ => KartBilgisiGoster(kart));
            Bagla(EventTriggerType.PointerExit, _ => kartBilgiPaneli.SetActive(false));
            Bagla(EventTriggerType.Deselect, _ => kartBilgiPaneli.SetActive(false));
            dugmeler.Add(dugme);
        }

        private void KartBilgisiGoster(KartTanimi kart)
        {
            if (kartBilgiPaneli == null) return;
            kartBilgiTuru.text = Al($"kart_turu_{kart.Tur.ToString().ToLowerInvariant()}");
            kartBilgiTuru.color = kart.Tur == KartTuru.Acil ? new Color32(192, 86, 27, 255) : new Color32(36, 111, 125, 255);
            kartBilgiAdi.text = Al(kart.Id);
            kartBilgiYazisi.text = Al(kart.Id + "_aciklama");
            var ek = new StringBuilder($"{Al("yonetim_butce")} {tur.Yonetim.Bedel(kart)}  ·  {Al("kart_bakim")} {kart.Bakim}");
            var etkiler = new StringBuilder();
            HalkVePaydas(etkiler, kart.DestekFarki, kart.PaydasFarki);
            if (kart.KosulVar)
            {
                string renk = tur.Yonetim.KosulTamam(kart) ? TurRengi : EksiRengi;
                etkiler.Append($"<color={renk}>{Al(PaydasId(kart.KosulPaydas))} {Isaretli(kart.KosulEnAz)} {Al("kart_ister")}</color>");
            }
            if (tur.Yonetim.Guclu(kart)) etkiler.Append($"<color={ArtiRengi}>{Al("kart_guclu")}</color>");
            if (etkiler.Length > 0) ek.Append("\n").Append(etkiler.ToString().TrimEnd());
            kartBilgiEk.text = ek.ToString();
            // Kart panelinin üstünde, sağda; üst çubuğa değmeyecek boyda.
            float alt = AltBosluk + KartPaneliYuksekligi + Aralik, ust = UstBosluk + UstYukseklik + Aralik;
            float bos = ((RectTransform)kok.transform).rect.height - alt - ust;
            Yerles((RectTransform)kartBilgiPaneli.transform, new Vector2(1f, 0f), new Vector2(-Kenar, alt), new Vector2(BilgiGenisligi, Mathf.Min(BilgiYuksekligi, bos)));
            kartBilgiPaneli.SetActive(true);
        }

        private void OnDestroy()
        {
            foreach (Sprite sprite in kartGorselleri.Values) if (sprite != null) Destroy(sprite);
            kartGorselleri.Clear();
            if (panelSprite != null) Destroy(panelSprite);
            if (panelDokusu != null) Destroy(panelDokusu);
        }

        // Açık zeminli düğmede okunan renkler.
        private const string TurRengi = "#444444", ArtiRengi = "#2e7d32", EksiRengi = "#b03030";

        private void HalkVePaydas(StringBuilder etki, int destek, PaydasFarki paydas)
        {
            if (destek != 0) etki.Append(Fark(Al("kart_halk"), destek));
            foreach (Paydas p in System.Enum.GetValues(typeof(Paydas)))
                if (paydas.Al(p) != 0) etki.Append(Fark(Al(PaydasId(p)), paydas.Al(p)));
        }

        private static string Fark(string ad, int sayi) => $"<color={(sayi > 0 ? ArtiRengi : EksiRengi)}>{ad} {Isaretli(sayi)}</color>  ";
        private static string PaydasId(Paydas p) => $"paydas_{p.ToString().ToLowerInvariant()}";
        private static string Isaretli(int sayi) => sayi > 0 ? $"+{sayi}" : sayi.ToString();

        private void Olay()
        {
            OlayTanimi olay = tur.Olay;
            if (olay == null) return;
            olayYazisi.text = Al(olay.Id);
            aDugmesi.GetComponentInChildren<Text>(true).text = olay.Uyari ? Al("olay_tamam") : SecenekYazisi(olay, true);
            aDugmesi.interactable = tur.SecenekAcik(true);
            bDugmesi.gameObject.SetActive(!olay.Uyari);
            bDugmesi.GetComponentInChildren<Text>(true).text = SecenekYazisi(olay, false);
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
            foreach (GameObject kart in yuvaKartlari)
                if (kart != null) { kart.SetActive(false); Destroy(kart); }
            yuvaKartlari.Clear();
            IReadOnlyList<Yuva> yuvalar = tur.Kumsal.Yuvalar;
            for (int i = 0; i < yuvalar.Count; i++)
            {
                Yuva y = yuvalar[i];
                var kart = new GameObject("YuvaKarti", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                kart.layer = LayerMask.NameToLayer("UI"); kart.transform.SetParent(yuvaIcerigi, false);
                kart.GetComponent<LayoutElement>().preferredHeight = 108;
                var zemin = kart.GetComponent<Image>(); Yuvarlak(zemin, new Color32(241, 237, 227, 255)); zemin.raycastTarget = false;
                Text ad = KartMetni("YuvaAdi", kart.transform, new Vector2(.06f, .62f), new Vector2(.52f, .96f), 24, TextAnchor.MiddleLeft);
                ad.fontStyle = FontStyle.Bold; ad.text = $"{Al("yuva")} {i + 1}";

                var rozet = new GameObject("SicaklikRozeti", typeof(RectTransform), typeof(Image));
                rozet.layer = LayerMask.NameToLayer("UI"); rozet.transform.SetParent(kart.transform, false);
                Konum((RectTransform)rozet.transform, .57f, .64f, .94f, .94f);
                var rozetZemini = rozet.GetComponent<Image>(); Yuvarlak(rozetZemini, new Color32(224, 237, 226, 255)); rozetZemini.raycastTarget = false;
                Text bant = KartMetni("Bant", rozet.transform, new Vector2(.03f, .04f), new Vector2(.97f, .96f), 20, TextAnchor.MiddleCenter);
                bant.color = new Color32(45, 105, 91, 255); bant.fontStyle = FontStyle.Bold;
                bant.text = Al($"bant_{tur.Kumsal.YuvaBandi(i).ToString().ToLowerInvariant()}");

                Text ayrinti = KartMetni("Ayrinti", kart.transform, new Vector2(.06f, .06f), new Vector2(.94f, .60f), 21, TextAnchor.UpperLeft);
                var bilgi = new StringBuilder($"{Etiket("yuva_yumurta")} {y.Yumurta}  ·  {Etiket("yuva_hafta")} {y.CikisinaHafta}");
                if (y.Kafesli) bilgi.Append($"\n{Etiket("yuva_kafesli")}");
                float risk = tur.AvciRiski(i);
                if (risk > 0f) bilgi.Append($"\n{Etiket("yuva_avci_riski")} %{Mathf.RoundToInt(risk * 100f)}");
                ayrinti.text = bilgi.ToString();
                yuvaKartlari.Add(kart);
            }
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
            var sb = new StringBuilder();
            if (tur.Kumsal.Yuvalar.Count > 0) sb.AppendLine($"<b>{Al("bolge")} {i + 1}</b>");
            if (tur.Yonetim.Korunuyor(i)) sb.AppendLine(Al("bolge_korunuyor"));
            foreach (BolgeKatmani katman in System.Enum.GetValues(typeof(BolgeKatmani)))
                sb.AppendLine($"{Al($"katman_{katman.ToString().ToLowerInvariant()}")}: {b.Seviye(katman)}");
            foreach (BolgeYapisi yapi in System.Enum.GetValues(typeof(BolgeYapisi)))
                if (b.Var(yapi)) sb.AppendLine(Al($"yapi_{yapi.ToString().ToLowerInvariant()}"));
            bolgeBilgisi.text = sb.ToString();
        }
    }
}
