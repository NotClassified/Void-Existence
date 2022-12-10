using System.Collections;
using System.Collections.Generic;

public class TimeObject
{
    float time;
    int minutes;
    int seconds;
    int miliseconds;

    public TimeObject()
    {
        time = 0;
        minutes = 0;
        seconds = 0;
        miliseconds = 0;
    }
    public void SetTime(float _time)
    {
        time = _time;
        minutes = (int)_time / 60;
        seconds = (int)_time % 60;
        miliseconds = (int)(_time * 100) % 100;
    }

    //convert seconds to minute:second:milisecond format
    public static string ConvertTimeMINSECMILI(float time)
    {
        string minuteText = ((int)time / 60).ToString();
        int seconds = (int)time % 60;
        string secondsText = seconds < 10 ? "0" + seconds.ToString() : seconds.ToString();
        int  miliseconds = (int)(time * 100) % 100;
        string milisecondsText = miliseconds < 10 ? "0" + miliseconds.ToString() : miliseconds.ToString();
        return minuteText + ":" + secondsText + ":" + milisecondsText;
    }
    //convert seconds to minute:second:milisecond format
    public static string ConvertTimeMINSECMILI(TimeObject timeO)
    {
        string secondsText = timeO.seconds < 10 ? "0" + timeO.seconds.ToString() : timeO.seconds.ToString();
        string milisecondsText = timeO.miliseconds < 10 ? "0" + timeO.miliseconds.ToString() : timeO.miliseconds.ToString();
        return timeO.minutes + ":" + secondsText + ":" + milisecondsText;
    }
    //convert seconds to minute:second format
    public static string ConvertTimeMINSEC(float time)
    {
        string minuteText = ((int)time / 60).ToString();
        int seconds = (int)time % 60;
        string secondsText = seconds < 10 ? "0" + seconds.ToString() : seconds.ToString();
        return minuteText + ":" + secondsText;
    }
    //convert seconds to minute:second format
    public static string ConvertTimeMINSEC(TimeObject timeO)
    {
        string secondsText = timeO.seconds < 10 ? "0" + timeO.seconds.ToString() : timeO.seconds.ToString();
        return timeO.minutes + ":" + secondsText;
    }
    public static string Miliseconds2Digit(float time)
    {
        int miliseconds = (int)(time * 100) % 100;
        return miliseconds < 10 ? "0" + miliseconds.ToString() : miliseconds.ToString();
    }
}
