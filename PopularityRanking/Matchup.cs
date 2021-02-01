using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Models;
using Microsoft.ML.Probabilistic.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Range = Microsoft.ML.Probabilistic.Models.Range;

namespace PopularityRanking
{
    public static class Matchup
    {
        public static void RunTwoPlayerMatchup(Participant winner, Participant loser)
        {
            var b = Variable.New<bool>().Attrib(new DoNotInfer());

            var winnerPopularity = Variable<double>.Random(winner.popularityGaussian);
            var loserPopularity = Variable<double>.Random(loser.popularityGaussian);

            b.SetTo(winnerPopularity > loserPopularity);
            b.ObservedValue = true;

            var winnerPost = Ranking.Engine.Infer<Gaussian>(winnerPopularity);
            var loserPost = Ranking.Engine.Infer<Gaussian>(loserPopularity);

            winner.popularityGaussian = winnerPost;
            loser.popularityGaussian = loserPost;
        }

        public static void RunAnyPlayersMatchup(Participant[] participants, double dynamicPopularityFactor = 3.0)
        {
            var range = new Range(participants.Length);
            var results = Variable.Array<bool>(range);
            var popularities = Variable.Array<double>(range);
            var variances = Variable.Array<double>(range);
            var priors = Variable.Array<Gaussian>(range);

            priors.ObservedValue = participants.Select(
                p => p.popularityGaussian).ToArray();
            variances.ObservedValue = participants.Select(
                p => dynamicPopularityFactor * dynamicPopularityFactor).ToArray();

            using (var loop = Variable.ForEach(range))
            {
                popularities[range] = Variable.GaussianFromMeanAndVariance(
                    Variable<double>.Random(priors[range]),
                    variances[range]);

                using (Variable.If(loop.Index > 0))
                {
                    results[loop.Index]
                        .SetTo(popularities[loop.Index - 1] > popularities[loop.Index]);
                    results.ObservedValue = participants.Select(p => true).ToArray();
                }
            }

            var participantPosts = Ranking.Engine.Infer<Gaussian[]>(popularities);

            for (int i = 0; i < participants.Length; i++)
                participants[i].popularityGaussian = participantPosts[i];
        }
    }
}
