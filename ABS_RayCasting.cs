using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static characterMove;
/*
 * 继承该抽象类，必须使用有参构造函数（无参构造函数被设为private）
 */
public abstract class ABS_RayCasting
{
    
    private static Ray ray_left, ray_right, ray_up, ray_down, ray_forward, ray_back;

    public struct ABS_Ray {
        public Ray[] rays;
        public bool[] RayHitStatus;

        public ABS_Ray(Ray[] _rays, bool[] _rayHitStatus)
        {
            rays = _rays;
            RayHitStatus = _rayHitStatus;
            
        }
    }

    public RaycastHit[] hit;
    public RaycastHit singlehit;
    public ABS_Ray zRay;
//Vector3.left,Vector3.right,Vector3.up,Vector3.down,Vector3.forward,Vector3.back

private ABS_RayCasting() { }
    public ABS_RayCasting(Vector3 _originV3)//参数为需要提供射线原点
    {
        ray_left.origin = _originV3;
        ray_right.origin = _originV3;
        ray_up.origin = _originV3;
        ray_down.origin = _originV3;
        ray_forward.origin = _originV3;
        ray_back.origin = _originV3;

        ray_left.direction = Vector3.left;
        ray_right.direction = Vector3.right;
        ray_up.direction = Vector3.up;
        ray_down.direction = Vector3.down;
        ray_forward.direction = Vector3.forward;
        ray_back.direction = Vector3.back;

        zRay = new ABS_Ray(new Ray[6] { ray_left, ray_right, ray_up, ray_down, ray_forward, ray_back },
            new bool[6] { false, false, false, false, false, false });
        hit = new RaycastHit[6];
    }

    public abstract void OnCollisionDetected(RaycastHit hit);//抽象方法，当射线击中Collider后功能的功能放在这


    //提供的射线方法
    public RaycastHit RayCastingFunc(Vector3 selfV3, Vector3 rayDir, float rayDirection)
    {
        Ray ray = new Ray(selfV3, rayDir);
        DrawRayFunc(ray, rayDirection);
        if (Physics.Raycast(selfV3, rayDir, out singlehit, rayDirection))
        {
            OnCollisionDetected(singlehit);
        }
        return singlehit;
    }

    public RaycastHit RayCastingFunc(ABS_Ray _zRay,short Index, float rayDirection)
    {
        DrawRayFunc(_zRay.rays[Index], rayDirection);
        if (Physics.Raycast(_zRay.rays[Index], out hit[Index], rayDirection))
        {
            zRay.RayHitStatus[Index] = true;
            OnCollisionDetected(hit[Index]);
        }
        return hit[Index];
    }

    public Vector3 Attach()
    {
        Vector3 JointForce = Vector3.zero;
        for (int i = 0; i < zRay.RayHitStatus.Length; i++)
        {
            if(zRay.RayHitStatus[i] == true)
            {
                JointForce += zRay.rays[i].direction;
            }
        }
        return JointForce;
    }


    private bool DrawRayFunc(Ray ray, float rayDirection)
    {
        Debug.DrawRay(ray.origin, ray.direction * rayDirection, Color.blue);
        return true;
    }

}
