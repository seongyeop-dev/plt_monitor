#define USE_NEWTONSOFT_JSON
#if USE_NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using System;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Yeop
{
    /// <summary>
    /// JSON 관리 시스템
    /// </summary>
    public static class NST_Json
    {
        #region Initialize
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]      //RuntimeInitializeOnLoadMethod 사용을 추천한다 //static NST_Core() => Initialize(); 는 RuntimeInitializeOnLoadMethod 와 기능이 중복된다
        public static void Initialize()
        {
        }
        #endregion

        /// <summary>
        /// JSON 파일에서 데이터를 로드합니다.
        /// </summary>
        /// <typeparam name="T">데이터 타입</typeparam>
        /// <param name="path">파일 경로</param>
        /// <returns>로드된 데이터 또는 기본값</returns>
        public static T ImportJson<T>(string path)
        {
            T data = default;

            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    data = DeserializeJson<T>(json);
                }
                else
                {
                    Debug.Log($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                            $"Message : File does not exist [{path}]\n");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                               $"Message : {ex.Message}\n" +
                               $"StackTrace : {ex.StackTrace}\n");
            }

            return data;
        }

        /// <summary>
        /// 데이터를 JSON 형식으로 파일에 저장합니다.
        /// </summary>
        /// <typeparam name="T">데이터 타입</typeparam>
        /// <param name="data">저장할 데이터</param>
        /// <param name="path">파일 경로</param>
        public static void ExportJson<T>(T data, string path)
        {
            try
            {
                string json = SerializeToJson(data);

                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                            $"Message : Serialized data is null or empty [{path}]\n");

                    return;
                }

                string directoryPath = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                               $"Message : {ex.Message}\n" +
                               $"StackTrace : {ex.StackTrace}\n");
            }
        }

        /// <summary>
        /// TextAsset에서 JSON 데이터를 로드합니다.
        /// </summary>
        /// <typeparam name="T">데이터 타입</typeparam>
        /// <param name="textAsset">TextAsset</param>
        /// <returns>로드된 데이터 또는 기본값</returns>
        public static T LoadJsonFromTextAsset<T>(TextAsset textAsset)
        {
            T data = default;

            try
            {
                if (textAsset != null)
                {
                    string json = textAsset.text;
                    data = DeserializeJson<T>(json);
                }
                else
                {
                    Debug.Log($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                            $"Message : TextAsset Null [{textAsset}]\n");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                               $"Message : {ex.Message}\n" +
                               $"StackTrace : {ex.StackTrace}\n");
            }

            return data;
        }
#if UNITY_EDITOR
        /// <summary>
        /// 데이터를 JSON 형식으로 파일에 저장하고 해당 파일을 TextAsset으로 반환합니다.
        /// </summary>
        /// <typeparam name="T">데이터 타입</typeparam>
        /// <param name="data">저장할 데이터</param>
        /// <param name="assetPath">TextAsset을 생성 또는 업데이트할 경로</param>
        /// <returns>생성된 또는 업데이트된 TextAsset</returns>
        public static TextAsset SaveJsonToTextAsset<T>(T data, string assetPath)
        {
            try
            {
                string json = SerializeToJson(data);

                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                            $"Message : Serialized data is null or empty [{assetPath}]\n");

                    return null;
                }

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                File.WriteAllBytes(assetPath, bytes);
                AssetDatabase.Refresh();

                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);

                return textAsset;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                               $"Message : {ex.Message}\n" +
                               $"StackTrace : {ex.StackTrace}\n");
                return null;
            }
        }
#endif

        #region 내부 헬퍼 메소드

        /// <summary>
        /// 데이터를 JSON 문자열로 직렬화합니다.
        /// </summary>
        /// <typeparam name="T">직렬화할 데이터 타입</typeparam>
        /// <param name="data">직렬화할 데이터</param>
        /// <returns>JSON 문자열</returns>
        public static string SerializeToJson<T>(T data)
        {
            string json;
#if USE_NEWTONSOFT_JSON
            json = JsonConvert.SerializeObject(data, Formatting.Indented);
#else
            json = JsonUtility.ToJson(data, true);
#endif
            return json;
        }

        /// <summary>
        /// JSON 문자열을 지정한 타입으로 디시리얼라이즈합니다.
        /// </summary>
        /// <typeparam name="T">디시리얼라이즈할 데이터 타입</typeparam>
        /// <param name="json">JSON 문자열</param>
        /// <returns>디시리얼라이즈된 데이터(T)</returns>
        public static T DeserializeJson<T>(string json)
        {
            T data = default;
#if USE_NEWTONSOFT_JSON
            data = JsonConvert.DeserializeObject<T>(json);
#else
            data = JsonUtility.FromJson<T>(json);
#endif
            return data;
        }

        #endregion
    }
}