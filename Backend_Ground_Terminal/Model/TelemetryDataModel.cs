﻿/*
* FILE : TelemetryDataModel.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This file defines the TelemetryDataModel class, which represents the structure of telemetry data in the Backend Ground Terminal system. 
*/

using System.ComponentModel.DataAnnotations;

namespace Backend_Ground_Terminal.Model
{

    public class TelemetryDataModel
    {
        [Key]
        public string? TailNumber { get; set; }
        public int SequenceNumber { get; set; }
        public string Timestamp { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Weight { get; set; }
        public float Altitude { get; set; }
        public float Pitch { get; set; }
        public float Bank { get; set; }
        public int Checksum { get; set; }
    }

}
