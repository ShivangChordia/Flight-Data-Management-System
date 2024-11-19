﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ground_Terminal_Management_System.Model
{

    public class TelemetryDataModel
    {
        public string TailNumber { get; set; }
        public int SequenceNumber { get; set; }
        public string Timestamp { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Altitude { get; set; }
        public float Pitch { get; set; }
        public float Bank { get; set; }
        public int Checksum { get; set; }
    }

}