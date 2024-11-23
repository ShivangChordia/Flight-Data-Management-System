/*
* FILE : TelemetryDataModel.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : The TelemetryDataModel class defines the structure for telemetry data used in the Temp_GUI_GroundTerminal application.
*/

using System.ComponentModel.DataAnnotations;

namespace Temp_GUI_GroundTerminal
{

    public class TelemetryDataModel
    {
        [Key]
        public string? TailNumber { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime? Timestamp { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Weight {  get; set; }
        public float Altitude { get; set; }
        public float Pitch { get; set; }
        public float Bank { get; set; }
        public int Checksum { get; set; }
    }

}
