using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
public sealed class MusuhArena : MonoBehaviour
{
    [SerializeField, Min(0f)] private float jarakDeteksi = 4f;
    [SerializeField, Min(0f)] private float jarakKejarDariAwal = 7f;

    [SerializeField, Min(1)] private int hpMaksimum = 3;

    public int HP { get; private set; }
    public int HPMaksimum => hpMaksimum;

    private const float JarakBerhenti = 0.82f;

    private enum Keadaan { Siaga, Mengejar, Kembali }

    private NavMeshAgent agen;
    private SistemGame sistem;
    private KarakterJalan pemain;
    private Vector3 posisiAwal;
    private Quaternion rotasiAwal;
    private Keadaan keadaan;
    private float waktuPerbaruiJalur;

    public void Siapkan(SistemGame pengelola, KarakterJalan target)
    {
        sistem = pengelola;
        HP = hpMaksimum;
        pemain = target;
        posisiAwal = transform.position;
        rotasiAwal = transform.rotation;
        agen = GetComponent<NavMeshAgent>();
        agen.enabled = true;
        if (!agen.isOnNavMesh)
        {
            Debug.LogError("Musuh tidak berada di area navigasi arena.", this);
            agen.enabled = false;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f || agen == null || !agen.isOnNavMesh || agen.isStopped)
        {
            return;
        }

        if (keadaan == Keadaan.Kembali)
        {
            if (!agen.pathPending && agen.remainingDistance <= agen.stoppingDistance)
            {
                agen.ResetPath();
                keadaan = Keadaan.Siaga;
                Debug.Log("Musuh kembali ke posisi awal.", this);
            }

            return;
        }

        Vector3 posisiTarget = pemain.transform.position;
        posisiTarget.y = posisiAwal.y;
        float jarakTargetKuadrat = (posisiTarget - transform.position).sqrMagnitude;
        bool dalamJangkauan = jarakTargetKuadrat <= jarakDeteksi * jarakDeteksi
            && (posisiTarget - posisiAwal).sqrMagnitude <= jarakKejarDariAwal * jarakKejarDariAwal;
        if (!dalamJangkauan)
        {
            if (keadaan == Keadaan.Mengejar)
            {
                keadaan = Keadaan.Kembali;
                agen.stoppingDistance = 0.1f;
                agen.SetDestination(posisiAwal);
                Debug.Log("Musuh kehilangan pemain dan kembali ke posisi awal.", this);
            }

            return;
        }

        if (keadaan == Keadaan.Siaga)
        {
            keadaan = Keadaan.Mengejar;
            agen.stoppingDistance = JarakBerhenti;
            waktuPerbaruiJalur = Time.time;
            Debug.Log("Musuh mendeteksi dan mengejar pemain.", this);
        }

        if (jarakTargetKuadrat <= JarakBerhenti * JarakBerhenti)
        {
            if (agen.hasPath || agen.pathPending)
            {
                agen.ResetPath();
                agen.velocity = Vector3.zero;
            }

            return;
        }


        if (Time.time >= waktuPerbaruiJalur)
        {
            waktuPerbaruiJalur = Time.time + 0.15f;
            if (NavMesh.SamplePosition(posisiTarget, out NavMeshHit titik, 0.9f, agen.areaMask))
            {
                agen.SetDestination(titik.position);
            }
            else
            {
                agen.ResetPath();
            }
        }
    }

    public void TerimaTembakan()
    {
        if (Time.timeScale == 0f || HP <= 0)
        {
            return;
        }

        HP--;
        Debug.Log($"{name} terkena proyektil. HP: {HP}/{hpMaksimum}.", this);
        if (HP == 0)
        {
            Hentikan();
            Debug.Log($"{name} dikalahkan.", this);
            gameObject.SetActive(false);
        }

        sistem.CatatMusuhTerkena();
    }


    public void Hentikan()
    {
        if (agen != null && agen.isOnNavMesh)
        {
            agen.ResetPath();
            agen.isStopped = true;
        }
    }

    public void KembaliKeAwal()
    {
        HP = hpMaksimum;
        if (!gameObject.activeSelf)
        {
            transform.SetPositionAndRotation(posisiAwal, rotasiAwal);
            gameObject.SetActive(true);
        }

        if (agen == null || !agen.isOnNavMesh)
        {
            return;
        }

        agen.ResetPath();
        agen.Warp(posisiAwal);
        transform.rotation = rotasiAwal;
        agen.isStopped = false;
        keadaan = Keadaan.Siaga;
    }

    private void OnTriggerStay(Collider lainnya)
    {
        if (sistem != null && lainnya.attachedRigidbody != null
            && lainnya.attachedRigidbody.gameObject == pemain.gameObject)
        {
            sistem.CobaBeriDamage(this);
        }
    }
}
