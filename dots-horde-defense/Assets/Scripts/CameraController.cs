using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public static CameraController Instance;
	
	[SerializeField] private float moveSpeed = 1f;
	[SerializeField] private float turnSpeed = 1f;
	[SerializeField] private bool zoomEnabled = true;
	[SerializeField] private float zoomSpeed = 1f;
	[SerializeField] private float zoomSteps = 1f;
	
	private Transform _transform;
	private Camera _camera;
	private bool _cameraFollowEnabled;
	private Transform _cameraFollowTarget;
	private Vector3 _cameraFollowTargetLastPosition;
	private float _cameraFollowTargetTimeNotMoved;

	private static PhysicsWorld PhysicsWorld =>
		World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
	
	
	private void Awake()
	{
		#region singleton

		if (Instance != null)
		{
			Destroy(this.gameObject);
			return;
		}
		
		Instance = this;

		#endregion
		
		_transform = this.transform;
		_camera = transform.GetComponentInChildren<Camera>();
	}

	private void Update()
	{
		if (_cameraFollowEnabled)
			AutomaticFollowTarget();
		else
			ManuallyMovePivot();
		
		RotateCamera();
		ZoomCamera();
	}
	
	public void SetPosition(Vector3 position) => _transform.position = position;
	
	public void SetRotation(Vector3 rotation) => _transform.rotation = Quaternion.Euler(rotation);
	
	public void FollowTarget(Transform target)
	{
		_cameraFollowEnabled = true;
		_cameraFollowTarget = target;
	}
	
	private void AutomaticFollowTarget()
	{
		// TODO: experimental feature
		
		if (_cameraFollowTarget is null)
			return;
		
		_transform.position = _cameraFollowTarget.position;
		
		if (transform.position == _cameraFollowTargetLastPosition)
		{
			_cameraFollowTargetTimeNotMoved += Time.deltaTime;
			
			if (_cameraFollowTargetTimeNotMoved <= 3.0f) 
				return;
			
			_cameraFollowEnabled = false;
			_cameraFollowTargetTimeNotMoved = 0;
		}
		else
		{
			_cameraFollowTargetLastPosition = transform.position;
			_cameraFollowTargetTimeNotMoved = 0;
		}
	}
	
	private void ManuallyMovePivot()
	{
		var moveDirection = Vector3.zero;
		
		if (Input.GetKey(KeyCode.A))
			moveDirection += Vector3.left;
		
		if (Input.GetKey(KeyCode.D))
			moveDirection += Vector3.right;
		
		if (Input.GetKey(KeyCode.W))
			moveDirection += Vector3.forward;
		
		if (Input.GetKey(KeyCode.S))
			moveDirection += Vector3.back;
		
		_transform.Translate(moveDirection * (moveSpeed * Time.deltaTime));
	}
	
	private void RotateCamera()
	{
		if (Input.GetKey(KeyCode.Q))
			_transform.Rotate(
				0.0f, 
				+1.0f * turnSpeed, 
				0.0f, 
				Space.Self);
		
		if (Input.GetKey(KeyCode.E))
			_transform.Rotate(
				0.0f, 
				-1.0f * turnSpeed, 
				0.0f, 
				Space.Self);
	}
	
	private void ZoomCamera()
	{
		if (!zoomEnabled)
			return;
		
		var zoomDirection = Input.mouseScrollDelta.y;

		if (Mathf.Abs(zoomDirection) <= 0)
			return;
		
		var cameraPosModifier = _camera.transform.forward * (zoomDirection * zoomSpeed);

		_camera.transform.Translate(cameraPosModifier, Space.World);
	}
	
	public bool Raycast_Normal(Vector3 target, LayerMask layerMask, out UnityEngine.RaycastHit hit)
	{
		var ray = _camera.ScreenPointToRay(target);
		return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
	}
	
	public bool Raycast(Vector3 screenPoint, CollisionFilter collisionFilter, out Unity.Physics.RaycastHit hit)
	{
		var ray = _camera.ScreenPointToRay(screenPoint);
		
		var raycastInput = new RaycastInput()
		{
			Start = ray.origin,
			End = ray.GetPoint(1000),
			Filter = collisionFilter,
		};

		return PhysicsWorld.CastRay(raycastInput, out hit);
	}
}
