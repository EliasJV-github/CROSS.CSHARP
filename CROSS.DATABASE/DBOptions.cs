namespace CROSS.DATABASE
{
    using System.Collections.Generic;

    public class DBOptions
    {
        public List<DBOptionItems> Connections { get; set; } = new List<DBOptionItems>();

        public string Name { get; set; } = string.Empty;
    }
}