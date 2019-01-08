//-----------------------------------------------------------------------
// Copyright 2016 Tobii AB (publ). All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class Gun : MonoBehaviour
{
    //How fast the gun will rotate / align to the new aim direction
    public float GunAlignmentSpeed = 0.2f;
    public float TimeBetweenShots = 0.2F;
    public double StopShootingDelay = 0.5F;
    public int BulletsPerShot = 1;
    public float SpreadAtOneMeter = 0.02f;
    public GameObject BulletHolePrefab;
    public AudioClip FireSound;
    public AnimationCurve FireAnimationRotationCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 0.0f);
    //Don't shoot while selecting objects

    public ExtendedViewBase ExtendedView;
    private AudioSource _audio;
    protected WeaponController WeaponController;
    protected LaserSight OptionalLaserSight;

    private Transform _animationTransform;
    private Quaternion _baseRotation;
    private float _animationTime;
    private bool _lastLeftTrigger;

    private float _nextFire = 0.0F;
    private float _lastFire = 0.0F;
    private bool shootflag = false;
    protected void Start()
    {
        shootflag = false;
        _audio = GetComponent<AudioSource>();
        OptionalLaserSight = GetComponentInChildren<LaserSight>();
        WeaponController = GetComponentInParent<WeaponController>();
        _animationTransform = transform.GetChild(0);
        _baseRotation = _animationTransform.localRotation;
    }

    protected void Update()
    {
        var leftTrigger = false;
        var rightTrigger = false;
        var leftTriggerDown = leftTrigger && !_lastLeftTrigger;
        var joystickButton1Down = Input.GetKeyDown(KeyCode.JoystickButton1);


        if (Input.GetKeyDown(KeyCode.Mouse1))       //마우스 오른쪽 클릭을 눌렀을 때
        {
            Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            
            WeaponController.StartAiming();
        }
        else if (BodySourceView.rh < 1.0f)          //마우스 오른쪽 클릭을 땠을때)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            WeaponController.StopAiming();
        }

        _lastLeftTrigger = leftTrigger;

        if ((WeaponController != null)
           && (Time.time > _lastFire + StopShootingDelay))
        {
            WeaponController.StopShooting();
        }
        if (BodySourceView.rh <= 1.0)
            shootflag = false;
        if ((Input.GetKeyDown(KeyCode.Mouse0)       //마우스 왼쪽 클릭 했을때
              || Input.GetKeyDown(KeyCode.E)
              || (BodySourceView.rh>2.0 && shootflag == false)
              || joystickButton1Down
              || rightTrigger)
           && Time.time > _nextFire)
        {
            _lastFire = Time.time;
            _nextFire = Time.time + TimeBetweenShots;           //총 쏘는 간격

            shootflag = true;
            var shootAtGaze = (BodySourceView.rh > 2.0)  || Input.GetKeyDown(KeyCode.E) || joystickButton1Down;   //
            Debug.Log(shootAtGaze + "11");
            Fire(shootAtGaze);

            
        }

        if (_animationTime < FireAnimationRotationCurve[FireAnimationRotationCurve.length - 1].time)
        {
            _animationTime += Time.deltaTime;
            _animationTransform.localRotation = _baseRotation * Quaternion.Euler(0.0f, FireAnimationRotationCurve.Evaluate(_animationTime), 0.0f);
        }
        else
        {
            _animationTransform.localRotation = _baseRotation;
        }

        if (WeaponController != null)
        {
            WeaponController.Calculate();
        }
        AlignGunToCrosshairDirection();
    }

    private void Fire(bool shootAtGaze)     //총을 쐈을 때      shootAtGaze = E로 발사할때
    {
        if (WeaponController != null)
        {

            if (shootAtGaze)
            {
                WeaponController.StartShootingAtGaze();
            }
            else
            {
                Debug.Log(" 실행");
                WeaponController.StartShooting();
            }
            WeaponController.Calculate();
        }

        _animationTime = 0.0f;

        if (FireSound != null)
        {
            _audio.clip = FireSound;
            _audio.Play();
        }

        var bulletsPerShot = shootAtGaze ? 10 : BulletsPerShot;

        //Only interact with stuff if we actually have a target intersection point
        if ((bulletsPerShot > 0)
           && (WeaponController != null)
           && WeaponController.IsWeaponHitObject)
        {
            ShootBullet(WeaponController.WeaponHitData);
        }

        var origin = ExtendedView.CameraWithoutExtendedView.transform.position;
        var mainDirection = ExtendedView.CameraWithoutExtendedView.transform.forward;
        if (WeaponController != null)
        {
            origin = ExtendedView.CameraWithExtendedView.transform.position;
            if (WeaponController.OptionalWeaponFireOriginOverride != null)
            {
                origin = WeaponController.OptionalWeaponFireOriginOverride.position;
            }
            mainDirection = WeaponController.WeaponHitData.point - origin;
        }

        mainDirection.Normalize();

        for (var i = 1; i < bulletsPerShot; i++)
        {
            var rand = Random.insideUnitCircle * SpreadAtOneMeter;
            var left = Vector3.Cross(mainDirection, Vector3.Dot(mainDirection, Vector3.up) > 0.95 ? Vector3.right : Vector3.up);
            var up = Vector3.Cross(mainDirection, left);
            var direction = mainDirection + rand.x * left + rand.y * up;
            RaycastHit hitInfo;
            if (Physics.Raycast(origin, direction, out hitInfo, WeaponController.MaxProjectionDistance, WeaponController.RaycastLayerMask))     //총쏜 것과 target 처리 부분
            {
                ShootBullet(hitInfo);
            }
        }
    }

    private void ShootBullet(RaycastHit hitInfo)
    {
        var hitObject = HitTarget(hitInfo.transform);
        SpawnBulletHole(hitInfo, hitObject);        //총알 구멍 표현
        SpawnLaser(hitInfo);                        //레이저 좌표값 표현
    }

    private void SpawnLaser(RaycastHit hitInfo)
    {
        if (OptionalLaserSight == null)
            return;

        var go = new GameObject("LaserBeam");       //레이저 오브젝트
        var line = go.AddComponent<LineRenderer>();
        line.materials = new[] { OptionalLaserSight.LaserSightMaterial };
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.SetVertexCount(2);
        line.SetWidth(0.01f, 0.01f);
        line.SetPosition(0, OptionalLaserSight.transform.position);
        line.SetPosition(1, hitInfo.point);
        Destroy(go, .1f);
    }

    private void SpawnBulletHole(RaycastHit hitInfo, Transform hitObject)
    {
        if (BulletHolePrefab == null) return;

        var hitRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        var bulletHole = (GameObject)Instantiate(BulletHolePrefab, hitInfo.point + hitInfo.normal * 0.0001f, hitRotation);
        if (hitObject != null)
        {
            bulletHole.transform.SetParent(hitObject);
        }
    }

    private Transform HitTarget(Transform go)       //맞은 판정 타겟이
    {
        var targetDummy = go.transform.GetComponent<TargetDummy>();     //맞은 오브젝트의 targetdummy 컴포넌트를 가져옴         
        if (targetDummy == null)        //만약 컴포넌트가 없으면 타겟이 아니므로
        {
            targetDummy = go.GetComponentInParent<TargetDummy>();
        }

        if (targetDummy != null)        //targetdummy가 있다면 타겟이므로
        {
            targetDummy.Hit();      //타겟의 hit()메소드 호출
        }
        return go.transform;
    }

    protected void OnDisable()
    {
        if (OptionalLaserSight != null)
        {
            OptionalLaserSight.IsEnabled = false;
        }
    }

    protected abstract void AlignGunToCrosshairDirection();
}