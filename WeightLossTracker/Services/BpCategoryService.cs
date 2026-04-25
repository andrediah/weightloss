namespace WeightLossTracker.Services;

public static class BpCategoryService
{
    public static string Classify(int systolic, int diastolic)
    {
        if (systolic > 180 || diastolic > 120) return "Crisis";
        if (systolic < 90  || diastolic < 60)  return "Hypotension";
        if (systolic < 120 && diastolic < 80)  return "Normal";
        if (systolic < 130 && diastolic < 80)  return "Elevated";
        if (systolic < 140 || diastolic < 90)  return "Stage 1";
        return "Stage 2";
    }
}
