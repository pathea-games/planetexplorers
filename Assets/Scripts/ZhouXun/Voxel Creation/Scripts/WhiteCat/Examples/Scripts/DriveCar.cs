using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WhiteCat;
using WhiteCat.BitwiseOperationExtension;

public class DriveCar : MonoBehaviour
{
	public TweenInterpolator playingUIInterpolator;
	public ObjectPool barrierPool;
	public Text scoreText;
	public Text bestScoreText;

	[Space(8)]

	public float acceleration = 5f;
	public float maxSpeed = 20f;

	[Space(8)]

	public float slideDuration = 0.25f;
	public float maxSlideAngle = 15;

	[Space(8)]

	public Vector3 cameraPosition = new Vector3(0, 2.5f, -5);
	public Vector3 cameraForward = new Vector3(0, -2, 8);
	public float cameraRotateDamping = 4;

	[Space(8)]

	public float recycleLength = 10;
	public float generateLength = 100;
	public float minDistance = 10;
	public float maxDistance = 40;


	Transform mainCamera;
	Transform carTransform;
	Transform driverTransform;
	PathDriver driver;

	int slideDirection;
	bool slideTwice;
	float slideTime;
	float slideFrom, slideTo;

	bool ready;
	float speed;
	int score;
	int bestScore;

	Vector3 originalCarLocalPosition;
	Vector3 carLocalPosition;
	Quaternion originalCarLocalRotation;
	Quaternion carTargetRotation;

	struct Barrier
	{
		public readonly float location;
		public readonly GameObject gameObject;

		public Barrier(float distance, GameObject gameObject)
		{
			this.location = distance;
			this.gameObject = gameObject;
		}
	}

	List<Barrier> barriers;
	float nextGenerateDistance;


	void OnEnable()
	{
		mainCamera = PETools.PEUtil.MainCamTransform;
		carTransform = SelectCar.carTransform;
		driverTransform = SelectCar.carDriver.transform;
		driver = SelectCar.carDriver;

		originalCarLocalPosition = carTransform.localPosition;
		originalCarLocalRotation = carTransform.localRotation;

		ready = false;
		speed = 0;
		score = 0;
		scoreText.text = "0m";
		
		slideDirection = 0;
		barriers = new List<Barrier>(16);
		nextGenerateDistance = generateLength;
	}


	void FixedUpdate()
	{
		speed += acceleration * Time.deltaTime;
		if(speed >= maxSpeed)
		{
			speed = maxSpeed;

			if(!ready)
			{
				ready = true;
				SelectCar.HideUnselected();
				playingUIInterpolator.Record();
				playingUIInterpolator.isPlaying = true;
			}
		}

		driver.location += speed * Time.deltaTime;

		int newScore = (((int)(driver.location)) / 10) * 10;
		if (newScore != score)
		{
			score = newScore;
			scoreText.text = score.ToString() + "m";
		}

		SlideCar();
		SetCameraTransform();
		ManageBarriers();
	}


	void Update()
	{
		if (!ready) return;

		if (
#if UNITY_STANDALONE
			Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)
#else
			Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(0).position.x < Screen.width * 0.5f
#endif
			)
		{
			if (slideDirection == 0)
			{
				if (carTransform.localPosition.x > -1)
				{
					slideDirection = -1;
					slideTwice = false;
					slideTime = 0;
					slideFrom = carTransform.localPosition.x;
					slideTo = slideFrom - 2;
				}
			}
			else if (slideDirection == 1)
			{
				slideDirection = -1;
				slideTwice = false;
				slideTime = 1 - slideTime;
				Utility.Swap(ref slideFrom, ref slideTo);
			}
			else
			{
				if (slideTo > -1) slideTwice = true;
			}
		}

		if (
#if UNITY_STANDALONE
			Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)
#else
			Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(0).position.x > Screen.width * 0.5f
#endif
			)
		{
			if (slideDirection == 0)
			{
				if (carTransform.localPosition.x < 1)
				{
					slideDirection = 1;
					slideTwice = false;
					slideTime = 0;
					slideFrom = carTransform.localPosition.x;
					slideTo = slideFrom + 2;
				}
			}
			else if (slideDirection == -1)
			{
				slideDirection = 1;
				slideTwice = false;
				slideTime = 1 - slideTime;
				Utility.Swap(ref slideFrom, ref slideTo);
			}
			else
			{
				if (slideTo < 1) slideTwice = true;
			}
		}
	}


	void SlideCar()
	{
		if (slideDirection != 0)
		{
			slideTime += Time.deltaTime / slideDuration;

			if (slideTime >= 1)
			{
				if (slideTwice)
				{
					slideTwice = false;
					slideTime = 0;
					slideFrom = slideTo;
					slideTo += slideDirection * 2;
				}
				else
				{
					slideDirection = 0;
					slideTime = 1;
				}
			}

			carLocalPosition.x = Utility.Interpolate(slideFrom, slideTo, slideTime, Interpolation.EaseInEaseOut);
			carTransform.localPosition = carLocalPosition;

			carTransform.localRotation = Quaternion.Euler(0, Interpolation.Bell(slideTime) * maxSlideAngle * slideDirection, 0);
		}
	}


	void SetCameraTransform()
	{
		if (ready) mainCamera.position = driverTransform.TransformPoint(cameraPosition);
		else
		{
			mainCamera.position = Vector3.Lerp(
				mainCamera.position,
				driverTransform.TransformPoint(cameraPosition),
				Mathf.Pow((speed / maxSpeed), 3));
		}

		mainCamera.rotation = Quaternion.Slerp(
			mainCamera.rotation,
			Quaternion.LookRotation(driverTransform.TransformVector(cameraForward), driverTransform.up),
			speed / maxSpeed * cameraRotateDamping * Time.deltaTime);
	}


	void ManageBarriers()
	{
		while (barriers.Count > 0 && barriers[0].location + recycleLength < driver.location)
		{
			ObjectPool.Recycle(barriers[0].gameObject);
			barriers.RemoveAt(0);
		}

		if (driver.location + generateLength > nextGenerateDistance)
		{
			int index = -1;
			float time = 0;
			driver.path.GetPathPositionAtPathLength(nextGenerateDistance, ref index, ref time);
			Vector3 position = driver.path.GetSplinePoint(index, time);
			Quaternion rotation = driver.path.GetSplineRotation(index, time);

			// use 3 bits represent left(0), middle(1), right(2)
			// 000 & 111 are invalid
			int bits = UnityEngine.Random.Range(1, 7);

			for(int i=0; i<3; i++)
			{
				if(bits.GetBit(i))
				{
					GameObject barrier = barrierPool.TakeOut();
					barrier.transform.position = position + rotation * new Vector3((i - 1) * 2, 0, 0);
					barrier.transform.rotation = rotation;
					barriers.Add(new Barrier(nextGenerateDistance, barrier));
				}
			}

			nextGenerateDistance += UnityEngine.Random.Range(minDistance, maxDistance);
		}
	}


	void OnDisable()
	{
		try
		{
			Time.timeScale = 1;

			for (int i = 0; i < barriers.Count; i++)
			{
				ObjectPool.Recycle(barriers[i].gameObject);
			}

			if (score > bestScore)
			{
				bestScore = score;
				bestScoreText.text = "<color=ffffff>" + "Best Score".ToLocalizationString() + "</color> " + score;
			}

			driver.location = 0;
			carTransform.localPosition = originalCarLocalPosition;
			carTransform.localRotation = originalCarLocalRotation;
		}
		catch { }
	}
}
