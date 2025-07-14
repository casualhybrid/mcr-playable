using System;
using UnityEngine;
using System.Globalization;

[CreateAssetMenu(fileName = "TimeManager", menuName = "ScriptableObjects/TimeManagerSO")]
public class TimeManagerSO : ScriptableObject
{
    [HideInInspector] public double inactivityPeriod;

    DateTime GetCurrentDateTime() {
        string date = GetCurrentTimeDateInString();

        DateTime dateTime; 

        bool success = DateTime.TryParseExact(date, "G", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out dateTime);

        if (!success)
        {
            UnityEngine.Console.LogError("Failed to convert string to time " + date);
        }

        return dateTime;
    }

    TimeSpan GetTimeDifference(DateTime newDate, DateTime oldDate) {
        return (newDate - oldDate);              // 61.06:05:21
    }

    public string GetCurrentTimeDateInString()
    {
        DateTime nowDate = DateTime.Now;
        string date = nowDate.ToString("G", DateTimeFormatInfo.InvariantInfo);
   
        return date;
    }

    public DateTime ConvertStringToDateTime(string dateStr) {
       
        //  DateTime convertedDateTime = DateTime.Parse(dateStr);
        DateTime convertedDateTime;

        bool success = DateTime.TryParseExact(dateStr, "G", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out convertedDateTime);
       
        if(!success)
        {
            UnityEngine.Console.LogError("Failed to convert string to time " + dateStr);
        }


      

        return convertedDateTime;
    }

    public int GetNoOfDays(DateTime oldDate) {
        DateTime newDate = GetCurrentDateTime();
        TimeSpan dateDiff = GetTimeDifference(newDate, oldDate);
        return dateDiff.Days;
    }

    public int GetNoOfHours(DateTime oldDate)
    {
        DateTime newDate = GetCurrentDateTime();
        TimeSpan dateDiff = GetTimeDifference(newDate, oldDate);
        return dateDiff.Hours;
    }

    public double GetDaysWithHours(string dateStr)
    {
        DateTime oldDate = ConvertStringToDateTime(dateStr);
        DateTime newDate = GetCurrentDateTime();
        TimeSpan dateDiff = GetTimeDifference(newDate, oldDate);
        return dateDiff.TotalDays;
    }

    public double GetTotalHours(DateTime oldDate)
    {
        DateTime newDate = GetCurrentDateTime();
        TimeSpan dateDiff = GetTimeDifference(newDate, oldDate);
        return dateDiff.TotalHours;
    }
}
