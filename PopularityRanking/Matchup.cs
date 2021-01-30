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

            var winnerPost = Program.Engine.Infer<Gaussian>(winnerPopularity);
            var loserPost = Program.Engine.Infer<Gaussian>(loserPopularity);

            winner.popularityGaussian = winnerPost;
            loser.popularityGaussian = loserPost;
        }

        public static void RunAnyPlayersMatchup(Participant[] participants)
        {
            var range = new Range(participants.Length);
            var results = Variable.Array<bool>(range); // DoNotInfer() ?
            var popularities = Variable.Array<double>(range);
            var means = Variable.Array<double>(range);
            var variances = Variable.Array<double>(range);
            means.ObservedValue = participants.Select(
                p => p.popularityGaussian.GetMean()).ToArray();
            variances.ObservedValue = participants.Select(
                p => p.popularityGaussian.GetVariance()).ToArray();

            using (var loop = Variable.ForEach(range))
            {
                popularities[range] = Variable.GaussianFromMeanAndVariance(
                    means[range], variances[range]);

                using (Variable.If(loop.Index > 0))
                {
                    results[loop.Index]
                        .SetTo(popularities[loop.Index - 1] > popularities[loop.Index]);
                    results.ObservedValue = participants.Select(p => true).ToArray();
                }
            }

            var participantPosts = Program.Engine.Infer<Gaussian[]>(popularities);

            for (int i = 0; i < participants.Length; i++)
                participants[i].popularityGaussian = participantPosts[i];
        }
    }
}
