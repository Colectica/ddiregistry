using System;
using System.IO;
using System.Text;
using System.Linq;
using Ddi.Registry.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using System.Collections.Generic;

namespace Ddi.Registry.ZoneWriter
{
    class Program
    {
        private static ZoneWriterOptions zoneOptions;

        public static int Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                  .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                  .AddJsonFile("zonesettings.json", true, true)
                  .AddJsonFile("zonesettings.Development.json", true, true)
                  .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(
                    config.GetConnectionString("DefaultConnection"));

            zoneOptions = new ZoneWriterOptions();
            config.GetSection("ZoneWriter").Bind(zoneOptions);

            using (ApplicationDbContext context = new ApplicationDbContext(optionsBuilder.Options))
            {
                var lastExport = context.ExportActions.Find(RegistryProvider.ExportAction);
                var lastUpdate = context.ExportActions.Find(RegistryProvider.UpdateAction);

                var lastExportTime = lastExport?.LastModified ?? DateTimeOffset.MinValue;
                var lastUpdateTime = lastUpdate?.LastModified ?? DateTimeOffset.MinValue;

                bool forceUpdate = false;
                if (args.Length != 0 && args[0] == "-f")
                {
                    forceUpdate = true;
                }
                if (lastExportTime > lastUpdateTime && !forceUpdate)
                {
                    return 100;
                }

                // calculate next soa
                long nextSoa = DateTimeOffset.UtcNow.Year * 1000000;
                nextSoa += DateTimeOffset.UtcNow.Month * 10000;
                nextSoa += DateTimeOffset.UtcNow.Day * 100;

                long lastSoa = 0;
                if(lastExport != null && lastExport.LastSoa.HasValue)
                {
                    lastSoa = lastExport.LastSoa.Value;
                }
                while(lastSoa > nextSoa)
                {
                    nextSoa++;
                }


                StringBuilder sb = GenerateZoneFile(nextSoa, context);
                string contents = sb.ToString();

                
                string tempfile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Console.WriteLine($"writing temp zone file to {tempfile}");
                File.WriteAllText(tempfile, contents, Encoding.ASCII);

                string filename = string.Format("{0}.zone", zoneOptions.BaseZone);
                Console.WriteLine($"setting filename to {filename}");
                string destFileName = Path.Combine(zoneOptions.ZoneFileLocation, filename);
                Console.WriteLine($"setting destFileName to {destFileName}");

                string backupFile = Path.Combine(
                    zoneOptions.ZoneFileLocation,
                    string.Format("{0}.last", zoneOptions.BaseZone));
                Console.WriteLine($"setting backupFile to {backupFile}");


                if (File.Exists(destFileName))
                {
                    Console.WriteLine($"Backing up prior zone file");

                    string root1 = Path.GetPathRoot(tempfile);
                    string root2 = Path.GetPathRoot(destFileName);
                    if (string.Compare(root1, root2, true) == 0)
                    {
                        // can only replace on the same drive
                        Console.WriteLine($"Replacing zone file with {tempfile} and writing backup at {backupFile}");
                        File.Replace(tempfile, destFileName, backupFile);

                        Console.WriteLine($"Deleting tempfile {tempfile}");
                        File.Delete(tempfile);
                    }
                    else
                    {
                        Console.WriteLine($"Deleting zone file");
                        File.Delete(destFileName);
                        File.Move(tempfile, destFileName);
                        Console.WriteLine($"Deleting tempfile {tempfile}");
                        File.Delete(tempfile);
                    }
                }
                else
                {
                    Console.WriteLine($"Moving {tempfile} zone file to {destFileName}");
                    File.Move(tempfile, destFileName);
                    Console.WriteLine($"Deleting tempfile {tempfile}");
                    File.Delete(tempfile);
                }
                

                if(lastExport == null)
                {
                    lastExport = new ExportAction() { Id = RegistryProvider.ExportAction };
                    context.Add(lastExport);
                }
                lastExport.LastModified = DateTimeOffset.UtcNow;
                lastExport.LastSoa = nextSoa;
                context.SaveChanges();
            }

            return 0;
        }

        public static StringBuilder GenerateZoneFile(long nextSoa, ApplicationDbContext context)
        {
            StringBuilder sb = new StringBuilder();
            sb.LineFormat("; zone file for {0}", zoneOptions.BaseZone);
            sb.AppendLine("$TTL 2d    ; 172800 secs default TTL for zone");
            sb.LineFormat("$ORIGIN {0}.", zoneOptions.BaseZone);
            sb.LineFormat("@             IN      SOA   {0}. hostmaster.example.com. (", zoneOptions.MasterNameserver);
            sb.LineFormat("            {0}      ; se = serial number", nextSoa);
            sb.AppendLine("            12h        ; ref = refresh");
            sb.AppendLine("            15m        ; ret = update retry");
            sb.AppendLine("            3w         ; ex = expiry");
            sb.AppendLine("            3h         ; min = minimum");
            sb.AppendLine("            )");
            sb.AppendLine();


            sb.AppendLine("; main zone A record");
            sb.LineFormat("             3600    IN      A   {0}", zoneOptions.DefaultARecord);

            sb.AppendLine("; main domain name servers");
            string result = sb.ToString();
            foreach (string nameserver in zoneOptions.Nameservers)
            {
                sb.AppendNs(nameserver);
            }

            var agencies = context.Agencies.Where(x => x.ApprovalState == ApprovalState.Approved).OrderBy(item => item.AgencyId).ToList();
            sb.AppendLine();
            sb.AppendLine("; service definitions");
            sb.AppendLine("; srvce.prot.name  ttl  class   rr  pri  weight port target");
            foreach (Agency a in agencies)
            {
                var hosted = context.Assignments.Include(x => x.Services).Where(x =>x.AgencyId == a.AgencyId && x.IsDelegated == false)
                    .OrderBy(item => item.AssignmentId).ToList();
                foreach (Assignment assignment in hosted)
                {
                    AppendServices(sb, assignment);
                }

            }

            sb.AppendLine();
            sb.AppendLine("; external nameserver delegations");
            foreach (Agency a in agencies)
            {
                var delegated = context.Assignments.Include(x => x.Delegations).Where(x => x.AgencyId == a.AgencyId && x.IsDelegated == true)
                    .OrderBy(item => item.AssignmentId).ToList();
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
            sb.LineFormat("; name server delegation for {0}", assignment.AssignmentId.ReverseDnsParts());
            string zone = string.Format("{0}.{1}.", assignment.AssignmentId.ReverseDnsParts(), zoneOptions.BaseZone);
            List<Delegation> delegations = assignment.Delegations.OrderBy(x => x.NameServer).ToList();
            foreach (Delegation d in delegations)
            {
                string result = string.Format("{0}  IN      NS     {1}.", zone, d.NameServer);
                sb.AppendLine(result);
            }
        }

        public static void AppendServices(StringBuilder sb, Assignment assignment)
        {
            sb.AppendLine();
            sb.LineFormat("; services for {0}", assignment.AssignmentId);
            List<Service> services = assignment.Services.OrderBy(item => item.ServiceName).ToList();
            foreach (Service s in services)
            {
                string result = string.Format("_{0}._{1}.{2} {3} IN      SRV {4}    {5}      {6}   {7}.",
                            s.ServiceName,
                            s.Protocol,
                            assignment.AssignmentId.ReverseDnsParts(),
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
            string[] parts = value.Split('.');
            Array.Reverse(parts);
            string result = string.Join(".", parts);
            return result;
        }

        public static void AppendNs(this StringBuilder sb, string nameserver)
        {
            string result = string.Format("              14400    IN      NS     {0}.", nameserver);
            sb.AppendLine(result);
        }

        public static void LineFormat(this StringBuilder sb, string format, params object[] args)
        {
            string result = string.Format(format, args);
            sb.AppendLine(result);
        }

    }
}

