using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class Lab01WorkSession
{
    public struct RecordEntry
    {
        public RecordEntry(int attemptNumber, WaterController.LabMeasurements measurements)
        {
            AttemptNumber = attemptNumber;
            Measurements = measurements;
        }

        public int AttemptNumber { get; }
        public WaterController.LabMeasurements Measurements { get; }
    }

    public struct SubmittedEntry
    {
        public SubmittedEntry(
            int attemptNumber,
            float diameter,
            float area,
            float flow,
            float velocity,
            float temperature,
            float viscosity,
            float reynolds,
            string regime)
        {
            AttemptNumber = attemptNumber;
            Diameter = diameter;
            Area = area;
            Flow = flow;
            Velocity = velocity;
            Temperature = temperature;
            Viscosity = viscosity;
            Reynolds = reynolds;
            Regime = regime;
        }

        public int AttemptNumber { get; }
        public float Diameter { get; }
        public float Area { get; }
        public float Flow { get; }
        public float Velocity { get; }
        public float Temperature { get; }
        public float Viscosity { get; }
        public float Reynolds { get; }
        public string Regime { get; }
    }

    public static bool HasSnapshot { get; private set; }
    public static bool AreaSolved { get; private set; }
    public static bool VelocitySolved { get; private set; }
    public static bool ViscositySolved { get; private set; }
    public static bool ReynoldsSolved { get; private set; }
    public static int RecordAttemptCount { get; private set; }
    public static int SnapshotVersion { get; private set; }
    public static int CurrentAttemptNumber => HasSnapshot ? RecordAttemptCount : 0;
    public static int RecordCount => recordHistory.Count;

    public static WaterController.LabMeasurements Snapshot { get; private set; }
    public static float Area { get; private set; }
    public static float Velocity { get; private set; }
    public static float Viscosity { get; private set; }
    public static float Reynolds { get; private set; }
    public static string Regime { get; private set; } = "-";

    private static readonly List<RecordEntry> recordHistory = new List<RecordEntry>();
    private static readonly List<SubmittedEntry> submittedRows = new List<SubmittedEntry>();

    public static bool CanSubmit => HasSnapshot && AreaSolved && VelocitySolved && ViscositySolved && ReynoldsSolved;
    public static bool HasValidSnapshot => HasSnapshot && IsPositive(Snapshot.pipeInnerDiameterMeters) && IsNonNegative(Snapshot.volumetricFlowCubicMetersPerSecond) && IsFinite(Snapshot.temperatureCelsius);
    public static IReadOnlyList<RecordEntry> RecordHistory => recordHistory;

    public static bool TryRecord(WaterController.LabMeasurements measurements, out string error)
    {
        error = string.Empty;

        if (!ValidateMeasurements(measurements, out error))
        {
            return false;
        }

        if (HasMatchingRecord(measurements))
        {
            error = "Такие данные уже записаны.";
            SceneActivityLog.Add("Данные", FormatMeasurementsForLog("Повтор не записан", measurements));
            return false;
        }

        RecordAttemptCount++;
        SnapshotVersion++;
        Snapshot = measurements;
        recordHistory.Add(new RecordEntry(RecordAttemptCount, measurements));
        Area = 0f;
        Velocity = 0f;
        Viscosity = 0f;
        Reynolds = 0f;
        Regime = "-";
        HasSnapshot = true;
        AreaSolved = false;
        VelocitySolved = false;
        ViscositySolved = false;
        ReynoldsSolved = false;
        SceneActivityLog.Add("Данные", FormatMeasurementsForLog($"Записана попытка {CurrentAttemptNumber}", measurements));
        return true;
    }

    public static bool TrySolveArea(float diameterMeters, out float area, out string error)
    {
        area = 0f;
        error = string.Empty;

        if (!IsPositive(diameterMeters))
        {
            error = "Диаметр должен быть больше нуля.";
            return false;
        }

        Area = Mathf.PI * diameterMeters * diameterMeters * 0.25f;
        AreaSolved = true;
        area = Area;
        return true;
    }

    public static bool TrySolveVelocity(float flowM3s, float areaM2, out float velocity, out string error)
    {
        velocity = 0f;
        error = string.Empty;

        if (!IsNonNegative(flowM3s))
        {
            error = "Расход не может быть отрицательным.";
            return false;
        }

        if (!IsPositive(areaM2))
        {
            error = "Площадь должна быть больше нуля.";
            return false;
        }

        Velocity = flowM3s / areaM2;
        VelocitySolved = true;
        velocity = Velocity;
        return true;
    }

    public static bool TrySolveViscosity(float temperatureCelsius, out float viscosity, out string error)
    {
        viscosity = 0f;
        error = string.Empty;

        if (!IsFinite(temperatureCelsius))
        {
            error = "Температура введена некорректно.";
            return false;
        }

        float denominator = 1f + 0.0337f * temperatureCelsius + 0.000221f * temperatureCelsius * temperatureCelsius;
        Viscosity = 0.0178f / Mathf.Max(denominator, 0.0001f) * 0.0001f;
        ViscositySolved = true;
        viscosity = Viscosity;
        return true;
    }

    public static bool TrySolveReynolds(float velocityMps, float diameterMeters, float viscosityM2s, out float reynolds, out string error)
    {
        reynolds = 0f;
        error = string.Empty;

        if (!IsPositive(velocityMps))
        {
            error = "Скорость должна быть больше нуля.";
            return false;
        }

        if (!IsPositive(diameterMeters))
        {
            error = "Диаметр должен быть больше нуля.";
            return false;
        }

        if (!IsPositive(viscosityM2s))
        {
            error = "Кинематическая вязкость должна быть больше нуля.";
            return false;
        }

        Reynolds = velocityMps * diameterMeters / viscosityM2s;
        ReynoldsSolved = true;
        Regime = ResolveRegime(Reynolds);
        reynolds = Reynolds;
        return true;
    }

    public static bool TrySubmit(float diameter, float area, float flow, float velocity, float temperature, float viscosity, float reynolds, out string error)
    {
        error = string.Empty;

        if (!CanSubmit)
        {
            error = "Сначала выполните все расчёты.";
            return false;
        }

        if (!ValidateMeasurementsAgainstSubmission(Snapshot, diameter, area, flow, velocity, temperature, viscosity, reynolds, out error))
        {
            return false;
        }

        SubmitValidatedRow(CurrentAttemptNumber, diameter, area, flow, velocity, temperature, viscosity, reynolds);
        return true;
    }

    public static bool TrySubmitRecord(
        int attemptNumber,
        WaterController.LabMeasurements measurements,
        float diameter,
        float area,
        float flow,
        float velocity,
        float temperature,
        float viscosity,
        float reynolds,
        out string error)
    {
        error = string.Empty;

        if (attemptNumber <= 0)
        {
            error = "Выберите записанные данные.";
            return false;
        }

        if (!ValidateMeasurementsAgainstSubmission(measurements, diameter, area, flow, velocity, temperature, viscosity, reynolds, out error))
        {
            return false;
        }

        SubmitValidatedRow(attemptNumber, diameter, area, flow, velocity, temperature, viscosity, reynolds);
        return true;
    }

    public static string BuildSubmittedTableText()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("№ записи | d, м | f, м2 | V, м3/с | w, м/с | t, °C | ν, м2/с | Re | Режим");
        builder.AppendLine("---------|------|-------|---------|--------|-------|---------|----|------");

        if (submittedRows.Count == 0)
        {
            builder.AppendLine("Записей пока нет. Заполните расчёты и нажмите \"Внести данные в таблицу\".");
            return builder.ToString();
        }

        for (int i = 0; i < submittedRows.Count; i++)
        {
            SubmittedEntry row = submittedRows[i];
            builder
                .Append(row.AttemptNumber)
                .Append(" | ")
                .Append(Format(row.Diameter, "F3"))
                .Append(" | ")
                .Append(Format(row.Area, "F6"))
                .Append(" | ")
                .Append(Format(row.Flow, "F6"))
                .Append(" | ")
                .Append(Format(row.Velocity, "F3"))
                .Append(" | ")
                .Append(Format(row.Temperature, "F1"))
                .Append(" | ")
                .Append(Format(row.Viscosity, "E3"))
                .Append(" | ")
                .Append(Format(row.Reynolds, "F0"))
                .Append(" | ")
                .AppendLine(row.Regime);
        }

        return builder.ToString();
    }

    public static string ResolveRegime(float reynolds)
    {
        if (reynolds < 2300f)
        {
            return "Ламинарный";
        }

        if (reynolds < 10000f)
        {
            return "Переходный";
        }

        return "Турбулентный";
    }

    public static string Format(float value, string format)
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    private static bool ValidateMeasurements(WaterController.LabMeasurements measurements, out string error)
    {
        if (!IsFinite(measurements.pipeInnerDiameterMeters) || !IsPositive(measurements.pipeInnerDiameterMeters))
        {
            error = "Некорректный диаметр стенда.";
            return false;
        }

        if (!IsFinite(measurements.volumetricFlowCubicMetersPerSecond) || !IsNonNegative(measurements.volumetricFlowCubicMetersPerSecond))
        {
            error = "Некорректный расход стенда.";
            return false;
        }

        if (!IsFinite(measurements.temperatureCelsius))
        {
            error = "Некорректная температура стенда.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool HasMatchingRecord(WaterController.LabMeasurements measurements)
    {
        for (int i = 0; i < recordHistory.Count; i++)
        {
            WaterController.LabMeasurements existing = recordHistory[i].Measurements;
            if (IsApproximately(existing.pipeInnerDiameterMeters, measurements.pipeInnerDiameterMeters, 0.0005f) &&
                IsApproximately(existing.volumetricFlowCubicMetersPerSecond, measurements.volumetricFlowCubicMetersPerSecond, 0.0000005f) &&
                IsApproximately(existing.temperatureCelsius, measurements.temperatureCelsius, 0.05f))
            {
                return true;
            }
        }

        return false;
    }

    private static void UpsertSubmittedRow(SubmittedEntry row)
    {
        for (int i = 0; i < submittedRows.Count; i++)
        {
            if (submittedRows[i].AttemptNumber == row.AttemptNumber)
            {
                submittedRows[i] = row;
                return;
            }
        }

        submittedRows.Add(row);
    }

    private static string FormatMeasurementsForLog(string prefix, WaterController.LabMeasurements measurements)
    {
        return $"{prefix}: d {Format(measurements.pipeInnerDiameterMeters, "F3")} м, " +
            $"V {Format(measurements.volumetricFlowCubicMetersPerSecond, "F6")} м3/с, " +
            $"t {Format(measurements.temperatureCelsius, "F1")} °C";
    }

    private static bool ValidateMeasurementsAgainstSubmission(
        WaterController.LabMeasurements measurements,
        float diameter,
        float area,
        float flow,
        float velocity,
        float temperature,
        float viscosity,
        float reynolds,
        out string error)
    {
        if (!ValidateMeasurements(measurements, out _))
        {
            error = "Сначала запишите корректные данные стенда.";
            return false;
        }

        if (Mathf.Abs(diameter - measurements.pipeInnerDiameterMeters) > 0.0005f)
        {
            error = "Диаметр не совпадает с записанными данными.";
            return false;
        }

        if (Mathf.Abs(flow - measurements.volumetricFlowCubicMetersPerSecond) > 0.0000005f)
        {
            error = "Расход не совпадает с записанными данными.";
            return false;
        }

        if (Mathf.Abs(temperature - measurements.temperatureCelsius) > 0.05f)
        {
            error = "Температура не совпадает с записанными данными.";
            return false;
        }

        float expectedArea = Mathf.PI * diameter * diameter * 0.25f;
        float expectedVelocity = flow / Mathf.Max(expectedArea, 0.0000001f);
        float expectedViscosity = 0.0178f / Mathf.Max(1f + 0.0337f * temperature + 0.000221f * temperature * temperature, 0.0001f) * 0.0001f;
        float expectedReynolds = expectedVelocity * diameter / Mathf.Max(expectedViscosity, 0.0000000001f);

        if (!IsApproximately(area, expectedArea, 0.0005f))
        {
            error = "Площадь рассчитана неверно.";
            return false;
        }

        if (!IsApproximately(velocity, expectedVelocity, 0.005f))
        {
            error = "Скорость рассчитана неверно.";
            return false;
        }

        if (!IsApproximately(viscosity, expectedViscosity, 0.0000005f))
        {
            error = "Вязкость рассчитана неверно.";
            return false;
        }

        if (!IsApproximately(reynolds, expectedReynolds, Mathf.Max(10f, expectedReynolds * 0.01f)))
        {
            error = "Число Рейнольдса рассчитано неверно.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static void SubmitValidatedRow(int attemptNumber, float diameter, float area, float flow, float velocity, float temperature, float viscosity, float reynolds)
    {
        Area = area;
        Velocity = velocity;
        Viscosity = viscosity;
        Reynolds = reynolds;
        Regime = ResolveRegime(reynolds);
        UpsertSubmittedRow(new SubmittedEntry(attemptNumber, diameter, area, flow, velocity, temperature, viscosity, reynolds, Regime));
        SceneActivityLog.Add("Данные", $"Строка внесена в таблицу: запись {attemptNumber}, Re {Format(reynolds, "F0")}, {Regime}");
    }

    private static bool IsPositive(float value)
    {
        return IsFinite(value) && value > 0f;
    }

    private static bool IsNonNegative(float value)
    {
        return IsFinite(value) && value >= 0f;
    }

    private static bool IsApproximately(float actual, float expected, float tolerance)
    {
        return IsFinite(actual) && IsFinite(expected) && Mathf.Abs(actual - expected) <= Mathf.Max(tolerance, 0f);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
