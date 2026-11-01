using System;
using IkiDunya.Camera;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using IkiDunya.UI.Night;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace IkiDunya.Editor
{
    /// <summary>Deniz bölümlerinin gri kutu sahnelerini kurar: Bölüm 1 (anne kıyıya döner) ve Bölüm 5 (yavru yosun adasına yüzer).</summary>
    public static class GriDenizKur
    {
        private const string Bolum1Sahnesi = "Assets/_Project/Scenes/Levels/Bolum1_Donus.unity";
        private const string Bolum5Sahnesi = "Assets/_Project/Scenes/Levels/Bolum5_IlkYuzus.unity";
        private const string AyarKlasoru = "Assets/_Project/ScriptableObjects/Settings";
        private const string KalipKlasoru = "Assets/_Project/Prefabs/Hazards";
        private const string SistemlerKalibi = "Assets/_Project/Prefabs/Sistemler.prefab";
        private const string DenizanasiDenetleyicisi = "Assets/Sandbox/Denizanasi/Denizanasi.controller";
        private const string PosetSurusuSahnesi = "Assets/_Project/Scenes/Levels/Parkur_PosetSurusu.unity";
        private const string CopGirdabiSahnesi = "Assets/_Project/Scenes/Levels/Parkur_CopGirdabi.unity";

        // Gri kutu ölçüleri (metre). Kıyı z = 0, deniz -z yönünde; su yüzeyi y = 0.
        private const float DenizUzunlugu = 200f;
        private const float DenizGenisligi = 60f;

        private static Material su, taban, kaya, cayir, ag, misina, denizanasi, poset, tekne, avci, sicakSu, akinti, yosun, isaret, plastik, dalga, agKenari, tekneGolgesi, kopuk, girdapCopu;

        [MenuItem("İki Dünya/Bölüm 1 sahnesini kur (deniz)")]
        public static void Bolum1Kur()
        {
            if (!GriKumsalKur.SahneAc(Bolum1Sahnesi, out Scene sahne)) return;
            Hazirla();
            var tehlike = GriKumsalKur.Ayar<DenizTehlikeAyarlari>("DenizTehlikeAyarlari");
            var ayarlar = AyarAdla<DenizAyarlari>("AnneDenizAyarlari", null);

            Quaternion kiyiya = Quaternion.identity;
            DenizHareketi anne = YuzucuKur("Anne", ayarlar, tehlike, new Vector3(0.9f, 0.35f, 1.1f), 0.45f, new Vector3(0f, 0.15f, 0.5f), "Anne_Caretta");
            Transform baslangic = Nokta("Baslangic", new Vector3(0f, -3f, -195f), kiyiya);

            Kontrol("KontrolNoktasi_1", -120f, kiyiya, false);
            Kontrol("KontrolNoktasi_2", -50f, kiyiya, false);
            // Akıntı kapısı: anne kapılır, poşet sürüsü parkuru gelir (Bölüm 1 burada biter).
            Kontrol("AkintiKapisi", -30f, kiyiya, false, true);

            // Akıntılar: biri kıyıya doğru (yardım eder), biri tersine, biri yana
            AkintiKur(new Vector3(-15f, -6f, -160f), new Vector3(20f, 12f, 40f), 0f, tehlike);
            AkintiKur(new Vector3(15f, -6f, -160f), new Vector3(20f, 12f, 40f), 180f, tehlike);
            AkintiKur(new Vector3(0f, -3f, -85f), new Vector3(30f, 6f, 20f), 90f, tehlike);

            // Besin: denizanası ve ona benzeyen poşet
            Vector3[] denizanalari = { new Vector3(-8f, -4f, -170f), new Vector3(12f, -5f, -150f), new Vector3(6f, -2f, -140f), new Vector3(-10f, -3f, -100f), new Vector3(8f, -3f, -70f) };
            Vector3[] posetler = { new Vector3(-4f, -3f, -155f), new Vector3(3f, -4f, -130f), new Vector3(10f, -2f, -110f), new Vector3(-12f, -2f, -60f) };
            foreach (Vector3 yer in denizanalari) YiyecekKur(Yiyecek.Turu.Denizanasi, yer, tehlike);
            foreach (Vector3 yer in posetler) YiyecekKur(Yiyecek.Turu.Poset, yer, tehlike);

            Object.Instantiate(HayaletAgKalibi(), new Vector3(5f, -6f, -135f), Quaternion.Euler(0f, 30f, 0f));
            Object.Instantiate(HayaletAgKalibi(), new Vector3(-8f, -3f, -75f), Quaternion.Euler(0f, -20f, 0f));
            TekneKur("Tekne_0", -140f, false, tehlike, null);
            TekneKur("Tekne_1", -80f, false, tehlike, null);
            SicakSuKur(new Vector3(-12f, -5f, -125f), 5f, tehlike);
            SicakSuKur(new Vector3(10f, -2f, -40f), 4f, tehlike);

            // Süs: oyun şeridinin kenarlarında kayalar ve deniz çayırı (Yusuf'un deniz tabanı modelleri)
            SuslerKur(new[] { new Vector3(-20f, 0f, -178f), new Vector3(22f, 0f, -150f), new Vector3(-24f, 0f, -100f), new Vector3(18f, 0f, -82f), new Vector3(-14f, 0f, -66f) },
                new[] { new Vector3(14f, 0f, -104f), new Vector3(-18f, 0f, -86f), new Vector3(22f, 0f, -62f), new Vector3(-22f, 0f, -170f) });

            GriKumsalKur.AraSahneKur("AraSahne_Bolum1", new Vector3(0f, 4f, -215f), new Vector3(0f, -3f, -190f), -1, AdimTuru.Donus);
            BolumKur(AdimTuru.Donus, ayarlar, anne, baslangic, false, null);
            GriKumsalKur.Kaydet(sahne, Bolum1Sahnesi);
        }

        [MenuItem("İki Dünya/Bölüm 5 sahnesini kur (deniz)")]
        public static void Bolum5Kur()
        {
            if (!GriKumsalKur.SahneAc(Bolum5Sahnesi, out Scene sahne)) return;
            Hazirla();
            var tehlike = GriKumsalKur.Ayar<DenizTehlikeAyarlari>("DenizTehlikeAyarlari");
            var kumsal = GriKumsalKur.Ayar<KumsalDurumu>("KumsalDurumu");
            DenizAyarlari ayarlar = YavruDenizAyarlari();

            Quaternion denize = Quaternion.Euler(0f, 180f, 0f);
            DenizHareketi yavru = YuzucuKur("Yavru", ayarlar, tehlike, new Vector3(0.16f, 0.06f, 0.2f), 0.12f, new Vector3(0f, 0.05f, 0.15f), "Yavru_Caretta");
            Transform baslangic = Nokta("Baslangic", new Vector3(0f, -0.5f, -3f), denize);

            // Dalga hattı: kıyıya gelen dalgaların altından dalınır
            var dalgaKok = new GameObject("DalgaHatti").AddComponent<DenizDalgasi>();
            GameObject tepe = GriKumsalKur.Kutu("Tepe", dalgaKok.transform, Vector3.zero, new Vector3(DenizGenisligi, 0.8f, 1.5f), dalga, "Water");
            Object.DestroyImmediate(tepe.GetComponent<Collider>());
            tepe.isStatic = false;
            GriKumsalKur.Bagla(dalgaKok, "ayarlar", tehlike);
            GriKumsalKur.Bagla(dalgaKok, "tepe", tepe.transform);

            Kontrol("KontrolNoktasi_1", -30f, denize, false);
            Kontrol("GirdapKapisi", -110f, denize, false, true);
            GameObject ada = GriKumsalKur.Kutu("YosunAdasi", null, new Vector3(0f, 0f, -195f), new Vector3(12f, 0.6f, 12f), yosun, "Default");
            Object.DestroyImmediate(ada.GetComponent<Collider>());
            // Yusuf'un yosun adası varsa gri kutunun yerine geçer (12 m). Kökü su çizgisinde kalır (tabana oturtulmaz).
            if (GriKumsalKur.ModelYerlestir(ada.transform, "Yosun_Adasi", Vector3.zero, 12f, false) != null)
                ada.GetComponent<Renderer>().enabled = false;
            Kontrol("YosunAdasi_Son", -195f, denize, true);

            // Sığ sular: kayalar ve deniz çayırı (avcı balıktan saklanılır)
            Vector3[] kayalar = { new Vector3(-8f, -5f, -50f), new Vector3(9f, -4.5f, -65f), new Vector3(-3f, -5f, -85f), new Vector3(12f, -5f, -100f) };
            for (int i = 0; i < kayalar.Length; i++)
            {
                // Yusuf'un kayası (pivot tabanın ortası, ~3 m, collider'lı); yoksa gri küp.
                if (GriKumsalKur.KalibiKoy($"Kaya_{i % 3 + 1}", null, kayalar[i] + Vector3.down * 1.5f) == null)
                    GriKumsalKur.Kutu("Kaya", null, kayalar[i], new Vector3(3f, 3f, 3f), kaya, "Ground");
            }
            CayirKur(new Vector3(6f, -5f, -60f), new Vector3(6f, 2.5f, 10f));
            CayirKur(new Vector3(-10f, -5f, -80f), new Vector3(6f, 2.5f, 8f));
            CayirKur(new Vector3(4f, -5f, -95f), new Vector3(8f, 2.5f, 8f));
            AvciKur(new Vector3(0f, -3f, -60f), tehlike);
            AvciKur(new Vector3(-5f, -4f, -90f), tehlike);

            // Açık deniz: hayalet ağ ve misina (Hayalet Ağ Temizliği azaltır), parlayan plastik, ışıklı tekne
            var yapilar = new GameObject("DenizYapilari").AddComponent<DenizYapilari>();
            GriKumsalKur.Bagla(yapilar, "ayarlar", tehlike);
            GriKumsalKur.Bagla(yapilar, "kumsal", kumsal);
            GriKumsalKur.Bagla(yapilar, "agKalibi", HayaletAgKalibi());
            GriKumsalKur.Bagla(yapilar, "misinaKalibi", MisinaKalibi());
            Vector3[] agYerleri = { new Vector3(-6f, -3f, -45f), new Vector3(8f, -4f, -70f), new Vector3(-4f, -8f, -125f), new Vector3(10f, -6f, -140f), new Vector3(-8f, -5f, -160f), new Vector3(5f, -3f, -180f) };
            var so = new SerializedObject(yapilar);
            SerializedProperty noktalar = so.FindProperty("noktalar");
            noktalar.arraySize = agYerleri.Length;
            for (int i = 0; i < agYerleri.Length; i++)
                noktalar.GetArrayElementAtIndex(i).objectReferenceValue = Nokta($"AgYeri_{i}", agYerleri[i], Quaternion.Euler(0f, i * 37f, 0f), yapilar.transform);
            so.ApplyModifiedPropertiesWithoutUndo();

            PlastikKur(new Vector3(10f, -3f, -130f), tehlike, kumsal);
            PlastikKur(new Vector3(-12f, -2f, -150f), tehlike, kumsal);
            PlastikKur(new Vector3(4f, -4f, -170f), tehlike, kumsal);
            TekneKur("IsikliTekne", -150f, true, tehlike, kumsal);

            // Girdaba kapılma: Bölüm 5'in ilk parçası burada biter, çöp girdabı parkuru gelir; sonra açık denizden devam.
            Transform devam = Nokta("Devam", new Vector3(0f, -4f, -118f), denize);

            // Ara sahne sadece ilk parçada: kumsaldan denize, dalgalara bakarak.
            GriKumsalKur.AraSahneKur("AraSahne_Bolum5", new Vector3(0f, 4f, 10f), new Vector3(0f, -0.5f, -10f), 0, AdimTuru.IlkYuzus);
            BolumKur(AdimTuru.IlkYuzus, ayarlar, yavru, baslangic, true, devam);
            GriKumsalKur.Kaydet(sahne, Bolum5Sahnesi);
        }

        // ---- Parkurlar (minigame sahneleri) ----

        /// <summary>Poşet sürüsü parkuru: anne kıyıya doğru 3 aşamalı bir akıntı koridorundan geçer (sakin, yoğun ve misinalı, karanlık). Aşama sonlarında akıntı duvarı.</summary>
        [MenuItem("İki Dünya/Parkur: Poşet sürüsü sahnesini kur")]
        public static void PosetSurusuKur()
        {
            if (!GriKumsalKur.SahneAc(PosetSurusuSahnesi, out Scene sahne)) return;
            Hazirla();
            var tehlike = GriKumsalKur.Ayar<DenizTehlikeAyarlari>("DenizTehlikeAyarlari");
            var parkur = GriKumsalKur.Ayar<ParkurAyarlari>("ParkurAyarlari");
            var ayarlar = AyarAdla<DenizAyarlari>("AnneDenizAyarlari", null);
            DenizHareketi anne = YuzucuKur("Anne", ayarlar, tehlike, new Vector3(0.9f, 0.35f, 1.1f), 0.45f, new Vector3(0f, 0.15f, 0.5f), "Anne_Caretta");

            // Aşamalar kıyıya doğru (+z): açık deniz, sığ sular, kumsalın önü. Her birinin alanı, başlangıcı, sonunda duvar.
            float[] asamaOrtasi = { -167f, -117f, -67f };
            float[] asamaYuksekligi = { -7f, -3.5f, -3f };
            float[] asamaKalinligi = { 10f, 4f, 3.5f };
            int sayi = asamaOrtasi.Length;
            var alanlar = new Object[sayi];
            var baslangiclar = new Object[sayi];
            var duvarlar = new Object[sayi];
            for (int i = 0; i < sayi; i++)
            {
                GameObject alan = GriKumsalKur.Kutu($"Asama_{i + 1}", null, new Vector3(0f, asamaYuksekligi[i], asamaOrtasi[i]), Vector3.one, akinti, "Default");
                Object.DestroyImmediate(alan.GetComponent<MeshRenderer>());
                alan.isStatic = false;
                var kutu = (BoxCollider)alan.GetComponent<Collider>();
                kutu.isTrigger = true;
                kutu.size = new Vector3(26f, asamaKalinligi[i], 44f);
                alanlar[i] = kutu;
                baslangiclar[i] = Nokta($"Asama_{i + 1}_Baslangic", new Vector3(0f, asamaYuksekligi[i], asamaOrtasi[i] - 24f), Quaternion.identity);

                // Akıntı duvarı: hedef olunca açılır; görünür perde ve geçilmez collider.
                GameObject duvar = GriKumsalKur.Kutu($"AkintiDuvari_{i + 1}", null, new Vector3(0f, -8f, asamaOrtasi[i] + 24.5f), new Vector3(DenizGenisligi, 20f, 1f), akinti, "Obstacle");
                duvar.isStatic = false;
                duvarlar[i] = duvar;
            }

            // Yoğun aşamada misinalar, karanlık aşamada hayalet ağ
            Vector3[] misinalar = { new Vector3(-6f, -3f, -125f), new Vector3(7f, -4f, -112f), new Vector3(0f, -2.5f, -100f) };
            foreach (Vector3 yer in misinalar) Object.Instantiate(MisinaKalibi(), yer, Quaternion.Euler(0f, 90f, 0f));
            Object.Instantiate(HayaletAgKalibi(), new Vector3(-8f, -3f, -70f), Quaternion.Euler(0f, 20f, 0f));
            Object.Instantiate(HayaletAgKalibi(), new Vector3(9f, -3f, -58f), Quaternion.Euler(0f, -30f, 0f));

            // Koridorun kenarlarında kayalar ve çayır (sürünün aktığı alanın dışında)
            SuslerKur(new[] { new Vector3(-20f, 0f, -175f), new Vector3(21f, 0f, -150f), new Vector3(-19f, 0f, -120f), new Vector3(20f, 0f, -96f), new Vector3(-21f, 0f, -70f), new Vector3(19f, 0f, -55f) },
                new[] { new Vector3(20f, 0f, -168f), new Vector3(-20f, 0f, -135f), new Vector3(22f, 0f, -110f), new Vector3(-19f, 0f, -85f), new Vector3(20f, 0f, -70f) });

            var kok = new GameObject("PosetSurusu");
            var surusu = kok.AddComponent<PosetSurusu>();
            GriKumsalKur.Bagla(surusu, "ayarlar", parkur);
            GriKumsalKur.Bagla(surusu, "yuzucu", anne);
            GriKumsalKur.Bagla(surusu, "denizanasiKalibi", YiyecekKalibi(Yiyecek.Turu.Denizanasi, tehlike, null));
            // Poşet ışıktan etkilenir: karanlık aşamada görünmez olur (denizanası ışıksız malzemeyle parlar).
            GriKumsalKur.Bagla(surusu, "posetKalibi", YiyecekKalibi(Yiyecek.Turu.Poset, tehlike,
                GriKumsalKur.Malzeme("PosetParkur", new Color(0.9f, 0.9f, 0.88f))));
            GriKumsalKur.DiziBagla(surusu, "alanlar", alanlar);
            GriKumsalKur.DiziBagla(surusu, "baslangiclar", baslangiclar);
            GriKumsalKur.DiziBagla(surusu, "duvarlar", duvarlar);
            GriKumsalKur.Bagla(surusu, "bitis", Nokta("Bitis", new Vector3(0f, -3f, -38f), Quaternion.identity));
            GriKumsalKur.Bagla(surusu, "gunes", GunesBul(sahne));

            anne.transform.SetPositionAndRotation(((Transform)baslangiclar[0]).position, Quaternion.identity);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));
            GriKumsalKur.Kaydet(sahne, PosetSurusuSahnesi);
        }

        /// <summary>Çöp girdabı parkuru: yavru girdabın merkezinde; iç içe 3 dönen çöp halkası, her birinde bir boşluk (parlak şamandıralarla işaretli), halkalarda çarpan ağır çöpler.</summary>
        [MenuItem("İki Dünya/Parkur: Çöp girdabı sahnesini kur")]
        public static void CopGirdabiKur()
        {
            if (!GriKumsalKur.SahneAc(CopGirdabiSahnesi, out Scene sahne)) return;
            Hazirla();
            var tehlike = GriKumsalKur.Ayar<DenizTehlikeAyarlari>("DenizTehlikeAyarlari");
            var parkur = GriKumsalKur.Ayar<ParkurAyarlari>("ParkurAyarlari");
            DenizHareketi yavru = YuzucuKur("Yavru", YavruDenizAyarlari(), tehlike, new Vector3(0.16f, 0.06f, 0.2f), 0.12f, new Vector3(0f, 0.05f, 0.15f), "Yavru_Caretta");

            var merkezNoktasi = new Vector3(0f, -6f, -150f);
            Transform merkez = Nokta("Merkez", merkezNoktasi, Quaternion.identity);
            GameObject yigin = GriKumsalKur.Kutu("ParlayanYigin", null, merkezNoktasi + Vector3.down * 2.5f, new Vector3(2.5f, 1.5f, 2.5f), plastik, "Default");
            Object.DestroyImmediate(yigin.GetComponent<Collider>());
            // Yusuf'un plastik yığını varsa gri kutunun yerine geçer (2,5 m). Kökü ortada (tabana oturtulmaz).
            if (GriKumsalKur.ModelYerlestir(yigin.transform, "Plastik_Yigini", Vector3.zero, 2.5f, false) != null)
                yigin.GetComponent<Renderer>().enabled = false;

            var halkalar = new Object[parkur.HalkaSayisi];
            for (int h = 0; h < parkur.HalkaSayisi; h++)
                halkalar[h] = HalkaKur(h, merkezNoktasi, parkur);

            // Girdabın çevresinde, en dış halkanın ötesinde kayalar ve çayır
            SuslerKur(new[] { new Vector3(19f, 0f, -138f), new Vector3(-20f, 0f, -160f), new Vector3(6f, 0f, -172f), new Vector3(-12f, 0f, -132f), new Vector3(22f, 0f, -165f) },
                new[] { new Vector3(-17f, 0f, -140f), new Vector3(16f, 0f, -170f), new Vector3(-8f, 0f, -173f), new Vector3(20f, 0f, -150f) });

            var kok = new GameObject("CopGirdabi");
            var girdap = kok.AddComponent<CopGirdabi>();
            GriKumsalKur.Bagla(girdap, "ayarlar", parkur);
            GriKumsalKur.Bagla(girdap, "yuzucu", yavru);
            GriKumsalKur.Bagla(girdap, "merkez", merkez);
            GriKumsalKur.DiziBagla(girdap, "halkalar", halkalar);

            yavru.transform.SetPositionAndRotation(merkezNoktasi, Quaternion.identity);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));
            GriKumsalKur.Kaydet(sahne, CopGirdabiSahnesi);
        }

        /// <summary>Bir çöp halkası: boşluk ileri yönde (açı 0); çevresinde dikey bir çöp duvarı, oyuncunun yüksekliğinde çarpan ağır çöpler, boşluğun iki yanında parlak şamandıralar.</summary>
        private static Transform HalkaKur(int h, Vector3 merkez, ParkurAyarlari parkur)
        {
            Transform halka = GriKumsalKur.Bos($"Halka_{h + 1}", null);
            halka.position = merkez;
            // Her halka farklı açıda başlasın.
            halka.rotation = Quaternion.Euler(0f, h * 137f, 0f);
            float yaricap = parkur.HalkaYaricapi(h);
            float yarimBosluk = parkur.CikisAcisi / 2f;
            float adim = Mathf.Max(6f, 360f / (yaricap * 2.2f));
            string[] hafifler = { "Poset", "Sise", "Samandira" };
            int k = 0;
            for (float aci = yarimBosluk + adim / 2f; aci < 360f - yarimBosluk; aci += adim, k++)
                for (int kat = -2; kat <= 2; kat++)
                {
                    if (kat == 0) continue;
                    Vector3 yer = Quaternion.Euler(0f, aci, 0f) * Vector3.forward * yaricap + Vector3.up * kat * 2.2f;
                    Transform parca = GriKumsalKur.Bos($"Cop_{k}_{kat}", halka);
                    parca.localPosition = yer;
                    parca.localRotation = Quaternion.Euler(aci * 3f, aci, kat * 40f);
                    if (GriKumsalKur.ModelYerlestir(parca, hafifler[(k + kat + 2) % hafifler.Length], Vector3.zero, 0.8f) != null) continue;
                    GameObject kutu = GriKumsalKur.Kutu("Gri", parca, Vector3.zero, new Vector3(0.8f, 0.8f, 0.3f), girdapCopu, "Default");
                    Object.DestroyImmediate(kutu.GetComponent<Collider>());
                    kutu.isStatic = false;
                }

            // Ağır çöpler: oyuncunun yüksekliğinde, dönerken çarpar.
            int agir = parkur.HalkaCopSayisi(h);
            for (int i = 0; i < agir; i++)
            {
                float aci = yarimBosluk + 10f + (360f - 2f * yarimBosluk - 20f) * (i + 0.5f) / agir;
                Transform parca = GriKumsalKur.Bos($"AgirCop_{i}", halka);
                parca.localPosition = Quaternion.Euler(0f, aci, 0f) * Vector3.forward * yaricap;
                if (GriKumsalKur.ModelYerlestir(parca, "Kutu", Vector3.down * 0.6f, 1.4f) != null) continue;
                GameObject varil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                varil.name = "Varil";
                Object.DestroyImmediate(varil.GetComponent<Collider>());
                varil.transform.SetParent(parca, false);
                varil.transform.localScale = new Vector3(0.9f, 0.7f, 0.9f);
                varil.GetComponent<Renderer>().sharedMaterial = tekne;
            }

            // Boşluğun iki yanında, her yükseklikte parlak şamandıralar: boşluk uzaktan görünsün.
            foreach (float kenar in new[] { -yarimBosluk, yarimBosluk })
                for (int kat = -2; kat <= 2; kat++)
                {
                    GameObject isik = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    isik.name = "BoslukIsareti";
                    Object.DestroyImmediate(isik.GetComponent<Collider>());
                    isik.transform.SetParent(halka, false);
                    isik.transform.localPosition = Quaternion.Euler(0f, kenar, 0f) * Vector3.forward * yaricap + Vector3.up * kat * 2.2f;
                    isik.transform.localScale = Vector3.one * 0.5f;
                    isik.GetComponent<Renderer>().sharedMaterial = isaret;
                }
            return halka;
        }

        /// <summary>Deniz tabanının üstüne süs kayaları (collider'lı) ve deniz çayırı (görüntü) koyar; yükseklik tabandan okunur. Model yoksa hiçbir şey koymaz.</summary>
        private static void SuslerKur(Vector3[] kayalar, Vector3[] cayirlar)
        {
            Physics.SyncTransforms();
            Transform kok = GriKumsalKur.Bos("Susler", null);
            string[] cayirAdlari = { "DenizCayiri_8x8", "DenizCayiri_6x10", "DenizCayiri_6x8" };
            for (int i = 0; i < kayalar.Length; i++)
            {
                GameObject kaya = GriKumsalKur.KalibiKoy($"Kaya_{i % 3 + 1}", kok, Tabana(kayalar[i]));
                GriKumsalKur.UzaktaGizle(kaya, 0.01f);
                if (kaya != null) kaya.transform.rotation *= Quaternion.Euler(0f, i * 71f, 0f);
            }
            for (int i = 0; i < cayirlar.Length; i++)
                GriKumsalKur.UzaktaGizle(GriKumsalKur.KalibiKoy(cayirAdlari[i % cayirAdlari.Length], kok, Tabana(cayirlar[i])), 0.02f, true);
        }

        /// <summary>Noktayı deniz tabanına indirir (taban yoksa açık deniz derinliği).</summary>
        private static Vector3 Tabana(Vector3 yer) => new Vector3(yer.x, Zemin.Yukseklik(new Vector3(yer.x, 0f, yer.z), -15f), yer.z);

        private static Light GunesBul(Scene sahne)
        {
            foreach (GameObject kok in sahne.GetRootGameObjects())
                if (kok.TryGetComponent(out Light isik) && isik.type == LightType.Directional) return isik;
            return null;
        }

        private static Yiyecek YiyecekKalibi(Yiyecek.Turu tur, DenizTehlikeAyarlari tehlike, Material malzeme)
        {
            GameObject nesne = YiyecekKur(tur, Vector3.zero, tehlike);
            // Gri küreyse karanlık aşama için ışıklı malzeme; model varsa kendi malzemesi kalır.
            Transform gri = nesne.transform.Find("Gri");
            if (malzeme != null && gri != null) gri.GetComponent<Renderer>().sharedMaterial = malzeme;
            string ad = malzeme != null ? $"{tur}Parkur" : tur.ToString();
            return GriKumsalKur.KalipKaydet(nesne, $"{KalipKlasoru}/{ad}.prefab").GetComponent<Yiyecek>();
        }

        /// <summary>Bölüm 5 yavrusunun deniz ayarları (GDD: çılgın yüzüş, nefes yok, ağ 10 basış, Space sadece daldırır).</summary>
        private static DenizAyarlari YavruDenizAyarlari() => AyarAdla<DenizAyarlari>("YavruDenizAyarlari", so =>
        {
            so.FindProperty("yuzmeHizi").floatValue = 2.5f;
            so.FindProperty("hareketEnerjisi").floatValue = 0f;
            so.FindProperty("surekliEnerji").floatValue = 0.5f;
            so.FindProperty("nefesSuresi").floatValue = 0f;
            so.FindProperty("agBasisi").intValue = 10;
            so.FindProperty("spaceSadeceDalar").boolValue = true;
        });

        // ---- Ortak ----

        private static void Hazirla()
        {
            su = GriKumsalKur.SaydamMalzeme("SuYuzeyi", new Color(0.2f, 0.45f, 0.6f, 0.6f));
            // Su altından da görünsün: iki yüzlü
            su.SetFloat("_Cull", 0f);
            taban = GriKumsalKur.Malzeme("DenizTabani", new Color(0.55f, 0.5f, 0.4f));
            kaya = GriKumsalKur.Malzeme("Kaya", new Color(0.35f, 0.35f, 0.38f));
            cayir = GriKumsalKur.SaydamMalzeme("DenizCayiri", new Color(0.2f, 0.6f, 0.3f, 0.5f));
            ag = GriKumsalKur.Malzeme("HayaletAg", new Color(0.85f, 0.95f, 1f), "Universal Render Pipeline/Unlit");
            misina = GriKumsalKur.Malzeme("Misina", new Color(0.95f, 0.95f, 0.8f), "Universal Render Pipeline/Unlit");
            denizanasi = GriKumsalKur.SaydamMalzeme("Denizanasi", new Color(1f, 0.6f, 0.85f, 0.7f));
            poset = GriKumsalKur.SaydamMalzeme("Poset", new Color(0.95f, 0.95f, 0.95f, 0.7f));
            tekne = GriKumsalKur.Malzeme("Tekne", new Color(0.9f, 0.9f, 0.9f));
            avci = GriKumsalKur.Malzeme("AvciBalik", new Color(0.15f, 0.18f, 0.22f));
            sicakSu = GriKumsalKur.SaydamMalzeme("SicakSu", new Color(0.5f, 0.15f, 0.1f, 0.35f));
            akinti = GriKumsalKur.SaydamMalzeme("Akinti", new Color(0.8f, 0.9f, 1f, 0.25f));
            yosun = GriKumsalKur.Malzeme("YosunAdasi", new Color(0.45f, 0.4f, 0.15f));
            isaret = GriKumsalKur.Malzeme("KontrolIsareti", new Color(1f, 0.85f, 0.2f));
            plastik = GriKumsalKur.Malzeme("ParlayanPlastik", new Color(1f, 1f, 0.7f), "Universal Render Pipeline/Unlit");
            dalga = GriKumsalKur.SaydamMalzeme("Dalga", new Color(0.9f, 0.95f, 1f, 0.6f));
            agKenari = GriKumsalKur.Malzeme("AgKenari", new Color(0.7f, 1f, 1f), "Universal Render Pipeline/Unlit");
            tekneGolgesi = GriKumsalKur.SaydamMalzeme("TekneGolgesi", new Color(0f, 0.02f, 0.05f, 0.55f));
            kopuk = GriKumsalKur.SaydamMalzeme("PervaneKopugu", new Color(1f, 1f, 1f, 0.75f));
            girdapCopu = GriKumsalKur.Malzeme("GirdapCopu", new Color(0.75f, 0.72f, 0.6f));

            // Su altı sisi
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.025f;
            RenderSettings.fogColor = new Color(0.1f, 0.35f, 0.45f);

            var deniz = new GameObject("Deniz").transform;
            GriKumsalKur.SuYuzeyiKur("SuYuzeyi", deniz, new Vector3(0f, 0f, -DenizUzunlugu / 2f), new Vector2(DenizGenisligi, DenizUzunlugu), su);

            // Taban: Yusuf'un deniz tabanı varsa o (aynı ölçüler: sığ düzlük y −6, açık deniz y −15, kayalık şev, mercanlar, çayır); yoksa üç gri basamak.
            GameObject denizTabani = GriKumsalKur.KalibiKoy("DenizTabani", deniz, Vector3.zero);
            // Performans: mercan ve çayır gibi küçük süsler gölge atmaz, uzakta çizilmez.
            if (denizTabani != null)
                foreach (Transform cocuk in denizTabani.transform)
                    if (cocuk.name.Contains("Mercan") || cocuk.name.Contains("Cayir") || cocuk.name.Contains("Dekor"))
                        GriKumsalKur.UzaktaGizle(cocuk.gameObject, 0.01f, true);
            if (denizTabani == null)
            {
                GriKumsalKur.Kutu("AcikDeniz", deniz, new Vector3(0f, -15.5f, -160f), new Vector3(DenizGenisligi, 1f, 80f), taban, "Ground");
                GriKumsalKur.Kutu("SigSular", deniz, new Vector3(0f, -6.5f, -85f), new Vector3(DenizGenisligi, 1f, 70f), taban, "Ground");
                GriKumsalKur.Kutu("KumsalOnu", deniz, new Vector3(0f, -3f, -25f), new Vector3(DenizGenisligi, 1f, 50f), taban, "Ground");
                GriKumsalKur.Kutu("Kumsal", deniz, new Vector3(0f, 0f, 15f), new Vector3(DenizGenisligi, 1f, 30f), taban, "Ground");
            }

            // Görünmez duvarlar
            foreach (float x in new[] { -DenizGenisligi / 2f, DenizGenisligi / 2f })
                Object.DestroyImmediate(GriKumsalKur.Kutu("Duvar", deniz, new Vector3(x, -10f, -DenizUzunlugu / 2f), new Vector3(1f, 40f, DenizUzunlugu), su, "Default").GetComponent<MeshRenderer>());
            Object.DestroyImmediate(GriKumsalKur.Kutu("Duvar", deniz, new Vector3(0f, -10f, -DenizUzunlugu - 1f), new Vector3(DenizGenisligi, 40f, 1f), su, "Default").GetComponent<MeshRenderer>());
        }

        internal static DenizHareketi YuzucuKur(string ad, DenizAyarlari ayarlar, DenizTehlikeAyarlari tehlike, Vector3 govdeOlcegi, float yaricap, Vector3 gozYeri, string model = null)
        {
            var nesne = new GameObject(ad);
            nesne.layer = LayerMask.NameToLayer("Turtle");
            var kontrol = nesne.AddComponent<CharacterController>();
            kontrol.radius = yaricap;
            kontrol.height = yaricap * 2f;
            kontrol.center = Vector3.zero;
            kontrol.skinWidth = yaricap * 0.1f;
            kontrol.stepOffset = 0f;
            nesne.AddComponent<Enerji>();
            var yuzucu = nesne.AddComponent<DenizHareketi>();

            Transform gorunus = GriKumsalKur.Govde(nesne.transform, Vector3.zero, govdeOlcegi, model);
            GriKumsalKur.SalinimEkle(nesne, gorunus, SalinimTuru.Deniz);
            GriKumsalKur.Bagla(nesne.AddComponent<EnerjiUyarisi>(), "ayarlar", GriKumsalKur.Ayar<GeribildirimAyarlari>("GeribildirimAyarlari"));
            BirinciSahisKamera goz = GriKumsalKur.KameraKur(nesne.transform, gozYeri, nesne.transform);
            // Su altı sisi ~100 m'de her şeyi örter; ötesini çizmeye gerek yok.
            goz.GetComponent<UnityEngine.Camera>().farClipPlane = 150f;

            // Su altı ışık hüzmeleri: gözün çevresinde, yüzeyden aşağı.
            var isiklar = new GameObject("SuIsiklari", typeof(MeshFilter), typeof(MeshRenderer));
            var isikGorunusu = isiklar.GetComponent<MeshRenderer>();
            isikGorunusu.sharedMaterial = GriKumsalKur.Malzeme("SuIsigi", Color.white, "IkiDunya/Gokyuzu");
            isikGorunusu.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            GriKumsalKur.Bagla(isiklar.AddComponent<SuIsiklari>(), "hedef", goz.transform);
            GriKumsalKur.Bagla(goz.gameObject.AddComponent<SuAltiGorunumu>(), "ayarlar", GriKumsalKur.Ayar<OrtamAyarlari>("OrtamAyarlari"));
            GriKumsalKur.Bagla(goz, "ayarlar", AyarAdla<KameraAyarlari>("DenizKameraAyarlari", so =>
            {
                so.FindProperty("asagiSinir").floatValue = -75f;
                so.FindProperty("yukariSinir").floatValue = 75f;
            }));

            GriKumsalKur.Bagla(yuzucu, "ayarlar", ayarlar);
            GriKumsalKur.Bagla(yuzucu, "tehlikeAyarlari", tehlike);
            GriKumsalKur.Bagla(yuzucu, "goz", goz.transform);

            // Göstergeler: enerji, nefes, ağdan kurtulma
            Transform tuval = GriKumsalKur.TuvalKur("DenizArayuzu", null);
            Cubuk enerji = GriKumsalKur.CubukKur("EnerjiCubugu", tuval, new Vector2(0.03f, 0.04f), new Vector2(0.28f, 0.065f), new Color(0.4f, 0.8f, 0.45f), false);
            var enerjiGostergesi = enerji.gameObject.AddComponent<EnerjiCubugu>();
            GriKumsalKur.Bagla(enerjiGostergesi, "enerji", nesne.GetComponent<Enerji>());
            GriKumsalKur.Bagla(enerjiGostergesi, "dolgu", enerji.transform.GetChild(0));
            Cubuk nefes = GriKumsalKur.CubukKur("NefesCubugu", tuval, new Vector2(0.03f, 0.08f), new Vector2(0.28f, 0.1f), new Color(0.5f, 0.8f, 1f), false);
            Cubuk kurtulma = GriKumsalKur.CubukKur("KurtulmaCubugu", tuval, new Vector2(0.42f, 0.42f), new Vector2(0.58f, 0.445f), Color.white, false);
            var gosterge = tuval.gameObject.AddComponent<DenizGostergesi>();
            GriKumsalKur.Bagla(gosterge, "yuzucu", yuzucu);
            GriKumsalKur.Bagla(gosterge, "nefes", nefes);
            GriKumsalKur.Bagla(gosterge, "kurtulma", kurtulma);
            // Yavaşlık: poşet yutunca enerji çubuğunun üstünde kahverengi, süre bitince boşalır.
            Cubuk yavaslik = GriKumsalKur.CubukKur("YavaslikCubugu", tuval, new Vector2(0.03f, 0.115f), new Vector2(0.28f, 0.13f), new Color(0.6f, 0.45f, 0.3f), false);
            GriKumsalKur.Bagla(gosterge, "yavaslik", yavaslik);
            return yuzucu;
        }

        private static void BolumKur(AdimTuru adim, DenizAyarlari ayarlar, DenizHareketi yuzucu, Transform baslangic, bool final, Transform devamBaslangici)
        {
            yuzucu.transform.SetPositionAndRotation(baslangic.position, baslangic.rotation);
            var bolum = new GameObject("DenizBolumu").AddComponent<DenizBolumu>();
            var so = new SerializedObject(bolum);
            so.FindProperty("adim").enumValueIndex = (int)adim;
            so.FindProperty("finalKamerasi").boolValue = final;
            so.ApplyModifiedPropertiesWithoutUndo();
            GriKumsalKur.Bagla(bolum, "ayarlar", ayarlar);
            GriKumsalKur.Bagla(bolum, "yuzucu", yuzucu);
            GriKumsalKur.Bagla(bolum, "baslangic", baslangic);
            GriKumsalKur.Bagla(bolum, "devamBaslangici", devamBaslangici);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));
        }

        private static Transform Nokta(string ad, Vector3 yer, Quaternion yon, Transform ebeveyn = null)
        {
            Transform nokta = GriKumsalKur.Bos(ad, ebeveyn);
            nokta.SetPositionAndRotation(yer, yon);
            return nokta;
        }

        /// <summary>Kıyıya paralel, denizi boydan boya kesen tetikleyici; yüzeyde sarı şamandıra.</summary>
        private static void Kontrol(string ad, float z, Quaternion yon, bool son, bool kapi = false)
        {
            GameObject alan = GriKumsalKur.Kutu(ad, null, new Vector3(0f, -10f, z), new Vector3(DenizGenisligi, 40f, 4f), isaret, "Default");
            alan.transform.rotation = yon;
            Object.DestroyImmediate(alan.GetComponent<MeshRenderer>());
            alan.GetComponent<Collider>().isTrigger = true;
            var nokta = alan.AddComponent<KontrolNoktasi>();
            var so = new SerializedObject(nokta);
            so.FindProperty("son").boolValue = son;
            so.FindProperty("kapi").boolValue = kapi;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Kapı uzaktan görünsün: denizi boydan boya kesen dönen akıntı perdesi.
            if (kapi)
            {
                GameObject perde = GriKumsalKur.Kutu($"{ad}_Perde", null, new Vector3(0f, -6f, z), new Vector3(DenizGenisligi, 12f, 1.5f), akinti, "Default");
                Object.DestroyImmediate(perde.GetComponent<Collider>());
            }

            GameObject samandira = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            samandira.name = "Samandira";
            Object.DestroyImmediate(samandira.GetComponent<Collider>());
            samandira.transform.position = new Vector3(4f, 0.2f, z);
            samandira.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            samandira.GetComponent<Renderer>().sharedMaterial = isaret;
        }

        private static void AkintiKur(Vector3 yer, Vector3 boyut, float yon, DenizTehlikeAyarlari tehlike)
        {
            GameObject alan = GriKumsalKur.Kutu("Akinti", null, yer, boyut, akinti, "Default");
            alan.transform.rotation = Quaternion.Euler(0f, yon, 0f);
            alan.GetComponent<Collider>().isTrigger = true;
            // Alanın kendisi görünmez (koca beyaz blok gibi duruyordu); akıntıyı süzülen çizgiler gösterir.
            Object.DestroyImmediate(alan.GetComponent<MeshRenderer>());
            alan.isStatic = false;
            var akim = alan.AddComponent<Akinti>();
            GriKumsalKur.Bagla(akim, "ayarlar", tehlike);

            Material cizgiMalzemesi = GriKumsalKur.SaydamMalzeme("AkintiCizgisi", new Color(0.85f, 0.95f, 1f, 0.35f));
            Transform kok = GriKumsalKur.Bos($"{alan.name}_Cizgiler", null);
            int sayi = Mathf.Clamp(Mathf.RoundToInt(boyut.x * boyut.z / 40f), 8, 30);
            var cizgiler = new Object[sayi];
            var rastgele = new System.Random(Mathf.RoundToInt(yer.x * 13f + yer.z * 7f));
            for (int i = 0; i < sayi; i++)
            {
                Vector3 yerel = new Vector3((float)rastgele.NextDouble() - 0.5f, (float)rastgele.NextDouble() - 0.5f, (float)rastgele.NextDouble() - 0.5f);
                GameObject cizgi = GriKumsalKur.Kutu("Cizgi", kok, alan.transform.TransformPoint(yerel), new Vector3(0.04f, 0.04f, 1.6f), cizgiMalzemesi, "Default");
                Object.DestroyImmediate(cizgi.GetComponent<Collider>());
                cizgi.isStatic = false;
                cizgi.transform.rotation = alan.transform.rotation;
                cizgiler[i] = cizgi.transform;
            }
            GriKumsalKur.DiziBagla(akim, "cizgiler", cizgiler);
        }

        private static GameObject YiyecekKur(Yiyecek.Turu tur, Vector3 yer, DenizTehlikeAyarlari tehlike)
        {
            // Kök: yeme alanı (tetikleyici küre) ve Yiyecek; görünüş çocukta (Abdü'nün denizanası, Yusuf'un poşeti ya da gri küre).
            var nesne = new GameObject(tur.ToString());
            nesne.transform.position = yer;
            var alan = nesne.AddComponent<SphereCollider>();
            alan.isTrigger = true;
            alan.radius = 0.3f;
            bool denizanasiMi = tur == Yiyecek.Turu.Denizanasi;
            GameObject model = GriKumsalKur.ModelYerlestir(nesne.transform, denizanasiMi ? "Denizanasi" : "Poset", new Vector3(0f, -0.25f, 0f), 0.6f);
            if (model != null && denizanasiMi) GriKumsalKur.AnimasyonBagla(model, DenizanasiDenetleyicisi);
            if (model == null)
            {
                GameObject kure = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                kure.name = "Gri";
                Object.DestroyImmediate(kure.GetComponent<Collider>());
                kure.transform.SetParent(nesne.transform, false);
                kure.transform.localScale = denizanasiMi ? new Vector3(0.6f, 0.45f, 0.6f) : new Vector3(0.6f, 0.4f, 0.5f);
                kure.GetComponent<Renderer>().sharedMaterial = denizanasiMi ? denizanasi : poset;
            }
            var yiyecek = nesne.AddComponent<Yiyecek>();
            var so = new SerializedObject(yiyecek);
            so.FindProperty("tur").enumValueIndex = (int)tur;
            so.ApplyModifiedPropertiesWithoutUndo();
            GriKumsalKur.Bagla(yiyecek, "ayarlar", tehlike);
            return nesne;
        }

        private static GameObject HayaletAgKalibi()
        {
            var kok = new GameObject("HayaletAg");
            kok.layer = LayerMask.NameToLayer("Trash");
            GameObject govde = GriKumsalKur.Kutu("Govde", kok.transform, Vector3.zero, new Vector3(4f, 3f, 0.1f), ag, "Trash");
            govde.isStatic = false;
            ((BoxCollider)govde.GetComponent<Collider>()).isTrigger = true;
            var so = new SerializedObject(govde.AddComponent<TrashTrap>());
            so.FindProperty("tur").enumValueIndex = (int)TrashTrap.Turu.HayaletAg;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Kenarları hafif parlar: uzaktan seçilsin.
            var kenarlar = new Renderer[4];
            for (int i = 0; i < 4; i++)
            {
                bool yatay = i < 2;
                Vector3 yer = yatay ? new Vector3(0f, i == 0 ? 1.5f : -1.5f, 0f) : new Vector3(i == 2 ? 2f : -2f, 0f, 0f);
                Vector3 boy = yatay ? new Vector3(4.1f, 0.08f, 0.12f) : new Vector3(0.08f, 3.1f, 0.12f);
                GameObject kenar = GriKumsalKur.Kutu($"Kenar_{i}", kok.transform, yer, boy, agKenari, "Trash");
                Object.DestroyImmediate(kenar.GetComponent<Collider>());
                kenar.isStatic = false;
                kenarlar[i] = kenar.GetComponent<Renderer>();
            }
            ParildatKur(kok, new Color(0.7f, 1f, 1f), kenarlar);
            return GriKumsalKur.KalipKaydet(kok, $"{KalipKlasoru}/HayaletAg.prefab");
        }

        private static GameObject MisinaKalibi()
        {
            var kok = new GameObject("Misina");
            kok.layer = LayerMask.NameToLayer("Trash");
            GameObject govde = GriKumsalKur.Kutu("Govde", kok.transform, Vector3.zero, new Vector3(6f, 0.03f, 0.03f), misina, "Trash");
            govde.isStatic = false;
            var kutu = (BoxCollider)govde.GetComponent<Collider>();
            kutu.isTrigger = true;
            // İnce çizgi; takılmak için çevresinde biraz pay
            kutu.size = new Vector3(1f, 20f, 20f);
            var so = new SerializedObject(govde.AddComponent<TrashTrap>());
            so.FindProperty("tur").enumValueIndex = (int)TrashTrap.Turu.Misina;
            so.ApplyModifiedPropertiesWithoutUndo();
            ParildatKur(kok, new Color(0.95f, 0.95f, 0.8f), govde.GetComponent<Renderer>());
            return GriKumsalKur.KalipKaydet(kok, $"{KalipKlasoru}/Misina.prefab");
        }

        private static void ParildatKur(GameObject kok, Color renk, params Renderer[] parcalar)
        {
            var parilti = kok.AddComponent<Parilti>();
            GriKumsalKur.Bagla(parilti, "ayarlar", GriKumsalKur.Ayar<DenizTehlikeAyarlari>("DenizTehlikeAyarlari"));
            GriKumsalKur.DiziBagla(parilti, "parcalar", parcalar);
            var so = new SerializedObject(parilti);
            so.FindProperty("renk").colorValue = renk;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void TekneKur(string ad, float z, bool isikli, DenizTehlikeAyarlari tehlike, KumsalDurumu kumsal)
        {
            Transform a = Nokta($"{ad}_A", new Vector3(-DenizGenisligi / 2f + 2f, 0.3f, z), Quaternion.identity);
            Transform b = Nokta($"{ad}_B", new Vector3(DenizGenisligi / 2f - 2f, 0.3f, z), Quaternion.identity);
            var kok = new GameObject(ad);
            kok.transform.position = a.position;
            // Balıkçı teknesi modeli (varsa); yoksa gri kutu.
            if (GriKumsalKur.ModelYerlestir(kok.transform, "BalikciTeknesi_Gorsel", new Vector3(0f, -0.4f, 0f), 5f) == null)
            {
                GameObject govde = GriKumsalKur.Kutu("Govde", kok.transform, Vector3.zero, new Vector3(2f, 0.8f, 5f), tekne, "Default");
                Object.DestroyImmediate(govde.GetComponent<Collider>());
                govde.isStatic = false;
            }

            // Pervane sesi: klip Abdü'den gelecek
            var ses = kok.AddComponent<AudioSource>();
            ses.loop = true;
            ses.spatialBlend = 1f;
            ses.playOnAwake = true;

            // Uyarı: suyun hemen altında teknenin önüne düşen gölge, arkasında titreyen pervane köpüğü
            GameObject golge = GriKumsalKur.Kutu("Golge", kok.transform, new Vector3(0f, -0.5f, 0f), new Vector3(3f, 0.02f, 7f), tekneGolgesi, "Default");
            Object.DestroyImmediate(golge.GetComponent<Collider>());
            golge.isStatic = false;
            var kopukler = new Object[3];
            for (int i = 0; i < kopukler.Length; i++)
            {
                GameObject parca = GriKumsalKur.Kutu($"Kopuk_{i}", kok.transform, new Vector3(0f, -0.2f, -3f - i * 1.5f), new Vector3(1.5f + i * 0.5f, 0.05f, 1.2f), kopuk, "Default");
                Object.DestroyImmediate(parca.GetComponent<Collider>());
                parca.isStatic = false;
                kopukler[i] = parca.transform;
            }

            var bilesen = kok.AddComponent<Tekne>();
            GriKumsalKur.Bagla(bilesen, "ayarlar", tehlike);
            GriKumsalKur.Bagla(bilesen, "a", a);
            GriKumsalKur.Bagla(bilesen, "b", b);
            GriKumsalKur.Bagla(bilesen, "golge", golge.transform);
            GriKumsalKur.DiziBagla(bilesen, "kopukler", kopukler);

            if (!isikli) return;
            IsikAlaniKur(kok.transform, Vector3.down * 0.5f, 10f, 6f, kumsal);
        }

        private static void SicakSuKur(Vector3 yer, float yaricap, DenizTehlikeAyarlari tehlike)
        {
            GameObject alan = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            alan.name = "SicakSu";
            alan.layer = LayerMask.NameToLayer("Hazard");
            alan.transform.position = yer;
            alan.transform.localScale = Vector3.one * yaricap * 2f;
            alan.GetComponent<Collider>().isTrigger = true;
            alan.GetComponent<Renderer>().sharedMaterial = sicakSu;
            var bolge = alan.AddComponent<HazardZone>();
            var so = new SerializedObject(bolge);
            so.FindProperty("tur").enumValueIndex = (int)HazardZone.Turu.SicakSu;
            so.ApplyModifiedPropertiesWithoutUndo();
            GriKumsalKur.Bagla(bolge, "denizAyarlari", tehlike);
        }

        private static void CayirKur(Vector3 yer, Vector3 boyut)
        {
            GameObject alan = GriKumsalKur.Kutu("DenizCayiri", null, yer, boyut, cayir, "Cover");
            alan.GetComponent<Collider>().isTrigger = true;
            alan.AddComponent<Cover>();
            // Görünüş Yusuf'un çayırı (alanın boyunda, pivot tabanın ortası); saklanmayı kutu yapmaya devam eder.
            if (GriKumsalKur.KalibiKoy($"DenizCayiri_{boyut.x:0}x{boyut.z:0}", null, yer + Vector3.down * boyut.y / 2f) != null)
                alan.GetComponent<Renderer>().enabled = false;
        }

        private static void AvciKur(Vector3 yer, DenizTehlikeAyarlari tehlike)
        {
            var kok = new GameObject("AvciBalik");
            kok.layer = LayerMask.NameToLayer("Predator");
            kok.transform.position = yer;
            GameObject balikModeli = GriKumsalKur.ModelYerlestir(kok.transform, "Avci_Balik", new Vector3(0f, -0.4f, 0f), 2f);
            if (balikModeli != null) GriKumsalKur.SalinimEkle(kok, balikModeli.transform, SalinimTuru.Balik);
            else
            {
                GameObject govde = GriKumsalKur.Kutu("Govde", kok.transform, Vector3.zero, new Vector3(0.6f, 0.8f, 2f), avci, "Predator");
                Object.DestroyImmediate(govde.GetComponent<Collider>());
                govde.isStatic = false;
            }
            GriKumsalKur.Bagla(kok.AddComponent<AvciBalik>(), "ayarlar", tehlike);
        }

        /// <summary>Parlayan plastik: çevresindeki yavruyu kendine çeker, değerse yavaşlatır.</summary>
        private static void PlastikKur(Vector3 yer, DenizTehlikeAyarlari tehlike, KumsalDurumu kumsal)
        {
            var kok = new GameObject("ParlayanPlastik");
            kok.transform.position = yer;
            IsikAlaniKur(kok.transform, Vector3.zero, 6f, 2f, kumsal);

            GameObject parca = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            parca.name = "Parca";
            parca.layer = LayerMask.NameToLayer("Trash");
            parca.transform.SetParent(kok.transform, false);
            parca.transform.localScale = new Vector3(0.5f, 0.2f, 0.4f);
            parca.GetComponent<Collider>().isTrigger = true;
            parca.GetComponent<Renderer>().sharedMaterial = plastik;
            // Yusuf'un parlayan plastiği (1-3) varsa kürenin görüntüsünün yerine geçer; tuzak ve collider aynı kalır.
            // Modelin kökü ortada: kürenin merkezine oturur (tabana oturtulmaz).
            int parcaNo = Mathf.Abs(Mathf.RoundToInt(yer.x + yer.z)) % 3 + 1;
            if (GriKumsalKur.ModelYerlestir(kok.transform, $"Parlayan_Plastik_{parcaNo}", Vector3.zero, 0.5f, false) != null)
                parca.GetComponent<Renderer>().enabled = false;
            var so = new SerializedObject(parca.AddComponent<TrashTrap>());
            so.FindProperty("tur").enumValueIndex = (int)TrashTrap.Turu.Plastik;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Yasakla sönmeyen bir ışık alanı (ışıklı tekne, parlayan plastik).</summary>
        private static void IsikAlaniKur(Transform ebeveyn, Vector3 yer, float yaricap, float isikMenzili, KumsalDurumu kumsal)
        {
            Transform alan = GriKumsalKur.Bos("IsikAlani", ebeveyn);
            alan.localPosition = yer;
            alan.gameObject.layer = LayerMask.NameToLayer("Hazard");
            var kure = alan.gameObject.AddComponent<SphereCollider>();
            kure.isTrigger = true;
            kure.radius = yaricap;

            var lamba = alan.gameObject.AddComponent<Light>();
            lamba.type = LightType.Point;
            lamba.range = isikMenzili;
            lamba.intensity = 3f;
            lamba.color = new Color(1f, 0.95f, 0.75f);

            var isik = alan.gameObject.AddComponent<IsikKaynagi>();
            GriKumsalKur.Bagla(isik, "ayarlar", GriKumsalKur.Ayar<KacisAyarlari>("KacisAyarlari"));
            GriKumsalKur.Bagla(isik, "kumsal", kumsal != null ? kumsal : GriKumsalKur.Ayar<KumsalDurumu>("KumsalDurumu"));
            GriKumsalKur.Bagla(isik, "isik", lamba);
            var so = new SerializedObject(isik);
            so.FindProperty("yasaklanabilir").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Ayar asset'ini sadece adıyla bulur; yoksa oluşturur ve ilk değerlerini yazar.</summary>
        private static T AyarAdla<T>(string ad, Action<SerializedObject> ilkDegerler) where T : ScriptableObject
        {
            string yol = $"{AyarKlasoru}/{ad}.asset";
            var ayar = AssetDatabase.LoadAssetAtPath<T>(yol);
            if (ayar != null) return ayar;

            ayar = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(ayar, yol);
            if (ilkDegerler != null)
            {
                var so = new SerializedObject(ayar);
                ilkDegerler(so);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            return ayar;
        }
    }
}
