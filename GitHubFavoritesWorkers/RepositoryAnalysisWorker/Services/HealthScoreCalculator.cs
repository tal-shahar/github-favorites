namespace RepositoryAnalysisWorker.Services;

public static class HealthScoreCalculator
{
    public static double Calculate(int starsSnapshot, int activityDays, int forks, int openIssues, int readmeLength)
    {
        var recentActivityScore = Math.Clamp(30 - activityDays, 0, 30);
        var forksToIssues = forks / (double)(openIssues + 1);
        var readmeScore = readmeLength > 1000 ? 2 : 1;

        return starsSnapshot * 0.4 + recentActivityScore * 1.0 + forksToIssues * 0.6 + readmeScore;
    }
}

