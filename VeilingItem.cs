using System;
using Newtonsoft.Json.Linq;

namespace Veiling
{
    public class VeilingItem
    {
        public int id { get; set; }
        public string nommer { get; set; }
        public string beskrywing { get; set; }
        public int bieer_id { get; set; }
        public float bedrag { get; set; }
        public bool betaal { get; set; }
        public string soort { get; set; }

        public VeilingItem(JToken item)
        {
            string sBetaal = (string)item["betaal"] ?? "0";
            sBetaal = (sBetaal == "0") ? "false" : "true";

            id = (int)item["id"];
            nommer = (string)item["nommer"];
            beskrywing = (string)item["beskrywing"];
            bieer_id = (int)item["bieer_id"];
            bedrag = float.Parse((string)item["bedrag"] ?? "0.00");
            betaal = bool.Parse(sBetaal);
            soort = (string)item["soort"];
        }
    }
}

