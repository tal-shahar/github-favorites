namespace RepositoryAnalysisWorker.Services;

public static class HealthScoreCalculator
{
    private const double StarsWeight = 40;
    private const double ActivityWeight = 30;
    private const double EngagementWeight = 20;
    private const double ReadmeWeight = 10;
    private const double MaxScore = StarsWeight + ActivityWeight + EngagementWeight + ReadmeWeight;

    public static double Calculate(int starsSnapshot, int activityDays, int forks, int openIssues, int readmeLength)
    {
        var starsScore = CalculateStarsScore(starsSnapshot);
        var activityScore = CalculateActivityScore(activityDays);
        var engagementScore = CalculateEngagementScore(forks, openIssues);
        var documentationScore = CalculateReadmeScore(readmeLength);

        var total = starsScore + activityScore + engagementScore + documentationScore;
        return Math.Round(Math.Clamp(total, 0, MaxScore), 2);
    }

    private static double CalculateStarsScore(int stars)
    {
        const double maxStarsReference = 50000d;
        if (stars <= 0)
        {
            return 0;
        }

        var normalized = Math.Log10(stars + 1) / Math.Log10(maxStarsReference + 1);
        return Math.Clamp(normalized * StarsWeight, 0, StarsWeight);
    }

    private static double CalculateActivityScore(int activityDays)
    {
        var cappedDays = Math.Clamp(activityDays, 0, 365);
        var normalized = 1 - (cappedDays / 365d);
        return Math.Clamp(normalized * ActivityWeight, 0, ActivityWeight);
    }

    private static double CalculateEngagementScore(int forks, int openIssues)
    {
        var ratio = forks / (double)(openIssues + 1);
        var normalized = Math.Clamp(ratio / 4d, 0, 1); // treat ratio >=4 as excellent
        return normalized * EngagementWeight;
    }

    private static double CalculateReadmeScore(int readmeLength)
    {
        const double targetLength = 5000d;
        var normalized = Math.Clamp(readmeLength / targetLength, 0, 1);
        return normalized * ReadmeWeight;
    }
}

