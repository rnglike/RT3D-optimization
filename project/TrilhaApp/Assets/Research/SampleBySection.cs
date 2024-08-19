using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Profiling;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem.XR;

public class SampleBySection : MonoBehaviour
{
    // Public variables
    public GameObject xrOrigin;
    public float rotationSpeed = 1.0f;
    public float coldBootTime = 1.0f;
    public float waitTime = 1.0f;
    public Transform[] sections;
    public int currentSection = 0;
    public string experimentName = "Experiment #1";

    // Private variables
    private bool isRotating = false;
    private bool isMoving = false;
    private Transform mainCamera;
    private Transform xrRig;
    private Transform xrControllerLeft;
    private Transform xrControllerRight;
    private StreamWriter csvWriter;
    private string folderPath;

    // Profiling recorders
    private ProfilerRecorder mainThreadTimeRecorder;
    private ProfilerRecorder renderRecorderAllThreads;

    // Method to calculate the average value from a ProfilerRecorder
    private static double GetRecorderAverage(ProfilerRecorder recorder)
    {
        int samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double totalValue = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (int i = 0; i < samplesCount; ++i)
                totalValue += samples[i].Value;
            totalValue /= samplesCount;
        }

        return totalValue;
    }

    // Unity lifecycle methods
    private void Start()
    {
        InitializeComponents();
        InitializeSections();
        CreateResearchFolder();
        StartCoroutine(WaitForBegin());
    }

    private void Update()
    {
        if (isRotating)
        {
            RotateXrRig();
        }
        else if (isMoving)
        {
            MoveToNextSection();
        }
    }

    private void LateUpdate()
    {
        if (isRotating)
        {
            LogRotationData();
        }
    }

    // Initialization methods
    private void InitializeComponents()
    {
        mainCamera = Camera.main.transform;
        xrRig = mainCamera.parent;
        xrControllerLeft = xrRig.Find("LeftHand Controller");
        xrControllerRight = xrRig.Find("RightHand Controller");

        // Disable the XR controllers to avoid tracking issues
        mainCamera.GetComponent<TrackedPoseDriver>().enabled = false;
        xrOrigin.GetComponent<XROrigin>().enabled = false;

        xrControllerLeft.gameObject.SetActive(false);
        xrControllerRight.gameObject.SetActive(false);

        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
        renderRecorderAllThreads = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Render Thread Frame Time");
    }

    private void InitializeSections()
    {
        // Get all the sections in the scene
        sections = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            sections[i] = transform.GetChild(i);
        }
    }

    private void CreateResearchFolder()
    {
        // Create a folder to store the research data
        folderPath = Application.isEditor ? Path.Combine(Application.dataPath, "Research/Logs") : Application.persistentDataPath;
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    // Logging methods
    private void LogRotationData()
    {
        double cpuTime = 0.0;
        double gpuTime = 0.0;

        if (mainThreadTimeRecorder.Valid)
        {
            cpuTime = mainThreadTimeRecorder.LastValue * 1e-6; // Convert nanoseconds to milliseconds
        }
        else
        {
            Debug.LogWarning("CPU Recorder is not valid");
        }

        if (renderRecorderAllThreads.Valid)
        {
            gpuTime = renderRecorderAllThreads.LastValue * 1e-6; // Convert nanoseconds to milliseconds
        }
        else
        {
            Debug.LogWarning("GPU Recorder is not valid");
        }

        Debug.Log($"CPU Time: {cpuTime} ms, GPU Time: {gpuTime} ms");

        string log = FormatLogRecord(
            xrRig.position.x,
            xrRig.position.y,
            xrRig.rotation.eulerAngles.y,
            cpuTime,
            gpuTime,
            1.0f / Time.deltaTime
        );

        csvWriter.WriteLine(log);
    }

    private string FormatLogRecord(float x, float y, float rotation, double cpuTime, double gpuTime, float fps)
    {
        return $"{x.ToString("F6").Replace(',', '.')},{y.ToString("F6").Replace(',', '.')},{rotation.ToString("F6").Replace(',', '.')},{cpuTime.ToString("F6").Replace(',', '.')},{gpuTime.ToString("F6").Replace(',', '.')},{fps.ToString("F6").Replace(',', '.')}";
    }

    // Rotation and movement methods
    private void RotateXrRig()
    {
        // Rotate the XR rig
        float angle = rotationSpeed * Time.deltaTime; // degrees per second
        xrRig.Rotate(0.0f, angle, 0.0f, Space.Self);

        // Check if the rotation is complete
        if (xrRig.rotation.eulerAngles.y >= 359.0f)
        {
            xrRig.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            isRotating = false;
            isMoving = true;
        }
    }

    private void MoveToNextSection()
    {
        // Move the XR rig to the next section
        xrRig.position = Vector3.MoveTowards(xrRig.position, sections[currentSection].position, 0.1f);

        // Check if the movement is complete
        if (Vector3.Distance(xrRig.position, sections[currentSection].position) < 0.1f)
        {
            isMoving = false;
            StartCoroutine(WaitForNextSection());
        }
    }

    // Coroutine methods
    private IEnumerator WaitForBegin()
    {
        yield return new WaitForSeconds(coldBootTime);
        StartRotation();
    }

    private IEnumerator WaitForNextSection()
    {
        yield return new WaitForSeconds(waitTime);
        currentSection++;
        if (currentSection >= sections.Length)
        {
            EndExperiment();
            currentSection = 0;
            yield break;
        }

        OpenNewCsvWriter();
        isRotating = true;
        isMoving = false;
    }

    // Experiment control methods
    public void StartRotation()
    {
        OpenNewCsvWriter();
        isRotating = true;
    }

    public void StopRotation()
    {
        isRotating = false;
        isMoving = true;
    }

    public void EndExperiment()
    {
        mainThreadTimeRecorder.Dispose();
        renderRecorderAllThreads.Dispose();
        mainCamera.GetComponent<TrackedPoseDriver>().enabled = true;
        xrOrigin.GetComponent<XROrigin>().enabled = true;
        xrControllerLeft.gameObject.SetActive(true);
        xrControllerRight.gameObject.SetActive(true);
        csvWriter.Close();
        Destroy(this);
    }

    private void OpenNewCsvWriter()
    {
        csvWriter?.Close();
        csvWriter = new StreamWriter(Path.Combine(folderPath, $"rotation_{experimentName}_{currentSection}.csv"));
        csvWriter.WriteLine("X,Y,Rotation,Main Thread,Render Thread,FPS");
    }
}
