using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ddi.Registry.Data;
using System.IO;
using System.Collections.ObjectModel;

namespace Ddi.Registry.ZoneWriter
{
    public class Program
    {
        private static readonly string ExportAction = "export";
        private static readonly string UpdateAction = "update";

        private static RegistryProvider provider = new RegistryProvider();

        public static int Main(string[] args)
        {
            

            DateTime lastExport = provider.GetLastAction(ExportAction);
            DateTime lastUpdate = provider.GetLastAction(UpdateAction);

            bool forceUpdate = false;
            if (args.Length != 0 && args[0] == "-f")
            {
                forceUpdate = true;
            }
            if (lastExport > lastUpdate && !forceUpdate) 
            { 
                return 100; 
            }

            long nextSoa = provider.GetNextSoa();

            StringBuilder sb = GenerateZoneFile(nextSoa);
            string contents = sb.ToString();


            string tempfile = Path.GetTempFileName();
            File.WriteAllText(tempfile, contents, Encoding.ASCII);

            string filename = string.Format("{0}.zone", Settings.Default.BaseZone);
            string destFileName = Path.Combine(Settings.Default.ZoneFileLocation, filename);

            string backupFile = Path.Combine(
                Settings.Default.ZoneFileLocation,
                string.Format("{0}.last", Settings.Default.BaseZone));
            if (File.Exists(destFileName))
            {
                string root1 = Path.GetPathRoot(tempfile);
                string root2 = Path.GetPathRoot(destFileName);
                if (string.Compare(root1, root2, true) == 0)
                {
                    // can only replace on the same drive
                    File.Replace(tempfile, destFileName, backupFile);
                }
                else
                {
                    File.Delete(destFileName);
                    File.Move(tempfile, destFileName);
                }
            }
            else
            {
                File.Move(tempfile, destFileName);
            }


            provider.RecordAction("export");
            return 0;
        }

        public static StringBuilder GenerateZoneFile(long nextSoa)
        {
            StringBuilder sb = new StringBuilder();
            sb.LineFormat("; zone file for {0}", Settings.Default.BaseZone);
            sb.AppendLine("$TTL 2d    ; 172800 secs default TTL for zone");
            sb.LineFormat("$ORIGIN {0}.", Settings.Default.BaseZone);
            sb.LineFormat("@             IN      SOA   {0}. hostmaster.example.com. (", Settings.Default.MasterNameserver);
            sb.LineFormat("            {0}      ; se = serial number", nextSoa);
            sb.AppendLine("            12h        ; ref = refresh");
            sb.AppendLine("            15m        ; ret = update retry");
            sb.AppendLine("            3w         ; ex = expiry");
            sb.AppendLine("            3h         ; min = minimum");
            sb.AppendLine("            )");
            sb.AppendLine();


            sb.AppendLine("; main zone A record");
            sb.LineFormat("             IN      A   {0}", Settings.Default.DefaultARecord);

            sb.AppendLine("; main domain name servers");
            string result = sb.ToString();
            foreach (string nameserver in Settings.Default.Nameserver)
            {
                sb.AppendNs(nameserver);
            }


            var agencies = provider.GetAgenciesByApprovalState(ApprovalState.Approved).OrderBy(item => item.AgencyName).ToList();
            sb.AppendLine();
            sb.AppendLine("; service definitions");
            sb.AppendLine("; srvce.prot.name  ttl  class   rr  pri  weight port target");
            foreach (Agency a in agencies)
            {
                var hosted = provider.GetAssignmentsForAgency(a.AgencyId)
                    .Where(item => item.IsDelegated == false)
                    .OrderBy(item => item.Name).ToList();
                foreach (Assignment assignment in hosted)
                {
                    AppendServices(sb, assignment);
                }

            }

            sb.AppendLine();
            sb.AppendLine("; external nameserver delegations");
            foreach (Agency a in agencies)
            {
                var delegated = provider.GetAssignmentsForAgency(a.AgencyId)
                        .Where(item => item.IsDelegated == true)
                        .OrderBy(item => item.Name).ToList();
                foreach (Assignment assignment in delegated)
                {
                    AppendDelegations(sb, assignment);
                }
            }

            return sb;
        }

        public static void AppendDelegations(StringBuilder sb, Assignment assignment)
        {
            sb.AppendLine();
            sb.LineFormat("; name server delegation for {0}", assignment.Name.ReverseDnsParts());
            string zone = string.Format("{0}.{1}.", assignment.Name.ReverseDnsParts(), Settings.Default.BaseZone);
            IEnumerable<Delegation> delegations = provider.GetDelegationsForAssignment(assignment.AssignmentId).OrderBy(item => item.NameServer);
            foreach (Delegation d in delegations)
            {
                string result = string.Format("{0}  IN      NS     {1}.",zone, d.NameServer);
                sb.AppendLine(result);
            }
        }

        public static void AppendServices(StringBuilder sb, Assignment assignment)
        {
            sb.AppendLine();
            sb.LineFormat("; services for {0}", assignment.Name);
            IEnumerable<Service> services = provider.GetServicesForAssignment(assignment.AssignmentId).OrderBy(item => item.ServiceName);
            foreach (Service s in services)
            {
                string result = string.Format("_{0}._{1}.{2} {3} IN      SRV {4}    {5}      {6}   {7}.",
                            s.ServiceName,
                            s.Protocol,
                            assignment.Name.ReverseDnsParts(),
                            s.TimeToLive,
                            s.Priority,
                            s.Weight,
                            s.Port,
                            s.Hostname);
                sb.AppendLine(result);
            }
        }
    }

    public static class Extensions 
    {
        public static string ReverseDnsParts(this string value) 
        {
            var parts = value.Split('.').Reverse();
            string result = string.Join(".", parts);
            return result;
        }

        public static void AppendNs(this StringBuilder sb, string nameserver)
        {
            string result = string.Format("              IN      NS     {0}.", nameserver);
            sb.AppendLine(result);
        }
        
        public static void LineFormat(this StringBuilder sb, string format, params object[] args) 
        {
            string result = string.Format(format, args);
            sb.AppendLine(result);
        }

    }
}
