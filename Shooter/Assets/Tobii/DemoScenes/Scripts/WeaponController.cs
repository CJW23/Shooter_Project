//-----------------------------------------------------------------------
// Copyright 2016 Tobii AB (publ). All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Tobii.Gaming;

/*
 * Aim At Gaze
 *
 * Aim At Gaze helps to decouple the aim direction from the view direction, allowing the player to fire in non-forward directions. 
 * This also helps to reduce screen movements which can be less disorienting, especially when you need to align your gun quickly for your next shot.
 * This component requies Extended View Component in order to do camera transitions.
 */
public class WeaponController : MonoBehaviour
{
    //This is where we are currently aiming in world space (the location we are aiming at)
    public Vector3 AimedAtLocation
    {
        get
        {
            return IsShootingAtGaze ? WorldGazeCrosshairTransformProjected.position : WorldCenterCrosshairTransformProjected.position;
        }
    }
    public bool ShowCrosshair = true;
    public bool IsAiming { get; protected set; }
    public bool IsShooting { get; protected set; }
    public bool IsShootingAtGaze { get; protected set; }
    //This is where we will shoot from. Usually the camera, but can be overridden. If overridden, it also enables the collision crosshair
    //since if the fire ray is not coincident with the camera normal, we can run into collision problems
    public Transform OptionalWeaponFireOriginOverride;

    //This is where any fired projectiles will hit
    public bool IsWeaponHitObject { get; private set; }
    public RaycastHit WeaponHitData { get; private set; }

    //If the path to our aimed position is blocked, we should display this crosshair
    public Image AimBlockedCrosshairImage;
    //This margin prevents the crosshair from being placed outside the screen, or too close to the edge.
    public float ScreenEdgeAimMarginPx = 40;
    //We need this to compensate for pre-saccade activation (the player pressed the aim button before the eye has started moving).
    public float AimDelaySecs = 0.02f;
    //If we have not gotten a new fixation before this time, simply use the center
    public float AimTimeoutTimeSecs = 0.7f;
    //If the gaze is within this deadzone in the center of the screen, we will not shift the crosshair
    public float NoAimShiftCenterDeadZoneRadius = 0.05f;
    //Filter gaze data for primary fire at gaze
    public float GazeFilterStrength = 1.0f;
    //Allow disabling
    public bool IsEnabled = true;

    public const float MaxProjectionDistance = 100000f;
    public const int RaycastLayerMask = ~0x24;//0b100100;           // ignore "ignore raycast" and "ui" layers
    public ExtendedViewBase ExtendedView;
    public GazePlotter GazePlotter;
    public Camera MainCamera;
    public Camera WeaponCamera;
    public float AimedFieldOfView = 1000;
    public float NormalFieldOfView = 50;

    protected Transform WorldCenterCrosshairTransformProjected;
    protected Transform WorldCenterCrosshairTransformFixed;
    protected Transform rotation;

    protected Transform WorldGazeCrosshairTransformProjected;
    protected Transform WorldGazeCrosshairTransformFixed;

    private Vector2 _screenCenterCrosshairPosition;
    private float _aimRequestTime;
    private bool _isRequestingAim;
    private Vector2 _filteredGazePoint;
    private bool _calculatedThisFrame;



    protected virtual void Start()
    {
        if (ExtendedView == null)
        {
            Debug.LogError("Missing Extended view component!");
        }
        GazePlotter = GameObject.Find("GazePlot").GetComponent<GazePlotter>();
        WorldCenterCrosshairTransformProjected = new GameObject("WorldCenterCrosshairProjected").transform;
        WorldCenterCrosshairTransformProjected.transform.parent = null;
        rotation = new GameObject("PlayerObject").transform;
        WorldCenterCrosshairTransformFixed = new GameObject("WorldCenterCrosshairFixed").transform;
        WorldCenterCrosshairTransformFixed.transform.parent = ExtendedView.CameraWithoutExtendedView.transform;
        WorldCenterCrosshairTransformFixed.transform.localPosition = Vector3.forward;

        WorldGazeCrosshairTransformProjected = new GameObject("WorldGazeCrosshairProjected").transform;
        WorldGazeCrosshairTransformProjected.transform.parent = null;

        WorldGazeCrosshairTransformFixed = new GameObject("WorldGazeCrosshairFixed").transform;
        WorldGazeCrosshairTransformFixed.transform.parent = ExtendedView.CameraWithoutExtendedView.transform;
        WorldGazeCrosshairTransformFixed.transform.localPosition = Vector3.forward;

        _screenCenterCrosshairPosition = new Vector2(0.5f, 0.5f);

        _filteredGazePoint = new Vector2(Screen.width, Screen.height) * 0.5f;
    }

    private void Update()
    {
        _calculatedThisFrame = false;

        if (IsAiming)
        {
            MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, AimedFieldOfView, Time.unscaledDeltaTime * 10);

        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, NormalFieldOfView, Time.unscaledDeltaTime * 5);
        }

        if (WeaponCamera != null)
        {
            WeaponCamera.fieldOfView = MainCamera.fieldOfView;
        }
    }

    public void Calculate()     //총을 쐈을 때
    {
        if (_calculatedThisFrame) return;
        _calculatedThisFrame = true;

        if (IsEnabled)
        {
            var gazePoint = TobiiAPI.GetGazePoint();
            if (gazePoint.IsRecent())
            {
                var w = (float)(1 - GazeFilterStrength * 0.9);
                _filteredGazePoint = _filteredGazePoint + (gazePoint.Screen - _filteredGazePoint) * w;
            }
            else
            {
                _filteredGazePoint = new Vector2(Screen.width, Screen.height) * 0.5f;
            }
        }
        //If the user wants to start aiming, see if we can accommodate
        if (_isRequestingAim)       //줌인을 했을때, 오른쪽 키 누르고 있을 때
        {

            //If we still haven't gotten any gaze data, don't wait anymore and start the aim in the center instead
            if (Time.unscaledTime - _aimRequestTime > AimTimeoutTimeSecs)
            {
                Debug.Log("here");
                IsAiming = true;
                _isRequestingAim = false;
                ExtendedView.AimAtWorldPosition(WorldCenterCrosshairTransformProjected.position);

            }
            else if (Time.unscaledTime - _aimRequestTime > AimDelaySecs)        //정상적인 루트
            {
                IsAiming = true;
                _isRequestingAim = false;

                var gazePoint = TobiiAPI.GetGazePoint();        //시선 좌표 가져오고

                if (gazePoint.IsRecent())       //시선 좌표가 있다면 eyetracking 없으면 안들어감.
                {

                    var normalizedCenteredGazeCoordinates = (gazePoint.Viewport - new Vector2(0.5f, 0.5f)) * 2;
                    Debug.Log("search gaze : " + normalizedCenteredGazeCoordinates);        //현재 화면에서 중앙을 중심으로 내 눈이 있는 위치(중앙을 보고 줌을 하면 (0,0)
                    Debug.Log("search gaze2 : " + normalizedCenteredGazeCoordinates.magnitude);        //현재 화면에서 중앙을 중심으로 내 눈이 있는 위치(중앙을 보고 줌을 하면 (0,0)
                    //Don't aim at gaze if the user is looking close to the center of the screen to avoid the crosshair jumping just a tiny amount
                    if (normalizedCenteredGazeCoordinates.magnitude < NoAimShiftCenterDeadZoneRadius)
                    {

                        ExtendedView.AimAtWorldPosition(WorldCenterCrosshairTransformProjected.position);
                    }
                    else        //정상적인 접근 부분
                    {
                        var viewportCoordinates = ExtendedView.CameraWithExtendedView.WorldToViewportPoint(WorldCenterCrosshairTransformProjected.position);        //카메라
                        var normalizedCoordinatesDelta = (gazePoint.Viewport - new Vector2(viewportCoordinates.x, viewportCoordinates.y)) * 2;

                        if (normalizedCoordinatesDelta.magnitude < NoAimShiftCenterDeadZoneRadius)
                        {
                            Debug.Log("sssss1");
                            ExtendedView.AimAtWorldPosition(WorldCenterCrosshairTransformProjected.position);
                        }
                        else
                        {
                            Debug.Log("ssss2");
                            var aimAtGazeTargetPosition = ScreenToWorldProjection(gazePoint.Screen);
                            Debug.Log(aimAtGazeTargetPosition);
                            ExtendedView.AimAtWorldPosition(aimAtGazeTargetPosition);
                        }
                    }
                }

            }
        }

        CalculateWorldCenterCrosshairPosition();        //이걸 주석 처리하고 눈으로 E가 써지는지 확인
                                                        //UpdateCenterCrosshairScreenPosition();

        if (IsShootingAtGaze)   //E를 눌렀을때 내 눈의 위치에 총을 쏨
        {
            CalculateWorldGazeCrosshairPosition();
        }
        Debug.Log("shot");
        FindHitDataForPotentialOverride();      //별로 관련 없음


    }

    private void CalculateWorldGazeCrosshairPosition()      //E눌렀을때     //여기서 시선 좌표 활용 방법
    {
        //WorldGazeCrosshairTransformFixed = 크로스헤어 오브젝트
        WorldGazeCrosshairTransformFixed.position = ScreenToWorldProjection(_filteredGazePoint);

        RaycastHit hitInfo;
        var direction = WorldGazeCrosshairTransformFixed.position - ExtendedView.CameraWithExtendedView.transform.position;
        direction.Normalize();
        //Debug.Log("direction : " + direction);
        //  Debug.Log("awdsd : " + WorldGazeCrosshairTransformProjected.position);
        // Debug.Log("cross : " + WorldGazeCrosshairTransformFixed.position);
        if (Physics.Raycast(ExtendedView.CameraWithExtendedView.transform.position, direction, out hitInfo, MaxProjectionDistance, RaycastLayerMask))
        {
            Debug.Log("wownice");
            WorldGazeCrosshairTransformProjected.position = hitInfo.point;      //맞은 타겟 정보?
            WeaponHitData = hitInfo;
            IsWeaponHitObject = true;
        }
        else        //안들어가는 부분
        {
            Debug.Log("qqq");
            WorldGazeCrosshairTransformProjected.position = WorldGazeCrosshairTransformFixed.position;

            IsWeaponHitObject = false;
        }
    }

    public void StartAiming()
    {
        transform.Rotate(0, 0, 0);
        //We want to wait for the next fixation before we actually start calculating the new aim direction
        _aimRequestTime = Time.unscaledTime;
        _isRequestingAim = true;
        GazePlotter.showCross();
    }

    public void StopAiming()
    {

        _isRequestingAim = false;
        IsAiming = false;
        ShowCrosshair = false;
        GazePlotter.stopCross();
    }

    //Since we allow extended view to shift the camera position when using the center crosshair (not gaze-aiming)
    //we need to figure out where to put the actual crosshair on screen every frame.
    private void UpdateCenterCrosshairScreenPosition()
    {
        _screenCenterCrosshairPosition = ExtendedView.CameraWithExtendedView.WorldToScreenPoint(WorldCenterCrosshairTransformProjected.position);
        //Clamp to pixel
        _screenCenterCrosshairPosition = new Vector2(Mathf.FloorToInt(_screenCenterCrosshairPosition.x + 0.25f), Mathf.FloorToInt(_screenCenterCrosshairPosition.y + 0.25f));
    }

    private void CalculateWorldCenterCrosshairPosition()
    {
        //When looking around with extended view, you don't want the crosshair to move away from what you were aiming at.
        //To make this happen, we have to use a fixed reference point that only moves when you move your aiming device (e.g. mouse)
        //This is why we exclude the extended view extra shift if it is present when doing these calculations

        //First figure out 3d position
        RaycastHit hitInfo;
        if (Physics.Raycast(ExtendedView.CameraWithoutExtendedView.transform.position,
           ExtendedView.CameraWithoutExtendedView.transform.forward, out hitInfo, MaxProjectionDistance, RaycastLayerMask))        //일반적으로 여기 실행
        {

            WorldCenterCrosshairTransformProjected.position = hitInfo.point;

            if (OptionalWeaponFireOriginOverride == null)
            //If we don't have an override, we can just use this directly! Fantastic!
            {
                WeaponHitData = hitInfo;
                IsWeaponHitObject = true;
            }
        }
        else
        {
            Debug.Log("whatthe");
            WorldCenterCrosshairTransformProjected.position = ExtendedView.CameraWithoutExtendedView.transform.position +
                                                ExtendedView.CameraWithoutExtendedView.transform.forward * 100f;
            WeaponHitData = new RaycastHit
            {
                point = WorldCenterCrosshairTransformProjected.position,
                normal =
                  (WorldCenterCrosshairTransformProjected.position - ExtendedView.CameraWithoutExtendedView.transform.position)
                     .normalized,
                distance =
                  (WorldCenterCrosshairTransformProjected.position - ExtendedView.CameraWithoutExtendedView.transform.position)
                     .magnitude
            };
            IsWeaponHitObject = false;
        }
    }

    //If we have overridden the fire origin, we need to test this ray as well
    //since if the fire ray is not coincident with the camera normal, we can run into collision problems
    private void FindHitDataForPotentialOverride()
    {
        if (OptionalWeaponFireOriginOverride == null)
        {
            return;
        }

        RaycastHit hitInfo;
        var direction = AimedAtLocation - OptionalWeaponFireOriginOverride.position;
        direction.Normalize();
        if (Physics.Raycast(OptionalWeaponFireOriginOverride.position, direction, out hitInfo, MaxProjectionDistance, RaycastLayerMask))
        {
            WeaponHitData = hitInfo;
            IsWeaponHitObject = true;
        }
        else
        {
            IsWeaponHitObject = false;
        }
    }

    public void StartShooting()
    {
        IsShooting = true;
    }

    public void StartShootingAtGaze()
    {
        IsShooting = true;
        IsShootingAtGaze = true;
    }

    public void StopShooting()
    {
        IsShooting = false;
        IsShootingAtGaze = false;
    }

    private Vector3 ScreenToWorldProjection(Vector2 screenPosition)
    {
        //Raycast to find a hit point
        var worldPositionFixed = ExtendedView.CameraWithExtendedView.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 100));
        var direction = worldPositionFixed - ExtendedView.CameraWithExtendedView.transform.position;

        Vector3 worldPosition;
        RaycastHit hitInfo;
        if (Physics.Raycast(ExtendedView.CameraWithExtendedView.transform.position, direction, out hitInfo, MaxProjectionDistance, RaycastLayerMask))
        {
            worldPosition = hitInfo.point;
        }
        else
        {
            worldPosition = ExtendedView.CameraWithExtendedView.transform.position + direction * 100f;
        }
        return worldPosition;
    }

    private void UpdateAimBlockedCrosshair()
    {
        if (AimBlockedCrosshairImage != null)
        {
            //Show partially blocked crosshair if our aim and hit positions differ. This can only happen if we have some special weapon origin.
            if ((OptionalWeaponFireOriginOverride != null)
               && (Vector3.Distance(AimedAtLocation, WeaponHitData.point) > 0.01f))
            {
                Debug.Log(WorldCenterCrosshairTransformProjected.position);
                Debug.Log("aaa" + AimedAtLocation);
                AimBlockedCrosshairImage.enabled = true;
                var screenPosition = ExtendedView.CameraWithExtendedView.WorldToScreenPoint(WeaponHitData.point);
                AimBlockedCrosshairImage.rectTransform.anchoredPosition = new Vector2(screenPosition.x - Screen.width / 2.0f, screenPosition.y - Screen.height / 2.0f);
            }
            else
            {
                AimBlockedCrosshairImage.enabled = false;
            }
        }
    }
}