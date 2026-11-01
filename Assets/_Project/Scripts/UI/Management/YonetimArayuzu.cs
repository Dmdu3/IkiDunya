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
            haftaMetni.text = $"<size=23><color=#3E5C59>{Al("yonetim_tur")}</color></size>\n<b>{y.Tur}</b>";
            butceMetni.text = $"<size=23><color=#3E5C59>{Al("yonetim_butce")}</color></size>\n<b>{y.Butce}</b>";
            if (y.Bakim > 0) butceMetni.text += $" <size=20>(−{y.Bakim})</size>";
            gonulluMetni.text = $"<size=23><color=#3E5C59>{Al("yonetim_gonullu")}</color></size>\n<b>{y.BosGonullu(k)}/{y.Gonullu(k)}</b>";
            destekMetni.text = $"<size=23><color=#3E5C59>{Al("yonetim_destek")}</color></size>\n<b>{y.Destek}/{y.Ayarlar.DestekEnFazla}</b>";
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
            if (hedefVazgec != null) hedefVazgec.gameObject.SetActive(hedefSeciliyor);
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

        /// <summary>Sol panel boşken (yuva yok, imleç haritada değil) haritayı kapatmasın.</summary>
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
            float bolgeAlani = bolgeVar ? (bolgeSatiri * 27f + 24f) : 0f;
            float yukseklik = Mathf.Clamp(.06f + (yuvaAlani + bolgeAlani) / 1080f, .17f, .60f);
            Konum((RectTransform)yuvaListesi.transform.parent, .015f, .875f - yukseklik, .22f, .875f);
            float baslikYuksekligi = Mathf.Clamp(.045f / yukseklik, .10f, .28f);
            float govdeUst = 1f - baslikYuksekligi - .03f;
            Konum((RectTransform)solBaslik.transform, .06f, govdeUst, .94f, .98f);
            float govdeAlt = .06f;
            if (yuvaVar && bolgeVar)
            {
                float ayrim = govdeAlt + (govdeUst - govdeAlt) * bolgeAlani / (yuvaAlani + bolgeAlani);
                Konum(yuvaGorunumu, .05f, ayrim + .025f, .95f, govdeUst);
                Konum(bolgeKutusu, .05f, govdeAlt, .95f, ayrim);
            }
            else
            {
                Konum(yuvaGorunumu, .05f, govdeAlt, .95f, govdeUst);
                Konum(bolgeKutusu, .05f, govdeAlt, .95f, govdeUst);
            }
            yuvaGorunumu.gameObject.SetActive(yuvaVar);
            bolgeKutusu.gameObject.SetActive(bolgeVar);
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

            Konum((RectTransform)ustBilgi.transform.parent, .015f, .89f, .985f, .99f);
            Transform ustPanel = ustBilgi.transform.parent;
            ustBilgi.gameObject.SetActive(false);
            BilgiZemini(ustPanel, .015f, .115f); BilgiZemini(ustPanel, .12f, .22f);
            BilgiZemini(ustPanel, .225f, .34f); BilgiZemini(ustPanel, .345f, .48f);
            BilgiZemini(ustPanel, .49f, .845f);
            BilgiZemini(ustPanel, .85f, .985f, new Color32(224, 237, 226, 255));
            haftaMetni = KartMetni("Hafta", ustPanel, new Vector2(.025f, .08f), new Vector2(.11f, .92f), 30, TextAnchor.MiddleLeft);
            butceMetni = KartMetni("Butce", ustPanel, new Vector2(.13f, .08f), new Vector2(.215f, .92f), 30, TextAnchor.MiddleLeft);
            gonulluMetni = KartMetni("Gonullu", ustPanel, new Vector2(.235f, .08f), new Vector2(.335f, .92f), 30, TextAnchor.MiddleLeft);
            destekMetni = KartMetni("Destek", ustPanel, new Vector2(.355f, .08f), new Vector2(.475f, .92f), 30, TextAnchor.MiddleLeft);
            iliskiMetni = KartMetni("Iliskiler", ustPanel, new Vector2(.50f, .08f), new Vector2(.835f, .92f), 23, TextAnchor.MiddleLeft);
            iliskiMetni.resizeTextForBestFit = true; iliskiMetni.resizeTextMinSize = 20; iliskiMetni.resizeTextMaxSize = 23;
            Konum((RectTransform)asamaBasligi.transform, .855f, .1f, .98f, .9f);
            asamaBasligi.color = turkuaz; asamaBasligi.fontStyle = FontStyle.Bold; asamaBasligi.fontSize = 25;
            asamaBasligi.alignment = TextAnchor.MiddleCenter;
            Konum((RectTransform)yuvaListesi.transform.parent, .015f, .56f, .22f, .89f);
            foreach (GameObject panel in new[] { raporPaneli, gorevPaneli, olayPaneli })
                Konum((RectTransform)panel.transform, .015f, .02f, .86f, .26f);
            Konum((RectTransform)kartPaneli.transform, .015f, .02f, .86f, .35f);
            PanelBasligi(raporPaneli, "asama_rapor");
            PanelBasligi(gorevPaneli, "asama_gonulluler");
            PanelBasligi(kartPaneli, "asama_kartlar");
            PanelBasligi(olayPaneli, "asama_olay");
            Konum((RectTransform)gorevDugmeleri, .02f, .06f, .98f, .74f);
            Konum((RectTransform)kartDugmeleri, .02f, .06f, .98f, .80f);
            Konum((RectTransform)raporYazisi.transform, .03f, .07f, .97f, .73f);
            Konum((RectTransform)olayYazisi.transform, .03f, .48f, .97f, .74f);
            Konum((RectTransform)aDugmesi.transform, .03f, .07f, .485f, .43f);
            Konum((RectTransform)bDugmesi.transform, .515f, .07f, .97f, .43f);
            Konum((RectTransform)ileriDugmesi.transform, .875f, .02f, .985f, .095f);
            Yuvarlak(ileriDugmesi.GetComponent<Image>(), turkuaz);
            ileriDugmesi.GetComponentInChildren<Text>().color = Color.white;
            foreach (Button secenek in new[] { aDugmesi, bDugmesi })
                Yuvarlak(secenek.GetComponent<Image>(), new Color32(241, 237, 228, 255));
            Transform sol = yuvaListesi.transform.parent;
            solBaslik = KartMetni("SolBaslik", sol, new Vector2(.06f, .86f), new Vector2(.94f, .98f), 27, TextAnchor.MiddleLeft);
            solBaslik.fontStyle = FontStyle.Bold; solBaslik.color = turkuaz;
            yuvaListesi.gameObject.SetActive(false);
            YuvaPaneliKur(sol);
            bolgeBilgisi.fontSize = 24;
            bolgeBilgisi.resizeTextForBestFit = true; bolgeBilgisi.resizeTextMinSize = 21; bolgeBilgisi.resizeTextMaxSize = 24;
            raporYazisi.fontSize = 26; olayYazisi.fontSize = 26;
            hedefYazisi.color = yazi;
            Konum((RectTransform)hedefYazisi.transform, .34f, .81f, .68f, .89f);

            hedefVazgec = Instantiate(dugmeKalibi, kok.transform);
            hedefVazgec.name = "HedefVazgec";
            Konum((RectTransform)hedefVazgec.transform, .69f, .815f, .79f, .885f);
            Yuvarlak(hedefVazgec.GetComponent<Image>(), kagit);
            hedefVazgec.GetComponentInChildren<Text>(true).text = Al("menu_geri");
            DugmeSesi.Bagla(hedefVazgec);
            hedefVazgec.onClick.AddListener(() =>
            {
                if (tur.BekleyenKart != null) tur.KartSec(tur.BekleyenKart);
                else if (tur.BekleyenGorev != null) tur.GorevSec(tur.BekleyenGorev);
            });
            hedefVazgec.gameObject.SetActive(false);

            kartBilgiPaneli = new GameObject("KartBilgiPaneli", typeof(RectTransform), typeof(Image));
            kartBilgiPaneli.layer = LayerMask.NameToLayer("UI");
            kartBilgiPaneli.transform.SetParent(kok.transform, false);
            var r = (RectTransform)kartBilgiPaneli.transform;
            r.anchorMin = new Vector2(.66f, .37f); r.anchorMax = new Vector2(.975f, .69f);
            r.offsetMin = r.offsetMax = Vector2.zero;
            var arka = kartBilgiPaneli.GetComponent<Image>(); Yuvarlak(arka, kagit); arka.raycastTarget = false;
            kartBilgiTuru = KartMetni("Tur", kartBilgiPaneli.transform, new Vector2(.07f, .80f), new Vector2(.93f, .94f), 20, TextAnchor.MiddleLeft);
            kartBilgiTuru.color = turkuaz; kartBilgiTuru.fontStyle = FontStyle.Bold;
            kartBilgiAdi = KartMetni("Ad", kartBilgiPaneli.transform, new Vector2(.07f, .62f), new Vector2(.93f, .80f), 29, TextAnchor.MiddleLeft);
            kartBilgiAdi.fontStyle = FontStyle.Bold; kartBilgiAdi.resizeTextForBestFit = true; kartBilgiAdi.resizeTextMinSize = 21; kartBilgiAdi.resizeTextMaxSize = 29;
            kartBilgiYazisi = KartMetni("Bilgi", kartBilgiPaneli.transform, new Vector2(.07f, .27f), new Vector2(.93f, .60f), 24, TextAnchor.UpperLeft);
            kartBilgiYazisi.resizeTextForBestFit = true;
            kartBilgiYazisi.resizeTextMinSize = 19;
            kartBilgiYazisi.resizeTextMaxSize = 24;
            kartBilgiEk = KartMetni("Etki", kartBilgiPaneli.transform, new Vector2(.07f, .05f), new Vector2(.93f, .25f), 21, TextAnchor.UpperLeft);
            kartBilgiEk.resizeTextForBestFit = true; kartBilgiEk.resizeTextMinSize = 17; kartBilgiEk.resizeTextMaxSize = 21;
            kartBilgiPaneli.SetActive(false);
        }

        private void PanelBasligi(GameObject panel, string id)
        {
            Text baslik = KartMetni("Baslik", panel.transform, new Vector2(.025f, .78f), new Vector2(.36f, .98f), 27, TextAnchor.MiddleLeft);
            baslik.fontStyle = FontStyle.Bold; baslik.text = Al(id);
            Text asamalar = KartMetni("Asamalar", panel.transform, new Vector2(.38f, .78f), new Vector2(.975f, .98f), 20, TextAnchor.MiddleRight);
            asamalar.resizeTextForBestFit = true; asamalar.resizeTextMinSize = 16; asamalar.resizeTextMaxSize = 20;
            asamaSeritleri.Add(asamalar);
        }

        private void BilgiZemini(Transform ebeveyn, float sol, float sag, Color32? renk = null)
        {
            var nesne = new GameObject("KaynakZemini", typeof(RectTransform), typeof(Image));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            nesne.transform.SetAsFirstSibling();
            Konum((RectTransform)nesne.transform, sol, .12f, sag, .88f);
            var resim = nesne.GetComponent<Image>();
            Yuvarlak(resim, renk ?? new Color32(239, 235, 224, 255));
            resim.raycastTarget = false;
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
            Konum((RectTransform)bolgeBilgisi.transform, .06f, .06f, .94f, .94f);
        }

        private static void Konum(RectTransform r, float sol, float alt, float sag, float ust)
        {
            r.anchorMin = new Vector2(sol, alt); r.anchorMax = new Vector2(sag, ust);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

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
            t.font = dugmeKalibi.GetComponentInChildren<Text>(true).font; t.fontSize = boyut;
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
            baslik.fontStyle = FontStyle.Bold; baslik.resizeTextForBestFit = true; baslik.resizeTextMinSize = 19; baslik.resizeTextMaxSize = 26; baslik.text = Al(gorev.Id);
            Text aciklama = KartMetni("GorevAciklama", dugme.transform, new Vector2(.30f, .06f), new Vector2(.97f, .66f), 24, TextAnchor.UpperLeft);
            aciklama.resizeTextForBestFit = true; aciklama.resizeTextMinSize = 20; aciklama.resizeTextMaxSize = 24; aciklama.text = Al(gorev.Id + "_aciklama");
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
            turYazisi.color = Color.white; turYazisi.resizeTextForBestFit = true; turYazisi.resizeTextMinSize = 16; turYazisi.resizeTextMaxSize = 21;
            turYazisi.text = Al($"kart_turu_{kart.Tur.ToString().ToLowerInvariant()}");

            var gorsel = new GameObject("Diorama", typeof(RectTransform), typeof(Image));
            gorsel.layer = LayerMask.NameToLayer("UI"); gorsel.transform.SetParent(dugme.transform, false);
            var gr = (RectTransform)gorsel.transform; gr.anchorMin = new Vector2(.02f, .33f); gr.anchorMax = new Vector2(.98f, .86f); gr.offsetMin = gr.offsetMax = Vector2.zero;
            var resim = gorsel.GetComponent<Image>(); resim.raycastTarget = false; resim.preserveAspect = true;
            Sprite sprite = KartGorseli(kart.Id);
            resim.sprite = sprite; resim.enabled = sprite != null;
            if (!oynanabilir) resim.color = new Color(1, 1, 1, .5f);

            Text ad = KartMetni("Ad", dugme.transform, new Vector2(.06f, .17f), new Vector2(.94f, .33f), 24, TextAnchor.MiddleLeft);
            ad.fontStyle = FontStyle.Bold; ad.resizeTextForBestFit = true; ad.resizeTextMinSize = 16; ad.resizeTextMaxSize = 24; ad.text = Al(kart.Id);
            Text maliyet = KartMetni("Maliyet", dugme.transform, new Vector2(.06f, .01f), new Vector2(.94f, .17f), 24, TextAnchor.MiddleLeft);
            maliyet.resizeTextForBestFit = true; maliyet.resizeTextMinSize = 18; maliyet.resizeTextMaxSize = 24;
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
                bant.resizeTextForBestFit = true; bant.resizeTextMinSize = 16; bant.resizeTextMaxSize = 20;
                bant.text = Al($"bant_{tur.Kumsal.YuvaBandi(i).ToString().ToLowerInvariant()}");

                Text ayrinti = KartMetni("Ayrinti", kart.transform, new Vector2(.06f, .06f), new Vector2(.94f, .60f), 21, TextAnchor.UpperLeft);
                ayrinti.resizeTextForBestFit = true; ayrinti.resizeTextMinSize = 18; ayrinti.resizeTextMaxSize = 21;
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
