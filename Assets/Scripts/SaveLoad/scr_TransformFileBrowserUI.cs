using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Transform Monitoring 파일 브라우저 UI
/// - 저장 폴더 내 JSON / CSV 파일 목록 표시
/// - Save / Load 버튼 연결
/// - 선택 파일 정보 표시
/// - 상태 텍스트를 마지막 동작 기준으로 유지
/// </summary>
public class scr_TransformFileBrowserUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private scr_TransformDataSaveLoadManager saveLoadManager;

    [Header("File List UI")]
    [SerializeField] private RectTransform fileButtonContainer;
    [SerializeField] private Button fileButtonPrefab;

    [Header("Mode Buttons")]
    [SerializeField] private Button showJsonButton;
    [SerializeField] private Button showCsvButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button saveJsonButton;
    [SerializeField] private Button saveCsvButton;
    [SerializeField] private Button loadSelectedButton;

    [Header("Info Text")]
    [SerializeField] private Text currentModeText;
    [SerializeField] private Text selectedFileText;
    [SerializeField] private Text statusText;

    [Header("Run Option")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool showJsonFilesByDefault = true;

    private readonly List<Button> spawnedButtons = new List<Button>();

    private string currentSearchPattern = "*.json";
    private string selectedFilePath = string.Empty;

    private void Start()
    {
        if (initializeOnStart)
        {
            InitializeUI();
        }
    }

    public void InitializeUI()
    {
        BindButtons();

        currentSearchPattern = showJsonFilesByDefault ? "*.json" : "*.csv";
        selectedFilePath = string.Empty;

        RefreshFileList(
            showJsonFilesByDefault ? "Showing JSON files" : "Showing CSV files",
            keepSelectedFile: false);
    }

    public void RefreshFileList()
    {
        RefreshFileList("File list refreshed", keepSelectedFile: true);
    }

    private void RefreshFileList(string statusMessage, bool keepSelectedFile)
    {
        ClearButtons();

        if (saveLoadManager == null)
        {
            SetStatus("SaveLoadManager missing");
            UpdateModeText();
            UpdateSelectedFileText();
            return;
        }

        List<string> filePaths = saveLoadManager.GetSavedFiles(currentSearchPattern);

        bool selectedFileStillExists = false;

        for (int i = 0; i < filePaths.Count; i++)
        {
            string filePath = filePaths[i];
            string fileName = Path.GetFileName(filePath);

            Button button = Instantiate(fileButtonPrefab, fileButtonContainer);
            button.gameObject.name = "File_" + fileName;
            button.transition = Selectable.Transition.None;

            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = fileName;
            }

            string capturedPath = filePath;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectFile(capturedPath));

            if (keepSelectedFile && IsSamePath(filePath, selectedFilePath))
            {
                selectedFileStillExists = true;
            }

            spawnedButtons.Add(button);
        }

        if (!keepSelectedFile || !selectedFileStillExists)
        {
            selectedFilePath = string.Empty;
        }

        UpdateModeText();
        UpdateSelectedFileText();
        SetStatus(statusMessage);
    }

    private void BindButtons()
    {
        BindButton(showJsonButton, () => SetSearchPattern("*.json"));
        BindButton(showCsvButton, () => SetSearchPattern("*.csv"));
        BindButton(refreshButton, () => RefreshFileList("File list refreshed", keepSelectedFile: true));
        BindButton(saveJsonButton, SaveJson);
        BindButton(saveCsvButton, SaveCsv);
        BindButton(loadSelectedButton, LoadSelectedFile);
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.transition = Selectable.Transition.None;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void SetSearchPattern(string pattern)
    {
        currentSearchPattern = pattern;
        selectedFilePath = string.Empty;

        string statusMessage = pattern == "*.json"
            ? "Showing JSON files"
            : "Showing CSV files";

        RefreshFileList(statusMessage, keepSelectedFile: false);
    }

    private void SelectFile(string path)
    {
        selectedFilePath = path;
        UpdateSelectedFileText();
        SetStatus("Selected file");
    }

    private void SaveJson()
    {
        if (saveLoadManager == null)
        {
            SetStatus("SaveLoadManager missing");
            return;
        }

        saveLoadManager.SaveJson();

        currentSearchPattern = "*.json";
        selectedFilePath = saveLoadManager.GetDefaultJsonPath();

        RefreshFileList("JSON saved", keepSelectedFile: true);
    }

    private void SaveCsv()
    {
        if (saveLoadManager == null)
        {
            SetStatus("SaveLoadManager missing");
            return;
        }

        saveLoadManager.SaveCsv();

        currentSearchPattern = "*.csv";

        List<string> csvFiles = saveLoadManager.GetSavedFiles(currentSearchPattern);
        selectedFilePath = csvFiles.Count > 0 ? csvFiles[0] : string.Empty;

        RefreshFileList("CSV saved", keepSelectedFile: true);
    }

    private void LoadSelectedFile()
    {
        if (saveLoadManager == null)
        {
            SetStatus("SaveLoadManager missing");
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedFilePath))
        {
            SetStatus("No file selected");
            return;
        }

        if (selectedFilePath.EndsWith(".json"))
        {
            TransformMonitoringSaveData data = saveLoadManager.LoadJsonFromPath(selectedFilePath);
            SetStatus(data != null ? "JSON loaded" : "JSON load failed");
            return;
        }

        if (selectedFilePath.EndsWith(".csv"))
        {
            List<Dictionary<string, object>> rows = saveLoadManager.LoadCsvFromPath(selectedFilePath);
            SetStatus(rows != null ? "CSV loaded" : "CSV load failed");
            return;
        }

        SetStatus("Unsupported file type");
    }

    private void UpdateModeText()
    {
        if (currentModeText == null)
        {
            return;
        }

        currentModeText.text = "Mode : " + GetModeLabel();
    }

    private string GetModeLabel()
    {
        return currentSearchPattern == "*.csv" ? "CSV" : "JSON";
    }

    private void UpdateSelectedFileText()
    {
        if (selectedFileText == null)
        {
            return;
        }

        string fileName = string.IsNullOrWhiteSpace(selectedFilePath)
            ? "-"
            : Path.GetFileName(selectedFilePath);

        selectedFileText.text = "Selected : " + fileName;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = "Status : " + message;
        }
    }

    private bool IsSamePath(string a, string b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
        {
            return false;
        }

        string fullA = Path.GetFullPath(a);
        string fullB = Path.GetFullPath(b);

        return string.Equals(fullA, fullB, System.StringComparison.OrdinalIgnoreCase);
    }

    private void ClearButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
            {
                Destroy(spawnedButtons[i].gameObject);
            }
        }

        spawnedButtons.Clear();
    }
}