using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Range = Microsoft.ML.Probabilistic.Models.Range;
using Microsoft.ML.Probabilistic.Algorithms;

namespace PopularityRanking
{
    [XmlRoot(ElementName = "ranking")]
    public class Ranking : IXmlSerializable
    {
        public static readonly InferenceEngine Engine = new InferenceEngine(new ExpectationPropagation());

        public List<Participant> Participants { get; set; } = new List<Participant>();
        public double DynamicPopularityFactor { get; set; } = 3.0;
        public Gaussian DrawMargin { get; set; } = Gaussian.FromMeanAndVariance(10, 10);

        public Participant[] RandomMatchup(int number)
        {
            if (number < 2)
                throw new ArgumentException("A matchup must have at least 2 participants.");
            if (number > Participants.Count)
                throw new ArgumentException("A matchup can't be larger than the total number of participants.");
            if (Participants.Count < 2)
                throw new ArgumentException("Cannot create matchups from an insufficiently populated participant list.");
            
            var list = new List<Participant>();
            var rand = new Random();

            while (list.Count < number)
            {
                var p = Participants[rand.Next(0, Participants.Count)];
                if (!list.Contains(p))
                    list.Add(p);
            }

            return list.ToArray();
        }

        public void AssignScores(int minScore = 1, int maxScore = 10)
        {
            var minPopularity = Participants.Min(p => p.Popularity);
            var maxPopularity = Participants.Max(p => p.Popularity);
            var stepPopularity = (maxPopularity - minPopularity) / (maxScore - minScore + 1);

            foreach (var p in Participants)
                p.Score = minScore + FindIntervalContainingNumber(
                    minPopularity, maxPopularity, stepPopularity, p.Popularity);
        }

        private int FindIntervalContainingNumber(
            double rangeStart, double rangeEnd, double rangeStep, double number)
        {
            if (number < rangeStart || number > rangeEnd)
                return -1;

            for (int i = 1; ; i++)
                if (number <= rangeStart + i * rangeStep)
                    return i - 1;
        }
        public void RunAnyPlayersMatchup(Participant[] participants)
        {
            var range = new Range(participants.Length);
            var results = Variable.Array<bool>(range);
            var popularities = Variable.Array<double>(range);
            var variances = Variable.Array<double>(range);
            var priors = Variable.Array<Gaussian>(range);

            priors.ObservedValue = participants.Select(
                p => p.popularityGaussian).ToArray();
            variances.ObservedValue = participants.Select(
                p => DynamicPopularityFactor * DynamicPopularityFactor).ToArray();

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

        public void RunAnyPlayersMatchupScored(Participant[] participants, int[] orderedScores)
        {
            var range = new Range(participants.Length);
            var results = Variable.Array<bool>(range);
            var popularities = Variable.Array<double>(range);
            var variances = Variable.Array<double>(range);
            var priors = Variable.Array<Gaussian>(range);
            var scores = Variable.Array<double>(range);
            var drawMargin = Variable.GaussianFromMeanAndVariance(
                DrawMargin.GetMean(),
                Math.Sqrt(DrawMargin.GetVariance()));
            scores.ObservedValue = orderedScores.Select(i => (double)i).ToArray();

            priors.ObservedValue = participants.Select(
                p => p.popularityGaussian).ToArray();
            variances.ObservedValue = participants.Select(
                p => DynamicPopularityFactor * DynamicPopularityFactor).ToArray();

            using (var loop = Variable.ForEach(range))
            {
                popularities[range] = Variable.GaussianFromMeanAndVariance(
                    Variable<double>.Random(priors[range]),
                    variances[range]);

                using (Variable.If(loop.Index > 0))
                {
                    using (Variable.If(scores[loop.Index] == scores[loop.Index - 1]))
                    {
                        Variable<bool>.ConstrainBetween(
                            popularities[loop.Index - 1] - popularities[loop.Index],
                            -drawMargin,
                            drawMargin);
                    }

                    using (Variable.If(scores[loop.Index] != scores[loop.Index - 1]))
                    {
                        results[loop.Index].SetTo(popularities[loop.Index - 1] >
                            popularities[loop.Index] / (scores[loop.Index] / scores[loop.Index - 1]));

                        results.ObservedValue = participants.Select(p => true).ToArray();
                    }
                }
            }

            var participantPosts = Ranking.Engine.Infer<Gaussian[]>(popularities);
            DrawMargin = Ranking.Engine.Infer<Gaussian>(drawMargin);

            for (int i = 0; i < participants.Length; i++)
                participants[i].popularityGaussian = participantPosts[i];
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToFirstAttribute();
            DynamicPopularityFactor = double.Parse(reader.GetAttribute("dynamic"));
            DrawMargin = Gaussian.FromMeanAndVariance(
                double.Parse(reader.GetAttribute("drawMean")),
                double.Parse(reader.GetAttribute("drawVariance")));

            while (reader.ReadToFollowing("participant"))
            {
                var p = new Participant();
                p.ReadXml(reader);
                Participants.Add(p);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("dynamic", DynamicPopularityFactor.ToString());
            writer.WriteAttributeString("drawMean", DrawMargin.GetMean().ToString());
            writer.WriteAttributeString("drawVariance", DrawMargin.GetVariance().ToString());

            foreach (var p in Participants)
            {
                writer.WriteStartElement("participant");
                p.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }
}
