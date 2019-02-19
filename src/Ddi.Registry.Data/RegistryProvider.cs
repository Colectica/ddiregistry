using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data;

namespace Ddi.Registry.Data
{
    public class RegistryProvider
    {

        #region actions
        public void RecordAction(string actionName)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE export_action
                                           SET last_modified=now() at time zone 'UTC'
                                         WHERE action_name=:action_name;";

                    command.AddParameter(DbType.String, "action_name", actionName);
                    command.ExecuteNonQuery();
                }
            }
        }

        public long GetNextSoa()
        {

            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT nextval('soa_sequence');";
                    object result = command.ExecuteScalar();
                    if (result is long) { return (long)result; }
                }
            }
            return 0;
        }

        public DateTime GetLastAction(string actionName)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT last_modified FROM export_action
                                         WHERE action_name=:action_name;";

                    command.AddParameter(DbType.String, "action_name", actionName);
                    object result = command.ExecuteScalar();
                    if (result is DateTime) { return (DateTime)result; }
                }
            }
            return DateTime.MinValue;
        }

        #endregion

        #region Person
        public void Add(Person item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO person(person_id, username, person_name, email, organization, job_title, 
                                                street_address, city, state, zip, country, phone, homepage)
                                            VALUES (:person_id, :username, :person_name, :email, :organization, :job_title, 
                                                :street_address, :city, :state, :zip, :country, :phone, :homepage);";

                    command.AddParameter(DbType.Guid, "person_id", item.PersonId);
                    command.AddParameter(DbType.String, "username", item.Username); 
                    command.AddParameter(DbType.String, "person_name",item.Name);
                    command.AddParameter(DbType.String, "email",item.Email); 
                    command.AddParameter(DbType.String, "organization",item.Organization);  
                    command.AddParameter(DbType.String, "job_title",item.JobTitle);  
                    command.AddParameter(DbType.String, "street_address",item.StreetAddress);  
                    command.AddParameter(DbType.String, "city",item.City);  
                    command.AddParameter(DbType.String, "state",item.State);  
                    command.AddParameter(DbType.String, "zip",item.Zip);  
                    command.AddParameter(DbType.String, "country",item.Country);  
                    command.AddParameter(DbType.String, "phone",item.Phone);
                    command.AddParameter(DbType.String, "homepage", item.HomePage);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(Person item) 
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE person
                                           SET username=:username, person_name=:person_name, email=:email, organization=:organization, 
                                               job_title=:job_title, street_address=:street_address, city=:city, state=:state, zip=:zip, country=:country, 
                                               phone=:phone, homepage=:homepage
                                         WHERE person_id=:person_id;";

                    command.AddParameter(DbType.Guid, "person_id", item.PersonId);
                    command.AddParameter(DbType.String, "username", item.Username);
                    command.AddParameter(DbType.String, "person_name", item.Name);
                    command.AddParameter(DbType.String, "email", item.Email);
                    command.AddParameter(DbType.String, "organization", item.Organization);
                    command.AddParameter(DbType.String, "job_title", item.JobTitle);
                    command.AddParameter(DbType.String, "street_address", item.StreetAddress);
                    command.AddParameter(DbType.String, "city", item.City);
                    command.AddParameter(DbType.String, "state", item.State);
                    command.AddParameter(DbType.String, "zip", item.Zip);
                    command.AddParameter(DbType.String, "country", item.Country);
                    command.AddParameter(DbType.String, "phone", item.Phone);
                    command.AddParameter(DbType.String, "homepage", item.HomePage);

                    command.ExecuteNonQuery();
                }
            }
        }



        public void Remove(Person item) 
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM person WHERE person_id=:person_id;";

                    command.AddParameter(DbType.Guid, "person_id", item.PersonId);
                    command.ExecuteNonQuery();
                }
            }
        }
        public Person GetPerson(Guid personId) 
        {
            Collection<Person> results = GetPeople(personId: personId);
            return results.FirstOrDefault();
        }
        public Collection<Person> GetPeopleForUser(string username) 
        {
            Collection<Person> results = GetPeople(username: username);
            return results;
        }

        private Collection<Person> GetPeople(Guid personId = default(Guid), string username = null)
        {
            Collection<Person> results = new Collection<Person>();

            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT * FROM person";

                    Collection<string> where = new Collection<string>();

                    if (personId != default(Guid))
                    {
                        where.Add("person_id=:person_id");
                        command.AddParameter(DbType.Guid, "person_id", personId);
                    }
                    if (username != null)
                    {
                        where.Add("username=:username");
                        command.AddParameter(DbType.String, "username", username);
                    }

                    if (where.Count != 0)
                    {
                        command.CommandText += " WHERE ";
                        command.CommandText += string.Join(" AND ", where);
                    }

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Person person = new Person();
                            int ordinal = reader.GetOrdinal("person_id");
                            person.PersonId = reader.GetGuid(ordinal);

                            ordinal = reader.GetOrdinal("username");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.Username = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("person_name");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.Name = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("email");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.Email = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("organization");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.Organization = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("job_title");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.JobTitle = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("street_address");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.StreetAddress = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("city");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.City = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("state");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.State = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("zip");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.Zip = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("country");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.Country = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("phone");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.Phone = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("homepage");
                            if (!reader.IsDBNull(ordinal))
                            {
                                person.HomePage = reader.GetString(ordinal);
                            }

                            results.Add(person);
                        }
                    }
                }
            }

            return results;
        }
        #endregion

        #region Delegation
        public void Add(Delegation item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO delegation(delegation_id, assignment_id, nameserver)
                        VALUES (:delegation_id, :assignment_id, :nameserver);";

                    command.AddParameter(DbType.Guid, "delegation_id", item.DelegationId);
                    command.AddParameter(DbType.Guid, "assignment_id", item.AssignmentId);
                    command.AddParameter(DbType.String, "nameserver", item.NameServer);

                    command.ExecuteNonQuery();
                }
            }
            RecordAction("update");
        }

        public void Update(Delegation item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE delegation
                                   SET assignment_id=:assignment_id, nameserver=:nameserver
                                 WHERE delegation_id=:delegation_id;";

                    command.AddParameter(DbType.Guid, "delegation_id", item.DelegationId);
                    command.AddParameter(DbType.Guid, "assignment_id", item.AssignmentId);
                    command.AddParameter(DbType.String, "nameserver", item.NameServer);

                    command.ExecuteNonQuery();
                }
            }
            RecordAction("update");
        }
        public void Remove(Delegation item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM delegation WHERE delegation_id=:delegation_id";

                    command.AddParameter(DbType.Guid, "delegation_id", item.DelegationId);
                    command.ExecuteNonQuery();
                }
            }
            RecordAction("update");
        }
        public Delegation GetDelegation(Guid delegationId)
        {
            Collection<Delegation> results = GetDelegations(delegationId: delegationId);
            return results.FirstOrDefault();
        }
        public Collection<Delegation> GetDelegationsForAssignment(Guid assignmentId) 
        {
            return GetDelegations(assignmentId: assignmentId);
        }

        private Collection<Delegation> GetDelegations(Guid delegationId = default(Guid), Guid assignmentId = default(Guid))
        {
            Collection<Delegation> results = new Collection<Delegation>();

            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT * FROM delegation";

                    Collection<string> where = new Collection<string>();

                    if (delegationId != default(Guid))
                    {
                        where.Add("delegation_id=:delegation_id");
                        command.AddParameter(DbType.Guid, "delegation_id", delegationId);
                    }
                    if (assignmentId != default(Guid))
                    {
                        where.Add("assignment_id=:assignment_id");
                        command.AddParameter(DbType.Guid, "assignment_id", assignmentId);
                    }

                    if (where.Count != 0)
                    {
                        command.CommandText += " WHERE ";
                        command.CommandText += string.Join(" AND ", where);
                    }

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Delegation item = new Delegation();
                            int ordinal = reader.GetOrdinal("delegation_id");
                            item.DelegationId = reader.GetGuid(ordinal);

                            ordinal = reader.GetOrdinal("assignment_id");
                            item.AssignmentId = reader.GetGuid(ordinal);

                            ordinal = reader.GetOrdinal("nameserver");
                            item.NameServer = reader.GetString(ordinal);
                            results.Add(item);
                        }
                    }
                }
            }

            return results;
        }
        #endregion

        #region Service
        public void Add(Service item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO service(service_id, assignment_id, service_name, protocol, ttl, priority, weight, port, hostname)
                                            VALUES ( :service_id, :assignment_id, :service_name, :protocol, :ttl, :priority, :weight, :port, :hostname);";

                    command.AddParameter(DbType.Guid, "service_id", item.ServiceId);
                    command.AddParameter(DbType.Guid, "assignment_id", item.AssignmentId);
                    command.AddParameter(DbType.String, "service_name", item.ServiceName);
                    command.AddParameter(DbType.String, "protocol", item.Protocol);
                    command.AddParameter(DbType.Int32, "ttl", item.TimeToLive);
                    command.AddParameter(DbType.Int32, "priority", item.Priority);
                    command.AddParameter(DbType.Int32, "weight", item.Weight);
                    command.AddParameter(DbType.Int32, "port", item.Port);
                    command.AddParameter(DbType.String, "hostname", item.Hostname);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(Service item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE service
                               SET assignment_id=:assignment_id, service_name=:service_name, protocol=:protocol, ttl=:ttl, 
                                   priority=:priority, weight=:weight, port=:port, hostname=:hostname
                             WHERE service_id=:service_id;";

                    command.AddParameter(DbType.Guid, "service_id", item.ServiceId);
                    command.AddParameter(DbType.Guid, "assignment_id", item.AssignmentId);
                    command.AddParameter(DbType.String, "service_name", item.ServiceName);
                    command.AddParameter(DbType.String, "protocol", item.Protocol);
                    command.AddParameter(DbType.Int32, "ttl", item.TimeToLive);
                    command.AddParameter(DbType.Int32, "priority", item.Priority);
                    command.AddParameter(DbType.Int32, "weight", item.Weight);
                    command.AddParameter(DbType.Int32, "port", item.Port);
                    command.AddParameter(DbType.String, "hostname", item.Hostname);

                    command.ExecuteNonQuery();
                }
            }
            RecordAction("update");
        }
        public void Remove(Service item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM service
                             WHERE service_id=:service_id;";

                    command.AddParameter(DbType.Guid, "service_id", item.ServiceId);
                    command.ExecuteNonQuery();
                }
            }
            RecordAction("update");
        }
        public Service GetService(Guid serviceId) 
        {
            Collection<Service> results = GetServices(serviceId: serviceId);
            return results.FirstOrDefault();
        }
        public Collection<Service> GetServicesForAssignment(Guid assignmentId)
        {
            Collection<Service> results = GetServices(assignmentId: assignmentId);
            return results;
        }

        private Collection<Service> GetServices(Guid serviceId = default(Guid), Guid assignmentId = default(Guid))
        {
            Collection<Service> results = new Collection<Service>();

            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT * FROM service";

                    Collection<string> where = new Collection<string>();

                    if (serviceId != default(Guid))
                    {
                        where.Add("service_id=:service_id");
                        command.AddParameter(DbType.Guid, "service_id", serviceId);
                    }
                    if (assignmentId != default(Guid))
                    {
                        where.Add("assignment_id=:assignment_id");
                        command.AddParameter(DbType.Guid, "assignment_id", assignmentId);
                    }

                    if (where.Count != 0)
                    {
                        command.CommandText += " WHERE ";
                        command.CommandText += string.Join(" AND ", where);
                    }

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Service item = new Service();
                            int ordinal = reader.GetOrdinal("service_id");
                            item.ServiceId = reader.GetGuid(ordinal);

                            ordinal = reader.GetOrdinal("assignment_id");
                            item.AssignmentId = reader.GetGuid(ordinal);

                            ordinal = reader.GetOrdinal("service_name");
                            item.ServiceName = reader.GetString(ordinal);

                            ordinal = reader.GetOrdinal("protocol");
                            item.Protocol = reader.GetString(ordinal);

                            ordinal = reader.GetOrdinal("ttl");
                            item.TimeToLive = reader.GetInt32(ordinal);

                            ordinal = reader.GetOrdinal("priority");
                            item.Priority = reader.GetInt32(ordinal);

                            ordinal = reader.GetOrdinal("weight");
                            item.Weight = reader.GetInt32(ordinal);

                            ordinal = reader.GetOrdinal("port");
                            item.Port = reader.GetInt32(ordinal);

                            ordinal = reader.GetOrdinal("hostname");
                            item.Hostname = reader.GetString(ordinal);
                            results.Add(item);
                        }
                    }
                }
            }

            return results;
        }
        #endregion

        #region Agency
        public void Add(Agency item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO agency(
                            agency_id, username, date_created, date_approved, last_modified, 
                            approval_state, agency_name, technical_id, admin_id)
                    VALUES (:agency_id, :username, :date_created, :date_approved, :last_modified, 
                            :approval_state, :agency_name, :technical_id, :admin_id);";

                    command.AddParameter(DbType.Guid, "agency_id", item.AgencyId);
                    command.AddParameter(DbType.String, "username", item.Username);
                    command.AddParameter(DbType.DateTime, "date_created", item.DateCreated);
                    command.AddParameter(DbType.DateTime, "date_approved", item.DateApproved);
                    command.AddParameter(DbType.DateTime, "last_modified", item.LastModified);
                    command.AddParameter(DbType.Int32, "approval_state", (int)item.ApprovalState);
                    command.AddParameter(DbType.String, "agency_name", item.AgencyName);
                    command.AddParameter(DbType.Guid, "technical_id", item.TechnicalContactId);
                    command.AddParameter(DbType.Guid, "admin_id", item.AdminContactId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(Agency item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE agency
                               SET username=:username, date_created=:date_created, date_approved=:date_approved, last_modified=:last_modified, 
                                   approval_state=:approval_state, agency_name=:agency_name, technical_id=:technical_id, admin_id=:admin_id
                             WHERE agency_id=:agency_id";

                    command.AddParameter(DbType.Guid, "agency_id", item.AgencyId);
                    command.AddParameter(DbType.String, "username", item.Username);
                    command.AddParameter(DbType.DateTime, "date_created", item.DateCreated);
                    command.AddParameter(DbType.DateTime, "date_approved", item.DateApproved);
                    command.AddParameter(DbType.DateTime, "last_modified", item.LastModified);
                    command.AddParameter(DbType.Int32, "approval_state", (int)item.ApprovalState);
                    command.AddParameter(DbType.String, "agency_name", item.AgencyName);
                    command.AddParameter(DbType.Guid, "technical_id", item.TechnicalContactId);
                    command.AddParameter(DbType.Guid, "admin_id", item.AdminContactId);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void Remove(Agency item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM agency
                                 WHERE agency_id=:agency_id";

                    command.AddParameter(DbType.Guid, "agency_id", item.AgencyId);
                    command.ExecuteNonQuery();
                }
            }
        }
        public Agency GetAgency(Guid agencyId) 
        {
            Collection<Agency> results = GetAgencies(agencyId: agencyId);
            return results.FirstOrDefault();
        }
        public Agency GetAgency(String agencyName)
        {
            Collection<Agency> results = GetAgencies(agencyName: agencyName);
            return results.FirstOrDefault();
        }
        public Collection<Agency> GetAgenciesByApprovalState(ApprovalState state)
        {
            Collection<Agency> results = GetAgencies(state: state);
            return results;
        }
        public Collection<Agency> GetAgenciesForUser(string user)
        {
            Collection<Agency> results = GetAgencies(user: user);
            return results;
        }
        public Collection<Agency> GetAgencies(Guid agencyId = default(Guid), string user = null, string agencyName = null, string partialName = null, ApprovalState state = ApprovalState.None)
        {
            Collection<Agency> results = new Collection<Agency>();

            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT * FROM agency";

                    Collection<string> where = new Collection<string>();

                    if (agencyId != default(Guid))
                    {
                        where.Add("agency_id=:agency_id");
                        command.AddParameter(DbType.Guid, "agency_id", agencyId);
                    }
                    if (user != null)
                    {
                        where.Add("username=:username");
                        command.AddParameter(DbType.String, "username", user);
                    }
                    if (agencyName != null)
                    {
                        where.Add("agency_name=:agency_name");
                        command.AddParameter(DbType.String, "agency_name", agencyName);
                    }
                    if (partialName != null)
                    {
                        partialName = "%" + partialName.ToLowerInvariant() + "%";
                        where.Add("agency_name LIKE :pagency_name");
                        command.AddParameter(DbType.String, "pagency_name", partialName);
                    }
                    if (state != ApprovalState.None)
                    {
                        where.Add("approval_state=:approval_state");
                        command.AddParameter(DbType.Int32, "approval_state", (int)state);
                    }

                    if (where.Count != 0)
                    {
                        command.CommandText += " WHERE ";
                        command.CommandText += string.Join(" AND ", where);
                    }

                    command.CommandText += " ORDER BY agency_name ASC ";

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Agency item = new Agency();
                            int ordinal = reader.GetOrdinal("agency_id");
                            item.AgencyId = reader.GetGuid(ordinal);

                            ordinal = reader.GetOrdinal("username");
                            if (!reader.IsDBNull(ordinal))
                            {
                                item.Username = reader.GetString(ordinal);
                            }

                            ordinal = reader.GetOrdinal("date_created");
                            item.DateCreated = reader.GetDateTime(ordinal);

                            ordinal = reader.GetOrdinal("date_approved");
                            if (!reader.IsDBNull(ordinal))
                            {
                                item.DateApproved = reader.GetDateTime(ordinal);
                            }

                            ordinal = reader.GetOrdinal("last_modified");
                            item.LastModified = reader.GetDateTime(ordinal);

                            ordinal = reader.GetOrdinal("approval_state");
                            item.ApprovalState = (ApprovalState)reader.GetInt32(ordinal);

                            ordinal = reader.GetOrdinal("agency_name");
                            item.AgencyName = reader.GetString(ordinal);

                            ordinal = reader.GetOrdinal("technical_id");
                            if (!reader.IsDBNull(ordinal))
                            {
                                item.TechnicalContactId = reader.GetGuid(ordinal);
                            }

                            ordinal = reader.GetOrdinal("admin_id");
                            if (!reader.IsDBNull(ordinal))
                            {
                                item.AdminContactId = reader.GetGuid(ordinal);
                            }


                            results.Add(item);
                        }
                    }
                }
            }

            return results;
        }
        #endregion

        #region Assignment
        public void Add(Assignment item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO \"assignment\"(assignment_id, agency_id, date_created, last_modified, assignment_name, delegated) ";
                    command.CommandText += @"VALUES (:assignment_id, :agency_id, :date_created, :last_modified, :assignment_name, :delegated);";

                    command.AddParameter(DbType.Guid, "assignment_id", item.AssignmentId);
                    command.AddParameter(DbType.Guid, "agency_id", item.AgencyId);
                    command.AddParameter(DbType.DateTime, "date_created", item.DateCreated);
                    command.AddParameter(DbType.DateTime, "last_modified", item.LastModified);
                    command.AddParameter(DbType.String, "assignment_name", item.Name);
                    command.AddParameter(DbType.Boolean, "delegated", item.IsDelegated);

                    command.ExecuteNonQuery();
                }
            }
            RecordAction("update");
        }

        public void Update(Assignment item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE \"assignment\" SET agency_id=:agency_id, date_created=:date_created, last_modified=:last_modified, assignment_name=:assignment_name, delegated=:delegated ";
                    command.CommandText += "WHERE assignment_id=:assignment_id;";

                    command.AddParameter(DbType.Guid, "assignment_id", item.AssignmentId);
                    command.AddParameter(DbType.Guid, "agency_id", item.AgencyId);
                    command.AddParameter(DbType.DateTime, "date_created", item.DateCreated);
                    command.AddParameter(DbType.DateTime, "last_modified", item.LastModified);
                    command.AddParameter(DbType.String, "assignment_name", item.Name);
                    command.AddParameter(DbType.Boolean, "delegated", item.IsDelegated);

                    command.ExecuteNonQuery();
                }
            }
            RecordAction("update");
        }
        public void Remove(Assignment item)
        {
            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM \"assignment\" ";
                    command.CommandText += "WHERE assignment_id=:assignment_id;";

                    command.AddParameter(DbType.Guid, "assignment_id", item.AssignmentId);
                    command.ExecuteNonQuery();
                }
            }
            RecordAction("update");
        }
        public Assignment GetAssignment(Guid assignmentId)
        {
            Collection<Assignment> results = GetAssignments(assignmentId: assignmentId);
            return results.FirstOrDefault();
        }
        public Assignment GetAssignment(string assignmentName)
        {
            Collection<Assignment> results = GetAssignments(assignmentName: assignmentName);
            return results.FirstOrDefault();
        }
        public Collection<Assignment> GetAssignmentsForAgency(Guid agencyId)
        {
            Collection<Assignment> results = GetAssignments(agencyId: agencyId);
            return results;
        }
        private Collection<Assignment> GetAssignments(Guid assignmentId = default(Guid), Guid agencyId = default(Guid), string assignmentName = null)
        {
            Collection<Assignment> results = new Collection<Assignment>();

            using (DbConnection connection = DataAccess.GetDbConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM \"assignment\"";

                    Collection<string> where = new Collection<string>();

                    if (assignmentId != default(Guid))
                    {
                        where.Add("assignment_id=:assignment_id");
                        command.AddParameter(DbType.Guid, "assignment_id", assignmentId);
                    }
                    if (agencyId != default(Guid))
                    {
                        where.Add("agency_id=:agency_id");
                        command.AddParameter(DbType.Guid, "agency_id", agencyId);
                    }
                    if (assignmentName != null)
                    {
                        where.Add("assignment_name=:assignment_name");
                        command.AddParameter(DbType.String, "assignment_name", assignmentName);
                    }
                    if (where.Count != 0)
                    {
                        command.CommandText += " WHERE ";
                        command.CommandText += string.Join(" AND ", where);
                    }

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Assignment item = new Assignment();
                            int ordinal = reader.GetOrdinal("assignment_id");
                            item.AssignmentId = reader.GetGuid(ordinal);

                            ordinal = reader.GetOrdinal("agency_id");
                            item.AgencyId = reader.GetGuid(ordinal);
                            
                            ordinal = reader.GetOrdinal("date_created");
                            item.DateCreated = reader.GetDateTime(ordinal);
                            
                            ordinal = reader.GetOrdinal("last_modified");
                            item.LastModified = reader.GetDateTime(ordinal);

                            ordinal = reader.GetOrdinal("assignment_name");
                            item.Name = reader.GetString(ordinal);

                            ordinal = reader.GetOrdinal("delegated");
                            item.IsDelegated = reader.GetBoolean(ordinal);

                            results.Add(item);
                        }
                    }
                }
            }

            return results;
        }

        #endregion

        #region Security Checks

        public bool ManagesPerson(string username, Guid personId)
        {
            Person person = GetPerson(personId);
            if (person != null && person.Username == username)
            {
                return true;
            }
            return false;
        }

        public bool ManagesDelegation(string username, Guid delegationId)
        {
            Delegation delegation = GetDelegation(delegationId);
            if (delegation == null) { return false; }

            Assignment assignment = GetAssignment(delegation.AssignmentId);
            if (assignment != null)
            {
                return ManagesAssignment(username, assignment.AssignmentId);
            }
            return false;
        }

        public bool ManagesService(string username, Guid serviceId)
        {
            Service service = GetService(serviceId);
            if (service == null) { return false; }

            Assignment assignment = GetAssignment(service.AssignmentId);
            if (assignment != null)
            {
                return ManagesAssignment(username, assignment.AssignmentId);
            }
            return false;
        }

        public bool ManagesAgency(string username, Guid agencyId)
        {
            Agency agency = GetAgency(agencyId);
            if (agency != null && agency.Username == username)
            {
                return true;
            }
            return false;
        }

        public bool ManagesAssignment(string username, Guid assignmentId)
        {
            Assignment assignment = GetAssignment(assignmentId);
            if (assignment == null) { return false; }

            Agency agency = GetAgency(assignment.AgencyId);
            if (agency != null && agency.Username == username)
            {
                return true;
            }
            return false;
        }



        #endregion
    }
}
