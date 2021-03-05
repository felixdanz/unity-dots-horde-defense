using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public class PhysicsUtilities
{
	public static CollisionFilter GetCollisionFilter(uint collidesWith)
	{
		return new CollisionFilter()
		{
			BelongsTo = 0xffffffff,
			CollidesWith = collidesWith,
			GroupIndex = 0,
		};
	}
	
	public static unsafe bool SphereCast(
		Unity.Physics.Systems.BuildPhysicsWorld buildPhysicsWorld,
		CollisionFilter collisionFilter,
		float3 from, 
		float3 to, 
		float radius, 
		out Entity hitEntity)
	{
		hitEntity = Entity.Null;
		
		var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

		var sphereGeometry = new SphereGeometry()
		{
			Center = float3.zero, 
			Radius = radius,
		};
		
		var sphereCollider = SphereCollider.Create(
			sphereGeometry,
			collisionFilter);
		
		var colliderCastInput = new ColliderCastInput()
		{
			Collider = (Collider*)sphereCollider.GetUnsafePtr(),
			Orientation = quaternion.identity,
			Start = from,
			End = to,
		};

		var hasHit = collisionWorld.CastCollider(colliderCastInput, out var hit);
		sphereCollider.Dispose();
		
		if (hasHit)
		{
			hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
		}
		
		return hasHit;
	}
}