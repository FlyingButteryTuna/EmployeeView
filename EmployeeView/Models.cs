using System;

namespace EmployeeViewer.Models
{
    public class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() => Name;
    }

    public class PersonRow
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public string StatusName { get; set; }
        public string DepName { get; set; }
        public string PostName { get; set; }
        public DateTime? DateEmploy { get; set; }
        public DateTime? DateUneploy { get; set; }

        public string FioShort
        {
            get
            {
                string i = string.IsNullOrEmpty(FirstName) ? "" : (FirstName.Substring(0, 1) + ".");
                string o = string.IsNullOrEmpty(SecondName) ? "" : (SecondName.Substring(0, 1) + ".");
                return $"{LastName} {i} {o}".Trim();
            }
        }

        public bool IsFired => DateUneploy.HasValue;
    }

    public class StatPoint
    {
        public DateTime Day { get; set; }
        public int Count { get; set; }
    }
}