﻿using System;
using UnityEngine;
using System.Collections.Generic;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Movement : MonoBehaviour
{
	private OrbitCamera _camera;

	// Via Marble Blast
	public float MaxRollVelocity = 15f;
	public float AngularAcceleration = 75f;
	public float BrakingAcceleration = 30f;
	public float AirAcceleration = 5f;
	public float Gravity = 20f;
	public float StaticFriction = 1.1f;
	public float KineticFriction = 0.7f;
	public float BounceKineticFriction = 0.2f;
	public float MaxDotSlide = 0.5f;
	public float JumpImpulse = 7.5f;
	public float MaxForceRadius = 50f;
	public float MinBounceVel = 0.1f;
	public float BounceRestitution = 0.5f;

	private float _remainingTime = 0.0f;

	// TODO: Clean up / rename the following
	private float CameraX => _camera.yaw;
	private float CameraY => _camera.pitch;
	private Vector3 Velocity;
	private Vector3 AngularVelocity;
	private float Radius => _collider.radius * transform.localScale.x;

	private Vector2 InputMovement =>
		new Vector2(-Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) + _fakeInput;
	private Vector2 _fakeInput = Vector2.zero;

	private bool Jump => Input.GetButton("Jump");

	private Vector3 GravityDir => Physics.gravity.normalized;
	private Vector3 _forwards = Vector3.forward;

	private bool _bounceYet;
	private float _bounceSpeed;
	private Vector3 _bouncePos;
	private Vector3 _bounceNormal;
	private float _slipAmount;
	private float _contactTime;
	private float _rollVolume;

	private List<MeshCollider> _colTests;

	class MeshData
	{
		public int[] Triangles;
		public Vector3[] Vertices;
	}

	private List<MeshData> _meshes;

	private Rigidbody _rigidBody;
	private SphereCollider _collider;
	private int _collisions;
	private float _lastJump;
	private Vector3 _lastNormal;

	private bool _test = false;
	private float _testStart = 0;
	
	class CollisionInfo
	{
		public Vector3 Point;
		public Vector3 Normal;
		public Vector3 Velocity;
		public Collider Collider;
		public float Friction;
		public float Restitution;
		public float Penetration;
	}

	void Start()
	{
		_camera = Camera.main.GetComponent<OrbitCamera>();
		_rigidBody = gameObject.GetComponent<Rigidbody>();
		_rigidBody.maxAngularVelocity = Mathf.Infinity;
		_collider = GetComponent<SphereCollider>();
        _meshes = new List<MeshData>();
		_colTests = new List<MeshCollider>();

        foreach (var item in FindObjectsOfType<MeshCollider>()) {
            _colTests.Add(item);
        }

		foreach (var mesh in _colTests)
		{
			GenerateMeshInfo(mesh);
		}
	}

	void GenerateMeshInfo(MeshCollider meshCollider)
	{
		var sharedMesh = meshCollider.sharedMesh;
		var triangles = sharedMesh.triangles;
		var vertices = sharedMesh.vertices;
		_meshes.Add(new MeshData
		{
			Triangles = triangles,
			Vertices = vertices
		});
	}
	
	public void AddMesh(MeshCollider meshCollider)
	{
		_colTests.Add(meshCollider);
		GenerateMeshInfo(meshCollider);
	}
	
	/*
function rmtest() {
	$mp::mymarble.setcollisionradius(0.2);
	$mp::mymarble.setvelocity("0 0 0");
	$mp::mymarble.setangularvelocity("0 0 0");
	$mp::mymarble.setcamerayaw(0);
	$mp::mymarble.setcamerapitch(0.45);
	$MP::MyMarble.settransform("16 -28 -8.8 0 0 0 1");
	b();
	schedule(1000, 0, "eval", "moveforward(1);$firstf = getRealTime();");
	schedule(5000, 0, "eval", "cancel($b);");
	echo("t,px,py,pz,vx,vy,vz,ox,oy,oz");
}
function f() {
	if (getRealTime() $= $lastf)
		return;
	$lastf = getRealTime();
	echo(strReplace((getRealTime() - $firstf) SPC $MP::MyMarble.getPosition() SPC $MP::MyMarble.getVelocity() SPC $MP::MyMarble.getAngularVelocity(), " ", ","));
}
	 */

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			Starttest();
		}
	}

	void Starttest()
	{
		Velocity = Vector3.zero;
		AngularVelocity = new Vector3(40, 0, 0);
		_camera.yaw = 0;
		_camera.pitch = 0.45f * Mathf.Rad2Deg;
		// transform.position = new Vector3(16, -8.8f, -28);
		transform.position = new Vector3(16, 1.2f, -28);
		transform.rotation = Quaternion.identity;
		transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
		_test = true;
		_testStart = Time.time;
		_fakeInput = Vector2.zero;
		// _fakeInput = Vector2.up;
	}
	
	// Per-tick updates
	void FixedUpdate()
	{
        if (_test)
		{
			Debug.Log(string.Format(
				"{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
				(Time.time - _testStart) * 1000.0f,
				transform.position.x,
				transform.position.z,
				transform.position.y,
				Velocity.x,
				Velocity.z,
				Velocity.y,
				AngularVelocity.x,
				AngularVelocity.z,
				AngularVelocity.y
				));
		}
		float dt = Time.fixedDeltaTime;
		_remainingTime += dt;
		while (_remainingTime > 0.008f)
		{
			float loopTime = 0.008f;
			_advancePhysics(ref loopTime);
			_remainingTime -= loopTime;
		}
	}

    public bool useUnityContacts = false;
    private HashSet<UnityEngine.Collision> _unityCollisions = new HashSet<UnityEngine.Collision>();

    private void OnCollisionStay(UnityEngine.Collision collision) {
        _unityCollisions.Add(collision);
    }
    private void OnCollisionExit(UnityEngine.Collision collision) {
        _unityCollisions.Remove(collision);
    }

    void _advancePhysics(ref float dt)
	{
		List<CollisionInfo> contacts = new List<CollisionInfo>();
		Vector3 pos = transform.position;
		Quaternion rot = transform.rotation;
		Vector3 velocity = Velocity;
		Vector3 omega = AngularVelocity;

        if (useUnityContacts) {
            foreach (var collision in _unityCollisions)
                foreach (var contact in collision.contacts) {
                    var penetration = -contact.separation;
                    var col = new CollisionInfo {
                        Penetration = penetration,
                        Restitution = 1,
                        Friction = 1,
                        Normal = contact.normal,
                        Point = contact.point
                    };
                    if (contact.otherCollider.attachedRigidbody != null) {
                        col.Collider = contact.otherCollider;
                        col.Velocity = contact.otherCollider.attachedRigidbody.GetPointVelocity(contact.point);
                    }
                    contacts.Add(col);
                    Debug.DrawRay(contact.point, contact.normal, Color.blue, 5);
                }
        }
        else {
            var radius = Radius + 0.0001f;
            for (var index = 0; index < _colTests.Count; index++) {
                var meshCollider = _colTests[index];
                var localPos = meshCollider.transform.InverseTransformPoint(pos);
                var length = _meshes[index].Triangles.Length;
                for (int i = 0; i < length; i += 3) {
                    var triangle0 = _meshes[index].Triangles[i];
                    var triangle1 = _meshes[index].Triangles[i + 1];
                    var triangle2 = _meshes[index].Triangles[i + 2];
                    var p0 = _meshes[index].Vertices[triangle0];
                    var p1 = _meshes[index].Vertices[triangle1];
                    var p2 = _meshes[index].Vertices[triangle2];
                    var normal = Vector3.Cross(p2 - p0, p2 - p1).normalized;

                    if (CollisionHelpers.ClosestPtPointTriangle(localPos, radius, p0, p1, p2, normal, out var closest) == false)
                        continue;

                    Vector3 vector32 = localPos - closest;
                    if (vector32.sqrMagnitude > radius * radius)
                        continue;

                    Vector3 vector33 = localPos - closest;
                    if (Vector3.Dot(localPos - closest, normal) < 0.0)
                        continue;

                    vector33.Normalize();
                    CollisionInfo collisionInfo = new CollisionInfo() {
                        Normal = meshCollider.transform.TransformDirection(vector33),
                        Point = meshCollider.transform.TransformPoint(closest)
                    };
                    Debug.DrawRay(collisionInfo.Point, collisionInfo.Normal, Color.red);
                    collisionInfo.Penetration = radius - Vector3.Dot(localPos - closest, collisionInfo.Normal);
                    collisionInfo.Restitution = 1f;
                    collisionInfo.Friction = 1f;
                    if (_colTests[index].attachedRigidbody != null)
                        collisionInfo.Velocity = _colTests[index].attachedRigidbody.GetPointVelocity(collisionInfo.Point);
                    contacts.Add(collisionInfo);
                }
            }
        }

        _updateMove(ref dt, ref velocity, ref omega, contacts);
		// velocity += _gravityDir * _gravity * dt;

		_updateIntegration(dt, ref pos, ref rot, velocity, omega);

        if (useUnityContacts) {
            _rigidBody.MovePosition(pos);
            _rigidBody.MoveRotation(rot);
        }
        else {
		    transform.position = pos;
		    transform.rotation = rot;
        }
		Velocity = velocity;
		AngularVelocity = omega;
	}

	protected virtual void _updateIntegration(float dt, ref Vector3 pos, ref Quaternion rot, Vector3 vel, Vector3 avel)
	{
		pos += vel * dt;
		Vector3 vector3 = avel;
		float num1 = vector3.magnitude;
		if (num1 <= 0.0000001)
			return;
		Quaternion quaternion = Quaternion.AngleAxis(dt * num1 * Mathf.Rad2Deg, vector3 * (1f / num1));
		quaternion.Normalize();
		rot = quaternion * rot;
		rot.Normalize();
	}

	private void _updateMove(
		ref float dt,
		ref Vector3 velocity,
		ref Vector3 angVelocity,
		List<CollisionInfo> contacts)
	{
		bool isMoving = _computeMoveForces(angVelocity, out var torque, out var targetAngVel);
		_velocityCancel(contacts, ref velocity, ref angVelocity, !isMoving, false);
		Vector3 externalForces = _getExternalForces(dt, contacts);
		_applyContactForces(dt, contacts, !isMoving, torque, targetAngVel, ref velocity, ref angVelocity, ref externalForces, out var angAccel);
		velocity += externalForces * dt;
		angVelocity += angAccel * dt;
		_velocityCancel(contacts, ref velocity, ref angVelocity, !isMoving, true);
		float contactTime = dt;
		// testMove(ref contactTime, ...)
		if (dt * 0.99 > contactTime)
		{
			velocity -= externalForces * (dt - contactTime);
			angVelocity -= angAccel * (dt - contactTime);
			dt = contactTime;
		}
		
		if (contacts.Count != 0)
			_contactTime += dt;
	}

	private bool _computeMoveForces(Vector3 angVelocity, out Vector3 torque, out Vector3 targetAngVel)
	{
		torque = Vector3.zero;
		targetAngVel = Vector3.zero;
		Vector3 relGravity = -GravityDir * Radius;
		Vector3 topVelocity = Vector3.Cross(angVelocity, relGravity);
		_getMarbleAxis(out var sideDir, out var motionDir, out Vector3 _);
		float topY = Vector3.Dot(topVelocity, motionDir);
		float topX = Vector3.Dot(topVelocity, sideDir);
		Vector2 move = InputMovement;
		// move.Normalize();
		float moveY = MaxRollVelocity * move.y;
		float moveX = MaxRollVelocity * move.x;
		if (Math.Abs(moveY) < 0.001f && Math.Abs(moveX) < 0.001f)
			return false;
		if (topY > moveY && moveY > 0.0)
			moveY = topY;
		else if (topY < moveY && moveY < 0.0)
			moveY = topY;
		if (topX > moveX && moveX > 0.0)
			moveX = topX;
		else if (topX < moveX && moveX < 0.0)
			moveX = topX;
		targetAngVel = Vector3.Cross(relGravity, moveY * motionDir + moveX * sideDir) / relGravity.sqrMagnitude;
		torque = targetAngVel - angVelocity;
		float targetAngAccel = torque.magnitude;
		if (targetAngAccel > AngularAcceleration)
		{
			torque *= AngularAcceleration / targetAngAccel;
		}

		return true;
	}

	private void _getMarbleAxis(out Vector3 sideDir, out Vector3 motionDir, out Vector3 upDir)
	{
		var m = Quaternion.Euler(CameraY, 0, 0) * Quaternion.Euler(0, CameraX, 0);
		upDir = -GravityDir;
		motionDir = m * _forwards;
		sideDir = Vector3.Cross(motionDir, upDir);
		sideDir.Normalize();
		motionDir = Vector3.Cross(upDir, sideDir);
	}

	private Vector3 _getExternalForces(float dt, List<CollisionInfo> contacts)
	{
		Vector3 force = GravityDir * Gravity;
		if (contacts.Count == 0)
		{
			_getMarbleAxis(out var sideDir, out var motionDir, out Vector3 _);
			force += (sideDir * InputMovement.x + motionDir * InputMovement.y) * AirAcceleration;
		}

		return force;
	}

	private void _velocityCancel(List<CollisionInfo> contacts, ref Vector3 velocity, ref Vector3 omega, bool surfaceSlide, bool noBounce)
	{
		bool flag1 = false;
		int iterations = 0;
		bool done = false;
		while (!done) {
			done = true;
			++iterations;
			foreach (var coll in contacts)
			{
				Vector3 relativeVelocity = velocity - coll.Velocity;
				float bounceSpeed = Vector3.Dot(coll.Normal, relativeVelocity);
				if (!flag1 && bounceSpeed < 0.0 || bounceSpeed < -0.001f)
				{
					Vector3 invBounce = bounceSpeed * coll.Normal;
					// _reportBounce(contacts[index].point, contacts[index].normal, -num3);
					if (noBounce)
					{
						velocity -= invBounce;
					}
					else if (coll.Collider != null)
					{
						CollisionInfo contact = coll;
                        if (false && contact.Collider.GetComponent<Movement>() is Movement owner) {
						    float num5 = 1f;
						    float num6 = 1f;
						    float num7 = 0.5f;
                            var vector3_4 =
                            (
                                (
                                    Vector3.Dot(
                                        (
                                            (velocity * num5) -
                                            (owner.Velocity * num6)
                                        ),
                                        contact.Normal
                                    ) *
                                    contact.Normal
                                ) *
                                (1f + num7)
                            );

                            /*Vector3.op_Multiply(
                                Vector3.op_Multiply(
                                    Vector3.Dot(
                                        Vector3.op_Subtraction(
                                            Vector3.op_Multiply(velocity, num5),
                                            Vector3.op_Multiply(owner.Velocity, num6)
                                        ),
                                        contact.normal
                                    ),
                                    contact.normal
                                ),
                                1f + num7
                            );*/
						    velocity = velocity - (vector3_4 / num5);
						    //velocity = Vector3.op_Subtraction(velocity, Vector3.op_Division(vector3_4, num5));
						    var moveComponent = owner;
						    moveComponent.Velocity = moveComponent.Velocity + (vector3_4 / num6);
						    //moveComponent.Velocity = Vector3.op_Addition(moveComponent.Velocity, Vector3.op_Division(vector3_4, num6));
						    contact.Velocity = owner.Velocity;
						    //contacts[index] = contact; // ?
						}
						else
						{
						    float num5 = 0.5f;
						    Vector3 vector34 = contact.Normal * (Vector3.Dot(velocity, contact.Normal) * (1f + num5));
						    velocity -= vector34;
                            
						}
					}
					else
					{
						if (coll.Velocity.magnitude > 0.00001 && !surfaceSlide &&
						    bounceSpeed > -MaxDotSlide * velocity.magnitude)
						{
							velocity -= invBounce;
							velocity.Normalize();
							velocity *= velocity.magnitude;
							surfaceSlide = true;
						}
						else if (bounceSpeed > -MinBounceVel)
						{
							velocity -= invBounce;
						}
						else
						{
							Vector3 velocityAdd = (float) -(1.0 + BounceRestitution * coll.Restitution) * invBounce;
							Vector3 velocityAtContact = relativeVelocity + Vector3.Cross(omega, -coll.Normal * Radius);
							float num5 = -Vector3.Dot(coll.Normal, relativeVelocity);
							Vector3 vector36 = velocityAtContact - coll.Normal * Vector3.Dot(coll.Normal, relativeVelocity);
							float num6 = vector36.magnitude;
							if (Math.Abs(num6) > 0.001f)
							{
								float inertia = (float) (5.0 * (BounceKineticFriction * coll.Friction) * num5 /
								                      (2.0 * Radius));
								if (inertia > num6 / Radius)
									inertia = num6 / Radius;
								Vector3 vector37 = vector36 / num6;
								Vector3 vAtC = Vector3.Cross(-coll.Normal, -vector37);
								Vector3 vector38 = inertia * vAtC;
								omega += vector38;
								velocity -= Vector3.Cross(-vector38, -coll.Normal * Radius);
							}

							velocity += velocityAdd;
						}
					}

					done = false;
				}
			}

			flag1 = true;
			if (iterations > 6 && noBounce)
				done = true;

            if (iterations > 1e7) {
                Debug.Log("Collision lock");
                break;
            }
		}

		if (velocity.sqrMagnitude >= 625.0)
			return;
		bool flag3 = false;
		Vector3 vector39 = new Vector3(0.0f, 0.0f, 0.0f);
		for (int index = 0; index < contacts.Count; ++index)
		{
			Vector3 vector32 = vector39 + contacts[index].Normal;
			if (vector32.sqrMagnitude < 0.01)
				vector32 += contacts[index].Normal;
			vector39 = vector32;
			flag3 = true;
		}

		if (!flag3)
			return;
		vector39.Normalize();
		float num8 = 0.0f;
		for (int index = 0; index < contacts.Count; ++index)
		{
			if (contacts[index].Penetration < Radius)
			{
				float num3 = 0.1f;
				float penetration = contacts[index].Penetration;
				float num4 = Vector3.Dot(velocity + num8 * vector39, contacts[index].Normal);
				if (num3 * num4 < penetration)
					num8 += (penetration - num4 * num3) / num3 / Vector3.Dot(contacts[index].Normal, vector39);
			}
		}

		float num9 = Mathf.Clamp(num8, -25f, 25f);
		velocity += num9 * vector39;
	}


	private void _applyContactForces(
		float dt,
		List<CollisionInfo> contacts,
		bool isCentered,
		Vector3 aControl,
		Vector3 desiredOmega,
		ref Vector3 velocity,
		ref Vector3 omega,
		ref Vector3 linAccel,
		out Vector3 angAccel)
	{
		angAccel = Vector3.zero;
		_slipAmount = 0.0f;
		Vector3 vector31 = GravityDir;
		int index1 = -1;
		float num1 = 0.0f;
		for (int index2 = 0; index2 < contacts.Count; ++index2)
		{
			// if (contacts[index2].collider == null)
			// {
			float num2 = -Vector3.Dot(contacts[index2].Normal, linAccel);
			if (num2 > num1)
			{
				num1 = num2;
				index1 = index2;
			}

			// }
		}

		CollisionInfo collisionInfo = index1 != -1 ? contacts[index1] : new CollisionInfo();
		if (index1 != -1 && Jump)
		{
			Vector3 vector32 = velocity - collisionInfo.Velocity;
			float num2 = Vector3.Dot(collisionInfo.Normal, vector32);
			if (num2 < 0.0)
				num2 = 0.0f;
			if (num2 < JumpImpulse)
			{
				velocity += collisionInfo.Normal * (JumpImpulse - num2);
				// MarbleControlComponent._soundBank.PlayCue(MarbleControlComponent._sounds[12]);
			}
		}

		for (int index2 = 0; index2 < contacts.Count; ++index2)
		{
			float num2 = -Vector3.Dot(contacts[index2].Normal, linAccel);
			if (num2 > 0.0 && Vector3.Dot(contacts[index2].Normal, velocity - contacts[index2].Velocity) <=
				0.00001)
			{
				linAccel += contacts[index2].Normal * num2;
			}
		}

		if (index1 != -1)
		{
			// (collisionInfo.velocity - (collisionInfo.normal * Vector3.Dot(collisionInfo.normal, collisionInfo.velocity)));
			Vector3 vector32 = velocity + Vector3.Cross(omega, -collisionInfo.Normal * Radius) - collisionInfo.Velocity;
			float num2 = vector32.magnitude;
			bool flag = false;
			Vector3 vector33 = new Vector3(0.0f, 0.0f, 0.0f);
			Vector3 vector34 = new Vector3(0.0f, 0.0f, 0.0f);
			if (num2 != 0.0)
			{
				flag = true;
				float num3 = KineticFriction * collisionInfo.Friction;
				float num4 = (float) (5.0 * num3 * num1 / (2.0 * Radius));
				float num5 = num1 * num3;
				float num6 = (num4 * Radius + num5) * dt;
				if (num6 > num2)
				{
					float num7 = num2 / num6;
					num4 *= num7;
					num5 *= num7;
					flag = false;
				}

				Vector3 vector35 = vector32 / num2;
				vector33 = num4 * Vector3.Cross(-collisionInfo.Normal, -vector35);
				vector34 = -num5 * vector35;
				_slipAmount = num2 - num6;
			}

			if (!flag)
			{
				Vector3 vector35 = -vector31 * Radius;
				Vector3 vector36 = Vector3.Cross(vector35, linAccel) / vector35.sqrMagnitude;
				if (isCentered)
				{
					Vector3 vector37 = omega + angAccel * dt;
					aControl = desiredOmega - vector37;
					float num3 = aControl.magnitude;
					if (num3 > BrakingAcceleration)
						aControl = aControl * BrakingAcceleration / num3;
				}

				Vector3 vector38 = -Vector3.Cross(aControl, -collisionInfo.Normal * Radius);
				Vector3 vector39 = Vector3.Cross(vector36, -collisionInfo.Normal * Radius) + vector38;
				float num4 = vector39.magnitude;
				float num5 = StaticFriction * collisionInfo.Friction;
				if (num4 > num5 * num1)
				{
					float num3 = KineticFriction * collisionInfo.Friction;
					vector38 *= num3 * num1 / num4;
				}

				linAccel += vector38;
				angAccel += vector36;
			}

			linAccel += vector34;
			angAccel += vector33;
		}

		angAccel += aControl;
	}


	static class CollisionHelpers
	{
		public static bool ClosestPtPointTriangle(
			Vector3 pt,
			float radius,
			Vector3 p0,
			Vector3 p1,
			Vector3 p2,
			Vector3 normal,
			out Vector3 closest)
		{
			closest = Vector3.zero;
			float num1 = Vector3.Dot(pt, normal);
			float num2 = Vector3.Dot(p0, normal);
			if (Mathf.Abs(num1 - num2) > radius * 1.1)
				return false;
			closest = pt + (num2 - num1) * normal;
			if (PointInTriangle(closest, p0, p1, p2))
				return true;
			float num3 = 10f;
			if (IntersectSegmentCapsule(pt, pt, p0, p1, radius, out var tSeg, out var tCap) &&
			    tSeg < num3)
			{
				closest = p0 + tCap * (p1 - p0);
				num3 = tSeg;
			}

			if (IntersectSegmentCapsule(pt, pt, p1, p2, radius, out tSeg, out tCap) &&
			    tSeg < num3)
			{
				closest = p1 + tCap * (p2 - p1);
				num3 = tSeg;
			}

			if (IntersectSegmentCapsule(pt, pt, p2, p0, radius, out tSeg, out tCap) &&
			    tSeg < num3)
			{
				closest = p2 + tCap * (p0 - p2);
				num3 = tSeg;
			}

			return num3 < 1.0;
		}

		public static bool PointInTriangle(Vector3 pnt, Vector3 a, Vector3 b, Vector3 c)
		{
			a -= pnt;
			b -= pnt;
			c -= pnt;
			Vector3 bc = Vector3.Cross(b, c);
			Vector3 ca = Vector3.Cross(c, a);
			if (Vector3.Dot(bc, ca) < 0.0)
				return false;
			Vector3 ab = Vector3.Cross(a, b);
			return Vector3.Dot(bc, ab) >= 0.0;
		}

		public static bool IntersectSegmentCapsule(
			Vector3 segStart,
			Vector3 segEnd,
			Vector3 capStart,
			Vector3 capEnd,
			float radius,
			out float seg,
			out float cap)
		{
			return ClosestPtSegmentSegment(segStart, segEnd, capStart, capEnd, out seg, out cap,
				out Vector3 _, out Vector3 _) < radius * radius;
		}

		public static float ClosestPtSegmentSegment(
			Vector3 p1,
			Vector3 q1,
			Vector3 p2,
			Vector3 q2,
			out float s,
			out float T,
			out Vector3 c1,
			out Vector3 c2)
		{
			float num1 = 0.0001f;
			Vector3 vector31 = q1 - p1;
			Vector3 vector32 = q2 - p2;
			Vector3 vector33 = p1 - p2;
			float num2 = Vector3.Dot(vector31, vector31);
			float num3 = Vector3.Dot(vector32, vector32);
			float num4 = Vector3.Dot(vector32, vector33);
			if (num2 <= num1 && num3 <= num1)
			{
				s = T = 0.0f;
				c1 = p1;
				c2 = p2;
				return Vector3.Dot(c1 - c2, c1 - c2);
			}

			if (num2 <= num1)
			{
				s = 0.0f;
				T = num4 / num3;
				T = Mathf.Clamp(T, 0.0f, 1f);
			}
			else
			{
				float num5 = Vector3.Dot(vector31, vector33);
				if (num3 <= num1)
				{
					T = 0.0f;
					s = Mathf.Clamp(-num5 / num2, 0.0f, 1f);
				}
				else
				{
					float num6 = Vector3.Dot(vector31, vector32);
					float num7 = (float) (num2 * num3 - num6 * num6);
					s = num7 == 0.0
						? 0.0f
						: Mathf.Clamp(
							(float) (num6 * num4 - num5 * num3) / num7, 0.0f, 1f);
					T = (num6 * s + num4) / num3;
					if (T < 0.0)
					{
						T = 0.0f;
						s = Mathf.Clamp(-num5 / num2, 0.0f, 1f);
					}
					else if (T > 1.0)
					{
						T = 1f;
						s = Mathf.Clamp((num6 - num5) / num2, 0.0f, 1f);
					}
				}
			}

			c1 = p1 + vector31 * s;
			c2 = p2 + vector32 * T;
			return Vector3.Dot(c1 - c2, c1 - c2);
		}
	}
}
