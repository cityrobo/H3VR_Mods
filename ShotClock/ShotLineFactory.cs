using UnityEngine;

namespace ShotTimer
{
	public class ShotLineFactory
	{
        private float shotcount = 0;
        private float start_time;

		public ShotLine Create(OverflowArray<ShotLine> shot_database)
		{
            float time = Time.time;
            shotcount = shot_database.Count;
            ShotLine line = new ShotLine(0,0);
            if (shot_database.Count != 0) line = new ShotLine(time - start_time, (time - start_time) / shotcount);
            else line = new ShotLine(time - start_time, time - start_time);
            Debug.Log("New time added!");
            Debug.Log("Current Shotcount: " + shotcount);
            Debug.Log("Time: " + line.Time + " Split: " + line.Split);

            return line;
        }

		public void Reset()
		{
            start_time = Time.time;
            Debug.Log("Starting ShotClock!");
        }
	}
}