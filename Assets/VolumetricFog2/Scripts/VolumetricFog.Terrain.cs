﻿//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist 2
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace VolumetricFogAndMist2 {

    public partial class VolumetricFog : MonoBehaviour {

        const string SURFACE_CAM_NAME = "SurfaceCam";

        public enum HeightmapCaptureResolution {
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024
        }

        RenderTexture rt;

        Camera surfaceCam;
        Matrix4x4 camMatrix;
        Vector3 lastCamPos;

        void DisposeSurfaceCapture() {
            if (rt != null) {
                rt.Release();
                DestroyImmediate(rt);
            }
        }

        void CheckSurfaceCapture() {
            if (surfaceCam == null) {
                Transform childCam = transform.Find(SURFACE_CAM_NAME);
                if (childCam != null) {
                    surfaceCam = childCam.GetComponent<Camera>();
                    if (surfaceCam == null) {
                        DestroyImmediate(childCam.gameObject);
                    }
                }
            }
        }


        void SurfaceCaptureSupportCheck() {

            Transform childCam = transform.Find(SURFACE_CAM_NAME);
            if (childCam != null) {
                surfaceCam = childCam.GetComponent<Camera>();
            }

            if (!activeProfile.terrainFit) {
                DisposeSurfaceCapture();
                return;
            }

            if (surfaceCam == null) {
                if (childCam != null) {
                    DestroyImmediate(childCam.gameObject);
                }
                if (surfaceCam == null) {
                    GameObject camObj = new GameObject(SURFACE_CAM_NAME, typeof(Camera));
                    camObj.transform.SetParent(transform, false);
                    surfaceCam = camObj.GetComponent<Camera>();
                    surfaceCam.depthTextureMode = DepthTextureMode.None;
                    surfaceCam.clearFlags = CameraClearFlags.Depth;
                    surfaceCam.allowHDR = false;
                    surfaceCam.allowMSAA = false;
                }

                surfaceCam.stereoTargetEye = StereoTargetEyeMask.None;
            }

            UniversalAdditionalCameraData camData = surfaceCam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null) {
                camData = surfaceCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
            if (camData != null) {
                camData.dithering = false;
                camData.renderPostProcessing = false;
                camData.renderShadows = false;
                camData.requiresColorTexture = false;
                camData.requiresDepthTexture = false;
                camData.stopNaN = false;

#if UNITY_2021_3_OR_NEWER
                CheckAndAssignDepthRenderer(camData);
#endif
            }

            surfaceCam.orthographic = true;
            surfaceCam.nearClipPlane = 1f;
            surfaceCam.enabled = false;

            if (rt != null && rt.width != (int)activeProfile.terrainFitResolution) {
                if (surfaceCam.targetTexture == rt) {
                    surfaceCam.targetTexture = null;
                }
                rt.Release();
                DestroyImmediate(rt);
            }

            if (rt == null) {
                rt = new RenderTexture((int)activeProfile.terrainFitResolution, (int)activeProfile.terrainFitResolution, 24, RenderTextureFormat.Depth);
                rt.antiAliasing = 1;
            }

            int thisLayer = 1 << gameObject.layer;
            if ((activeProfile.terrainLayerMask & thisLayer) != 0) {
                activeProfile.terrainLayerMask &= ~thisLayer; // exclude fog layer
            }

            surfaceCam.cullingMask = activeProfile.terrainLayerMask;
            surfaceCam.targetTexture = rt;

            if (activeProfile.terrainFit) {
                ScheduleHeightmapCapture();
            }
        }

#if UNITY_2021_3_OR_NEWER
        UniversalRendererData depthRendererData;
        void CheckAndAssignDepthRenderer(UniversalAdditionalCameraData camData) {
            UniversalRenderPipelineAsset pipe = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            if (pipe == null) return;

            ScriptableRendererData[] rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pipe);

            if (depthRendererData == null) {
                depthRendererData = Resources.Load<UniversalRendererData>("Shaders/VolumetricFogDepthRenderer");
                if (depthRendererData == null) {
                    Debug.LogError("Volumetric Fog Depth Renderer asset not found.");
                    return;
                }
                depthRendererData.postProcessData = null;
            }
            int depthRendererIndex = -1;
            for (int k = 0; k < rendererDataList.Length; k++) {
                if (rendererDataList[k] == depthRendererData) {
                    depthRendererIndex = k;
                    break;
                }
            }
            if (depthRendererIndex < 0) {
                depthRendererIndex = rendererDataList.Length;
                System.Array.Resize<ScriptableRendererData>(ref rendererDataList, depthRendererIndex + 1);
                rendererDataList[depthRendererIndex] = depthRendererData;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(pipe);
#endif
            }
            camData.SetRenderer(depthRendererIndex);
        }
#endif

        /// <summary>
        /// Updates terrain heightmap on this volumetric fog
        /// </summary>
        public void ScheduleHeightmapCapture() {
            if (surfaceCam != null) {
                surfaceCam.Render();
                surfaceCam.enabled = false;
                if (!fogMat.IsKeywordEnabled(ShaderParams.SKW_SURFACE)) {
                    fogMat.EnableKeyword(ShaderParams.SKW_SURFACE);
                }

            }
        }


        void SetupCameraCaptureMatrix() {

            Vector3 camPos = transform.position + new Vector3(0, transform.lossyScale.y * 0.51f, 0);
            surfaceCam.farClipPlane = 10000;
            surfaceCam.transform.position = camPos;
            surfaceCam.transform.eulerAngles = new Vector3(90, 0, 0);
            Vector3 size = transform.lossyScale;
            surfaceCam.orthographicSize = Mathf.Max(size.x * 0.5f, size.z * 0.5f);

            ComputeSufaceTransform(surfaceCam.projectionMatrix, surfaceCam.worldToCameraMatrix);

            fogMat.SetMatrix(ShaderParams.SurfaceCaptureMatrix, camMatrix);
            fogMat.SetTexture(ShaderParams.SurfaceDepthTexture, surfaceCam.targetTexture);
            fogMat.SetVector(ShaderParams.SurfaceData, new Vector4(camPos.y, activeProfile.terrainFogHeight, activeProfile.terrainFogMinAltitude, activeProfile.terrainFogMaxAltitude));
        }

        void SurfaceCaptureUpdate() {

            if (surfaceCam == null) return;

            SetupCameraCaptureMatrix();

            if (!surfaceCam.enabled && lastCamPos != surfaceCam.transform.position) {
                lastCamPos = surfaceCam.transform.position;
                ScheduleHeightmapCapture();
                requireUpdateMaterial = true;
            }
        }

        static Matrix4x4 identityMatrix = Matrix4x4.identity;

        void ComputeSufaceTransform(Matrix4x4 proj, Matrix4x4 view) {
            // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
            // apply z reversal to projection matrix. We need to do it manually here.
            if (SystemInfo.usesReversedZBuffer) {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }

            Matrix4x4 worldToShadow = proj * view;

            var textureScaleAndBias = identityMatrix;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;

            // Apply texture scale and offset to save a MAD in shader.
            camMatrix = textureScaleAndBias * worldToShadow;
        }


    }


}