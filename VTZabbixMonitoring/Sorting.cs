using System;
using System.Collections;

namespace VTZabbixMonitoring
{
    internal class Sorting
    {
        static Hashtable ViolationCode = new Hashtable();

        static public void HashVuolation()
        {
            ViolationCode.Add("0", "0 - Stream");
            ViolationCode.Add("2", "2 - OverSpeed");
            ViolationCode.Add("4", "4 - WrongDirection");
            ViolationCode.Add("5", "5 - BusLane");
            ViolationCode.Add("10", "10 - RedLightCross");
            ViolationCode.Add("31", "31 - SeatBelt");
            ViolationCode.Add("32", "32 - PhoneInHand");
            ViolationCode.Add("81", "81 - WrongCross");
            ViolationCode.Add("83", "83 - StopLine");
            ViolationCode.Add("90", "90 - WrongTurnTwoFrontier");
            ViolationCode.Add("112", "112 - WrongLineTurn");
            ViolationCode.Add("113", "113 - NoForwardZnak");
            ViolationCode.Add("114", "114 - NoUTurnOnlyForward");
            ViolationCode.Add("127", "127 - Lights");
            ViolationCode.Add("134", "134 - SeatBelt_Passanger");
        }






    }
}
