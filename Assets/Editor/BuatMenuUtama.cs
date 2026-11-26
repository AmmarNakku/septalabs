using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static BuatArenaKetangkasan;

public static class BuatMenuUtama
{
    private const string Pratinjau = "Assets/ArenaKetangkasan/PratinjauMenu.renderTexture";
    private static readonly Color Latar = new Color32(12, 23, 33, 255);
    private static readonly Color Permukaan = new Color32(22, 37, 51, 255);
    private static readonly Color Tepi = new Color32(51, 75, 90, 255);
    private static readonly Color Terang = new Color32(234, 246, 249, 255);
    private static readonly Color Aksen = new Color32(113, 229, 201, 255);
    private static readonly Color Redup = new Color32(176, 200, 212, 255);

    [MenuItem("Septalabs/Buat Menu Utama")]
    public static void Buat()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene arena = EditorSceneManager.OpenScene(NavigasiScene.Arena);
        var hasil = UnityEngine.Object.FindFirstObjectByType<LayarHasilRonde>(FindObjectsInactive.Include);
        if (hasil == null)
        {
            throw new InvalidOperationException("Layar hasil ronde tidak ditemukan di arena.");
        }

        if (TambahkanTombolMenu(hasil) && !EditorSceneManager.SaveScene(arena))
        {
            throw new InvalidOperationException("Perubahan di arena gagal disimpan.");
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(NavigasiScene.Menu) == null)
        {
            BuatSceneMenu();
            AssetDatabase.ImportAsset(NavigasiScene.Menu, ImportAssetOptions.ForceSynchronousImport);
        }

        AturDaftarScene();
        EditorSceneManager.OpenScene(NavigasiScene.Menu);
    }

    internal static void AturDaftarScene()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(entry => entry.path == NavigasiScene.Menu || entry.path == NavigasiScene.Arena);
        scenes.Insert(0, new EditorBuildSettingsScene(NavigasiScene.Arena, true));
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(NavigasiScene.Menu) != null)
        {
            scenes.Insert(0, new EditorBuildSettingsScene(NavigasiScene.Menu, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void BuatSceneMenu()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var ui = new GameObject("Menu Utama", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var skala = ui.GetComponent<CanvasScaler>();
        skala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        skala.referenceResolution = new Vector2(960f, 540f);
        skala.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        var latar = BuatGambar("Latar", ui.transform, Latar).rectTransform;
        latar.anchorMin = Vector2.zero;
        latar.anchorMax = Vector2.one;
        latar.offsetMin = latar.offsetMax = Vector2.zero;

        Kotak("Garis Judul", latar, Aksen, new Vector2(44f, 3f), new Vector2(-398f, 202f));
        TeksKiri("Judul", latar, "Arena\nKetangkasan", 47, FontStyle.Bold, Terang, new Vector2(397f, 116f), new Vector2(-221.5f, 131f));
        TeksKiri("Tujuan", latar, "Kumpulkan semua koin dan kalahkan musuh.", 17, FontStyle.Normal, Redup,
            new Vector2(390f, 34f), new Vector2(-225f, 51f));

        var kontrol = Kotak("Panel Kontrol", latar, Permukaan, new Vector2(390f, 178f), new Vector2(-225f, -71f));
        TeksKiri("Judul Kontrol", kontrol, "Kontrol", 18, FontStyle.Bold, Terang, new Vector2(350f, 22f), new Vector2(0f, 64f));
        Kotak("Garis Kontrol", kontrol, Tepi, new Vector2(350f, 1f), new Vector2(0f, 45f));
        BarisKontrol(kontrol, "WASD / Panah", "Bergerak", 28f);
        BarisKontrol(kontrol, "Spasi", "Lompat", 5f);
        BarisKontrol(kontrol, "Klik kiri", "Tembak", -18f);
        BarisKontrol(kontrol, "Tahan klik kanan + geser", "Putar kamera", -41f);
        BarisKontrol(kontrol, "Scroll mouse", "Zoom kamera", -64f);

        Button mulai = BuatTombol("Mulai", latar, new Vector2(390f, 54f), new Vector2(-225f, -205f), true);
        var navigasi = ui.AddComponent<NavigasiScene>();
        UnityEventTools.AddPersistentListener(mulai.onClick, navigasi.Mulai);
        HubungkanKlikGameView(ui, mulai);
        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<EventSystem>().firstSelectedGameObject = mulai.gameObject;

        var bingkai = Kotak("Bingkai Pratinjau", latar, Tepi, new Vector2(420f, 420f), new Vector2(213f, 0f));
        var gambar = new GameObject("Pratinjau Arena", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        gambar.transform.SetParent(bingkai, false);
        AturTengah(gambar.rectTransform, new Vector2(418f, 418f), Vector2.zero);
        gambar.raycastTarget = false;
        gambar.texture = BuatDiorama();

        if (!EditorSceneManager.SaveScene(scene, NavigasiScene.Menu))
        {
            throw new InvalidOperationException("Scene MenuUtama gagal disimpan.");
        }
    }

    private static void BarisKontrol(Transform panel, string tombol, string aksi, float y)
    {
        TeksKiri(tombol, panel, tombol, 14, FontStyle.Bold, Aksen, new Vector2(215f, 21f), new Vector2(-67.5f, y));
        TeksKiri(aksi, panel, aksi, 14, FontStyle.Normal, Redup, new Vector2(131f, 21f), new Vector2(109.5f, y));
    }

    private static void TeksKiri(string nama, Transform induk, string label, int ukuran, FontStyle gaya,
        Color warna, Vector2 dimensi, Vector2 posisi)
    {
        Text teks = BuatTeksHasil(nama, induk, label, ukuran, gaya, warna, dimensi, posisi);
        teks.alignment = TextAnchor.MiddleLeft;
    }

    private static RectTransform Kotak(string nama, Transform induk, Color warna, Vector2 ukuran, Vector2 posisi)
    {
        Image gambar = BuatGambar(nama, induk, warna);
        gambar.raycastTarget = false;
        AturTengah(gambar.rectTransform, ukuran, posisi);
        return gambar.rectTransform;
    }

    private static RenderTexture BuatDiorama()
    {
        var tekstur = AssetDatabase.LoadAssetAtPath<RenderTexture>(Pratinjau);
        if (tekstur == null)
        {
            tekstur = new RenderTexture(512, 512, 24) { antiAliasing = 4 };
            AssetDatabase.CreateAsset(tekstur, Pratinjau);
        }

        var layar = new GameObject("Kamera Menu", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        layar.cullingMask = 0;
        layar.clearFlags = CameraClearFlags.SolidColor;
        layar.backgroundColor = Latar;

        var kamera = new GameObject("Kamera Diorama", typeof(Camera)).GetComponent<Camera>();
        kamera.depth = -1f;
        kamera.clearFlags = CameraClearFlags.SolidColor;
        kamera.backgroundColor = new Color32(19, 35, 47, 255);
        kamera.orthographic = true;
        kamera.orthographicSize = 6.65f;
        kamera.transform.position = new Vector3(11f, 12f, -15f);
        kamera.transform.LookAt(new Vector3(0f, 0.4f, 0f));
        kamera.targetTexture = tekstur;

        var cahaya = new GameObject("Cahaya Diorama", typeof(Light)).GetComponent<Light>();
        cahaya.type = LightType.Directional;
        cahaya.color = new Color32(255, 241, 216, 255);
        cahaya.intensity = 2.2f;
        cahaya.shadows = LightShadows.Soft;
        cahaya.transform.rotation = Quaternion.Euler(54f, -40f, 0f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.49f, 0.54f, 0.60f);

        Transform diorama = new GameObject("Diorama Arena").transform;
        TambahModel(diorama, PrimitiveType.Cube, "Lantai", new Vector3(0f, -0.45f, 0f), new Vector3(8.8f, 0.9f, 8.8f));
        TambahModel(diorama, PrimitiveType.Cube, "TepiArena", new Vector3(0f, -0.01f, 0f), new Vector3(8.86f, 0.05f, 8.86f));
        TambahModel(diorama, PrimitiveType.Cube, "Lantai", new Vector3(0f, 0.02f, 0f), new Vector3(8.62f, 0.06f, 8.62f));
        TambahModel(diorama, PrimitiveType.Cube, "Rintangan", new Vector3(-1.6f, 0.6f, 0.5f), new Vector3(3.3f, 1.15f, 0.85f));
        TambahModel(diorama, PrimitiveType.Cube, "Rintangan", new Vector3(2.25f, 0.75f, 1.7f), new Vector3(1.2f, 1.45f, 2.65f));
        TambahModel(diorama, PrimitiveType.Cube, "Player", new Vector3(-0.9f, 0.6f, -2.45f), new Vector3(1.1f, 1.1f, 1.1f));
        TambahModel(diorama, PrimitiveType.Capsule, "Musuh", new Vector3(-1.7f, 0.86f, 2.65f), new Vector3(1.1f, 0.8f, 1.1f));
        for (int i = 0; i < 3; i++)
        {
            GameObject koin = TambahModel(diorama, PrimitiveType.Cylinder, "Koin", new Vector3(0.85f + 0.88f * i, 0.48f, -1.85f),
                new Vector3(0.68f, 0.10f, 0.68f));
            koin.transform.localRotation = Quaternion.Euler(90f, -24f, 0f);
        }

        return tekstur;
    }

    private static GameObject TambahModel(Transform induk, PrimitiveType bentuk, string bahan, Vector3 posisi, Vector3 ukuran)
    {
        GameObject model = GameObject.CreatePrimitive(bentuk);
        model.name = bahan;
        model.transform.SetParent(induk, false);
        model.transform.localPosition = posisi;
        model.transform.localScale = ukuran;
        UnityEngine.Object.DestroyImmediate(model.GetComponent<Collider>());
        model.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>($"Assets/ArenaKetangkasan/{bahan}.mat");
        return model;
    }
}
