using System;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuatArenaKetangkasan
{
    private const string Root = "Assets/ArenaKetangkasan";
    private const string ScenePath = Root + "/ArenaKetangkasan.unity";

    [MenuItem("Septalabs/Buat Arena Ketangkasan")]
    public static void Buat()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
        {
            EditorSceneManager.OpenScene(ScenePath);
            return;
        }

        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        if (!AssetDatabase.IsValidFolder(Root))
        {
            AssetDatabase.CreateFolder("Assets", "ArenaKetangkasan");
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Transform lingkungan = new GameObject("Lingkungan").transform;
        GameObject lantai = Primitif(PrimitiveType.Plane, "Lantai", Vector3.zero, new Vector3(2f, 1f, 2f));
        lantai.transform.SetParent(lingkungan);
        Warnai(lantai, "Lantai", new Color32(58, 88, 104, 255));

        GameObject rintangan = Primitif(PrimitiveType.Cube, "Rintangan", Vector3.zero, new Vector3(2.5f, 1.5f, 1.5f));
        Warnai(rintangan, "Rintangan", new Color32(232, 150, 61, 255));
        rintangan.AddComponent<ZonaRintangan>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rintangan, Root + "/Rintangan.prefab");
        UnityEngine.Object.DestroyImmediate(rintangan);
        Vector3[] posisiRintangan =
        {
            new(-3f, 0.75f, -2f), new(3f, 0.75f, 0f), new(0f, 0.75f, 3f),
            new(-5f, 0.75f, 5f), new(5f, 0.75f, 6f)
        };
        for (int i = 0; i < posisiRintangan.Length; i++)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, lingkungan);
            instance.name = $"Rintangan {i + 1}";
            instance.transform.localPosition = posisiRintangan[i];
        }

        GameObject player = Primitif(PrimitiveType.Cube, "Player", new Vector3(0f, 0.55f, -5f), Vector3.one);
        player.tag = "Player";
        Warnai(player, "Player", new Color32(88, 226, 195, 255));
        Rigidbody badan = player.AddComponent<Rigidbody>();
        badan.constraints = RigidbodyConstraints.FreezeRotation;
        badan.interpolation = RigidbodyInterpolation.Interpolate;
        badan.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        KarakterJalan jalan = player.AddComponent<KarakterJalan>();

        var kamera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(KameraOrbit));
        kamera.tag = "MainCamera";
        kamera.transform.position = player.transform.position + new Vector3(0f, 5f, -7f);
        kamera.transform.LookAt(player.transform);
        IsiReferensi(kamera.GetComponent<KameraOrbit>(), "pemain", player.transform);
        IsiReferensi(jalan, "kamera", kamera.transform);

        var sistem = new GameObject("GameManager").AddComponent<SistemGame>();
        IsiReferensi(sistem, "pemain", jalan);
        IsiReferensi(sistem, "lingkungan", lingkungan);
        TambahkanKoinDanHud(sistem);
        TambahkanMusuh(sistem, lingkungan);
        TambahkanTembakan(sistem, player, kamera.transform);

        TambahkanAudioDanCahaya(sistem, player.GetComponent<TembakanPemain>(), lantai);
        TambahkanLayarHasil(sistem);
        TambahkanMenuJeda(sistem);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new InvalidOperationException("Scene ArenaKetangkasan gagal disimpan.");
        }

        BuatMenuUtama.AturDaftarScene();
        Selection.activeGameObject = player;
    }

    internal static void TambahkanKoinDanHud(SistemGame sistem)
    {
        var grupKoin = new GameObject("Koin Arena").transform;
        string jalurPrefab = Root + "/Koin.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(jalurPrefab);
        if (prefab == null)
        {
            var koin = new GameObject("Koin", typeof(Koin));
            var pemicu = koin.GetComponent<SphereCollider>();
            pemicu.isTrigger = true;
            pemicu.radius = 0.42f;

            GameObject bentuk = Primitif(PrimitiveType.Cylinder, "Visual", Vector3.zero, new Vector3(0.63f, 0.1f, 0.63f));
            bentuk.transform.SetParent(koin.transform, false);
            bentuk.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            UnityEngine.Object.DestroyImmediate(bentuk.GetComponent<Collider>());
            Warnai(bentuk, "Koin", new Color32(255, 221, 65, 255));

            prefab = PrefabUtility.SaveAsPrefabAsset(koin, jalurPrefab);
            UnityEngine.Object.DestroyImmediate(koin);
        }

        Vector3[] posisiKoin =
        {
            new(0f, 0.56f, -2.5f), new(-5f, 0.56f, -0.5f), new(5.5f, 0.56f, 2f),
            new(-2.2f, 0.56f, 5.5f), new(2f, 0.56f, 8f)
        };
        foreach (Vector3 posisi in posisiKoin)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, grupKoin);
            instance.transform.localPosition = posisi;
        }

        var canvas = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var skala = canvas.GetComponent<CanvasScaler>();
        skala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        skala.referenceResolution = new Vector2(960f, 540f);

        var hud = new GameObject("Skor dan Waktu", typeof(RectTransform), typeof(Text), typeof(Shadow)).GetComponent<Text>();
        hud.transform.SetParent(canvas.transform, false);
        RectTransform rect = hud.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(20f, -16f);
        rect.sizeDelta = new Vector2(260f, 110f);
        hud.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hud.fontSize = 23;
        hud.fontStyle = FontStyle.Bold;
        hud.color = Color.white;
        hud.raycastTarget = false;
        hud.text = "Skor  0\nWaktu  00:00";
        hud.GetComponent<Shadow>().effectDistance = new Vector2(1.5f, -1.5f);
        IsiReferensi(sistem, "hud", hud);
    }


    internal static void TambahkanMusuh(SistemGame sistem, Transform lingkungan)
    {
        var navigasi = lingkungan.gameObject.AddComponent<NavMeshSurface>();
        navigasi.collectObjects = CollectObjects.Children;
        navigasi.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        var objek = new GameObject("Musuh Arena", typeof(MusuhArena));
        objek.transform.position = new Vector3(4.6f, 0f, -3.4f);

        var agen = objek.GetComponent<NavMeshAgent>();
        agen.enabled = false;
        agen.speed = 2.6f;
        agen.acceleration = 12f;
        agen.angularSpeed = 540f;
        agen.radius = 0.45f;
        agen.height = 1.5f;

        var pemicu = objek.GetComponent<CapsuleCollider>();
        pemicu.isTrigger = true;
        pemicu.center = new Vector3(0f, 0.75f, 0f);
        pemicu.radius = 0.48f;
        pemicu.height = 1.5f;

        GameObject visual = Primitif(PrimitiveType.Capsule, "Visual Musuh", pemicu.center, new Vector3(0.96f, 0.75f, 0.96f));
        visual.transform.SetParent(objek.transform, false);
        UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
        Warnai(visual, "Musuh", new Color32(228, 68, 92, 255));
        IsiReferensi(sistem, "musuh", objek.GetComponent<MusuhArena>());

        var serialized = new SerializedObject(sistem);
        var hud = (Text)serialized.FindProperty("hud").objectReferenceValue;
        hud.rectTransform.sizeDelta = new Vector2(260f, 135f);
        hud.text = "HP  3/3" + Environment.NewLine + hud.text;
    }


    internal static void TambahkanTembakan(SistemGame sistem, GameObject pemain, Transform kamera)
    {
        string jalurPrefab = Root + "/Proyektil.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(jalurPrefab);
        if (prefab == null)
        {
            GameObject proyektil = Primitif(PrimitiveType.Sphere, "Proyektil", Vector3.zero, new Vector3(0.23f, 0.23f, 0.46f));
            UnityEngine.Object.DestroyImmediate(proyektil.GetComponent<Collider>());
            proyektil.AddComponent<ProyektilPemain>();
            Warnai(proyektil, "Proyektil", new Color32(220, 252, 255, 255));
            prefab = PrefabUtility.SaveAsPrefabAsset(proyektil, jalurPrefab);
            UnityEngine.Object.DestroyImmediate(proyektil);
        }

        var tembakan = pemain.AddComponent<TembakanPemain>();
        IsiReferensi(tembakan, "kamera", kamera);
        IsiReferensi(tembakan, "prefab", prefab.GetComponent<ProyektilPemain>());

        var serialized = new SerializedObject(sistem);
        var hud = (Text)serialized.FindProperty("hud").objectReferenceValue;
        hud.rectTransform.sizeDelta = new Vector2(260f, 162f);
        hud.text += Environment.NewLine + "Musuh  3/3";
    }


    internal static void TambahkanAudioDanCahaya(SistemGame sistem, TembakanPemain tembakan, GameObject lantai)
    {
        var audio = sistem.gameObject.AddComponent<AudioArena>();
        var sumber = audio.GetComponent<AudioSource>();
        sumber.playOnAwake = false;
        sumber.spatialBlend = 0f;
        sumber.volume = 0.82f;
        IsiReferensi(audio, "tembakan", MuatSuara("Tembak"));
        IsiReferensi(audio, "musuhTerkena", MuatSuara("MusuhKena"));
        IsiReferensi(audio, "koin", MuatSuara("AmbilKoin"));
        IsiReferensi(audio, "pemainTerkena", MuatSuara("PemainKena"));
        IsiReferensi(tembakan, "audioArena", audio);

        var cahayaUtama = GameObject.Find("Directional Light")?.GetComponent<Light>();
        if (cahayaUtama == null)
        {
            cahayaUtama = new GameObject("Directional Light").AddComponent<Light>();
        }

        cahayaUtama.type = LightType.Directional;
        cahayaUtama.color = new Color32(255, 244, 225, 255);
        cahayaUtama.intensity = 1.15f;
        cahayaUtama.shadows = LightShadows.Soft;
        cahayaUtama.shadowStrength = 0.65f;
        cahayaUtama.transform.rotation = Quaternion.Euler(51f, -38f, 0f);
        RenderSettings.sun = cahayaUtama;

        var cahayaIsi = new GameObject("Cahaya Isi Arena").AddComponent<Light>();
        cahayaIsi.type = LightType.Directional;
        cahayaIsi.color = new Color32(193, 221, 255, 255);
        cahayaIsi.intensity = 0.34f;
        cahayaIsi.shadows = LightShadows.None;
        cahayaIsi.transform.rotation = Quaternion.Euler(26f, 144f, 0f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.46f, 0.49f, 0.53f);

        BuatPenandaTepi(lantai);
    }

    private static AudioClip MuatSuara(string nama)
    {
        string path = $"{Root}/Audio/{nama}.wav";
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path)
            ?? throw new InvalidOperationException($"Audio arena tidak ditemukan: {path}");
    }

    private static void BuatPenandaTepi(GameObject lantai)
    {
        var tepi = new GameObject("Penanda Tepi Arena").transform;
        tepi.SetParent(lantai.transform, false);
        Bounds batas = lantai.GetComponent<MeshFilter>().sharedMesh.bounds;
        Vector3 skalaLantai = lantai.transform.lossyScale;
        float lebarX = 0.11f / Mathf.Abs(skalaLantai.x);
        float lebarZ = 0.11f / Mathf.Abs(skalaLantai.z);
        float tinggi = 0.014f / Mathf.Abs(skalaLantai.y);
        float y = batas.max.y + tinggi;

        string jalurMaterial = Root + "/TepiArena.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(jalurMaterial);
        if (material == null)
        {
            string shader = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
                ? "Universal Render Pipeline/Unlit"
                : "Unlit/Color";
            material = new Material(Shader.Find(shader)) { color = new Color32(145, 209, 221, 255) };
            AssetDatabase.CreateAsset(material, jalurMaterial);
        }

        void Tambah(string nama, Vector3 posisi, Vector3 ukuran)
        {
            var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = nama;
            strip.transform.SetParent(tepi, false);
            strip.transform.localPosition = posisi;
            strip.transform.localScale = ukuran;
            UnityEngine.Object.DestroyImmediate(strip.GetComponent<Collider>());
            var renderer = strip.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        float x = batas.center.x;
        float z = batas.center.z;
        Tambah("Depan", new Vector3(x, y, batas.max.z - lebarZ * 0.5f), new Vector3(batas.size.x, tinggi, lebarZ));
        Tambah("Belakang", new Vector3(x, y, batas.min.z + lebarZ * 0.5f), new Vector3(batas.size.x, tinggi, lebarZ));
        var sisi = new Vector3(lebarX, tinggi, batas.size.z - 2f * lebarZ);
        Tambah("Kiri", new Vector3(batas.min.x + lebarX * 0.5f, y, z), sisi);
        Tambah("Kanan", new Vector3(batas.max.x - lebarX * 0.5f, y, z), sisi);
    }


    internal static void TambahkanLayarHasil(SistemGame sistem)
    {
        var referensi = new SerializedObject(sistem);
        var hud = (Text)referensi.FindProperty("hud").objectReferenceValue;
        Canvas canvas = hud.canvas;
        if (!canvas.TryGetComponent<GraphicRaycaster>(out _))
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        var latar = BuatLatarModal("Layar Hasil Ronde", canvas);
        var panel = BuatGambar("Panel Hasil", latar, new Color32(22, 37, 51, 255));
        AturTengah(panel.rectTransform, new Vector2(400f, 250f), Vector2.zero);
        var tepi = panel.gameObject.AddComponent<Outline>();
        tepi.effectColor = new Color32(104, 155, 171, 255);
        tepi.effectDistance = new Vector2(1f, -1f);

        Transform isi = panel.transform;
        Text judul = BuatTeksHasil("Judul", isi, "Kamu Menang", 36, FontStyle.Bold, new Color32(255, 223, 134, 255),
            new Vector2(350f, 52f), new Vector2(0f, 72f));
        Text ringkasan = BuatTeksHasil("Ringkasan Ronde", isi, string.Empty, 20, FontStyle.Normal, new Color32(231, 242, 249, 255),
            new Vector2(320f, 64f), new Vector2(0f, 8f));

        Button tombol = BuatTombol("Main Lagi", isi, new Vector2(204f, 46f), new Vector2(0f, -74f), true);
        UnityEventTools.AddPersistentListener(tombol.onClick, sistem.MainLagi);
        var layar = latar.gameObject.AddComponent<LayarHasilRonde>();
        IsiReferensi(layar, "judul", judul);
        IsiReferensi(layar, "ringkasan", ringkasan);
        IsiReferensi(layar, "tombolMainLagi", tombol);
        IsiReferensi(sistem, "layarHasil", layar);
        TambahkanTombolMenu(layar);
        latar.gameObject.SetActive(false);
    }

    [MenuItem("Septalabs/Pasang Menu Jeda")]
    public static void PasangMenuJeda()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene arena = EditorSceneManager.OpenScene(ScenePath);
        var sistem = UnityEngine.Object.FindFirstObjectByType<SistemGame>();
        if (TambahkanMenuJeda(sistem) && !EditorSceneManager.SaveScene(arena))
        {
            throw new InvalidOperationException("Menu jeda gagal disimpan di arena.");
        }
    }

    private static bool TambahkanMenuJeda(SistemGame sistem)
    {
        if (sistem.TryGetComponent<JedaRonde>(out _))
        {
            return false;
        }

        var hud = (Text)new SerializedObject(sistem).FindProperty("hud").objectReferenceValue;
        RectTransform latar = BuatLatarModal("Layar Jeda", hud.canvas);
        var panel = BuatGambar("Panel Jeda", latar, new Color32(22, 37, 51, 255)).rectTransform;
        AturTengah(panel, new Vector2(400f, 260f), Vector2.zero);
        BuatTeksHasil("Judul", panel, "Game Dijeda", 34, FontStyle.Bold, new Color32(231, 242, 249, 255),
            new Vector2(350f, 48f), new Vector2(0f, 83f));
        BuatTeksHasil("Petunjuk", panel, "Tekan Esc untuk melanjutkan", 18, FontStyle.Normal, new Color32(176, 200, 212, 255),
            new Vector2(350f, 28f), new Vector2(0f, 46f));

        Button lanjut = BuatTombol("Lanjutkan", panel, new Vector2(230f, 46f), new Vector2(0f, -17f), true);
        Button kembali = BuatTombol("Kembali ke Menu", panel, new Vector2(230f, 46f), new Vector2(0f, -75f), false);
        var jeda = sistem.gameObject.AddComponent<JedaRonde>();
        IsiReferensi(jeda, "panel", latar.gameObject);
        IsiReferensi(jeda, "tombolLanjutkan", lanjut);
        UnityEventTools.AddPersistentListener(lanjut.onClick, jeda.Lanjutkan);
        UnityEventTools.AddPersistentListener(kembali.onClick, sistem.GetComponent<NavigasiScene>().KembaliKeMenu);
        HubungkanKlikGameView(latar.gameObject, lanjut, kembali);
        latar.gameObject.SetActive(false);
        return true;
    }

    private static RectTransform BuatLatarModal(string nama, Canvas canvas)
    {
        RectTransform latar = BuatGambar(nama, canvas.transform, new Color32(8, 16, 25, 196)).rectTransform;
        latar.anchorMin = Vector2.zero;
        latar.anchorMax = Vector2.one;
        latar.offsetMin = latar.offsetMax = Vector2.zero;
        return latar;
    }

    internal static bool TambahkanTombolMenu(LayarHasilRonde layar)
    {
        if (layar.TryGetComponent<KlikTombolGameView>(out _))
        {
            return false;
        }

        var data = new SerializedObject(layar);
        var judul = (Text)data.FindProperty("judul").objectReferenceValue;
        var ringkasan = (Text)data.FindProperty("ringkasan").objectReferenceValue;
        var utama = (Button)data.FindProperty("tombolMainLagi").objectReferenceValue;
        var panel = (RectTransform)utama.transform.parent;
        panel.sizeDelta = new Vector2(400f, 320f);
        judul.rectTransform.anchoredPosition = new Vector2(0f, 104f);
        ringkasan.rectTransform.anchoredPosition = new Vector2(0f, 35f);
        AturTengah((RectTransform)utama.transform, new Vector2(204f, 46f), new Vector2(0f, -37f));

        Button kembali = BuatTombol("Kembali ke Menu", panel, new Vector2(204f, 46f), new Vector2(0f, -95f), false);
        var navigasi = layar.gameObject.AddComponent<NavigasiScene>();
        UnityEventTools.AddPersistentListener(kembali.onClick, navigasi.KembaliKeMenu);
        HubungkanKlikGameView(layar.gameObject, utama, kembali);
        return true;
    }

    internal static Button BuatTombol(string label, Transform induk, Vector2 ukuran, Vector2 posisi, bool utama)
    {
        var gambarTombol = BuatGambar(label, induk, Color.white);
        AturTengah(gambarTombol.rectTransform, ukuran, posisi);
        var tombol = gambarTombol.gameObject.AddComponent<Button>();
        tombol.targetGraphic = gambarTombol;
        var warna = tombol.colors;
        warna.normalColor = utama ? new Color32(113, 229, 201, 255) : new Color32(43, 66, 82, 255);
        warna.highlightedColor = utama ? new Color32(153, 244, 222, 255) : new Color32(57, 84, 101, 255);
        warna.selectedColor = warna.highlightedColor;
        warna.pressedColor = utama ? new Color32(81, 193, 169, 255) : new Color32(63, 95, 111, 255);
        tombol.colors = warna;
        Color warnaLabel = utama ? new Color32(15, 43, 49, 255) : new Color32(231, 242, 249, 255);
        BuatTeksHasil("Label", tombol.transform, label, 20, FontStyle.Bold, warnaLabel, ukuran, Vector2.zero);
        return tombol;
    }

    internal static void HubungkanKlikGameView(GameObject panel, params Button[] tombol)
    {
        var data = new SerializedObject(panel.AddComponent<KlikTombolGameView>());
        var daftar = data.FindProperty("tombol");
        daftar.arraySize = tombol.Length;
        for (int i = 0; i < tombol.Length; i++)
        {
            daftar.GetArrayElementAtIndex(i).objectReferenceValue = tombol[i];
        }
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    internal static Image BuatGambar(string nama, Transform induk, Color warna)
    {
        var gambar = new GameObject(nama, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        gambar.transform.SetParent(induk, false);
        gambar.color = warna;
        return gambar;
    }

    internal static Text BuatTeksHasil(string nama, Transform induk, string label, int ukuranFont, FontStyle gaya,
        Color warna, Vector2 ukuran, Vector2 posisi)
    {
        var teks = new GameObject(nama, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        teks.transform.SetParent(induk, false);
        AturTengah(teks.rectTransform, ukuran, posisi);
        teks.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        teks.fontSize = ukuranFont;
        teks.fontStyle = gaya;
        teks.alignment = TextAnchor.MiddleCenter;
        teks.color = warna;
        teks.raycastTarget = false;
        teks.text = label;
        return teks;
    }

    internal static void AturTengah(RectTransform rect, Vector2 ukuran, Vector2 posisi)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = ukuran;
        rect.anchoredPosition = posisi;
    }


    private static GameObject Primitif(PrimitiveType bentuk, string nama, Vector3 posisi, Vector3 skala)
    {
        GameObject objek = GameObject.CreatePrimitive(bentuk);
        objek.name = nama;
        objek.transform.position = posisi;
        objek.transform.localScale = skala;
        return objek;
    }

    private static void Warnai(GameObject objek, string nama, Color warna)
    {
        string path = $"{Root}/{nama}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/Lit")
                : Shader.Find("Standard");
            material = new Material(shader) { color = warna };
            AssetDatabase.CreateAsset(material, path);
        }

        objek.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void IsiReferensi(UnityEngine.Object komponen, string field, UnityEngine.Object nilai)
    {
        var serialized = new SerializedObject(komponen);
        serialized.FindProperty(field).objectReferenceValue = nilai;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
