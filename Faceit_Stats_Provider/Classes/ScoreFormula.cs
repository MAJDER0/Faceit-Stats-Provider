using static Faceit_Stats_Provider.Classes.StatsHelper;

namespace Faceit_Stats_Provider.Classes
{
    public class ScoreFormula
    {
        public static double Score(string map, string overallKD, string overallMatchesOnMap, string overallWR, string overallKR, string last20KD, string last20WR, string last20MatchesOnMap, string last20KR)
        {

            double OverallKD = ParseDoubleOrDefault(overallKD);
            double OverallWR = ParseDoubleOrDefault(overallWR);
            double OverallMatches = ParseDoubleOrDefault(overallMatchesOnMap);

            double OverallKR = ParseDoubleOrDefault(overallKR);
            double Last20KD = ParseDoubleOrDefault(last20KD);
            double Last20WR = ParseDoubleOrDefault(last20WR);

            double LastQuantityOfMatches = ParseDoubleOrDefault(last20MatchesOnMap);
            double Last20KR = ParseDoubleOrDefault(last20KR);


            double MinOverallKD = 0, MaxOverallKD = 3;
            double MinOverallMatches = 0, MaxOverallMatches = 17000;
            double MinKR = 0, MaxKR = 2;
            double MinWR = 0, MaxWR = 1;
            double MinLast20Matches = 0, MaxLast20Matches = 100;
            double MinLast20KD = 0, MaxLast20KD = 5;


            double NormalizedOverallKD = NormalizeFactor(OverallKD, MinOverallKD, MaxOverallKD);
            double NormalizedOverallWR = NormalizeFactor(OverallWR / 100, MinWR, MaxWR);
            double NormalizedOverallMatches = NormalizeFactor(OverallMatches, MinOverallMatches, MaxOverallMatches);
            double NormalizedOverallKR = NormalizeFactor(OverallKR, MinKR, MaxKR);

            double NormalizedLast20KD = NormalizeFactor(Last20KD, MinLast20KD, MaxLast20KD);
            double NormalizedLast20WR = NormalizeFactor(Last20WR / 100, MinWR, MaxWR);
            double NormalizedLast20QuantityOfMatches = NormalizeFactor(LastQuantityOfMatches, MinLast20Matches, MaxLast20Matches);
            double NormalizedLast20KR = NormalizeFactor(Last20KR, MinKR, MaxKR);

            double score =
            (0.785 * NormalizedOverallMatches)
            + (0.105 * NormalizedOverallWR)
            + (0.05 * NormalizedOverallKR)
            + (0.05 * NormalizedOverallKD)

            + (0.0030 * NormalizedLast20QuantityOfMatches)
            + (0.0020 * NormalizedLast20KD)
            + (0.0020 * NormalizedLast20KR)
            + (0.0030 * NormalizedLast20WR);

            return score;
        }
    }
}