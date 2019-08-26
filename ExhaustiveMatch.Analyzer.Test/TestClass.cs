using System;
using System.ComponentModel;

class TestClass
{
	void TestMethod(DayOfWeek dayOfWeek)
	{
		switch (dayOfWeek)
		{
			case DayOfWeek.Monday:
			case DayOfWeek.Tuesday:
			case DayOfWeek.Wednesday:
			case DayOfWeek.Thursday:
			case DayOfWeek.Friday:
				Console.WriteLine("Weekday");
				break;
			case DayOfWeek.Saturday:
				// Omitted Sunday
				Console.WriteLine("Weekend");
				break;
			default:
				throw new InvalidEnumArgumentException(nameof(dayOfWeek), (int)dayOfWeek, typeof(DayOfWeek));
		}
	}
}