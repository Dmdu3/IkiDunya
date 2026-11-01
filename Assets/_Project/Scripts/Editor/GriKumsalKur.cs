using IkiDunya.Camera;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using IkiDunya.UI.Menu;
using IkiDunya.UI.Night;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IkiDunya.Editor
{
    /// <summary>Kumsal sahnelerini kurar: deneme sahneleri (yuva gecesi, kaçış gecesi) ve oyunun Kumsal sahnesi (iki gece + geçici insan turu, adım adım açılır).</summary>
    public static class GriKumsalKur
    {
        private const string YuvaSahnesi = "Assets/_Project/Scenes/Prototypes/GriKumsal.unity";
        private const string KacisSahnesi = "Assets/_Project/Scenes/Prototypes/GriKacis.unity";
        private const string KumsalSahnesi = "Assets/_Project/Scenes/Levels/Kumsal.unity";
        private const string CopSeliSahnesi = "Assets/_Project/Scenes/Levels/Parkur_CopSeli.unity";
        private const string SistemlerKalibi = "Assets/_Project/Prefabs/Sistemler.prefab";
        private const string KalipKlasoru = "Assets/_Project/Prefabs/Hazards";
        private const string MalzemeKlasoru = "Assets/_Project/Scenes/Prototypes/GriKumsal";
        private const string AyarKlasoru = "Assets/_Project/ScriptableObjects/Settings";
        private const string YavruKalibi = "Assets/_Project/Prefabs/Turtles/Yavru.prefab";
        private const string GolgelikKalibi = "Assets/_Project/Prefabs/Structures/Golgelik.prefab";
        private const string TilkiCitiKalibi = "Assets/_Project/Prefabs/Structures/TilkiCiti.prefab";
        private const string YuvaKafesiKalibi = "Assets/_Project/Prefabs/Structures/YuvaKafesi.prefab";
        private const string KayipIziKalibi = "Assets/_Project/Prefabs/Environment/KayipIzi.prefab";
        // Yusuf'un kumsal arazisi ve objeleri: önce kalıcı yerine, yoksa Yusuf'un deneme klasörüne bakılır.
        private static readonly string[] AraziKaliplari = { "Assets/_Project/Prefabs/Environment/KumsalArazi.prefab", "Assets/Sandbox/Yusuf/KumsalArazi/KumsalArazi.prefab" };
        private static readonly string[] ModelKlasorleri =
        {
            "Assets/_Project/Models/Kumsal", "Assets/_Project/Models/Canlilar", "Assets/_Project/Models/Deniz",
            "Assets/Sandbox/Yusuf/KumsalObjeleri", "Assets/Sandbox/Yusuf/Canlilar", "Assets/Sandbox/Yusuf/DenizTabani",
            "Assets/Sandbox/Tekne", "Assets/Sandbox/Denizanasi",
        };
        // Kumsal arazisi yoksa deniz yüzeyi (gri kutu); arazide denizin kendi yüksekliği okunur.
        private const float GriSuSeviyesi = -0.55f;

        /// <summary>Bu kurulumda gri kutu yerine kumsal arazisi kullanıldı mı.</summary>
        private static bool araziVar;

        // Gri kutu ölçüleri (metre). Oyun değeri değil; sadece deneme sahnesinin yerleşimi.
        // Kıyı x ekseni boyunca uzanır; z = 0 deniz kenarı, z arttıkça karaya doğru.
        private const float KumsalUzunlugu = 80f;
        private const float KumsalDerinligi = 40f;
        private const float DenizDerinligi = 30f;
        private const float DalgaCizgisiZ = 6f;
        private const int BolgeSayisi = 8;

        private static Material kum, kumKoyu, deniz, taban, beyaz, engel, cop, kaplumbaga, insan, leke, iz, koni, tilkiKonisi, golge, siper, tilki, termalSerit, ayYolu, isikHalkasi;

        [MenuItem("İki Dünya/Gri kutu kumsalı kur (yuva gecesi)")]
        public static void YuvaGecesiKur()
        {
            if (!SahneAc(YuvaSahnesi, out Scene sahne)) return;
            KumSicakligi kumSicakligi = OrtakKur(Bos("Insanlar", null));
            DenemeKumsaliKur();
            YuvaKur(kumSicakligi, null);
            AraziyeOturt(sahne);
            OrtamKur(sahne);
            Kaydet(sahne, YuvaSahnesi);
        }

        [MenuItem("İki Dünya/Gri kutu kaçışı kur")]
        public static void KacisGecesiKur()
        {
            if (!SahneAc(KacisSahnesi, out Scene sahne)) return;
            KumSicakligi kumSicakligi = OrtakKur(null);
            DenemeKumsaliKur();
            KacisKur(kumSicakligi, null);
            AraziyeOturt(sahne);
            OrtamKur(sahne);
            Kaydet(sahne, KacisSahnesi);
        }

        /// <summary>Oyunun Kumsal sahnesi (Bölüm 2–4 ve final): yuva gecesi, kaçış gecesi, insan turu (yönetim ekranı) ve "30 yıl sonra"; her biri kendi adımında açılır.</summary>
        [MenuItem("İki Dünya/Kumsal sahnesini kur (oyun)")]
        public static void KumsalSahnesiKur()
        {
            if (!SahneAc(KumsalSahnesi, out Scene sahne)) return;
            Transform yuvaGrubu = Grup("YuvaGecesiNesneleri", AdimTuru.YuvaGecesi);
            // İnsanlar kumsalın durumundan kurulur, sadece yuva gecesinde görünür.
            KumSicakligi kumSicakligi = OrtakKur(Bos("Insanlar", yuvaGrubu));
            YuvaKur(kumSicakligi, yuvaGrubu);
            KacisKur(kumSicakligi, Grup("KacisGecesiNesneleri", AdimTuru.KacisGecesi));
            InsanTuruKur(kumSicakligi, Grup("InsanTuruNesneleri", AdimTuru.InsanTuru));
            MenuKur.FinalKur(Grup("FinalNesneleri", AdimTuru.Final), sahne, kaplumbaga, leke);
            // Ara sahneler: anne denizden kumsala bakarken; yavrular yuvadan çıkmadan kumsala tepeden.
            AraSahneKur("AraSahne_YuvaGecesi", new Vector3(0f, 7f, -16f), new Vector3(0f, 0f, 8f), -1, AdimTuru.YuvaGecesi);
            AraSahneKur("AraSahne_KacisGecesi", new Vector3(-6f, 12f, 4f), new Vector3(0f, 0f, 24f), -1, AdimTuru.KacisGecesi);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));
            AraziyeOturt(sahne);
            OrtamKur(sahne);
            Kaydet(sahne, KumsalSahnesi);
        }

        // ---- Yuva gecesi ----

        private static void YuvaKur(KumSicakligi kumSicakligi, Transform grup)
        {
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");

            // Şezlong, çöp ve insanlar KumsalYapilari ile kumsalın durumundan kurulur.
            var baslangic = new GameObject("AnneBaslangic").transform;
            baslangic.SetPositionAndRotation(new Vector3(0f, 0.1f, 1f), Quaternion.identity);
            GameObject anne = AnneKur(baslangic);
            anne.transform.SetParent(grup, true);

            var gece = new GameObject("YuvaGecesi").AddComponent<YuvaGecesi>();
            Bagla(gece, "ayarlar", Ayar<AnneAyarlari>("AnneAyarlari"));
            Bagla(gece, "kumsal", kumsal);
            Bagla(gece, "kum", kumSicakligi);
            Bagla(gece, "anne", anne.GetComponent<AnneKara>());
            Bagla(gece, "kazma", anne.GetComponent<AnneKazma>());
            Bagla(gece, "baslangic", baslangic);

            // Arayüz: enerji ve kazma çubuğu
            Transform tuval = TuvalKur("YuvaArayuzu", grup);
            Cubuk enerjiCubugu = CubukKur("EnerjiCubugu", tuval, new Vector2(0.03f, 0.04f), new Vector2(0.28f, 0.065f), new Color(0.4f, 0.8f, 0.45f), false);
            var enerjiGostergesi = enerjiCubugu.gameObject.AddComponent<EnerjiCubugu>();
            Bagla(enerjiGostergesi, "enerji", anne.GetComponent<Enerji>());
            Bagla(enerjiGostergesi, "dolgu", enerjiCubugu.transform.GetChild(0));

            Cubuk kazmaCubugu = CubukKur("KazmaCubugu", tuval, new Vector2(0.4f, 0.42f), new Vector2(0.6f, 0.45f), Color.white, false);
            var kazmaGostergesi = tuval.gameObject.AddComponent<KazmaGostergesi>();
            Bagla(kazmaGostergesi, "kazma", anne.GetComponent<AnneKazma>());
            Bagla(kazmaGostergesi, "cubuk", kazmaCubugu);

            SonucPaneliKur(gece, null);
        }

        // ---- Kaçış gecesi ----

        private static void KacisKur(KumSicakligi kumSicakligi, Transform grup)
        {
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");
            var kacisAyarlari = Ayar<KacisAyarlari>("KacisAyarlari");
            var yavruAyarlari = Ayar<YavruAyarlari>("YavruAyarlari");
            var avciAyarlari = Ayar<AvciAyarlari>("AvciAyarlari");

            var gunDogumu = new GameObject("GunDogumu").AddComponent<GunDogumu>();
            Bagla(gunDogumu, "ayarlar", kacisAyarlari);

            // Dalga: kıyının son metrelerinde gelip giden su şeridi
            var dalga = new GameObject("Dalga").AddComponent<Dalga>();
            dalga.transform.SetParent(kumSicakligi.transform, false);
            GameObject serit = Kutu("SuSeridi", dalga.transform, new Vector3(0f, 0.02f, 0f), new Vector3(KumsalUzunlugu, 0.04f, 1f), deniz, "Water");
            Object.DestroyImmediate(serit.GetComponent<Collider>());
            serit.isStatic = false;
            // Arazinin stilize denizi dalgayı zaten gösteriyor; şerit sadece oyun içi (çekiliyor mu, nerede).
            if (araziVar) serit.GetComponent<Renderer>().enabled = false;
            Bagla(dalga, "ayarlar", kacisAyarlari);
            Bagla(dalga, "suSeridi", serit.transform);

            // Sıcak kum lekeleri: sıcak bantlarda
            Transform lekeler = Bos("SicakLekeler", grup);
            Vector3[] lekeYerleri = { new Vector3(-3f, 0f, 16f), new Vector3(4f, 0f, 12f), new Vector3(-12f, 0f, 19f), new Vector3(12f, 0f, 21f) };
            for (int i = 0; i < lekeYerleri.Length; i++)
                SicakLeke($"SicakLeke_{i}", lekeler, lekeYerleri[i], kacisAyarlari, gunDogumu, kumSicakligi, kumsal);

            // Çöp, şişe ve tekerlek izi KumsalYapilari ile kumsalın durumundan kurulur; kum kalesi atlanır.
            Transform copler = Bos("Copler", grup);
            var kale = Bos("KumKalesi_0", copler);
            kale.position = new Vector3(0f, 0f, 11f);
            kale.gameObject.layer = LayerMask.NameToLayer("Trash");
            Kutu("Govde", kale, new Vector3(0f, 0.07f, 0f), new Vector3(1.5f, 0.14f, 0.5f), kumKoyu, "Trash");
            ModelEkle(kale.gameObject, "Kum_Kalesi");

            // Kumul gölgesi: tilkiden saklanılan siperler
            Transform siperler = Bos("Siperler", grup);
            Siper("KumulGolgesi_0", siperler, new Vector3(-6f, 0f, 22f), new Vector3(3f, 1f, 4f));
            Siper("KumulGolgesi_1", siperler, new Vector3(6f, 0f, 19f), new Vector3(3f, 1f, 4f));

            // Avcılar ve ışık
            Transform avcilar = Bos("Avcilar", grup);
            YengecKur("Yengec_0", avcilar, new Vector3(-5f, 0f, 8f), avciAyarlari);
            YengecKur("Yengec_1", avcilar, new Vector3(5f, 0f, 5f), avciAyarlari);
            MartiKur(avcilar, avciAyarlari);
            TilkiKur(avcilar, avciAyarlari, kumsal);

            // Sürü ve gözü
            var yuva = new GameObject("VarsayilanYuva").transform;
            yuva.position = new Vector3(0f, 0.1f, 22f);

            var suru = new GameObject("Suru").AddComponent<Suru>();
            suru.transform.SetParent(grup, false);
            suru.transform.SetPositionAndRotation(yuva.position, Quaternion.LookRotation(Vector3.back));
            var goz = KameraKur(suru.transform, new Vector3(0f, 0.5f, 0f), null, true);
            Bagla(suru, "ayarlar", yavruAyarlari);
            Bagla(suru, "yavruKalibi", YavruKalibiKur().GetComponent<Yavru>());
            Bagla(suru, "kamera", goz);

            var gece = new GameObject("KacisGecesi").AddComponent<KacisGecesi>();
            Bagla(gece, "ayarlar", yavruAyarlari);
            Bagla(gece, "kacisAyarlari", kacisAyarlari);
            Bagla(gece, "kumsal", kumsal);
            Bagla(gece, "kum", kumSicakligi);
            Bagla(gece, "suru", suru);
            Bagla(gece, "gunDogumu", gunDogumu);
            Bagla(gece, "dalga", dalga);
            Bagla(gece, "varsayilanYuva", yuva);

            // Arayüz: kumdan çıkış paneli, gün doğumu ve kurtarma çubuğu
            Transform tuval = TuvalKur("KacisArayuzu", grup);
            var panel = new GameObject("CikisPaneli", typeof(RectTransform), typeof(Image));
            panel.layer = LayerMask.NameToLayer("UI");
            panel.transform.SetParent(tuval, false);
            Yasla((RectTransform)panel.transform, Vector2.zero, Vector2.one);
            panel.GetComponent<Image>().color = Tema().Perde;

            // Çubuklar alttan başlar (en büyük yazı boyutunda da üstteki ipucu kutusuna değmez); altında çıkış enerjisi.
            Cubuk kesit = CubukKur("KumKesiti", panel.transform, new Vector2(0.44f, 0.1f), new Vector2(0.49f, 0.4f), new Color(0.86f, 0.78f, 0.6f), true);
            Cubuk sicaklik = CubukKur("YuzeySicakligi", panel.transform, new Vector2(0.53f, 0.1f), new Vector2(0.56f, 0.4f), Color.red, true);
            Cubuk cikisEnerjisi = CubukKur("CikisEnerjisi", panel.transform, new Vector2(0.35f, 0.06f), new Vector2(0.65f, 0.085f), new Color(0.4f, 0.8f, 0.45f), false);
            Cubuk gunCubugu = CubukKur("GunDogumuCubugu", tuval, new Vector2(0.3f, 0.94f), new Vector2(0.7f, 0.96f), new Color(1f, 0.75f, 0.3f), false);
            Cubuk yardimCubugu = CubukKur("YardimCubugu", tuval, new Vector2(0.42f, 0.42f), new Vector2(0.58f, 0.445f), Color.white, false);

            var gosterge = tuval.gameObject.AddComponent<KacisGostergesi>();
            Bagla(gosterge, "gece", gece);
            Bagla(gosterge, "ayarlar", yavruAyarlari);
            Bagla(gosterge, "gunDogumu", gunDogumu);
            Bagla(gosterge, "termal", Ayar<TermalAyarlari>("TermalAyarlari"));
            Bagla(gosterge, "cikisPaneli", panel);
            Bagla(gosterge, "kumKesiti", kesit);
            Bagla(gosterge, "yuzeySicakligi", sicaklik);
            Bagla(gosterge, "cikisEnerjisi", cikisEnerjisi);
            Bagla(gosterge, "gunDogumuCubugu", gunCubugu);
            Bagla(gosterge, "suru", suru);
            Bagla(gosterge, "yardimCubugu", yardimCubugu);
            Bagla(gosterge, "suruSayisi", SuruSayisiKur(tuval));

            SonucPaneliKur(null, gece);
        }

        // ---- Çöp seli parkuru ----

        /// <summary>Çöp seli parkuru (fırtına gecesi): 40 m genişliğinde bir kumsal parçası; yuva yukarıda, deniz aşağıda. Su dalga dalga basar;
        /// alçak kum tepeleri (zıplanarak çıkılır) güvenli yer, selin getirdiği çöpler yolları daraltır, kasalar suyla gelir, ağlar takar.</summary>
        [MenuItem("İki Dünya/Parkur: Çöp seli sahnesini kur")]
        public static void CopSeliKur()
        {
            if (!SahneAc(CopSeliSahnesi, out Scene sahne)) return;
            const float genislik = 40f;
            const float derinlik = 48f;

            var dunya = new GameObject("Dunya").transform;
            Kutu("Kum", dunya, new Vector3(0f, -0.5f, derinlik / 2f - 2f), new Vector3(genislik, 1f, derinlik), kum, "Ground");
            Kutu("DenizTabani", dunya, new Vector3(0f, -2f, -15f), new Vector3(genislik, 1f, 30f), taban, "Ground");
            SuYuzeyiKur("Deniz", dunya, new Vector3(0f, -0.3f, -15f), new Vector2(genislik, 30f), deniz);
            // Kenarlar: sürü kumsal parçasından çıkmasın.
            foreach (float x in new[] { -genislik / 2f - 0.5f, genislik / 2f + 0.5f })
            {
                GameObject duvar = Kutu("Kenar", dunya, new Vector3(x, 1f, derinlik / 2f - 10f), new Vector3(1f, 3f, derinlik + 20f), engel, "Obstacle");
                Object.DestroyImmediate(duvar.GetComponent<MeshRenderer>());
            }

            // Kum tepeleri: suyun ulaşamadığı alçak düzlükler (Space ile çıkılır).
            Material tepe = Malzeme("KumTepesi", new Color(0.93f, 0.86f, 0.68f));
            Vector3[] tepeler =
            {
                new Vector3(0f, 0f, 6f), new Vector3(13f, 0f, 8f), new Vector3(-8f, 0f, 10f), new Vector3(-15f, 0f, 16f),
                new Vector3(6f, 0f, 14f), new Vector3(-3f, 0f, 20f), new Vector3(10f, 0f, 23f), new Vector3(-10f, 0f, 27f),
                new Vector3(2f, 0f, 30f), new Vector3(-5f, 0f, 35f), new Vector3(8f, 0f, 35f), new Vector3(15f, 0f, 30f),
            };
            Transform tepeKoku = Bos("KumTepeleri", dunya);
            foreach (Vector3 yer in tepeler)
                Kutu("Tepe", tepeKoku, yer + Vector3.up * 0.075f, new Vector3(3f, 0.15f, 3f), tepe, "Ground");

            // Selin getirdiği çöpler: yolları daraltan engeller (Yusuf'un modelleri, yoksa gri kutu).
            Transform copKoku = Bos("SelCopleri", dunya);
            (string model, Vector3 yer, Vector3 boy)[] copler =
            {
                ("Sezlong", new Vector3(-4f, 0f, 12f), new Vector3(0.7f, 0.5f, 2f)),
                ("Sezlong", new Vector3(9f, 0f, 18f), new Vector3(0.7f, 0.5f, 2f)),
                ("Kaya_Magara", new Vector3(-12f, 0f, 21f), new Vector3(2.5f, 1.2f, 2f)),
                ("Cop_Yigini_2", new Vector3(3f, 0f, 25f), new Vector3(1.5f, 0.5f, 1.5f)),
                ("Cop_Yigini_1", new Vector3(-7f, 0f, 31f), new Vector3(1.5f, 0.5f, 1.5f)),
                ("Samandira", new Vector3(14f, 0f, 13f), new Vector3(0.8f, 0.8f, 0.8f)),
                ("Kum_Kalesi", new Vector3(-1f, 0f, 16f), new Vector3(1.5f, 0.4f, 0.6f)),
                ("Kutu", new Vector3(12f, 0f, 28f), new Vector3(0.8f, 0.6f, 0.8f)),
            };
            for (int i = 0; i < copler.Length; i++)
            {
                Transform parca = Bos($"Cop_{i}", copKoku);
                parca.position = copler[i].yer;
                parca.rotation = Quaternion.Euler(0f, i * 53f, 0f);
                parca.gameObject.layer = LayerMask.NameToLayer("Obstacle");
                Kutu("Govde", parca, new Vector3(0f, copler[i].boy.y / 2f, 0f), copler[i].boy, engel, "Obstacle").isStatic = false;
                ModelEkle(parca.gameObject, copler[i].model);
            }

            // Ağlar: takılan yavruyu öncü E ile kurtarır.
            GameObject agKalibi = AssetDatabase.LoadAssetAtPath<GameObject>($"{KalipKlasoru}/Ag.prefab");
            if (agKalibi == null) agKalibi = TuzakKalibi("Ag", new Vector3(0.8f, 0.02f, 0.8f), TrashTrap.Turu.Ag);
            foreach (Vector3 yer in new[] { new Vector3(-2f, 0f, 9f), new Vector3(7f, 0f, 21f), new Vector3(-9f, 0f, 24f) })
            {
                var ag = (GameObject)PrefabUtility.InstantiatePrefab(agKalibi, copKoku);
                ag.transform.position = yer;
            }

            // Sel: denetleyici kıyıda (z = 0, kara +z); su ve köpük onun çocukları.
            var kok = new GameObject("CopSeli");
            Material selSuyu = SaydamMalzeme("SelSuyu", new Color(0.3f, 0.42f, 0.45f, 0.8f));
            GameObject su = Kutu("SelSuyu", kok.transform, new Vector3(0f, 0.07f, 0f), new Vector3(genislik, 0.06f, 1f), selSuyu, "Water");
            Object.DestroyImmediate(su.GetComponent<Collider>());
            su.isStatic = false;
            GameObject kopuk = Kutu("Kopuk", kok.transform, new Vector3(0f, 0.12f, 0f), new Vector3(genislik, 0.15f, 0.8f), beyaz, "Water");
            Object.DestroyImmediate(kopuk.GetComponent<Collider>());
            kopuk.isStatic = false;

            // Yüzen kasalar: su gelince onunla karaya sürüklenir.
            Material kasaMalzemesi = Malzeme("Kasa", new Color(0.6f, 0.42f, 0.25f));
            var kasalar = new Object[4];
            for (int i = 0; i < kasalar.Length; i++)
            {
                Transform kasa = Bos($"Kasa_{i}", null);
                kasa.position = new Vector3(-14f + i * 9f, 0f, 3f + (i % 2) * 2f);
                if (ModelYerlestir(kasa, "Kutu", Vector3.zero, 1.2f) == null)
                    Kutu("Gri", kasa, new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 0.9f), kasaMalzemesi, "Default").isStatic = false;
                kasalar[i] = kasa;
            }

            // Yuva ve sürü (yavrular denize bakarak çıkar)
            Transform yuva = Bos("Yuva", null);
            yuva.position = new Vector3(0f, 0.1f, 40f);
            var suru = new GameObject("Suru").AddComponent<Suru>();
            suru.transform.SetPositionAndRotation(yuva.position, Quaternion.LookRotation(Vector3.back));
            var goz = KameraKur(suru.transform, new Vector3(0f, 0.5f, 0f), null);
            Bagla(suru, "ayarlar", Ayar<YavruAyarlari>("YavruAyarlari"));
            Bagla(suru, "yavruKalibi", YavruKalibiKur().GetComponent<Yavru>());
            Bagla(suru, "kamera", goz);

            var sel = kok.AddComponent<CopSeli>();
            Bagla(sel, "ayarlar", Ayar<ParkurAyarlari>("ParkurAyarlari"));
            Bagla(sel, "oyunAyarlari", Ayar<OyunAyarlari>("OyunAyarlari"));
            Bagla(sel, "yavruAyarlari", Ayar<YavruAyarlari>("YavruAyarlari"));
            Bagla(sel, "kumsal", Ayar<KumsalDurumu>("KumsalDurumu"));
            Bagla(sel, "suru", suru);
            Bagla(sel, "yuva", yuva);
            Bagla(sel, "su", su.transform);
            Bagla(sel, "kopuk", kopuk.transform);
            DiziBagla(sel, "kasalar", kasalar);

            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));
            OrtamKur(sahne);
            Kaydet(sahne, CopSeliSahnesi);
        }

        // ---- İnsan turu ----

        private static void InsanTuruKur(KumSicakligi kumSicakligi, Transform grup)
        {
            // Kumsala tepeden bakan kamera: yönetim ekranının haritası.
            var kamera = new GameObject("YonetimKamerasi");
            kamera.transform.SetParent(grup, false);
            kamera.transform.SetPositionAndRotation(new Vector3(0f, 55f, KumsalDerinligi / 2f), Quaternion.Euler(90f, 0f, 0f));
            var kameraBileseni = kamera.AddComponent<UnityEngine.Camera>();
            kamera.AddComponent<AudioListener>();
            kamera.AddComponent<TekKulak>();

            // Kayıp izleri: kanıt renginde simgeler, tıklayınca kanıt açılır
            var harita = Bos("KayipIziHaritasi", grup).gameObject.AddComponent<KayipIziHaritasi>();
            Bagla(harita, "kumsal", Ayar<KumsalDurumu>("KumsalDurumu"));
            Bagla(harita, "ayarlar", Ayar<HaritaAyarlari>("HaritaAyarlari"));
            Bagla(harita, "kamera", kameraBileseni);
            Bagla(harita, "simgeMalzemesi", termalSerit);

            // Yönetim ekranı: harita seçimi, yuva simgeleri, tur akışı, arayüz
            YonetimKur.SahneyeKur(grup, kameraBileseni, kumSicakligi, termalSerit);
        }

        // ---- Ortak ----

        /// <summary>Hepsini kur sırasında true: sahneler sormadan baştan kurulur.</summary>
        internal static bool SormadanKur;

        internal static bool SahneAc(string yol, out Scene sahne)
        {
            sahne = default;
            if (!SormadanKur && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            if (!SormadanKur && AssetDatabase.LoadAssetAtPath<SceneAsset>(yol) != null &&
                !EditorUtility.DisplayDialog("Gri kutu", $"{yol} var. Baştan kurulsun mu?", "Kur", "Vazgeç"))
                return false;

            KatmanKurulumu.Kur();
            // Yeni oyunun kumsalı başlangıç asset'inden kurulsun.
            var oyunAyarlari = Ayar<OyunAyarlari>("OyunAyarlari");
            if (oyunAyarlari.KumsalBaslangici == null)
            {
                Bagla(oyunAyarlari, "kumsalBaslangici", Ayar<KumsalBaslangici>("KumsalBaslangici"));
                EditorUtility.SetDirty(oyunAyarlari);
            }
            Klasor("Assets/_Project/Scenes/Prototypes", "GriKumsal");
            MalzemeleriHazirla();

            sahne = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            foreach (GameObject kok in sahne.GetRootGameObjects())
                if (kok.GetComponent<UnityEngine.Camera>() != null) Object.DestroyImmediate(kok);
            return true;
        }

        internal static void Kaydet(Scene sahne, string yol)
        {
            EditorSceneManager.SaveScene(sahne, yol);
            AssetDatabase.SaveAssets();
            // Oyun sahneleri (Levels) build listesinde olmalı; yoksa sona eklenir.
            if (yol.StartsWith("Assets/_Project/Scenes/Levels/"))
            {
                var liste = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                if (!liste.Exists(s => s.path == yol))
                {
                    // Menü ilk sahne olmalı (oyun onunla açılır).
                    if (yol.EndsWith("/Menu.unity")) liste.Insert(0, new EditorBuildSettingsScene(yol, true));
                    else liste.Add(new EditorBuildSettingsScene(yol, true));
                    EditorBuildSettings.scenes = liste.ToArray();
                }
            }
            Debug.Log($"Gri kutu kuruldu: {yol}");
        }

        private static void MalzemeleriHazirla()
        {
            kum = Malzeme("Kum", new Color(0.86f, 0.78f, 0.6f));
            kumKoyu = Malzeme("KumKoyu", new Color(0.8f, 0.72f, 0.55f));
            deniz = Malzeme("Deniz", new Color(0.15f, 0.35f, 0.55f));
            taban = Malzeme("Taban", new Color(0.45f, 0.42f, 0.35f));
            beyaz = Malzeme("Beyaz", Color.white);
            engel = Malzeme("Engel", new Color(0.55f, 0.3f, 0.25f));
            cop = Malzeme("Cop", new Color(0.3f, 0.5f, 0.3f));
            kaplumbaga = Malzeme("Kaplumbaga", new Color(0.35f, 0.3f, 0.2f));
            insan = Malzeme("Insan", new Color(0.3f, 0.35f, 0.6f));
            leke = Malzeme("SicakLeke", new Color(0.85f, 0.3f, 0.15f));
            iz = Malzeme("KayipIzi", new Color(0.25f, 0.1f, 0.1f));
            // Termal şeritler yarı saydam; kum altından görünür.
            termalSerit = SaydamMalzeme("TermalSerit", Color.white);
            isikHalkasi = SaydamMalzeme("IsikHalkasi", new Color(1f, 0.85f, 0.5f, 0.18f));
            koni = SaydamMalzeme("GorusKonisi", new Color(1f, 0.95f, 0.5f, 0.3f));
            tilkiKonisi = SaydamMalzeme("TilkiKonisi", new Color(1f, 0.5f, 0.2f, 0.3f));
            golge = SaydamMalzeme("Golge", new Color(0f, 0f, 0f, 0.5f));
            siper = Malzeme("Siper", new Color(0.62f, 0.56f, 0.42f));
            tilki = Malzeme("Tilki", new Color(0.85f, 0.45f, 0.15f));
            ayYolu = Malzeme("AyYolu", new Color(0.85f, 0.9f, 1f), "Universal Render Pipeline/Unlit");
        }

        /// <summary>Kum, deniz, kumul, dalga çizgisi, bölgeler, kum sıcaklığı ve termal görünüm, kumsal yapıları.</summary>
        private static KumSicakligi OrtakKur(Transform insanKoku)
        {
            var dunya = new GameObject("Dunya").transform;

            // Yusuf'un kumsal arazisi varsa o (kumsal, kumullar, stilize deniz); yoksa gri kutular.
            float suSeviyesi = AraziKur(dunya);
            araziVar = !float.IsNaN(suSeviyesi);
            if (!araziVar)
            {
                Kutu("Kum", dunya, new Vector3(0f, -0.5f, KumsalDerinligi / 2f), new Vector3(KumsalUzunlugu, 1f, KumsalDerinligi), kum, "Ground");
                Kutu("DenizTabani", dunya, new Vector3(0f, -2f, -DenizDerinligi / 2f), new Vector3(KumsalUzunlugu, 1f, DenizDerinligi), taban, "Ground");
                SuYuzeyiKur("Deniz", dunya, new Vector3(0f, GriSuSeviyesi, -DenizDerinligi / 2f), new Vector2(KumsalUzunlugu, DenizDerinligi), deniz);

                var kumul = new GameObject("Kumul").transform;
                kumul.SetParent(dunya);
                for (int i = 0; i < 8; i++)
                {
                    float x = -KumsalUzunlugu / 2f + 5f + i * 10f;
                    Kutu($"Kumul_{i}", kumul, new Vector3(x, 0.8f, KumsalDerinligi - 3f), new Vector3(9f, 2.5f, 6f), kumKoyu, "Ground");
                }
            }

            var dalgaCizgisi = new GameObject("DalgaCizgisi").transform;
            dalgaCizgisi.SetParent(dunya);
            for (float x = -KumsalUzunlugu / 2f + 1f; x < KumsalUzunlugu / 2f; x += 2f)
            {
                GameObject parca = Kutu("Parca", dalgaCizgisi, new Vector3(x, 0.01f, DalgaCizgisiZ), new Vector3(1f, 0.02f, 0.15f), beyaz, "Default");
                Object.DestroyImmediate(parca.GetComponent<Collider>());
            }

            // Bölgelerin gri şeritleri sadece düz gri kumda (araziye gömülürdü).
            var bolgeler = new GameObject("Bolgeler").transform;
            bolgeler.SetParent(dunya);
            float bolgeGenisligi = KumsalUzunlugu / BolgeSayisi;
            for (int i = 0; i < (araziVar ? 0 : BolgeSayisi); i++)
            {
                float x = -KumsalUzunlugu / 2f + bolgeGenisligi * (i + 0.5f);
                GameObject bolge = Kutu($"Bolge_{i}", bolgeler, new Vector3(x, 0.005f, KumsalDerinligi / 2f),
                    new Vector3(bolgeGenisligi, 0.01f, KumsalDerinligi), i % 2 == 0 ? kum : kumKoyu, "Default");
                Object.DestroyImmediate(bolge.GetComponent<Collider>());
            }

            KumSicakligi kumSicakligi = TermalKur();
            if (araziVar) KiyiOlc(kumSicakligi, suSeviyesi);
            // Dalga çizgisi kumul onarımı olan bölgede denize kayar.
            var cizgi = dalgaCizgisi.gameObject.AddComponent<DalgaCizgisi>();
            Bagla(cizgi, "kum", kumSicakligi);
            Bagla(cizgi, "kumsal", Ayar<KumsalDurumu>("KumsalDurumu"));
            // Ay ışığı: denizin üstünde doğru yönü gösteren parlak yol; sadece gecelerde (gündüz yönetim ekranında beyaz çubuk görünürdü).
            Transform gece = Grup("GeceNesneleri", new[] { AdimTuru.YuvaGecesi, AdimTuru.KacisGecesi });
            gece.SetParent(dunya, false);
            GameObject ay = Kutu("AyYolu", gece, new Vector3(-6f, -0.3f, -DenizDerinligi / 2f), new Vector3(3f, 0.01f, DenizDerinligi), ayYolu, "Default");
            Object.DestroyImmediate(ay.GetComponent<Collider>());
            // Bulut ayın önüne geçince yansıma da söner.
            ay.AddComponent<AyYansimasi>();
            // Kıyının dalga sesi: göz sahte aya kansa da kulak denizin yönünü söyler.
            new GameObject("KiyiSesi").AddComponent<KiyiSesi>().transform.SetParent(gece, false);

            KumsalYapilariKur(kumSicakligi, insanKoku);
            return kumSicakligi;
        }

        /// <summary>Kumsal arazisi kalıbını koyar ve deniz yüzeyinin yüksekliğini verir; kalıp yoksa NaN.</summary>
        private static float AraziKur(Transform dunya)
        {
            GameObject kalip = null;
            foreach (string yol in AraziKaliplari)
                if (kalip == null) kalip = AssetDatabase.LoadAssetAtPath<GameObject>(yol);
            if (kalip == null) return float.NaN;

            var arazi = (GameObject)PrefabUtility.InstantiatePrefab(kalip);
            arazi.transform.SetParent(dunya, false);
            Physics.SyncTransforms();
            Transform deniz = arazi.transform.Find("Deniz");
            return deniz != null ? deniz.position.y : GriSuSeviyesi;
        }

        /// <summary>Kıyı çizgisini araziden ölçer: kumsal boyunca her metrede zeminin deniz yüzeyini geçtiği z. Sıcaklık bantları, dalga çizgisi ve kaçış dalgası bunu izler.</summary>
        private static void KiyiOlc(KumSicakligi kumSicakligi, float suSeviyesi)
        {
            const float aralik = 1f;
            int sayi = Mathf.RoundToInt(KumsalUzunlugu / aralik) + 1;
            var kiyi = new float[sayi];
            for (int i = 0; i < sayi; i++)
            {
                float x = -KumsalUzunlugu / 2f + i * aralik;
                kiyi[i] = 0f;
                // Denizden karaya doğru: zeminin suyun üstüne çıktığı ilk yer.
                for (float z = -15f; z <= 25f; z += 0.1f)
                {
                    float h = Zemin.Yukseklik(new Vector3(x, 0f, z), float.NegativeInfinity);
                    if (h < suSeviyesi) continue;
                    kiyi[i] = z;
                    break;
                }
            }

            var so = new SerializedObject(kumSicakligi);
            SerializedProperty dizi = so.FindProperty("kiyiCizgisi");
            dizi.arraySize = sayi;
            for (int i = 0; i < sayi; i++) dizi.GetArrayElementAtIndex(i).floatValue = kiyi[i];
            so.FindProperty("kiyiAraligi").floatValue = aralik;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Düz kuma (y = 0) göre yerleştirilmiş kaçış ve final parçalarını arazinin yüksekliğine taşır.</summary>
        private static void AraziyeOturt(Scene sahne)
        {
            if (!araziVar) return;
            Physics.SyncTransforms();
            var gruplar = new System.Collections.Generic.HashSet<string> { "SicakLekeler", "Copler", "Siperler", "Avcilar", "SicakIzler" };
            var tekler = new System.Collections.Generic.HashSet<string> { "AnneBaslangic", "Anne", "VarsayilanYuva", "Suru", "YuruyusSonu" };
            var tasinacak = new System.Collections.Generic.List<Transform>();
            foreach (GameObject kok in sahne.GetRootGameObjects())
                foreach (Transform t in kok.GetComponentsInChildren<Transform>(true))
                    if (tekler.Contains(t.name) || (t.parent != null && gruplar.Contains(t.parent.name))) tasinacak.Add(t);
            foreach (Transform t in tasinacak)
                t.position += Vector3.up * Zemin.Yukseklik(t.position, 0f);
        }

        /// <summary>Yusuf'un modelini bulur: önce kalıcı klasörler, sonra Sandbox; malzemeli kalıp (.prefab) yoksa .fbx.</summary>
        internal static GameObject ModelBul(string model)
        {
            foreach (string uzanti in new[] { "prefab", "fbx" })
                foreach (string klasor in ModelKlasorleri)
                {
                    var kaynak = AssetDatabase.LoadAssetAtPath<GameObject>($"{klasor}/{model}.{uzanti}");
                    if (kaynak != null) return kaynak;
                }
            return null;
        }

        /// <summary>Hazır kalıbı (collider'ı ve layer'ıyla, ör. deniz tabanı, kaya) olduğu gibi koyar; kalıbın kendi dönüşü korunur. Yoksa null.</summary>
        internal static GameObject KalibiKoy(string model, Transform ebeveyn, Vector3 konum)
        {
            GameObject kaynak = ModelBul(model);
            if (kaynak == null) return null;
            var nesne = (GameObject)PrefabUtility.InstantiatePrefab(kaynak);
            nesne.transform.SetParent(ebeveyn, true);
            nesne.transform.position = konum;
            return nesne;
        }

        /// <summary>Performans: obje ekranda çok küçük kalınca çizilmez (tek seviyeli LODGroup). Ayrıca gölgeyi istenirse kapatır.</summary>
        internal static void UzaktaGizle(GameObject kok, float ekranOrani, bool golgeYok = false)
        {
            if (kok == null) return;
            Renderer[] gorunusler = kok.GetComponentsInChildren<Renderer>(true);
            if (golgeYok) foreach (Renderer r in gorunusler) r.shadowCastingMode = ShadowCastingMode.Off;
            LODGroup grup = kok.GetComponent<LODGroup>();
            if (grup == null) grup = kok.AddComponent<LODGroup>();
            grup.SetLODs(new[] { new LOD(ekranOrani, gorunusler) });
            grup.RecalculateBounds();
        }

        /// <summary>Modele animasyon denetleyicisi bağlar (ör. denizanasının kasılması); denetleyici yoksa dokunmaz.</summary>
        internal static void AnimasyonBagla(GameObject model, string denetleyiciYolu)
        {
            var denetleyici = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(denetleyiciYolu);
            if (model == null || denetleyici == null) return;
            Animator animator = model.GetComponentInChildren<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = denetleyici;
            // Animasyon modeli yerinden oynatmasın (yeri Yiyecek ve sürü belirler).
            animator.applyRootMotion = false;
        }

        /// <summary>Kalıba Yusuf'un modelini ekler (varsa) ve gri kutunun görüntüsünü gizler; collider ve oynanış aynı kalır.
        /// boy: modelin en uzun kenarı (0 ise modelin gerçek boyu).</summary>
        private static void ModelEkle(GameObject kok, string model, float boy = 0f)
        {
            if (ModelBul(model) == null) return;
            foreach (Renderer gri in kok.GetComponentsInChildren<Renderer>(true)) gri.enabled = false;
            ModelYerlestir(kok.transform, model, Vector3.zero, boy);
        }

        /// <summary>Modeli ebeveynin altına koyar: en uzun boyu verilen boya ölçeklenir (0 ise gerçek boyu), tabanı verilen yerel noktaya oturur.
        /// Collider'ı silinir, layer'ı ebeveynden alır. Model yoksa null.
        /// tabanaOtur false ise model kendi kökünü korur (kökü ortada olan modeller: yosun adası, plastik yığını).</summary>
        internal static GameObject ModelYerlestir(Transform ebeveyn, string model, Vector3 taban, float boy, bool tabanaOtur = true)
        {
            GameObject kaynak = ModelBul(model);
            if (kaynak == null) return null;
            var gorunus = (GameObject)PrefabUtility.InstantiatePrefab(kaynak);
            gorunus.name = "Model";
            gorunus.transform.SetParent(ebeveyn, false);
            gorunus.transform.localPosition = Vector3.zero;
            // Kalıbın kendi dönüşü korunur: Blender'dan gelen modellerde eksen düzeltmesi oradadır (yoksa model dik ya da ters durur).
            gorunus.transform.localRotation = kaynak.transform.localRotation;
            foreach (Transform t in gorunus.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = ebeveyn.gameObject.layer;
            foreach (Collider fazla in gorunus.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(fazla);

            if (boy > 0f && Sinirlar(gorunus, out Bounds once))
            {
                float enUzun = Mathf.Max(once.size.x, once.size.y, once.size.z);
                if (enUzun > 0.0001f) gorunus.transform.localScale *= boy / enUzun;
            }
            if (tabanaOtur && Sinirlar(gorunus, out Bounds sonra))
            {
                // Taban (yerel) dünyada nerede; modelin en altı oraya.
                Vector3 hedef = ebeveyn.TransformPoint(taban);
                gorunus.transform.position += new Vector3(hedef.x - sonra.center.x, hedef.y - sonra.min.y, hedef.z - sonra.center.z);
            }
            return gorunus;
        }

        private static bool Sinirlar(GameObject nesne, out Bounds sinir)
        {
            sinir = default;
            bool var = false;
            foreach (Renderer r in nesne.GetComponentsInChildren<Renderer>(true))
            {
                if (!var) sinir = r.bounds;
                else sinir.Encapsulate(r.bounds);
                var = true;
            }
            return var;
        }

        private static KumSicakligi TermalKur()
        {
            // Konumu deniz kenarının ortası; +z karaya doğru.
            var nesne = new GameObject("KumSicakligi");
            var kumSicakligi = nesne.AddComponent<KumSicakligi>();
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");
            Bagla(kumSicakligi, "ayarlar", Ayar<TermalAyarlari>("TermalAyarlari"));
            Bagla(kumSicakligi, "kumsal", kumsal);
            var so = new SerializedObject(kumSicakligi);
            so.FindProperty("uzunluk").floatValue = KumsalUzunlugu;
            so.FindProperty("derinlik").floatValue = KumsalDerinligi;
            so.FindProperty("bolgeSayisi").intValue = BolgeSayisi;
            so.FindProperty("dalgaCizgisi").floatValue = DalgaCizgisiZ;
            so.ApplyModifiedPropertiesWithoutUndo();

            var termal = nesne.AddComponent<TermalGorunum>();
            Bagla(termal, "kum", kumSicakligi);
            Bagla(termal, "kumsal", kumsal);
            Bagla(termal, "seritMalzemesi", termalSerit);
            return kumSicakligi;
        }

        private static void KumsalYapilariKur(KumSicakligi kumSicakligi, Transform insanKoku)
        {
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");
            var yapilar = new GameObject("KumsalYapilari").AddComponent<KumsalYapilari>();
            Bagla(yapilar, "kumsal", kumsal);
            Bagla(yapilar, "kum", kumSicakligi);
            Bagla(yapilar, "golgelikKalibi", GolgelikKalibiKur());
            Bagla(yapilar, "kayipIziKalibi", KayipIziKalibiKur());
            Bagla(yapilar, "copYiginiKalibi", TuzakKalibi("CopYigini", new Vector3(1.2f, 0.4f, 1.2f), TrashTrap.Turu.CopYigini, "Cop_Yigini_1"));
            Bagla(yapilar, "agKalibi", TuzakKalibi("Ag", new Vector3(0.8f, 0.02f, 0.8f), TrashTrap.Turu.Ag));
            Bagla(yapilar, "tekerlekIziKalibi", TuzakKalibi("TekerlekIzi", new Vector3(6f, 0.02f, 0.5f), TrashTrap.Turu.TekerlekIzi, "Tekerlek_Izi", 6f));
            Bagla(yapilar, "siseKalibi", KutuKalibi("Sise", new Vector3(0.15f, 0.3f, 0.15f), cop, "Trash", "Sise"));
            Bagla(yapilar, "sezlongKalibi", KutuKalibi("Sezlong", new Vector3(0.7f, 0.5f, 2f), engel, "Obstacle", "Sezlong"));
            Bagla(yapilar, "isikKalibi", KalipKaydet(IsikKur("OtelIsigi", null, Vector3.zero, Ayar<KacisAyarlari>("KacisAyarlari"), kumsal), $"{KalipKlasoru}/OtelIsigi.prefab"));
            Bagla(yapilar, "tilkiCitiKalibi", TilkiCitiKalibiKur());
            Bagla(yapilar, "yuvaKafesiKalibi", YuvaKafesiKalibiKur());
            // Üç ayrı insan (Yusuf'un modelleri); kumsalda karışık görünür.
            Insan insan1 = InsanKalibiKur("Insan", "Insan");
            Bagla(yapilar, "insanKalibi", insan1);
            DiziBagla(yapilar, "insanKaliplari", insan1, InsanKalibiKur("Insan_02", "Insan_02"), InsanKalibiKur("Insan_03", "Insan_03"));
            Bagla(yapilar, "insanKoku", insanKoku);
        }

        /// <summary>Deneme sahnelerinde Play başında kumsalı ilk haline getiren nesne.</summary>
        private static void DenemeKumsaliKur()
        {
            var deneme = new GameObject("DenemeKumsali").AddComponent<DenemeKumsali>();
            Bagla(deneme, "ayarlar", Ayar<OyunAyarlari>("OyunAyarlari"));
            Bagla(deneme, "kumsal", Ayar<KumsalDurumu>("KumsalDurumu"));
        }

        // ---- Yuva gecesi parçaları ----

        private static GameObject AnneKur(Transform baslangic)
        {
            var anne = new GameObject("Anne");
            anne.layer = LayerMask.NameToLayer("Turtle");
            anne.transform.SetPositionAndRotation(baslangic.position, baslangic.rotation);

            var kontrol = anne.AddComponent<CharacterController>();
            kontrol.radius = 0.4f;
            kontrol.height = 0.8f;
            kontrol.center = new Vector3(0f, 0.4f, 0f);
            kontrol.stepOffset = 0.2f;

            var anneAyarlari = Ayar<AnneAyarlari>("AnneAyarlari");
            anne.AddComponent<Enerji>();
            Bagla(anne.AddComponent<AnneKara>(), "ayarlar", anneAyarlari);
            Bagla(anne.AddComponent<AnneKazma>(), "ayarlar", anneAyarlari);

            Transform anneGorunusu = Govde(anne.transform, new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.35f, 1.1f), "Anne_Caretta");
            SalinimEkle(anne, anneGorunusu, SalinimTuru.Kara, anne.GetComponent<AnneKazma>());
            IzEkle(anne, new Vector2(0.35f, 0.2f), 0.35f, 0.4f, 60);
            anne.AddComponent<EnerjiUyarisi>();
            Bagla(anne.GetComponent<EnerjiUyarisi>(), "ayarlar", Ayar<GeribildirimAyarlari>("GeribildirimAyarlari"));
            KameraKur(anne.transform, new Vector3(0f, 0.35f, 0.6f), anne.transform);
            return anne;
        }

        /// <summary>Takan çöp kalıbı: kök yerde, collider ve TrashTrap çocukta.</summary>
        private static GameObject TuzakKalibi(string ad, Vector3 boyut, TrashTrap.Turu tur, string model = null, float modelBoy = 0f)
        {
            var kok = new GameObject(ad);
            kok.layer = LayerMask.NameToLayer("Trash");
            GameObject tuzak = Kutu("Govde", kok.transform, new Vector3(0f, boyut.y / 2f, 0f), boyut, cop, "Trash");
            tuzak.isStatic = false;
            var kutu = (BoxCollider)tuzak.GetComponent<Collider>();
            kutu.isTrigger = true;
            // Yassı tuzaklar da yavruyu içine alsın diye collider yukarı uzatılır.
            if (boyut.y < 0.1f)
            {
                kutu.size = new Vector3(1f, 0.3f / boyut.y, 1f);
                kutu.center = new Vector3(0f, 0.15f / boyut.y, 0f);
            }
            var bilesen = tuzak.AddComponent<TrashTrap>();
            var so = new SerializedObject(bilesen);
            so.FindProperty("tur").enumValueIndex = (int)tur;
            so.ApplyModifiedPropertiesWithoutUndo();
            if (model != null) ModelEkle(kok, model, modelBoy);
            return KalipKaydet(kok, $"{KalipKlasoru}/{ad}.prefab");
        }

        /// <summary>Tek kutudan engel kalıbı: kök yerde, kutu üstünde. Model verilirse kutu görünmez, model görünür.</summary>
        private static GameObject KutuKalibi(string ad, Vector3 boyut, Material malzeme, string katman, string model = null)
        {
            var kok = new GameObject(ad);
            kok.layer = LayerMask.NameToLayer(katman);
            Kutu("Govde", kok.transform, new Vector3(0f, boyut.y / 2f, 0f), boyut, malzeme, katman).isStatic = false;
            if (model != null) ModelEkle(kok, model);
            return KalipKaydet(kok, $"{KalipKlasoru}/{ad}.prefab");
        }

        private static Insan InsanKalibiKur(string ad, string model)
        {
            // Kök yerde durur; kapsül gövde tabanı yere değsin diye bir alt objede.
            var kok = new GameObject(ad);
            GameObject govde = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            govde.name = "Govde";
            govde.transform.SetParent(kok.transform, false);
            govde.transform.localPosition = new Vector3(0f, 1f, 0f);
            govde.GetComponent<Renderer>().sharedMaterial = insan;

            // Yusuf'un insanı varsa kapsül gizlenir; yürürken sallanan, ya model ya kapsül.
            Transform insanGorunusu = govde.transform;
            if (ModelBul(model) != null)
            {
                govde.GetComponent<Renderer>().enabled = false;
                insanGorunusu = ModelYerlestir(kok.transform, model, Vector3.zero, 0f).transform;
            }
            SalinimEkle(kok, insanGorunusu, SalinimTuru.Kara);
            UzaktaGizle(kok, 0.015f);

            var insanBileseni = kok.AddComponent<Insan>();
            Bagla(insanBileseni, "ayarlar", Ayar<InsanAyarlari>("InsanAyarlari"));
            Bagla(insanBileseni, "koniMalzemesi", koni);
            return KalipKaydet(kok, $"{KalipKlasoru}/{ad}.prefab").GetComponent<Insan>();
        }

        // ---- Kaçış parçaları ----

        private static void SicakLeke(string ad, Transform ebeveyn, Vector3 konum, KacisAyarlari ayarlar, GunDogumu gunDogumu, KumSicakligi kumSicakligi, KumsalDurumu kumsal)
        {
            GameObject nesne = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nesne.name = ad;
            nesne.layer = LayerMask.NameToLayer("Hazard");
            nesne.transform.SetParent(ebeveyn, false);
            nesne.transform.position = konum + Vector3.up * 0.015f;
            nesne.transform.localScale = new Vector3(2f, 0.01f, 2f);
            nesne.GetComponent<Renderer>().sharedMaterial = leke;

            // Silindirin kendi collider'ı yassı ölçekte küreye döner; yerine yavruyu içine alan bir kutu.
            Object.DestroyImmediate(nesne.GetComponent<Collider>());
            var kutu = nesne.AddComponent<BoxCollider>();
            kutu.isTrigger = true;
            kutu.size = new Vector3(1f, 60f, 1f);
            kutu.center = new Vector3(0f, 30f, 0f);

            var bolge = nesne.AddComponent<HazardZone>();
            Bagla(bolge, "ayarlar", ayarlar);
            Bagla(bolge, "gunDogumu", gunDogumu);
            Bagla(bolge, "kum", kumSicakligi);
            Bagla(bolge, "kumsal", kumsal);
        }

        private static void Siper(string ad, Transform ebeveyn, Vector3 konum, Vector3 boyut)
        {
            GameObject nesne = Kutu(ad, ebeveyn, konum + Vector3.up * 0.5f, boyut, siper, "Cover");
            nesne.GetComponent<Collider>().isTrigger = true;
            Object.DestroyImmediate(nesne.GetComponent<MeshRenderer>());
            // Görünen gölge: kumun üstünde ince koyu şerit
            GameObject gorunus = Kutu($"{ad}_Gorunus", ebeveyn, konum + Vector3.up * 0.006f, new Vector3(boyut.x, 0.01f, boyut.z), siper, "Default");
            Object.DestroyImmediate(gorunus.GetComponent<Collider>());
            nesne.AddComponent<Cover>();
        }

        private static void YengecKur(string ad, Transform ebeveyn, Vector3 konum, AvciAyarlari ayarlar)
        {
            var kok = Bos(ad, ebeveyn);
            kok.position = konum;
            GameObject delik = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            delik.name = "TitreyenKum";
            Object.DestroyImmediate(delik.GetComponent<Collider>());
            delik.transform.SetParent(kok, false);
            delik.transform.localPosition = Vector3.up * 0.01f;
            delik.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f);
            delik.GetComponent<Renderer>().sharedMaterial = kumKoyu;

            // Yengeç deliğin kenarında bekler (Yusuf'un modeli).
            ModelYerlestir(kok, "Yengec", new Vector3(0.45f, 0f, 0f), 0.35f);

            var yengec = kok.gameObject.AddComponent<Yengec>();
            Bagla(yengec, "ayarlar", ayarlar);
            Bagla(yengec, "titreyenKum", delik.transform);
        }

        private static void MartiKur(Transform ebeveyn, AvciAyarlari ayarlar)
        {
            var kok = Bos("Marti", ebeveyn);
            kok.position = new Vector3(0f, 15f, 15f);
            GameObject golgeNesnesi = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            golgeNesnesi.name = "Golge";
            Object.DestroyImmediate(golgeNesnesi.GetComponent<Collider>());
            golgeNesnesi.transform.SetParent(kok, false);
            golgeNesnesi.transform.localScale = new Vector3(ayarlar.MartiDalisYaricapi * 2f, 0.005f, ayarlar.MartiDalisYaricapi * 2f);
            var renderer = golgeNesnesi.GetComponent<Renderer>();
            renderer.sharedMaterial = golge;
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            GameObject martiModeli = ModelYerlestir(kok, "Marti", Vector3.zero, 0.9f);
            if (martiModeli != null) SalinimEkle(kok.gameObject, martiModeli.transform, SalinimTuru.Kus);

            var marti = kok.gameObject.AddComponent<Marti>();
            Bagla(marti, "ayarlar", ayarlar);
            Bagla(marti, "golge", golgeNesnesi.transform);
        }

        private static void TilkiKur(Transform ebeveyn, AvciAyarlari ayarlar, KumsalDurumu kumsal)
        {
            var noktaA = Bos("TilkiYolu_A", ebeveyn);
            noktaA.position = new Vector3(-25f, 0f, 30f);
            var noktaB = Bos("TilkiYolu_B", ebeveyn);
            noktaB.position = new Vector3(25f, 0f, 30f);

            var kok = Bos("Tilki", ebeveyn);
            kok.SetPositionAndRotation(noktaA.position, Quaternion.LookRotation(Vector3.right));
            // Gövde boş bir kök: içinde gri kutu ya da Yusuf'un modeli; tilki eşelerken bu sallanır.
            GameObject govde = Bos("Govde", kok).gameObject;
            govde.layer = LayerMask.NameToLayer("Predator");
            GameObject tilkiGorunusu = ModelYerlestir(govde.transform, "Tilki", Vector3.zero, 0.9f);
            if (tilkiGorunusu == null)
            {
                tilkiGorunusu = Kutu("Gri", govde.transform, new Vector3(0f, 0.2f, 0f), new Vector3(0.3f, 0.35f, 0.8f), tilki, "Predator");
                Object.DestroyImmediate(tilkiGorunusu.GetComponent<Collider>());
                tilkiGorunusu.isStatic = false;
            }
            SalinimEkle(kok.gameObject, tilkiGorunusu.transform, SalinimTuru.Kara);

            var bilesen = kok.gameObject.AddComponent<Tilki>();
            Bagla(bilesen, "ayarlar", ayarlar);
            Bagla(bilesen, "kumsal", kumsal);
            Bagla(bilesen, "koniMalzemesi", tilkiKonisi);
            Bagla(bilesen, "govde", govde);
            var so = new SerializedObject(bilesen);
            SerializedProperty noktalar = so.FindProperty("noktalar");
            noktalar.arraySize = 2;
            noktalar.GetArrayElementAtIndex(0).objectReferenceValue = noktaA;
            noktalar.GetArrayElementAtIndex(1).objectReferenceValue = noktaB;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject IsikKur(string ad, Transform ebeveyn, Vector3 konum, KacisAyarlari ayarlar, KumsalDurumu kumsal)
        {
            var kok = Bos(ad, ebeveyn);
            kok.position = konum;
            kok.gameObject.layer = LayerMask.NameToLayer("Hazard");
            GameObject direk = Kutu("Direk", kok, new Vector3(0f, 1.5f, 0f), new Vector3(0.2f, 3f, 0.2f), engel, "Obstacle");
            direk.isStatic = false;

            var lamba = new GameObject("Lamba").AddComponent<Light>();
            lamba.transform.SetParent(kok, false);
            lamba.transform.localPosition = Vector3.up * 3f;
            lamba.type = LightType.Point;
            lamba.range = 16f;
            lamba.intensity = 4f;
            lamba.color = new Color(1f, 0.85f, 0.6f);

            // Çekim alanı: lambanın çevresindeki kum
            var alan = kok.gameObject.AddComponent<SphereCollider>();
            alan.isTrigger = true;
            alan.radius = 9f;

            var isik = kok.gameObject.AddComponent<IsikKaynagi>();
            Bagla(isik, "ayarlar", ayarlar);
            Bagla(isik, "kumsal", kumsal);
            Bagla(isik, "isik", lamba);

            // Kumdaki ışık halkası: çekim alanını gösterir
            GameObject halka = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            halka.name = "Halka";
            Object.DestroyImmediate(halka.GetComponent<Collider>());
            halka.transform.SetParent(kok, false);
            halka.transform.localPosition = Vector3.up * 0.02f;
            halka.transform.localScale = new Vector3(alan.radius * 2f, 0.005f, alan.radius * 2f);
            var halkaGorunus = halka.GetComponent<Renderer>();
            halkaGorunus.sharedMaterial = isikHalkasi;
            halkaGorunus.shadowCastingMode = ShadowCastingMode.Off;
            Bagla(isik, "halka", halka);

            // Gece yavrunun gözünde lamba ay sanılır: gökte, lambanın yönünde gerçek aydan parlak bir ay belirir, direk gizlenir.
            // Gerçek ay denizin üstündedir; sahte ay yavruyu karaya çeker. Yaklaşınca ay lambaya düşer; Işık Yasağı'nda hiç görünmez.
            GameObject sahteAy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sahteAy.name = "Yanilgi_Ay";
            Object.DestroyImmediate(sahteAy.GetComponent<Collider>());
            sahteAy.transform.SetParent(kok, false);
            var ayGorunus = sahteAy.GetComponent<Renderer>();
            ayGorunus.sharedMaterial = Malzeme("SahteAy", Color.white, "IkiDunya/Gokyuzu");
            ayGorunus.shadowCastingMode = ShadowCastingMode.Off;
            ayGorunus.receiveShadows = false;
            sahteAy.SetActive(false);

            // Kumda sahte aya giden parlak yol (gerçek ayın denizdeki yansıması gibi); noktaları AlgiNesnesi kumun üstüne oturtur.
            var yol = new GameObject("Yanilgi_AyYolu").AddComponent<LineRenderer>();
            yol.transform.SetParent(kok, false);
            yol.useWorldSpace = true;
            yol.positionCount = 16;
            yol.alignment = LineAlignment.TransformZ;
            yol.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            yol.widthCurve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(0.6f, 1f), new Keyframe(1f, 0.6f));
            // Uçları erir: yol değil, kumda bir ışık parıltısı.
            var gradyan = new Gradient();
            gradyan.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.7f, 0.25f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0.2f, 1f) });
            yol.colorGradient = gradyan;
            yol.sharedMaterial = Malzeme("SahteAyYolu", Color.white, "IkiDunya/Gokyuzu");
            yol.shadowCastingMode = ShadowCastingMode.Off;
            yol.receiveShadows = false;
            yol.enabled = false;

            var algi = kok.gameObject.AddComponent<AlgiNesnesi>();
            Bagla(algi, "gercekGorunus", direk.transform);
            Bagla(algi, "yanilgiGorunus", sahteAy);
            Bagla(algi, "odak", lamba.transform);
            Bagla(algi, "isikYasagiKumsali", kumsal);
            Bagla(algi, "ayYolu", yol);
            var so = new SerializedObject(algi);
            so.FindProperty("tur").enumValueIndex = (int)AlgiTuru.LambaAy;
            so.ApplyModifiedPropertiesWithoutUndo();
            return kok.gameObject;
        }

        // ---- Sonuç paneli ----

        private static void SonucPaneliKur(YuvaGecesi yuvaGecesi, KacisGecesi kacisGecesi)
        {
            Transform tuval = TuvalKur("SonucArayuzu", null);
            // İpucu kutusunun (40) üstünde: sonuç açıkken gelen ipucu panelin altında kalır.
            tuval.GetComponent<Canvas>().sortingOrder = 46;
            var panel = tuval.gameObject.AddComponent<GeceSonucuPaneli>();
            Bagla(panel, "termal", Ayar<TermalAyarlari>("TermalAyarlari"));
            Bagla(panel, "metinler", Ayar<MetinTablosu>("MetinTablosu"));

            if (yuvaGecesi != null)
            {
                Bagla(panel, "yuvaGecesi", yuvaGecesi);
                RectTransform kutu = PanelKur("YuvaSonucu", tuval, "sonuc_yuva_baslik", 2);
                Bagla(panel, "yuvaPaneli", kutu.gameObject);
                // 1. satır: bandın rengi ve adı; 2. satır: yumurta sayısı
                Bagla(panel, "yuvaBandi", RenkKaresi(kutu, 0, Color.white));
                SatirAdi(kutu, 0, "sonuc_bant");
                Bagla(panel, "bantAdi", SayiYazisi(kutu, 0, 32));
                SatirAdi(kutu, 1, "sonuc_yumurta");
                Bagla(panel, "yumurtaSayisi", SayiYazisi(kutu, 1, 48));
            }

            if (kacisGecesi != null)
            {
                Bagla(panel, "kacisGecesi", kacisGecesi);
                RectTransform kutu = PanelKur("KacisSonucu", tuval, "sonuc_kacis_baslik", 4, SonucNedenSeridi);
                Bagla(panel, "kayipNedenleri", KayipNedenleriKur(kutu, 4));
                Bagla(panel, "kacisPaneli", kutu.gameObject);
                Color[] renkler = { new Color(0.4f, 0.8f, 0.45f), new Color(0.85f, 0.25f, 0.2f), new Color(1f, 0.55f, 0.75f), new Color(0.4f, 0.65f, 1f) };
                string[] alanlar = { "kurtulan", "kaybedilen", "disi", "erkek" };
                for (int i = 0; i < alanlar.Length; i++)
                {
                    RenkKaresi(kutu, i, renkler[i]);
                    SatirAdi(kutu, i, $"sonuc_{alanlar[i]}");
                    Bagla(panel, alanlar[i], SayiYazisi(kutu, i, 48));
                }
            }
        }

        // Sonuç paneli ölçüleri (tuval birimi): başlık şeridi, satır yüksekliği, alttaki "devam" şeridi. 4 satırlı panel 1280x720'ye sığar.
        // Kaçış sonucunda satırların altındaki kayıp nedenleri şeridi; 4 satır + şerit 1280x720'ye sığar.
        private const float SonucGenisligi = 650f, SonucBasligi = 100f, SonucSatiri = 100f, SonucAlti = 70f, SonucNedenSeridi = 56f;

        /// <summary>Kaçış sonucunda son satırın altında, kayıpların nedenlerini yazan soluk şerit (yazıyı GeceSonucuPaneli kurar).</summary>
        private static Text KayipNedenleriKur(RectTransform panel, int satirSayisi)
        {
            var nesne = new GameObject("KayipNedenleri", typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(panel, false);
            SatiraKoy((RectTransform)nesne.transform, 0f, 1f, 38f, 36f, SatirUstu(satirSayisi), SonucNedenSeridi);
            var yazi = nesne.GetComponent<Text>();
            TemaYazisi(yazi, false, Tema().SolukYazi);
            yazi.fontSize = 24;
            yazi.alignment = TextAnchor.MiddleLeft;
            return yazi;
        }

        /// <summary>Koşuda gün doğumu çubuğunun altında kumda, denizde, kayıp yavru sayısı (yazıyı KacisGostergesi kurar).</summary>
        private static Text SuruSayisiKur(Transform tuval)
        {
            var nesne = new GameObject("SuruSayisi", typeof(RectTransform), typeof(Text), typeof(Shadow));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(tuval, false);
            Yasla((RectTransform)nesne.transform, new Vector2(0.3f, 0.895f), new Vector2(0.7f, 0.935f));
            var yazi = nesne.GetComponent<Text>();
            TemaYazisi(yazi, false, Tema().Yazi);
            yazi.fontSize = 26;
            yazi.alignment = TextAnchor.MiddleCenter;
            nesne.SetActive(false);
            return yazi;
        }

        /// <summary>Panel: üstte başlık, altta "devam etmek için…" satırı; aradaki satırlar renk, ad, sayı (ve istenirse ek şerit). Boyu satır sayısına göre, ekranın ortasında.</summary>
        private static RectTransform PanelKur(string ad, Transform tuval, string baslikId, int satirSayisi, float ekYukseklik = 0f)
        {
            var panel = new GameObject(ad, typeof(RectTransform), typeof(Image));
            panel.layer = LayerMask.NameToLayer("UI");
            panel.transform.SetParent(tuval, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(SonucGenisligi, SonucBasligi + satirSayisi * SonucSatiri + ekYukseklik + SonucAlti);
            ArayuzTemasi tema = Tema();
            panel.GetComponent<Image>().color = tema.Panel;
            // Başlık serif, solda; altında kısa vurgu çizgisi.
            Text baslik = PanelYazisi(rect, "Baslik", new Vector2(0f, 1f), Vector2.one, 44, TextAnchor.LowerLeft, baslikId);
            TemaYazisi(baslik, true, tema.Yazi);
            SatiraKoy((RectTransform)baslik.transform, 0f, 1f, 36f, 36f, 0f, SonucBasligi - 22f);
            var cizgi = new GameObject("Cizgi", typeof(RectTransform), typeof(Image));
            cizgi.layer = LayerMask.NameToLayer("UI");
            cizgi.transform.SetParent(rect, false);
            Yerles((RectTransform)cizgi.transform, new Vector2(0f, 1f), new Vector2(38f, -(SonucBasligi - 12f)), new Vector2(90f, 3f));
            cizgi.GetComponent<Image>().color = tema.Vurgu;
            Text devam = PanelYazisi(rect, "Devam", Vector2.zero, new Vector2(1f, 0f), 22, TextAnchor.MiddleLeft, "sonuc_devam");
            devam.color = tema.SolukYazi;
            ((RectTransform)devam.transform).offsetMin = new Vector2(38f, 8f);
            ((RectTransform)devam.transform).offsetMax = new Vector2(-36f, SonucAlti - 8f);
            panel.SetActive(false);
            return rect;
        }

        /// <summary>Kutuyu panelin üstünden ust birim aşağıda, yukseklik boyunda, yatayda solCapa..sagCapa arasına (kenar boşluklarıyla) koyar.</summary>
        private static void SatiraKoy(RectTransform r, float solCapa, float sagCapa, float sol, float sag, float ust, float yukseklik)
        {
            r.anchorMin = new Vector2(solCapa, 1f);
            r.anchorMax = new Vector2(sagCapa, 1f);
            r.offsetMin = new Vector2(sol, -ust - yukseklik);
            r.offsetMax = new Vector2(-sag, -ust);
        }

        private static float SatirUstu(int satir) => SonucBasligi + satir * SonucSatiri;

        /// <summary>Panelde i. satırın solundaki renkli kare.</summary>
        private static Image RenkKaresi(RectTransform panel, int satir, Color renk)
        {
            var kare = new GameObject($"Renk_{satir}", typeof(RectTransform), typeof(Image));
            kare.layer = LayerMask.NameToLayer("UI");
            kare.transform.SetParent(panel, false);
            // Satırın solunda ince renk çubuğu.
            SatiraKoy((RectTransform)kare.transform, 0f, 0f, 38f, -44f, SatirUstu(satir) + 30f, 40f);
            var resim = kare.GetComponent<Image>();
            resim.color = renk;
            return resim;
        }

        /// <summary>Panelde i. satırın adı (metinler.md'den).</summary>
        private static void SatirAdi(RectTransform panel, int satir, string id)
        {
            Text ad = PanelYazisi(panel, $"Ad_{satir}", Vector2.zero, Vector2.one, 30, TextAnchor.MiddleLeft, id);
            ad.color = Tema().SolukYazi;
            SatiraKoy((RectTransform)ad.transform, 0f, 0.6f, 64f, 0f, SatirUstu(satir), SonucSatiri);
        }

        /// <summary>Panelde i. satırın sağındaki sayı (ya da bant adı).</summary>
        private static Text SayiYazisi(RectTransform panel, int satir, int boyut)
        {
            var nesne = new GameObject($"Sayi_{satir}", typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(panel, false);
            SatiraKoy((RectTransform)nesne.transform, 0.6f, 1f, 0f, 36f, SatirUstu(satir), SonucSatiri);
            var yazi = nesne.GetComponent<Text>();
            TemaYazisi(yazi, true, Tema().Yazi);
            yazi.fontSize = boyut;
            yazi.alignment = TextAnchor.MiddleRight;
            return yazi;
        }

        /// <summary>Metin tablosundan id ile dolan sabit yazı.</summary>
        internal static Text PanelYazisi(RectTransform panel, string ad, Vector2 enAz, Vector2 enCok, int boyut, TextAnchor hiza, string id)
        {
            var nesne = new GameObject(ad, typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(panel, false);
            Yasla((RectTransform)nesne.transform, enAz, enCok);
            var yazi = nesne.GetComponent<Text>();
            TemaYazisi(yazi, false, Tema().Yazi);
            yazi.fontSize = boyut;
            yazi.alignment = hiza;
            var metin = nesne.AddComponent<MetinYazisi>();
            Bagla(metin, "tablo", Ayar<MetinTablosu>("MetinTablosu"));
            var so = new SerializedObject(metin);
            so.FindProperty("id").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
            return yazi;
        }

        // ---- Kalıplar (prefab) ----

        private static GameObject YavruKalibiKur()
        {
            var yavru = new GameObject("Yavru");
            yavru.layer = LayerMask.NameToLayer("Turtle");
            var kontrol = yavru.AddComponent<CharacterController>();
            kontrol.radius = 0.1f;
            kontrol.height = 0.2f;
            kontrol.center = new Vector3(0f, 0.1f, 0f);
            kontrol.stepOffset = 0.05f;
            kontrol.skinWidth = 0.01f;
            yavru.AddComponent<Enerji>();
            yavru.AddComponent<Yavru>();
            Transform yavruGorunusu = Govde(yavru.transform, new Vector3(0f, 0.04f, 0f), new Vector3(0.16f, 0.06f, 0.2f), "Yavru_Caretta");
            SalinimEkle(yavru, yavruGorunusu, SalinimTuru.Kara);
            IzEkle(yavru, new Vector2(0.06f, 0.04f), 0.12f, 0.06f, 12);
            return KalipKaydet(yavru, YavruKalibi);
        }

        private static GameObject GolgelikKalibiKur()
        {
            var golgelik = new GameObject("Golgelik");
            int katman = LayerMask.NameToLayer("Structure");
            golgelik.layer = katman;
            Vector3[] direkler = { new Vector3(-1.5f, 1f, -1.5f), new Vector3(1.5f, 1f, -1.5f), new Vector3(-1.5f, 1f, 1.5f), new Vector3(1.5f, 1f, 1.5f) };
            foreach (Vector3 direk in direkler)
                Kutu("Direk", golgelik.transform, direk, new Vector3(0.1f, 2f, 0.1f), beyaz, "Structure").isStatic = false;
            Kutu("Tente", golgelik.transform, new Vector3(0f, 2f, 0f), new Vector3(3.4f, 0.05f, 3.4f), beyaz, "Structure").isStatic = false;
            // Yusuf'un gölgeliği varsa direk ve tentenin yerine geçer (3,4 m tente).
            ModelEkle(golgelik, "Golgelik", 3.4f);
            return KalipKaydet(golgelik, GolgelikKalibi);
        }

        /// <summary>Tilki çiti parçası (4 m): bölgenin kara tarafına dizilir. Model yoksa kalıp kurulmaz.</summary>
        private static GameObject TilkiCitiKalibiKur() => ModelKalibiKur("TilkiCiti", "Tilki_Citi", 4f, TilkiCitiKalibi);

        /// <summary>Yuva kafesi (1,04 m tel kafes): kafesli yuvanın etrafına konur. Model yoksa kalıp kurulmaz.</summary>
        private static GameObject YuvaKafesiKalibiKur() => ModelKalibiKur("YuvaKafesi", "Yuva_Kafesi", 1.04f, YuvaKafesiKalibi);

        /// <summary>Sadece modelden oluşan yapı kalıbı. Model yoksa null döner; çağıran taraf yapıyı hiç kurmaz.</summary>
        private static GameObject ModelKalibiKur(string ad, string model, float boy, string yol)
        {
            if (ModelBul(model) == null) return null;
            var kok = new GameObject(ad);
            kok.layer = LayerMask.NameToLayer("Structure");
            ModelYerlestir(kok.transform, model, Vector3.zero, boy);
            return KalipKaydet(kok, yol);
        }

        private static GameObject KayipIziKalibiKur()
        {
            GameObject isaret = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            isaret.name = "KayipIzi";
            Object.DestroyImmediate(isaret.GetComponent<Collider>());
            isaret.transform.localScale = new Vector3(0.3f, 0.005f, 0.3f);
            isaret.GetComponent<Renderer>().sharedMaterial = iz;
            return KalipKaydet(isaret, KayipIziKalibi);
        }

        internal static GameObject KalipKaydet(GameObject nesne, string yol)
        {
            GameObject kalip = PrefabUtility.SaveAsPrefabAsset(nesne, yol);
            Object.DestroyImmediate(nesne);
            return kalip;
        }

        /// <summary>Gece ve gündüz ışığı: sahnenin yönlü ışığı ve bütün kameralar OrtamIsigi'ne bağlanır.</summary>
        private static void OrtamKur(Scene sahne)
        {
            var ortam = new GameObject("OrtamIsigi").AddComponent<OrtamIsigi>();
            Bagla(ortam, "ayarlar", Ayar<OrtamAyarlari>("OrtamAyarlari"));
            Bagla(ortam, "gokyuzu", GokyuzuKur());
            foreach (GameObject kok in sahne.GetRootGameObjects())
                if (kok.TryGetComponent(out Light isik) && isik.type == LightType.Directional)
                {
                    Bagla(ortam, "isik", isik);
                    isik.transform.rotation = Quaternion.Euler(35f, 160f, 0f);
                }

            var kameralar = new System.Collections.Generic.List<UnityEngine.Camera>();
            foreach (GameObject kok in sahne.GetRootGameObjects())
                kameralar.AddRange(kok.GetComponentsInChildren<UnityEngine.Camera>(true));
            var so = new SerializedObject(ortam);
            SerializedProperty dizi = so.FindProperty("kameralar");
            dizi.arraySize = kameralar.Count;
            for (int i = 0; i < kameralar.Count; i++) dizi.GetArrayElementAtIndex(i).objectReferenceValue = kameralar[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Bölüm başı ara sahne kamerası: verilen yerden verilen noktaya bakar, oyuncunun gözüne süzülür.</summary>
        internal static void AraSahneKur(string ad, Vector3 yer, Vector3 bakis, int sira, params AdimTuru[] turler)
        {
            var nesne = new GameObject(ad);
            nesne.transform.SetPositionAndRotation(yer, Quaternion.LookRotation(bakis - yer));
            var kamera = nesne.AddComponent<UnityEngine.Camera>();
            kamera.enabled = false;
            var ara = nesne.AddComponent<AraSahne>();
            var so = new SerializedObject(ara);
            so.FindProperty("sira").intValue = sira;
            SerializedProperty dizi = so.FindProperty("turler");
            dizi.arraySize = turler.Length;
            for (int i = 0; i < turler.Length; i++) dizi.GetArrayElementAtIndex(i).enumValueIndex = (int)turler[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Gece gökyüzü: yıldızlar ve ay (sissiz shader); OrtamIsigi gece açar.</summary>
        private static GameObject GokyuzuKur()
        {
            var gok = new GameObject("GeceGokyuzu", typeof(MeshFilter), typeof(MeshRenderer));
            gok.GetComponent<MeshRenderer>().sharedMaterial = Malzeme("Yildizlar", Color.white, "IkiDunya/Gokyuzu");
            gok.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            GameObject ay = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ay.name = "Ay";
            Object.DestroyImmediate(ay.GetComponent<Collider>());
            ay.transform.SetParent(gok.transform, false);
            ay.transform.localScale = Vector3.one * 10f;
            var ayGorunus = ay.GetComponent<Renderer>();
            ayGorunus.sharedMaterial = Malzeme("Ay", new Color(1f, 0.97f, 0.85f, 1f), "IkiDunya/Gokyuzu");
            ayGorunus.shadowCastingMode = ShadowCastingMode.Off;
            var gokyuzu = gok.AddComponent<GeceGokyuzu>();
            Bagla(gokyuzu, "ay", ay.transform);
            // Koyu bulut alfa karışımı ister (gökyüzü shader'ı eklemeli); gece sisi de koyu, bulutu açmaz.
            Bagla(gokyuzu, "bulutMalzemesi", SaydamMalzeme("Bulut", new Color(0.06f, 0.07f, 0.1f, 0.9f)));
            return gok;
        }

        /// <summary>Kumda yüzgeç izi bırakır (boy kaplumbağaya göre).</summary>
        private static void IzEkle(GameObject kaplumbaga, Vector2 boy, float aralik, float yanAcikligi, int enFazla)
        {
            var iz = kaplumbaga.AddComponent<IzBirakici>();
            Bagla(iz, "malzeme", SaydamMalzeme("KumIzi", new Color(0.32f, 0.25f, 0.16f, 0.5f)));
            var so = new SerializedObject(iz);
            so.FindProperty("boy").vector2Value = boy;
            so.FindProperty("aralik").floatValue = aralik;
            so.FindProperty("yanAcikligi").floatValue = yanAcikligi;
            so.FindProperty("enFazla").intValue = enFazla;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Dalgalanan su yüzeyi (çarpışmasız, Water layer).</summary>
        internal static GameObject SuYuzeyiKur(string ad, Transform ebeveyn, Vector3 konum, Vector2 boyut, Material malzeme)
        {
            var su = new GameObject(ad, typeof(MeshFilter), typeof(MeshRenderer));
            su.layer = LayerMask.NameToLayer("Water");
            su.transform.SetParent(ebeveyn, false);
            su.transform.localPosition = konum;
            su.GetComponent<MeshRenderer>().sharedMaterial = malzeme;
            var dalga = su.AddComponent<SuDalgalanmasi>();
            var so = new SerializedObject(dalga);
            so.FindProperty("boyut").vector2Value = boyut;
            so.ApplyModifiedPropertiesWithoutUndo();
            return su;
        }

        // ---- Küçük yardımcılar ----

        /// <summary>Kaplumbağanın görünen gövdesi: "Kabuk" (ilk çocuk; yavru ters dönünce bu çevrilir). Model verilir ve bulunursa Yusuf'un modeli (boyu gövdenin uzunluğuna göre), yoksa gri küre.</summary>
        internal static Transform Govde(Transform ebeveyn, Vector3 konum, Vector3 olcek, string model = null)
        {
            var kabuk = new GameObject("Kabuk").transform;
            kabuk.gameObject.layer = ebeveyn.gameObject.layer;
            kabuk.SetParent(ebeveyn, false);
            kabuk.SetSiblingIndex(0);
            kabuk.localPosition = konum;
            if (model != null)
            {
                GameObject gorunus = ModelYerlestir(kabuk, model, new Vector3(0f, -olcek.y / 2f, 0f), Mathf.Max(olcek.x, olcek.z));
                if (gorunus != null) return gorunus.transform;
            }

            GameObject kure = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            kure.name = "Gri";
            Object.DestroyImmediate(kure.GetComponent<Collider>());
            kure.layer = ebeveyn.gameObject.layer;
            kure.transform.SetParent(kabuk, false);
            kure.transform.localScale = olcek;
            kure.GetComponent<Renderer>().sharedMaterial = kaplumbaga;
            return kure.transform;
        }

        /// <summary>Köke kodla animasyon ekler: kök hareket ettikçe görünüş türüne göre sallanır (kazma verilirse kazarken eğilir).</summary>
        internal static void SalinimEkle(GameObject kok, Transform gorunus, SalinimTuru tur, AnneKazma kazma = null)
        {
            if (gorunus == null) return;
            var salinim = kok.AddComponent<Salinim>();
            Bagla(salinim, "ayarlar", Ayar<AnimasyonAyarlari>("AnimasyonAyarlari"));
            Bagla(salinim, "gorunus", gorunus);
            if (kazma != null) Bagla(salinim, "kazma", kazma);
            var so = new SerializedObject(salinim);
            so.FindProperty("tur").enumValueIndex = (int)tur;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Kaplumbağanın gözü: kamera, bakış, algı (yanılgılar). İçgüdü: yavru gece en parlak aya döner (kaçış gecesi).</summary>
        internal static BirinciSahisKamera KameraKur(Transform ebeveyn, Vector3 konum, Transform govde, bool icgudu = false)
        {
            var goz = new GameObject("Goz");
            goz.transform.SetParent(ebeveyn, false);
            goz.transform.localPosition = konum;
            goz.tag = "MainCamera";
            goz.AddComponent<UnityEngine.Camera>().nearClipPlane = 0.03f;
            goz.AddComponent<AudioListener>();
            goz.AddComponent<TekKulak>();
            var bakis = goz.AddComponent<BirinciSahisKamera>();
            Bagla(bakis, "ayarlar", Ayar<KameraAyarlari>("KameraAyarlari"));
            Bagla(bakis, "geribildirim", Ayar<GeribildirimAyarlari>("GeribildirimAyarlari"));
            if (govde != null) Bagla(bakis, "govde", govde);
            var algi = goz.AddComponent<KaplumbagaAlgisi>();
            Bagla(algi, "ayarlar", Ayar<AlgiAyarlari>("AlgiAyarlari"));
            if (icgudu)
            {
                var so = new SerializedObject(algi);
                so.FindProperty("icgudu").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Kaplumbağanın görüşü: uzak bulanık ve soluk, parlak ışık taşar. Sadece bu kamerada (yönetim ekranı etkilenmez);
            // göz kapanınca (başka adım) hacim de kapanır.
            UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(goz.GetComponent<UnityEngine.Camera>()).renderPostProcessing = true;
            var hacim = goz.AddComponent<Volume>();
            hacim.isGlobal = true;
            hacim.sharedProfile = KaplumbagaGorusu();
            return bakis;
        }

        /// <summary>Kaplumbağa görüşünün post-process profili (Settings/KaplumbagaGorusu). Varsa dokunulmaz: sanatçı Inspector'dan ayarlar.</summary>
        private static VolumeProfile KaplumbagaGorusu()
        {
            string yol = $"{AyarKlasoru}/KaplumbagaGorusu.asset";
            var profil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(yol);
            if (profil != null) return profil;

            profil = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profil, yol);

            // Uzak bulanık: belirsiz bir leke ne olduğunu ancak yaklaşınca belli eder.
            var odak = profil.Add<UnityEngine.Rendering.Universal.DepthOfField>(true);
            odak.mode.Override(UnityEngine.Rendering.Universal.DepthOfFieldMode.Gaussian);
            odak.gaussianStart.Override(9f);
            odak.gaussianEnd.Override(40f);
            odak.gaussianMaxRadius.Override(1f);
            odak.highQualitySampling.Override(true);
            // Parlak ışık taşar: sahte ay (HDR) gerçek aydan çekici parlar.
            var parilti = profil.Add<UnityEngine.Rendering.Universal.Bloom>(true);
            parilti.threshold.Override(1f);
            parilti.intensity.Override(0.6f);
            parilti.scatter.Override(0.7f);
            var renk = profil.Add<UnityEngine.Rendering.Universal.ColorAdjustments>(true);
            renk.saturation.Override(-12f);
            var kenar = profil.Add<UnityEngine.Rendering.Universal.Vignette>(true);
            kenar.intensity.Override(0.22f);
            kenar.smoothness.Override(0.5f);

            foreach (VolumeComponent bilesen in profil.components) AssetDatabase.AddObjectToAsset(bilesen, profil);
            EditorUtility.SetDirty(profil);
            AssetDatabase.SaveAssets();
            return profil;
        }

        internal static Transform TuvalKur(string ad, Transform grup)
        {
            // GraphicRaycaster olmadan düğmeler tıklanmaz.
            var tuval = new GameObject(ad, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            tuval.layer = LayerMask.NameToLayer("UI");
            tuval.transform.SetParent(grup, false);
            tuval.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var olcek = tuval.GetComponent<CanvasScaler>();
            olcek.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            olcek.referenceResolution = new Vector2(1920f, 1080f);
            // Oyuncunun yazı boyutu ayarı bütün tuvallere uygulanır.
            tuval.AddComponent<ArayuzOlcegi>();
            return tuval.transform;
        }

        /// <summary>Adımına göre açılıp kapanan nesne grubu (AdimNesneleri).</summary>
        private static Transform Grup(string ad, AdimTuru tur, params ParkurTuru[] parkurlar) => Grup(ad, new[] { tur }, parkurlar);

        private static Transform Grup(string ad, AdimTuru[] adimlar, params ParkurTuru[] parkurlar)
        {
            var grup = new GameObject(ad).AddComponent<AdimNesneleri>();
            var so = new SerializedObject(grup);
            SerializedProperty turler = so.FindProperty("turler");
            turler.arraySize = adimlar.Length;
            for (int i = 0; i < adimlar.Length; i++) turler.GetArrayElementAtIndex(i).enumValueIndex = (int)adimlar[i];
            SerializedProperty parkurDizisi = so.FindProperty("parkurlar");
            parkurDizisi.arraySize = parkurlar.Length;
            for (int i = 0; i < parkurlar.Length; i++) parkurDizisi.GetArrayElementAtIndex(i).enumValueIndex = (int)parkurlar[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return grup.transform;
        }

        internal static Transform Bos(string ad, Transform ebeveyn)
        {
            var nesne = new GameObject(ad).transform;
            nesne.SetParent(ebeveyn, false);
            return nesne;
        }

        /// <summary>Koyu arka plan ve renkli dolgudan bir çubuk; Cubuk bileşeni arka plandadır, dolgu ilk çocuğudur.</summary>
        internal static Cubuk CubukKur(string ad, Transform ebeveyn, Vector2 enAz, Vector2 enCok, Color renk, bool dikey)
        {
            int ui = LayerMask.NameToLayer("UI");
            var arka = new GameObject(ad, typeof(RectTransform), typeof(Image));
            arka.layer = ui;
            arka.transform.SetParent(ebeveyn, false);
            Sabitle((RectTransform)arka.transform, enAz, enCok);
            arka.GetComponent<Image>().color = Tema().Panel;

            var dolgu = new GameObject("Dolgu", typeof(RectTransform), typeof(Image));
            dolgu.layer = ui;
            dolgu.transform.SetParent(arka.transform, false);
            Yasla((RectTransform)dolgu.transform, Vector2.zero, Vector2.one);
            var resim = dolgu.GetComponent<Image>();
            resim.color = renk;

            var cubuk = arka.AddComponent<Cubuk>();
            Bagla(cubuk, "dolgu", dolgu.transform);
            Bagla(cubuk, "dolguResmi", resim);
            var so = new SerializedObject(cubuk);
            so.FindProperty("dikey").boolValue = dikey;
            so.ApplyModifiedPropertiesWithoutUndo();
            return cubuk;
        }

        internal static void Yasla(RectTransform rect, Vector2 enAz, Vector2 enCok)
        {
            rect.anchorMin = enAz;
            rect.anchorMax = enCok;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        /// <summary>Ekran oranıyla verilen kutuyu (1920x1080'de) aynı yerde ama tuval biriminde sabit boyla kurar: en yakın kenara ya da ortaya çapalanır.
        /// Yazı boyutu ayarı tuvali büyütünce kutu da büyür ve o kenardan içeri doğru büyür (ekrandan taşmaz).</summary>
        internal static void Sabitle(RectTransform rect, Vector2 enAz, Vector2 enCok)
        {
            Vector2 referans = new Vector2(1920f, 1080f);
            Vector2 orta = (enAz + enCok) * 0.5f;
            Vector2 capa = new Vector2(Kenari(orta.x), Kenari(orta.y));
            rect.anchorMin = rect.anchorMax = rect.pivot = capa;
            // Pivot çapayla aynı: kutunun çapa tarafındaki kenarı (ortadaysa ortası) referanstaki yerinde.
            Vector2 kenar = new Vector2(Mathf.Lerp(enAz.x, enCok.x, capa.x), Mathf.Lerp(enAz.y, enCok.y, capa.y));
            rect.anchoredPosition = Vector2.Scale(kenar - capa, referans);
            rect.sizeDelta = Vector2.Scale(enCok - enAz, referans);
        }

        private static float Kenari(float orta) => orta < 0.34f ? 0f : orta > 0.66f ? 1f : 0.5f;

        /// <summary>Tek noktaya çapalı, tuval biriminde sabit boylu kutu (pivot çapayla aynı köşede).</summary>
        internal static void Yerles(RectTransform rect, Vector2 capa, Vector2 konum, Vector2 boyut)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = capa;
            rect.anchoredPosition = konum;
            rect.sizeDelta = boyut;
        }

        private const string YaziTipiKlasoru = "Assets/_Project/Art/UI/Fonts";

        /// <summary>Arayüz teması: asset'i yoksa kurar, yazı tiplerini bağlar, oyun ayarlarına bağlar.</summary>
        internal static ArayuzTemasi Tema()
        {
            var tema = Ayar<ArayuzTemasi>("ArayuzTemasi");
            var so = new SerializedObject(tema);
            if (so.FindProperty("baslikYazisi").objectReferenceValue == null)
                so.FindProperty("baslikYazisi").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>($"{YaziTipiKlasoru}/Lora.ttf");
            if (so.FindProperty("govdeYazisi").objectReferenceValue == null)
                so.FindProperty("govdeYazisi").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>($"{YaziTipiKlasoru}/SourceSans3-Regular.ttf");
            if (so.FindProperty("kalinYazi").objectReferenceValue == null)
                so.FindProperty("kalinYazi").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>($"{YaziTipiKlasoru}/SourceSans3-Semibold.ttf");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tema);
            var ayarlar = Ayar<OyunAyarlari>("OyunAyarlari");
            Bagla(ayarlar, "tema", tema);
            EditorUtility.SetDirty(ayarlar);
            return tema;
        }

        /// <summary>Yazıya temanın yazı tipini ve rengini verir (baslik: serif, değilse sans).</summary>
        internal static void TemaYazisi(Text yazi, bool baslik, Color renk)
        {
            ArayuzTemasi tema = Tema();
            yazi.font = baslik ? tema.BaslikYazisi : tema.GovdeYazisi;
            yazi.color = renk;
        }

        internal static GameObject Kutu(string ad, Transform ebeveyn, Vector3 konum, Vector3 boyut, Material malzeme, string katman)
        {
            GameObject kutu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kutu.name = ad;
            kutu.transform.SetParent(ebeveyn, false);
            kutu.transform.localPosition = konum;
            kutu.transform.localScale = boyut;
            kutu.layer = LayerMask.NameToLayer(katman);
            kutu.isStatic = true;
            kutu.GetComponent<Renderer>().sharedMaterial = malzeme;
            return kutu;
        }

        internal static void Klasor(string ust, string ad)
        {
            if (!AssetDatabase.IsValidFolder($"{ust}/{ad}")) AssetDatabase.CreateFolder(ust, ad);
        }

        internal static Material Malzeme(string ad, Color renk, string shader = "Universal Render Pipeline/Lit")
        {
            string yol = $"{MalzemeKlasoru}/{ad}.mat";
            var malzeme = AssetDatabase.LoadAssetAtPath<Material>(yol);
            if (malzeme == null)
            {
                malzeme = new Material(Shader.Find(shader));
                AssetDatabase.CreateAsset(malzeme, yol);
            }
            malzeme.SetColor("_BaseColor", renk);
            EditorUtility.SetDirty(malzeme);
            return malzeme;
        }

        internal static Material SaydamMalzeme(string ad, Color renk)
        {
            Material malzeme = Malzeme(ad, renk, "Universal Render Pipeline/Unlit");
            malzeme.SetFloat("_Surface", 1f);
            malzeme.SetFloat("_Blend", 0f);
            malzeme.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            malzeme.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            malzeme.SetFloat("_ZWrite", 0f);
            malzeme.SetOverrideTag("RenderType", "Transparent");
            malzeme.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            malzeme.renderQueue = (int)RenderQueue.Transparent;
            return malzeme;
        }

        /// <summary>Ayar asset'ini Settings klasöründe (önce adıyla) bulur; yoksa varsayılan değerlerle oluşturur.</summary>
        internal static T Ayar<T>(string ad) where T : ScriptableObject
        {
            // Aynı türden birden çok asset olabilir (ör. anne ve yavru deniz ayarları): önce adıyla bakılır.
            var adiyla = AssetDatabase.LoadAssetAtPath<T>($"{AyarKlasoru}/{ad}.asset");
            if (adiyla != null) return adiyla;

            string[] bulunan = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { AyarKlasoru });
            if (bulunan.Length > 0) return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(bulunan[0]));

            var ayar = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(ayar, $"{AyarKlasoru}/{ad}.asset");
            return ayar;
        }

        internal static void Bagla(Object hedef, string alan, Object deger)
        {
            var so = new SerializedObject(hedef);
            so.FindProperty(alan).objectReferenceValue = deger;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void DiziBagla(Object hedef, string alan, params Object[] degerler)
        {
            var so = new SerializedObject(hedef);
            SerializedProperty dizi = so.FindProperty(alan);
            dizi.arraySize = degerler.Length;
            for (int i = 0; i < degerler.Length; i++) dizi.GetArrayElementAtIndex(i).objectReferenceValue = degerler[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
