namespace Ddi.Registry.Web.Models
{
    public class DdiUrn
    {
        private string agency;
        public string Agency 
        {
            get { return agency; }
            set 
            {
                if (value != null) { agency = value.ToLowerInvariant(); }
                else { agency = null; }
            }

        }
        public string Identifier { get; set; }
        public string Version { get; set; }

        public static bool TryParse(string urn, out DdiUrn ddiUrn)
        {
            ddiUrn = null;

            if (string.IsNullOrWhiteSpace(urn))
            {
                return false;
            }

            var parts = urn.Split(':', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            if(parts.Length != 5) { return false; }

            if(parts[0].ToLower() != "urn" && parts[1].ToLower() != "ddi") { return false; }

            ddiUrn = new DdiUrn();
            ddiUrn.Agency = parts[2];
            ddiUrn.Identifier = parts[3];
            ddiUrn.Version = parts[4];

            return true;
        }

        public override string ToString()
        {
            return "urn:ddi:" + Agency + ":" + Identifier + ":" + Version;
        }

    }
}
