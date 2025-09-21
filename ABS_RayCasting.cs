using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static characterMove;
using static UnityEditor.FilePathAttribute;


public abstract class ABS_RayCasting : MonoBehaviour
{
    [Header("吸附设置")]
    public float adhesionDistance = 0.1f;
    public float stoppingDistance = 0.05f;
    private Vector3 attachmentPoint;
    private Vector3 JointForce = Vector3.zero;


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
    public bool Attached = false;
    //Vector3.left,Vector3.right,Vector3.up,Vector3.down,Vector3.forward,Vector3.back

    public void Init(Vector3 _originV3)
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

    //1、设置ray,初始提供六个方向的射线
    public void setRay(int rayIndex, Vector3 rayOrigin)
    {
        zRay.rays[rayIndex].origin = rayOrigin;
    }
    public void setRay(int rayIndex ,Vector3 rayOrigin,Vector3 rayDirection)
    {
        zRay.rays[rayIndex].origin = rayOrigin;
        zRay.rays[rayIndex].direction = rayDirection;
    }

    public void StartRay(float[] rayDistance, GameObject go)
    {
        Debug.Log($"hello here [1]");

            for (int i = 0; i < 6; i++)
            {
                Debug.Log($"hello here [3]");
                StartCoroutine(RayThead(i, rayDistance[i], go));
            }



    }

    public void StartRay(int mask , float[] rayDistance, GameObject go)//开始发出射线；mask是掩码，说明发出哪几个射线
                                                                        //rayDirection是射线距离，是一个数组，若只有一个射线距离，有多个射线，则多个射线距离是相同的
                                                                        //go是实现类脚本的物体，用来给射线定起点，默认为物体的tansform.position
    {
        int rdIndex = 0;
        if(rayDistance.Length == 1)
        {
            for (int i = 0; i < 6; i++)
            {
                if ((mask & (1 << (i - 1))) != 0)
                {
                    StartCoroutine(RayThead(i, rayDistance[rdIndex], go));
                }
            }
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                if ((mask & (1 << (i - 1))) != 0)
                {
                    StartCoroutine(RayThead(i, rayDistance[rdIndex++], go));
                }
            }
        }

    }


    private RaycastHit RayCastingFunc(ABS_Ray _zRay,int Index, float rayDistance,GameObject go)
    {
        _zRay.rays[Index].origin = go.transform.position;
        DrawRayFunc(_zRay.rays[Index], rayDistance);
        if (Physics.Raycast(_zRay.rays[Index], out hit[Index], rayDistance))
        {
            
            zRay.RayHitStatus[Index] = true;
            OnCollisionDetected(hit[Index]);
        }
        return hit[Index];
    }

    public Vector3 Attach(RaycastHit hit,GameObject gameObject)
    {
        Vector3 HitclosestPoint = hit.collider.ClosestPoint(gameObject.transform.position);
        Vector3 GOclosestPoint = gameObject.GetComponent<Collider>().ClosestPoint(HitclosestPoint);


        if (Attached)//若已经吸附，则直接返回合力，而不计算合力
        {
            return JointForce;
        }
        JointForce = Vector3.zero;
        for (int i = 0;i < zRay.RayHitStatus.Length; i++)
        {
            if(zRay.RayHitStatus[i] == true)
            {
                JointForce += HitclosestPoint - GOclosestPoint;
            }
        }

        //Vector3 surfaceNormal = (gameObject.transform.position - HitclosestPoint).normalized;//吸附方向并归一化处理
        //attachmentPoint = HitclosestPoint + surfaceNormal * adhesionDistance;
        Debug.Log("Attach : "+Attached);
        Debug.Log($"hit Position: {hit.transform.position}");
        Debug.Log("ACS : " + attachmentPoint+" c: "+ HitclosestPoint+"s :");
        
        if (Attached)
            return JointForce;
        for (int i = 0; i < zRay.RayHitStatus.Length; i++)
        {
            if(zRay.RayHitStatus[i] == true)
            {
                Debug.Log("JointForce Before: " + JointForce);
                JointForce = HitclosestPoint;
                Debug.Log("JointForce After: " + JointForce);
            }
        }
        return JointForce;
    }

    public bool StopMove(bool InputSwitch)
    {
        if (!InputSwitch)
            InputSwitch = true;
        return InputSwitch;
    }
    private bool DrawRayFunc(Ray ray, float rayDistance)
    {
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.blue);
        return true;
    }

    public IEnumerator RayThead(int Index,float rayDistance,GameObject gameObject)
    {
        Debug.Log($"hello here[2]:  {zRay}");
        while (true)
        {
            RayCastingFunc(zRay, Index, rayDistance, gameObject);
            yield return null;
        }
    }


}
