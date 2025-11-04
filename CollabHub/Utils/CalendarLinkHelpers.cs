using System;
using System.Linq;
using System.Text;
using CollabHub.Models;

namespace CollabHub.Utils
{
    public static class CalendarLinkHelpers
    {
        public static string BuildGoogleCalendarLink(Event ev)
        {
            // Гарантуємо UTC
            var startUtc = ev.StartsAt.Kind == DateTimeKind.Utc
                ? ev.StartsAt
                : ev.StartsAt.ToUniversalTime();

            var endUtc = ev.EndsAt.Kind == DateTimeKind.Utc
                ? ev.EndsAt
                : ev.EndsAt.ToUniversalTime();

            // Формат для Google: yyyyMMddTHHmmssZ
            var dates = $"{startUtc:yyyyMMdd'T'HHmmss'Z'}/{endUtc:yyyyMMdd'T'HHmmss'Z'}";

            var title = Uri.EscapeDataString(ev.Title ?? "Event");
            var details = Uri.EscapeDataString(ev.Description ?? string.Empty);

            var locationRaw = ev.Venue != null
                ? string.Join(", ", new[] { ev.Venue.Name, ev.Venue.Address }
                    .Where(s => !string.IsNullOrWhiteSpace(s)))
                : string.Empty;
            var location = Uri.EscapeDataString(locationRaw);

            var url =
                $"https://calendar.google.com/calendar/render?action=TEMPLATE" +
                $"&text={title}" +
                $"&details={details}" +
                $"&location={location}" +
                $"&dates={dates}";

            return url;
        }

        // BONUS: генерація .ics-контенту (для Outlook/Apple і т.д.)
        public static string BuildIcsContent(Event ev)
        {
            var startUtc = ev.StartsAt.Kind == DateTimeKind.Utc
                ? ev.StartsAt
                : ev.StartsAt.ToUniversalTime();

            var endUtc = ev.EndsAt.Kind == DateTimeKind.Utc
                ? ev.EndsAt
                : ev.EndsAt.ToUniversalTime();

            string uid = $"collabhub-event-{ev.Id}@collabhub.local";
            string summary = EscapeIcs(ev.Title ?? "Event");
            string description = EscapeIcs(ev.Description ?? string.Empty);

            string location = string.Empty;
            if (ev.Venue != null)
            {
                location = EscapeIcs(string.Join(", ", new[]
                {
                    ev.Venue.Name,
                    ev.Venue.Address
                }.Where(s => !string.IsNullOrWhiteSpace(s))));
            }

            string dtStart = startUtc.ToString("yyyyMMdd'T'HHmmss'Z'");
            string dtEnd = endUtc.ToString("yyyyMMdd'T'HHmmss'Z'");
            string dtStamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//CollabHub//EN");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:PUBLISH");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{uid}");
            sb.AppendLine($"DTSTAMP:{dtStamp}");
            sb.AppendLine($"DTSTART:{dtStart}");
            sb.AppendLine($"DTEND:{dtEnd}");
            sb.AppendLine($"SUMMARY:{summary}");
            if (!string.IsNullOrEmpty(description))
                sb.AppendLine($"DESCRIPTION:{description}");
            if (!string.IsNullOrEmpty(location))
                sb.AppendLine($"LOCATION:{location}");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        private static string EscapeIcs(string s)
        {
            return s
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n");
        }
    }
}
