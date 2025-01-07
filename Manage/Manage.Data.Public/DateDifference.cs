using System.Globalization;
using System.Text;

namespace Manage.Data.Public
{
    public static class DateDifference
    {
        public static string DateDiffWithHour(System.DateTime d)
        {
            DateTime LastDate = d;
            TimeSpan ts = DateTime.Now - LastDate;
            PersianCalendar pc = new PersianCalendar();
            int DifferenceYear = DateTime.Now.Year - LastDate.Year;
            int DiffernceMounth = DateTime.Now.Month - LastDate.Month;

            if (DateTime.Now.Month > LastDate.Month)
                DiffernceMounth = DateTime.Now.Month - LastDate.Month;
            else
                DiffernceMounth = LastDate.Month - DateTime.Now.Month;
            int DifferenceDays = ts.Days;
            if (DateTime.Now.Month - 1 == LastDate.Month)
            {
                DiffernceMounth = 0;
            }
            if (DateTime.Now.Year - 1 == LastDate.Year && LastDate.Month > DateTime.Now.Month)
            {
                DifferenceYear = 0;
                DiffernceMounth = 12 - DiffernceMounth;
            }
            StringBuilder Result = new System.Text.StringBuilder("");

            if (DifferenceYear > 0)
            {
                Result.Append(DifferenceYear.ToString() + " سال پیش" + " ، " + GetHour(LastDate));
            }
            else if (DiffernceMounth > 0)
            {
                Result.Append(DiffernceMounth.ToString() + " ماه پیش" + " ، " + GetHour(LastDate));
            }
            else if (DifferenceDays > 0)
                Result.Append(DifferenceDays.ToString() + " روز پیش" + " ، " + GetHour(LastDate));
            else if (DifferenceDays == 0)
                Result.Append(" امروز" + " ، " + GetHour(LastDate));

            return Result.ToString();
        }
        public static string DateDiff(System.DateTime d)
        {
            DateTime LastDate = d;
            TimeSpan ts = DateTime.Now - LastDate;
            PersianCalendar pc = new PersianCalendar();
            int DifferenceYear = DateTime.Now.Year - LastDate.Year;
            int DiffernceMounth = DateTime.Now.Month - LastDate.Month;

            if (DateTime.Now.Month > LastDate.Month)
                DiffernceMounth = DateTime.Now.Month - LastDate.Month;
            else
                DiffernceMounth = LastDate.Month - DateTime.Now.Month;
            int DifferenceDays = ts.Days;
            if (DateTime.Now.Month - 1 == LastDate.Month)
            {
                DiffernceMounth = 0;
            }
            if (DateTime.Now.Year - 1 == LastDate.Year && LastDate.Month > DateTime.Now.Month)
            {
                DifferenceYear = 0;
                DiffernceMounth = 12 - DiffernceMounth;
            }
            StringBuilder Result = new System.Text.StringBuilder("");

            if (DifferenceYear > 0)
            {
                Result.Append(DifferenceYear.ToString() + " سال پیش");
            }
            else if (DiffernceMounth > 0)
            {
                Result.Append(DiffernceMounth.ToString() + " ماه پیش");
            }
            else if (DifferenceDays > 0)
                Result.Append(DifferenceDays.ToString() + " روز پیش");
            else if (DifferenceDays == 0)
                Result.Append(" امروز");

            return Result.ToString();
        }

        public static string MiladiToShamsiWithHour(System.DateTime d)
        {
            PersianCalendar pc = new PersianCalendar();
            string Result = pc.GetYear(d) + "-" + pc.GetMonth(d).ToString("D2") + "-" + pc.GetDayOfMonth(d).ToString("D2") + " " + pc.GetHour(d).ToString("D2") + ":" + pc.GetMinute(d).ToString("D2") + ":" + pc.GetSecond(d).ToString("D2");

            return Result.ToString();
        }
        public static string MiladiToShamsi(System.DateTime d)
        {
            PersianCalendar pc = new PersianCalendar();
            string Result = pc.GetYear(d) + "-" + GetMounth(pc.GetMonth(d)) + "-" + pc.GetDayOfMonth(d);

            return Result;
        }
        public static DateTime ShamsiToMiladiFromString(string d)
        {
            PersianCalendar pc = new PersianCalendar();
            var splited = d.Split('-');
            DateTime dt = new DateTime(int.Parse(splited[0]), int.Parse(splited[1]), int.Parse(splited[2]), pc);

            return dt;
        }

        public static bool IsActive(System.DateTime d)
        {
            System.DateTime now = System.DateTime.Now;
            if (d > now)
            {
                return true;
            }
            return false;
        }
        public static string GetHour(DateTime lastdate)
        {
            PersianCalendar pc = new PersianCalendar();
            string result = " ساعت " + (((pc.GetHour(lastdate)) < 10) ? ("0" + pc.GetHour(lastdate).ToString()) : (pc.GetHour(lastdate)).ToString()) + ":" + (((pc.GetMinute(lastdate)) < 10) ? ("0" + pc.GetMinute(lastdate).ToString()) : (pc.GetMinute(lastdate)).ToString());
            return result;
        }
        public static string getDay(DayOfWeek day)
        {
            string Result = "";
            switch (day)
            {
                case DayOfWeek.Friday:
                    Result = "جمعه";
                    break;
                case DayOfWeek.Monday:
                    Result = "دوشنبه";
                    break;
                case DayOfWeek.Saturday:
                    Result = "شنبه";
                    break;
                case DayOfWeek.Sunday:
                    Result = "یکشنبه";
                    break;
                case DayOfWeek.Thursday:
                    Result = "پنج شنبه";
                    break;
                case DayOfWeek.Tuesday:
                    Result = "سه شنبه";

                    break;
                case DayOfWeek.Wednesday:
                    Result = "چهارشنبه";
                    break;
                default:
                    break;
            }
            return Result;
        }
        public static string GetMounth(int month)
        {
            string[] monthInYear = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
            return monthInYear[month - 1];
        }
    }
}
