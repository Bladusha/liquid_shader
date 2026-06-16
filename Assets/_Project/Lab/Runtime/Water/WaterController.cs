using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using LiquidShader.RuntimeData;

public class WaterController : MonoBehaviour
{
    public struct LabMeasurements
    {
        public float fillFraction;
        public float filledLengthMeters;
        public float pipeInnerDiameterMeters;
        public float pipeAreaSquareMeters;
        public float coilLengthMeters;
        public float flowRateLitersPerMinute;
        public float volumetricFlowCubicMetersPerSecond;
        public float velocityMetersPerSecond;
        public float temperatureCelsius;
        public float densityKgPerCubicMeter;
        public float kinematicViscosityM2PerSecond;
        public float reynoldsNumber;
        public float physicalRegimeFactor;
        public float visualTurbulenceFactor;
        public float shaderFlowSpeed;
        public float shaderTemperature;
        public float shaderSurfaceOscillation;
        public float shaderBubbleAmount;
        public float shaderBubbleSize;
        public string regimeName;
    }

    [Header("Renderer")]
    public Renderer waterRenderer;
    private Material waterMaterial;
    public float transitionSpeed = 2f;

    [Header("Buttons")]
    public BtnSet[] setButtons;
    public BtnPlus[] plusButtons;

    [Header("Path Range")]
    public float pathStart = 0f;
    public float pathEnd = 1f;

    [Header("Lab 01 Calibration")]
    [SerializeField] private float pipeInnerDiameterMeters = 0.012f;
    [SerializeField] private float coilLengthMeters = 14f;
    [SerializeField] private float minPhysicalFlowRateLitersPerMinute = 0.35f;
    [SerializeField] private float maxPhysicalFlowRateLitersPerMinute = 12f;
    [SerializeField] private float minPhysicalTemperatureCelsius = 10f;
    [SerializeField] private float maxPhysicalTemperatureCelsius = 40f;
    [SerializeField] private float laminarReynoldsLimit = 2300f;
    [SerializeField] private float turbulentReynoldsLimit = 10000f;
    [SerializeField] private bool saveMeasurementsToRuntimeData = true;
    [SerializeField] private float measurementSaveInterval = 0.25f;

    [Header("Current State")]
    public BtnSet currentActiveButton;

    private readonly Dictionary<BtnPlus, float> plusAmounts = new Dictionary<BtnPlus, float>();
    private float nextMeasurementSaveTime;

    private float baseFill;
    private float baseFlow;
    private float baseTemperature;
    private float baseSurfaceOscillation;
    private float baseBubbleSize;
    private float baseBubbleAmount;
    private float baseFrontTilt;
    private float baseFrontFade;
    private float baseAlpha;

    private float targetFill;
    private float targetFlow;
    private float targetTemperature;
    private float targetSurfaceOscillation;
    private float targetBubbleSize;
    private float targetBubbleAmount;
    private float targetFrontTilt;
    private float targetFrontFade;
    private float targetAlpha;

    private float curFill;
    private float curFlow;
    private float curTemperature;
    private float curSurfaceOscillation;
    private float curBubbleSize;
    private float curBubbleAmount;
    private float curFrontTilt;
    private float curFrontFade;
    private float curAlpha;

    private static readonly int FillLevelId = Shader.PropertyToID("_FillLevel");
    private static readonly int FlowSpeedId = Shader.PropertyToID("_FlowSpeed");
    private static readonly int TemperatureId = Shader.PropertyToID("_Temperature");
    private static readonly int SurfaceOscillationId = Shader.PropertyToID("_SurfaceOscillation");
    private static readonly int BubbleSizeId = Shader.PropertyToID("_BubbleSize");
    private static readonly int BubbleAmountId = Shader.PropertyToID("_BubbleAmount");
    private static readonly int FrontTiltId = Shader.PropertyToID("_FrontTilt");
    private static readonly int FrontFadeId = Shader.PropertyToID("_FrontFade");
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int PathStartId = Shader.PropertyToID("_min");
    private static readonly int PathEndId = Shader.PropertyToID("_max");

    public LabMeasurements CurrentMeasurements => BuildLabMeasurements(
        curFill,
        curFlow,
        curTemperature,
        curSurfaceOscillation,
        curBubbleAmount,
        curBubbleSize);

    public float PipeInnerDiameterMeters => pipeInnerDiameterMeters;
    public float CoilLengthMeters => coilLengthMeters;

    private void Awake()
    {
        BindButtons();
        ResetButtonVisuals();
    }

    private void Start()
    {
        if (waterRenderer != null)
        {
            waterMaterial = waterRenderer.material;
            curFill = ReadMaterialFloat(FillLevelId, 0.5f);
            curFlow = ReadMaterialFloat(FlowSpeedId, 0.75f);
            curTemperature = ReadMaterialFloat(TemperatureId, 0.35f);
            curSurfaceOscillation = ReadMaterialFloat(SurfaceOscillationId, 0.3f);
            curBubbleSize = ReadMaterialFloat(BubbleSizeId, 1.25f);
            curBubbleAmount = ReadMaterialFloat(BubbleAmountId, 0.35f);
            curFrontTilt = ReadMaterialFloat(FrontTiltId, 0.12f);
            curFrontFade = ReadMaterialFloat(FrontFadeId, 0.06f);
            curAlpha = ReadMaterialFloat(AlphaId, 0.82f);

            pathStart = ReadMaterialFloat(PathStartId, pathStart);
            pathEnd = ReadMaterialFloat(PathEndId, pathEnd);

            targetFill = curFill;
            targetFlow = curFlow;
            targetTemperature = curTemperature;
            targetSurfaceOscillation = curSurfaceOscillation;
            targetBubbleSize = curBubbleSize;
            targetBubbleAmount = curBubbleAmount;
            targetFrontTilt = curFrontTilt;
            targetFrontFade = curFrontFade;
            targetAlpha = curAlpha;

            baseFill = curFill;
            baseFlow = curFlow;
            baseTemperature = curTemperature;
            baseSurfaceOscillation = curSurfaceOscillation;
            baseBubbleSize = curBubbleSize;
            baseBubbleAmount = curBubbleAmount;
            baseFrontTilt = curFrontTilt;
            baseFrontFade = curFrontFade;
            baseAlpha = curAlpha;

            ApplyStaticMaterialSettings();

            if (currentActiveButton != null)
            {
                SelectMode(currentActiveButton);
            }
        }
    }

    public void SelectMode(BtnSet pressedButton)
    {
        if (pressedButton == null)
        {
            return;
        }

        if (!IsAssignedSetButton(pressedButton))
        {
            return;
        }

        if (currentActiveButton != null && currentActiveButton != pressedButton)
        {
            currentActiveButton.Release();
        }

        currentActiveButton = pressedButton;
        currentActiveButton.SetPressedVisual(true);

        baseFill = pressedButton.fillLevel;
        baseFlow = pressedButton.flowSpeed;
        baseTemperature = pressedButton.temperature;
        baseSurfaceOscillation = pressedButton.surfaceOscillation;
        baseBubbleSize = pressedButton.bubbleSize;
        baseBubbleAmount = pressedButton.bubbleAmount;
        baseFrontTilt = pressedButton.frontTilt;
        baseFrontFade = pressedButton.frontFade;
        baseAlpha = pressedButton.alpha;

        RebuildTargets();
    }

    public void TogglePlus(BtnPlus plusButton)
    {
        if (plusButton == null)
        {
            return;
        }

        if (!IsAssignedPlusButton(plusButton))
        {
            return;
        }

        float currentAmount = 0f;
        plusAmounts.TryGetValue(plusButton, out currentAmount);
        SetPlusAmount(plusButton, currentAmount > 0.001f ? 0f : 1f);
    }

    public void SetPlusAmount(BtnPlus plusButton, float amount)
    {
        if (plusButton == null)
        {
            return;
        }

        if (!IsAssignedPlusButton(plusButton))
        {
            return;
        }

        float clampedAmount = Mathf.Clamp01(amount);
        if (clampedAmount <= 0.001f)
        {
            plusAmounts.Remove(plusButton);
        }
        else
        {
            plusAmounts[plusButton] = clampedAmount;
        }

        RebuildTargets();
    }

    private void Update()
    {
        if (waterMaterial == null) return;

        ApplyStaticMaterialSettings();

        curFill = Damp(curFill, targetFill);
        curFlow = Damp(curFlow, targetFlow);
        curTemperature = Damp(curTemperature, targetTemperature);
        curSurfaceOscillation = Damp(curSurfaceOscillation, targetSurfaceOscillation);
        curBubbleSize = Damp(curBubbleSize, targetBubbleSize);
        curBubbleAmount = Damp(curBubbleAmount, targetBubbleAmount);
        curFrontTilt = Damp(curFrontTilt, targetFrontTilt);
        curFrontFade = Damp(curFrontFade, targetFrontFade);
        curAlpha = Damp(curAlpha, targetAlpha);

        waterMaterial.SetFloat(FillLevelId, curFill);
        waterMaterial.SetFloat(FlowSpeedId, curFlow);
        waterMaterial.SetFloat(TemperatureId, curTemperature);
        waterMaterial.SetFloat(SurfaceOscillationId, curSurfaceOscillation);
        waterMaterial.SetFloat(BubbleSizeId, curBubbleSize);
        waterMaterial.SetFloat(BubbleAmountId, curBubbleAmount);
        waterMaterial.SetFloat(FrontTiltId, curFrontTilt);
        waterMaterial.SetFloat(FrontFadeId, curFrontFade);
        waterMaterial.SetFloat(AlphaId, curAlpha);

        SaveMeasurementsIfNeeded();
    }

    private void ApplyStaticMaterialSettings()
    {
        waterMaterial.SetFloat(PathStartId, pathStart);
        waterMaterial.SetFloat(PathEndId, pathEnd);
    }

    private void BindButtons()
    {
        AssignController(setButtons);
        AssignController(plusButtons);
    }

    private void ResetButtonVisuals()
    {
        if (setButtons != null)
        {
            foreach (BtnSet button in setButtons)
            {
                if (button == null)
                {
                    continue;
                }

                button.Release();
            }
        }

        if (plusButtons != null)
        {
            plusAmounts.Clear();

            foreach (BtnPlus button in plusButtons)
            {
                if (button == null)
                {
                    continue;
                }

                button.SetPressedVisual(false);
            }
        }
    }

    private void AssignController(BtnSet[] buttons)
    {
        if (buttons == null)
        {
            return;
        }

        foreach (BtnSet button in buttons)
        {
            if (button != null)
            {
                button.controller = this;
            }
        }
    }

    private void AssignController(BtnPlus[] buttons)
    {
        if (buttons == null)
        {
            return;
        }

        foreach (BtnPlus button in buttons)
        {
            if (button != null)
            {
                button.controller = this;
            }
        }
    }

    private bool IsAssignedSetButton(BtnSet button)
    {
        if (button == null)
        {
            return false;
        }

        if (button.controller == this)
        {
            return true;
        }

        if (setButtons == null || setButtons.Length == 0)
        {
            return true;
        }

        foreach (BtnSet assignedButton in setButtons)
        {
            if (assignedButton == null)
            {
                continue;
            }

            if (assignedButton == button)
            {
                return true;
            }

            if (button.transform.IsChildOf(assignedButton.transform) || assignedButton.transform.IsChildOf(button.transform))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAssignedPlusButton(BtnPlus button)
    {
        if (button == null)
        {
            return false;
        }

        if (button.controller == this)
        {
            return true;
        }

        if (plusButtons == null || plusButtons.Length == 0)
        {
            return true;
        }

        foreach (BtnPlus assignedButton in plusButtons)
        {
            if (assignedButton == null)
            {
                continue;
            }

            if (assignedButton == button)
            {
                return true;
            }

            if (button.transform.IsChildOf(assignedButton.transform) || assignedButton.transform.IsChildOf(button.transform))
            {
                return true;
            }
        }

        return false;
    }

    private void RebuildTargets()
    {
        targetFill = baseFill;
        targetFlow = baseFlow;
        targetTemperature = baseTemperature;
        targetSurfaceOscillation = baseSurfaceOscillation;
        targetBubbleSize = baseBubbleSize;
        targetBubbleAmount = baseBubbleAmount;
        targetFrontTilt = baseFrontTilt;
        targetFrontFade = baseFrontFade;
        targetAlpha = baseAlpha;

        foreach (KeyValuePair<BtnPlus, float> pair in plusAmounts)
        {
            BtnPlus plusButton = pair.Key;
            float plusAmount = pair.Value;

            if (plusButton == null)
            {
                continue;
            }

            targetFill += plusButton.fillLevelDelta * plusAmount;
            targetFlow += plusButton.flowSpeedDelta * plusAmount;
            targetTemperature += plusButton.temperatureDelta * plusAmount;
            targetSurfaceOscillation += plusButton.surfaceOscillationDelta * plusAmount;
            targetBubbleSize += plusButton.bubbleSizeDelta * plusAmount;
            targetBubbleAmount += plusButton.bubbleAmountDelta * plusAmount;
            targetFrontTilt += plusButton.frontTiltDelta * plusAmount;
            targetFrontFade += plusButton.frontFadeDelta * plusAmount;
            targetAlpha += plusButton.alphaDelta * plusAmount;
        }

        targetFill = Mathf.Clamp01(targetFill);
        targetFlow = Mathf.Clamp(targetFlow, 0f, 5f);
        targetTemperature = Mathf.Clamp01(targetTemperature);
        targetSurfaceOscillation = Mathf.Clamp01(targetSurfaceOscillation);
        targetBubbleSize = Mathf.Clamp(targetBubbleSize, 0.5f, 8f);
        targetBubbleAmount = Mathf.Clamp01(targetBubbleAmount);
        targetFrontTilt = Mathf.Clamp(targetFrontTilt, -1f, 1f);
        targetFrontFade = Mathf.Clamp(targetFrontFade, 0.001f, 0.2f);
        targetAlpha = Mathf.Clamp01(targetAlpha);
    }

    private float Damp(float current, float target)
    {
        if (transitionSpeed <= 0f)
        {
            return target;
        }

        float t = 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime);
        float value = Mathf.Lerp(current, target, t);

        if (Mathf.Abs(target - value) < 0.0001f)
        {
            return target;
        }

        return value;
    }

    private float ReadMaterialFloat(int propertyId, float fallback)
    {
        if (waterMaterial != null && waterMaterial.HasProperty(propertyId))
        {
            return waterMaterial.GetFloat(propertyId);
        }

        return fallback;
    }

    public string BuildLabSummary()
    {
        LabMeasurements m = CurrentMeasurements;
        return
            "<b>ЛР №1. Изучение режимов течения жидкости</b>\n" +
            $"Диаметр трубопровода: {m.pipeInnerDiameterMeters * 1000f:F0} мм\n" +
            $"Длина змеевика: {m.coilLengthMeters:F1} м\n" +
            $"Заполнение тракта: {m.fillFraction * 100f:F0}% ({m.filledLengthMeters:F2} м)\n\n" +
            "<b>Текущие измеряемые параметры</b>\n" +
            $"Расход: {m.flowRateLitersPerMinute:F2} л/мин\n" +
            $"Объемный расход: {m.volumetricFlowCubicMetersPerSecond:F6} м3/с\n" +
            $"Площадь сечения: {m.pipeAreaSquareMeters:F6} м2\n" +
            $"Скорость потока: {m.velocityMetersPerSecond:F3} м/с\n" +
            $"Температура воды: {m.temperatureCelsius:F1} °C\n" +
            $"Плотность воды: {m.densityKgPerCubicMeter:F1} кг/м3\n" +
            $"Кинематическая вязкость: {m.kinematicViscosityM2PerSecond:E3} м2/с\n" +
            $"Число Рейнольдса: {m.reynoldsNumber:F0}\n" +
            $"Режим течения: {m.regimeName}\n\n" +
            "<b>Связь с параметрами шейдера</b>\n" +
            $"Flow Speed: {m.shaderFlowSpeed:F2} / 5.00\n" +
            $"Temperature: {m.shaderTemperature:F2} / 1.00\n" +
            $"Surface Oscillation: {m.shaderSurfaceOscillation:F2}\n" +
            $"Bubble Amount: {m.shaderBubbleAmount:F2}\n" +
            $"Bubble Size: {m.shaderBubbleSize:F2}\n" +
            $"Визуальная турбулентность: {m.visualTurbulenceFactor * 100f:F0}%\n\n" +
            "<b>Порядок выполнения</b>\n" +
            "1. Включить стенд, питание и насос.\n" +
            "2. Регулировать расход воды, добиваясь разных режимов течения.\n" +
            "3. Зафиксировать расход Q и температуру t для каждого режима.\n" +
            "4. Рассчитать f, w, ν и Re.\n" +
            "5. Определить режим и занести данные в таблицу.";
    }

    public string BuildLabTableSummary()
    {
        LabMeasurements m = CurrentMeasurements;
        return
            "<b>Текущая строка таблицы ЛР №1</b>\n\n" +
            $"d: {m.pipeInnerDiameterMeters:F3} м\n" +
            $"f: {m.pipeAreaSquareMeters:F6} м2\n" +
            $"V: {m.volumetricFlowCubicMetersPerSecond:F6} м3/с ({m.flowRateLitersPerMinute:F2} л/мин)\n" +
            $"w: {m.velocityMetersPerSecond:F3} м/с\n" +
            $"t: {m.temperatureCelsius:F1} °C\n" +
            $"ν: {m.kinematicViscosityM2PerSecond:E3} м2/с\n" +
            $"Re: {m.reynoldsNumber:F0}\n" +
            $"Режим: {m.regimeName}";
    }

    private LabMeasurements BuildLabMeasurements(
        float fill01,
        float shaderFlow,
        float shaderTemperature,
        float surfaceOscillation,
        float bubbleAmount,
        float bubbleSize)
    {
        float clampedFill = Mathf.Clamp01(fill01);
        float flowT = Mathf.InverseLerp(0f, 5f, Mathf.Clamp(shaderFlow, 0f, 5f));
        float flowRateLitersPerMinute = Mathf.Lerp(minPhysicalFlowRateLitersPerMinute, maxPhysicalFlowRateLitersPerMinute, flowT);
        float volumetricFlow = flowRateLitersPerMinute / 60000f;
        float area = Mathf.PI * pipeInnerDiameterMeters * pipeInnerDiameterMeters * 0.25f;
        float velocity = area > 0.0000001f ? volumetricFlow / area : 0f;
        float temperatureCelsius = Mathf.Lerp(minPhysicalTemperatureCelsius, maxPhysicalTemperatureCelsius, Mathf.Clamp01(shaderTemperature));
        float kinematicViscosity = CalculateKinematicViscosity(temperatureCelsius);
        float density = CalculateWaterDensity(temperatureCelsius);
        float reynolds = kinematicViscosity > 0.0000000001f
            ? velocity * pipeInnerDiameterMeters / kinematicViscosity
            : 0f;
        float bubbleSize01 = Mathf.InverseLerp(0.5f, 8f, bubbleSize);
        float visualTurbulence = Mathf.Clamp01(surfaceOscillation * 0.35f + bubbleAmount * 0.45f + bubbleSize01 * 0.20f);

        return new LabMeasurements
        {
            fillFraction = clampedFill,
            filledLengthMeters = coilLengthMeters * clampedFill,
            pipeInnerDiameterMeters = pipeInnerDiameterMeters,
            pipeAreaSquareMeters = area,
            coilLengthMeters = coilLengthMeters,
            flowRateLitersPerMinute = flowRateLitersPerMinute,
            volumetricFlowCubicMetersPerSecond = volumetricFlow,
            velocityMetersPerSecond = velocity,
            temperatureCelsius = temperatureCelsius,
            densityKgPerCubicMeter = density,
            kinematicViscosityM2PerSecond = kinematicViscosity,
            reynoldsNumber = reynolds,
            physicalRegimeFactor = Mathf.InverseLerp(laminarReynoldsLimit, turbulentReynoldsLimit, reynolds),
            visualTurbulenceFactor = visualTurbulence,
            shaderFlowSpeed = shaderFlow,
            shaderTemperature = shaderTemperature,
            shaderSurfaceOscillation = surfaceOscillation,
            shaderBubbleAmount = bubbleAmount,
            shaderBubbleSize = bubbleSize,
            regimeName = ResolveRegimeName(reynolds)
        };
    }

    private string ResolveRegimeName(float reynolds)
    {
        if (reynolds < laminarReynoldsLimit)
        {
            return "Ламинарный";
        }

        if (reynolds < turbulentReynoldsLimit)
        {
            return "Переходный";
        }

        return "Турбулентный";
    }

    private float CalculateKinematicViscosity(float temperatureCelsius)
    {
        float denominator = 1f + 0.0337f * temperatureCelsius + 0.000221f * temperatureCelsius * temperatureCelsius;
        if (denominator <= 0.0001f)
        {
            denominator = 0.0001f;
        }

        return 0.0178f / denominator * 0.0001f;
    }

    private float CalculateWaterDensity(float temperatureCelsius)
    {
        float numerator = (temperatureCelsius + 288.9414f) * Mathf.Pow(temperatureCelsius - 3.9863f, 2f);
        float denominator = 508929.2f * (temperatureCelsius + 68.12963f);
        return 1000f * (1f - numerator / denominator);
    }

    private void SaveMeasurementsIfNeeded()
    {
        if (!saveMeasurementsToRuntimeData || Time.unscaledTime < nextMeasurementSaveTime)
        {
            return;
        }

        nextMeasurementSaveTime = Time.unscaledTime + Mathf.Max(0.02f, measurementSaveInterval);

        LabMeasurements m = CurrentMeasurements;
        GameStateStore store = GameStateStore.Instance;
        store.SetValue("Lab01.FillPercent", (m.fillFraction * 100f).ToString("F1", CultureInfo.InvariantCulture));
        store.SetValue("Lab01.FilledLengthMeters", m.filledLengthMeters.ToString("F3", CultureInfo.InvariantCulture));
        store.SetValue("Lab01.FlowRateLpm", m.flowRateLitersPerMinute.ToString("F3", CultureInfo.InvariantCulture));
        store.SetValue("Lab01.VolumeFlowM3s", m.volumetricFlowCubicMetersPerSecond.ToString("F6", CultureInfo.InvariantCulture));
        store.SetValue("Lab01.FlowVelocityMps", m.velocityMetersPerSecond.ToString("F4", CultureInfo.InvariantCulture));
        store.SetValue("Lab01.TemperatureC", m.temperatureCelsius.ToString("F2", CultureInfo.InvariantCulture));
        store.SetValue("Lab01.KinematicViscosity", m.kinematicViscosityM2PerSecond.ToString("E6", CultureInfo.InvariantCulture));
        store.SetValue("Lab01.Reynolds", m.reynoldsNumber.ToString("F0", CultureInfo.InvariantCulture));
        store.SetValue("Lab01.Regime", m.regimeName);
        store.SetValue("Lab01.VisualTurbulencePercent", (m.visualTurbulenceFactor * 100f).ToString("F1", CultureInfo.InvariantCulture));
    }
}
