/* 
Copyright © 2016 NaturalPoint Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. 
*/

using System;
using UnityEngine;


/// <summary>
/// Implements live tracking of streamed OptiTrack rigid body data onto an object.
/// </summary>
public class OptitrackRigidBody : MonoBehaviour
{
    [Tooltip("The object containing the OptiTrackStreamingClient script.")]
    public OptitrackStreamingClient StreamingClient;

    [Tooltip("The Streaming ID of the rigid body in Motive")]
    public Int32 RigidBodyId;

    [Tooltip("Subscribes to this asset when using Unicast streaming.")]
    public bool NetworkCompensation = true;

    [Tooltip("アバターがこれより下にいかないようにするY座標")]
    public float floorHeight = 0.0f;

    [Tooltip("アバターがこれより上にいかないようにするY座標")]
    public float topHeight = 0.0f;

    [Tooltip("OptiTrackの座標とUnityのスケールを合わせる倍率")]
    public float scale = 1.0f; 


    void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if ( this.StreamingClient == null )
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if ( this.StreamingClient == null )
            {
                Debug.LogError( GetType().FullName + ": Streaming client not set, and no " + typeof( OptitrackStreamingClient ).FullName + " components found in scene; disabling this component.", this );
                this.enabled = false;
                return;
            }
        }

        this.StreamingClient.RegisterRigidBody( this, RigidBodyId );
    }


#if UNITY_2017_1_OR_NEWER
    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }


    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }


    void OnBeforeRender()
    {
        UpdatePose();
    }
#endif


    void Update()
    {
        UpdatePose();
    }

    public Vector3 initPos;
    public Vector3 initRot;
    public bool is_charaPos;
    public bool isPlayer;
    void UpdatePose()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId, NetworkCompensation);
        if (rbState != null)
        {
            Vector3 targetPosition = rbState.Pose.Position * scale;

            // is_charaPosがオンなら初期位置オフセットを加える
            if (is_charaPos)
            {
                targetPosition += initPos;
            }

            if (isPlayer)
            {
                // ★★★ めり込み防止処理 ★★★
                // Y座標が floorHeight よりも低くならないようにする
                targetPosition.y = Mathf.Max(targetPosition.y, floorHeight);

                // アバタが浮かないようにする
                targetPosition.y = Mathf.Min(targetPosition.y, topHeight);

                Vector3 newPosition = new Vector3(
                this.transform.localPosition.x, // X座標は現在の値を維持
                targetPosition.y,                // Y座標はOptiTrackの値を使用
                this.transform.localPosition.z   // Z座標は現在の値を維持
                );

                this.transform.localPosition = newPosition;
            }
            else
            {
                this.transform.localPosition = targetPosition;
            }
            this.transform.localRotation = rbState.Pose.Orientation * Quaternion.Euler(initRot);
        }
    }
}
