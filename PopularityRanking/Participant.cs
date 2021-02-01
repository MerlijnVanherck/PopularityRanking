using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PopularityRanking
{
    public class Participant : IXmlSerializable
    {
        public const double DefaultMean = 1000;
        public const double DefaultDeviation = 300;

        public int Id { get; set; } = 0;

        public string Name { get; set; } = "placeholder";

        public double Popularity { get =>
                popularityGaussian.GetMean() - Math.Sqrt(popularityGaussian.GetVariance()); }

        public double Precision { get => popularityGaussian.Precision; }

        internal Gaussian popularityGaussian = Gaussian.FromMeanAndVariance(
            DefaultMean, DefaultDeviation * DefaultDeviation);

        public int? Score { get; set; } = null;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToNextAttribute();
            this.Id = reader.ReadContentAsInt();
            reader.MoveToNextAttribute();
            this.Name = reader.ReadContentAsString();
            reader.MoveToNextAttribute();
            var mean = reader.ReadContentAsDouble();
            reader.MoveToNextAttribute();
            var deviation = reader.ReadContentAsDouble();

            this.popularityGaussian = Gaussian.FromMeanAndVariance(mean, deviation * deviation);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("id", Id.ToString());
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("mean", popularityGaussian.GetMean().ToString());
            writer.WriteAttributeString("deviation",
                Math.Sqrt(popularityGaussian.GetVariance()).ToString());
        }

        public override string ToString()
        {
            return $"#{Id} {Name} ({Popularity})" + (Score != null ? $" ({Score})":"");
        }
    }
}
