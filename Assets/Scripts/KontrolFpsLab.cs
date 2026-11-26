// Ammar Mohamad Qatar - 2491345011 - GJ24B
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public sealed class KontrolFpsLab : MonoBehaviour
{
    [SerializeField] private Camera kameraPemain;
    [SerializeField, Min(0f)] private float kecepatanJalan = 4f;
    [SerializeField, Min(0f)] private float sensitivitasMouse = 0.12f;

    private CharacterController pengendali;
    private float kecepatanVertikal;
    private float pitch;
#if UNITY_EDITOR
    private Vector2 deltaMouseEditor;
#endif

    private void Awake()
    {
        pengendali = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Input.GetMouseButtonDown(0) && Application.isFocused)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        Vector2 delta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * (sensitivitasMouse * 16f);
#if UNITY_EDITOR
        if (deltaMouseEditor != Vector2.zero)
        {
            delta = deltaMouseEditor * sensitivitasMouse;
        }
        deltaMouseEditor = Vector2.zero;
#endif
        if (Cursor.lockState == CursorLockMode.Locked && kameraPemain != null)
        {
            transform.Rotate(0f, delta.x, 0f);
            pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
            kameraPemain.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        Vector2 arah = Vector2.zero;
        if (Application.isFocused)
        {
            arah.x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
            arah.y = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
            arah = Vector2.ClampMagnitude(arah, 1f);
        }

        float deltaWaktu = Time.deltaTime;
        if (pengendali.isGrounded && kecepatanVertikal < 0f)
        {
            kecepatanVertikal = -2f;
        }

        kecepatanVertikal += Physics.gravity.y * deltaWaktu;
        Vector3 gerak = (transform.right * arah.x + transform.forward * arah.y) * kecepatanJalan;
        gerak.y = kecepatanVertikal;
        pengendali.Move(gerak * deltaWaktu);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        Event input = Event.current;
        switch (input.rawType)
        {
            case EventType.MouseDown when input.button == 0 && Application.isFocused:
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case EventType.KeyDown when input.keyCode == KeyCode.Escape:
                Cursor.lockState = CursorLockMode.None;
                break;
            case EventType.MouseMove:
            case EventType.MouseDrag:
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    deltaMouseEditor += new Vector2(input.delta.x, -input.delta.y);
                }
                break;
        }
    }
#endif

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
