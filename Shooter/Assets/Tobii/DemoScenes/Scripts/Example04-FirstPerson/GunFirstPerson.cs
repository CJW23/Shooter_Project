//-----------------------------------------------------------------------
// Copyright 2016 Tobii AB (publ). All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

public class GunFirstPerson : Gun
{
	protected override void AlignGunToCrosshairDirection()
	{
		//If we have a lasersight, align the gun to its direction instead
		if (OptionalLaserSight != null)
		{
           //십자선을 사용자가 회전하는거 에 맞춰 이동
			var laserSightDesiredHeading = WeaponController.WeaponHitData.point - OptionalLaserSight.transform.position;
			laserSightDesiredHeading.Normalize();
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(laserSightDesiredHeading), GunAlignmentSpeed);
		}
		else
		{
            var desiredHeading = WeaponController.WeaponHitData.point - transform.position;
			desiredHeading.Normalize();
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(desiredHeading), GunAlignmentSpeed);
		}
	}
}