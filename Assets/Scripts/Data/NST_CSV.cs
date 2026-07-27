using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


namespace Yeop
{
    /// <summary>
    /// CSV 데이터 관리 시스템  
    /// CSVHelper 가 동일하게 동작한다
    /// </summary>
    public static class NST_CSV
    {
        #region Initialize
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]      //RuntimeInitializeOnLoadMethod 사용을 추천한다 //static NST_Core() => Initialize(); 는 RuntimeInitializeOnLoadMethod 와 기능이 중복된다
        public static void Initialize()
        {
        }
        #endregion

        // 정규식 패턴들
        private const string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private const string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        private static readonly char[] TRIM_CHARS = { '\"' };

        #region Public Methods

        /// <summary>
        /// Resources 폴더 내의 CSV 파일을 불러와 파싱합니다.
        /// </summary>
        /// <param name="fileName">Resources 폴더 내 CSV 파일 이름 (확장자 제외)</param>
        /// <returns>파싱된 CSV 데이터를 담은 Dictionary의 리스트</returns>
        public static List<Dictionary<string, object>> ImportCsvFromResources(string fileName)
        {
            var list = new List<Dictionary<string, object>>();
            try
            {
                TextAsset data = Resources.Load<TextAsset>(fileName);
                if (data == null)
                {
                    Debug.Log($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                            $"Message : CSV file not found in Resources [{fileName}]\n");
                    return list;
                }
                list = ParseCsv(data.text);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                               $"Message : {ex.Message}\n" +
                               $"StackTrace : {ex.StackTrace}\n");
            }
            return list;
        }

        /// <summary>
        /// 지정된 파일 경로의 CSV 파일을 불러와 파싱합니다.
        /// </summary>
        /// <param name="filePath">CSV 파일 경로</param>
        /// <returns>파싱된 CSV 데이터를 담은 Dictionary의 리스트</returns>
        public static List<Dictionary<string, object>> ImportCsvFromFile(string filePath)
        {
            var list = new List<Dictionary<string, object>>();
            try
            {
                if (File.Exists(filePath))
                {
                    // 기존 Encoding.ASCII 대신 UTF-8(BOM 없이)로 파일을 읽습니다.
                    string csvText = File.ReadAllText(filePath, new UTF8Encoding(false));
                    list = ParseCsv(csvText);
                }
                else
                {
                    Debug.Log($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                            $"Message : File does not exist [{filePath}]\n");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                               $"Message : {ex.Message}\n" +
                               $"StackTrace : {ex.StackTrace}\n");
            }
            return list;
        }

        /// <summary>
        /// 주어진 CSV 데이터를 파일로 내보냅니다.
        /// </summary>
        /// <param name="data">CSV 데이터 (Dictionary 리스트)</param>
        /// <param name="filePath">저장할 CSV 파일 경로</param>
        public static void ExportCSV(List<Dictionary<string, object>> data, string filePath)
        {
            try
            {
                if (data == null || data.Count == 0)
                {
                    Debug.Log($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                             $"Message : No data to export.\n");
                    return;
                }

                // CSV 파일 작성
                StringBuilder sb = new StringBuilder();

                // Header 추출 (첫 번째 Dictionary의 키 사용)
                var headerKeys = new List<string>(data[0].Keys);
                for (int i = 0; i < headerKeys.Count; i++)
                {
                    sb.Append(EscapeCsvField(headerKeys[i]));
                    if (i < headerKeys.Count - 1)
                        sb.Append(",");
                }
                sb.AppendLine();

                // 각 데이터 행 작성
                foreach (var row in data)
                {
                    for (int i = 0; i < headerKeys.Count; i++)
                    {
                        row.TryGetValue(headerKeys[i], out object value);
                        string strValue = value != null ? value.ToString() : "";
                        sb.Append(EscapeCsvField(strValue));
                        if (i < headerKeys.Count - 1)
                            sb.Append(",");
                    }
                    sb.AppendLine();
                }

                // 저장할 디렉토리 생성 여부 확인
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // UTF-8(BOM 없이)로 파일 저장
                using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(false)))
                {
                    writer.Write(sb.ToString());
                }

                Debug.Log($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                            $"Message : CSV 파일이 UTF-8로 저장되었습니다: [{filePath}]\n");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                               $"Message : {ex.Message}\n" +
                               $"StackTrace : {ex.StackTrace}\n");
            }
        }

        #endregion

        #region 내부 헬퍼 메소드

        /// <summary>
        /// CSV 텍스트를 파싱하여 Dictionary의 리스트로 변환합니다.
        /// </summary>
        /// <param name="csvText">CSV 형식의 텍스트</param>
        /// <returns>파싱된 데이터의 Dictionary 리스트</returns>
        private static List<Dictionary<string, object>> ParseCsv(string csvText)
        {
            var list = new List<Dictionary<string, object>>();
            try
            {
                string[] lines = Regex.Split(csvText, LINE_SPLIT_RE);
                if (lines.Length <= 1)
                    return list;

                string[] header = Regex.Split(lines[0], SPLIT_RE);

                for (int h = 0; h < header.Length; h++)
                {
                    header[h] = header[h].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "").Trim();
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    // 공백 라인 건너뛰기
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] values = Regex.Split(lines[i], SPLIT_RE);
                    if (values.Length == 0 || string.IsNullOrEmpty(values[0]))
                        continue;

                    Dictionary<string, object> entry = new Dictionary<string, object>();
                    for (int j = 0; j < header.Length && j < values.Length; j++)
                    {
                        string value = values[j];
                        value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                        // <br>를 개행문자로, <c>를 콤마로 치환
                        value = value.Replace("<br>", "\n");
                        value = value.Replace("<c>", ",");

                        object finalValue = value;
                        if (int.TryParse(value, out int n))
                        {
                            finalValue = n;
                        }
                        else if (float.TryParse(value, out float f))
                        {
                            finalValue = f;
                        }
                        entry[header[j]] = finalValue;
                    }
                    list.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}]\n" +
                               $"Message : {ex.Message}\n" +
                               $"StackTrace : {ex.StackTrace}\n");
            }
            return list;
        }

        /// <summary>
        /// CSV 필드를 이스케이프하여 반환합니다.  
        /// 내부 큰따옴표를 두 개의 큰따옴표로 치환하고 전체 필드를 큰따옴표로 감쌉니다.
        /// </summary>
        /// <param name="field">원본 필드 문자열</param>
        /// <returns>이스케이프 처리된 필드 문자열</returns>
        private static string EscapeCsvField(string field)
        {
            if (field == null)
                return "";
            string escaped = field.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        #endregion
    }
}
