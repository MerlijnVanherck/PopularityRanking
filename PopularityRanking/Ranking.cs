using Microsoft.ML.Probabilistic.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PopularityRanking
{
    [XmlRoot(ElementName = "ranking")]
    public class Ranking : IXmlSerializable
    {
        public static readonly InferenceEngine Engine = new InferenceEngine();

        public List<Participant> Participants { get; set; } = new List<Participant>();

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

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            while (reader.ReadToFollowing("participant"))
            {
                var p = new Participant();
                p.ReadXml(reader);
                Participants.Add(p);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var p in Participants)
            {
                writer.WriteStartElement("participant");
                p.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }
}
