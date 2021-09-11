using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
public class CameraManagerScript : MonoBehaviour
{
    [SerializeField] private Transform cameraParent;
    [SerializeField] private CinemachineVirtualCamera playerCam;
    [SerializeField] private CinemachineVirtualCamera panCam;
    [SerializeField] private CinemachineVirtualCamera selectedCam;
    [SerializeField] private CinemachineVirtualCamera unitCam1;
    [SerializeField] private CinemachineVirtualCamera staticCam;
    [SerializeField] public CinemachineBrain cameraBrain;
    [SerializeField] private MouseTarget mouseTarget;

    public float multiplier { get; private set; }
    private bool cardSelected;
    private Vector3 mouseScreenPos = Vector3.negativeInfinity;
    private Vector3 delta;
    private float clicked = 0;
    private float clicktime = 0;
    private float clickDelay = 0.2f;
    bool doubleClick = false;
    bool toggle = false;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Camera.main.transparencySortMode = TransparencySortMode.Orthographic;
        cameraBrain.m_DefaultBlend.m_Time *= Globals.AnimationMultiplier;
        for (int i = 0; i < cameraBrain.m_CustomBlends.m_CustomBlends.Length; i++)
        {
            cameraBrain.m_CustomBlends.m_CustomBlends[i].m_Blend.m_Time *= Globals.AnimationMultiplier;
        }
    }
    private void OnApplicationFocus(bool focus)
    {
        mouseScreenPos = Input.mousePosition;
        toggle = true;
    }
    private void LateUpdate()
    {
        if (toggle)
        {
            mouseScreenPos = Input.mousePosition;
            toggle = false;
        }
        if (mouseScreenPos != Vector3.negativeInfinity)
        {
            delta = Input.mousePosition - mouseScreenPos;
            delta.z = 0;
        }
        mouseScreenPos = Input.mousePosition;
        multiplier = Camera.main.orthographicSize * 2f / Screen.height;
        if (!Globals.IsGamePaused)
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                IncrementOrthoSize(Input.mouseScrollDelta.y * -2f);
            }
            if (cardSelected)
            {
                UpdateCardSelectedCamera();
            }
            Debug.DrawLine(Camera.main.ScreenToWorldPoint(Input.mousePosition), Globals.ScreenToWorld(), Color.magenta);
            if (Input.GetMouseButtonDown(1))
            {
                clicked++;
                if (clicked > 1 && Time.time - clicktime < clickDelay)
                {
                    clicked = 0;
                    clicktime = 0;
                    doubleClick = true;
                    Globals.World.transform.parent.eulerAngles = new Vector3(-30, 0, 0);
                }
                else
                {
                    if (clicked == 1)
                    {
                        clicktime = Time.time;
                    }
                    if (clicked > 2 || Time.time - clicktime > clickDelay)
                    {
                        clicked = 1;
                        clicktime = Time.time;
                    }
                }
            }
            if (Input.GetMouseButton(1) && !doubleClick)
            {
                Vector3 target = new Vector3(Globals.World.transform.parent.eulerAngles.x + delta.y * 0.1f, Globals.World.transform.parent.eulerAngles.y - delta.x * 0.1f, 0); //Globals.World.transform.parent.eulerAngles.y - delta.x * 0.1f for y 
                if (target.x < 350 && target.x > 280)
                {
                    Globals.World.transform.parent.eulerAngles = target;
                }
            }
            if (Input.GetMouseButtonUp(1) && doubleClick)
            {
                doubleClick = false;
            }
        }
    }
    public void IncrementOrthoSize(float incremement)
    {
        playerCam.m_Lens.OrthographicSize += incremement;
        panCam.m_Lens.OrthographicSize += incremement;
        selectedCam.m_Lens.OrthographicSize += incremement;
        unitCam1.m_Lens.OrthographicSize += incremement;
        staticCam.m_Lens.OrthographicSize += incremement;
    }
    public void UpdatePanCamera()
    {
        Plane plane = new Plane(Globals.World.transform.up, Vector3.zero);
        Ray ray = new Ray(delta * multiplier, Camera.main.transform.forward);
        plane.Raycast(ray, out float enter);
        mouseTarget.transform.position = mouseTarget.transform.position - ray.GetPoint(enter);
    }
    public void UpdateCardSelectedCamera()
    {
        Plane plane = new Plane(Globals.World.transform.up, Vector3.zero);
        Ray ray = new Ray(delta * multiplier, Camera.main.transform.forward);
        plane.Raycast(ray, out float enter);
        mouseTarget.transform.position = mouseTarget.transform.position + ray.GetPoint(enter);
    }
    private void ActivateMouseTarget()
    {
        if (cameraBrain.IsBlending)
        {
            mouseTarget.transform.position = Globals.CameraPosToWorld(Camera.main.WorldToScreenPoint(cameraBrain.CurrentCameraState.CorrectedPosition));
            Debug.DrawLine(Vector3.zero, mouseTarget.transform.position, Color.green, 20f);
        }
        else
        {
            mouseTarget.transform.position = Globals.CameraPosToWorld(Camera.main.WorldToScreenPoint(GetCurrentCamera().transform.position));
            Debug.DrawLine(Vector3.zero, mouseTarget.transform.position, Color.blue, 20f);
        }
        mouseTarget.enabled = true;
    }
    public void ActivateSelectedCam()
    {
        ActivateMouseTarget();
        selectedCam.Priority = 12;
        cardSelected = true;
        selectedCam.enabled = true;
    }
    public void CardDeselected()
    {
        selectedCam.Priority = 8;
        cardSelected = false;
        mouseTarget.enabled = false;
        selectedCam.enabled = false;
    }
    public bool CanUpdate()
    {
        return !cardSelected && !EventSystem.current.IsPointerOverGameObject();
    }
    public void ActivatePanCam()
    {
        if (!cardSelected)
        {
            ActivateMouseTarget();
            ActivateCamera(panCam);
        }
    }
    public void SetupPlayerCam(Transform playerTransform)
    {
        playerCam.Follow = playerTransform;
    }
    public void ActivatePlayerCam()
    {
        ActivateCamera(playerCam);
    }
    public void ActivateUnitCam(Transform unitToFollow)
    {
        if (cameraBrain.IsBlending)
        {
            staticCam.transform.position = cameraBrain.CurrentCameraState.CorrectedPosition;
        }
        else
        {
            staticCam.transform.position = GetCurrentCamera().transform.position;
        }
        StartCoroutine(DelayedUnitCamera(unitToFollow));
    }
    private IEnumerator DelayedUnitCamera(Transform unitToFollow)
    {
        ActivateCamera(staticCam);
        yield return new WaitForSecondsRealtime(0.1f);
        unitCam1.Follow = unitToFollow;
        ActivateCamera(unitCam1);
    }
    public IEnumerator WaitForCameraBlend()
    {
        yield return new WaitForSeconds(0.01f);
        yield return new WaitWhile(() => cameraBrain.IsBlending);
    }
    private void ActivateCamera(CinemachineVirtualCamera camera)
    {
        if (GetCurrentCamera() != camera.gameObject)
        {
            cameraBrain.ActiveVirtualCamera.Priority = 9;
        }
        camera.Priority = 11;
    }
    public GameObject GetCurrentCamera()
    {
        return cameraBrain.ActiveVirtualCamera.VirtualCameraGameObject;
    }
}
