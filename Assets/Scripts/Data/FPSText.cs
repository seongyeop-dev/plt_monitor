using System;
using UnityEngine;

namespace Naon
{
    //[DisallowMultipleComponent]
    public class FpsText : MonoBehaviour
    {
        private float _deltaTime = 0.0f;

        private void Update()
        {
            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            try
            {
                DisplayFps();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                               $"Message : {ex.Message}\n" +
                               $"StackTrace : {ex.StackTrace}\n");
            }
        }

        /// <summary>
        /// FPS를 화면에 표시합니다.
        /// </summary>
        private void DisplayFps()
        {
            float fps = 1.0f / _deltaTime;
            float msec = _deltaTime * 1000.0f;

            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(0, 0, Screen.width, Screen.height / 50);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = Screen.height / 50;
            style.normal.textColor = GetFpsColor(fps);

            string text = string.Format("{0:0.0} ms, {1:0.} fps", msec, fps);
            GUI.Label(rect, text, style);
        }

        /// <summary>
        /// FPS 값에 따라 색상을 반환합니다.
        /// </summary>
        /// <param name="fps">현재 FPS</param>
        /// <returns>색상(Color)</returns>
        private Color GetFpsColor(float fps)
        {
            if (fps >= 50f) return Color.green;
            if (fps >= 30f) return Color.yellow;
            if (fps >= 15f) return new Color(1.0f, 0.5f, 0.0f); // 주황색
            return Color.red;
        }
    }
}