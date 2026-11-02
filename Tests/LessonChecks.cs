using System.Globalization;

namespace EasyCreator;

/// <summary>Checks shared by the track test suites: the track's shape and that quiz tolerances aren't too loose.</summary>
public static class LessonChecks
{
    /// <summary>20 lessons, unique ids, levels never going down and ending at 5.</summary>
    public static void Shape(Track track, Action<string, bool, string> check)
    {
        check($"{track.Id} track has 20 lessons ({track.Lessons.Count})", track.Lessons.Count == 20, "");
        check("lesson ids are unique", track.Lessons.Select(l => l.Id).Distinct().Count() == track.Lessons.Count, "");
        check("levels never go down and end at 5",
            track.Lessons.Zip(track.Lessons.Skip(1)).All(p => p.First.Level <= p.Second.Level) && track.Lessons[^1].Level == 5, "");
    }

    /// <summary>A numeric answer 10 % off must be rejected. Answers that are 0 or don't parse can't be tested this way;
    /// they are reported as failures instead of passing silently.</summary>
    public static void TenPercentOffRejected(Track track, int fromLesson, Action<string, bool, string> check)
    {
        foreach (var l in track.Lessons.Skip(fromLesson))
            for (int i = 0; i < l.Questions.Count; i++)
            {
                var q = l.Questions[i];
                if (q.Kind != "number") continue;
                if (!Question.TryNum(q.Answer, out var a) || a == 0)
                    check($"{l.Id} q{i + 1}: answer is a non-zero number", false, q.Answer);
                else
                    check($"{l.Id} q{i + 1}: 10 % off is rejected", !q.IsCorrect((a * 1.1).ToString(CultureInfo.InvariantCulture)), "");
            }
    }
}
