using Ddi.Registry.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;

namespace Ddi.Registry.Data.Test
{
    
    
    /// <summary>
    ///This is a test class for RegistryProviderTest and is intended
    ///to contain all RegistryProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RegistryProviderTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }       


        #region Mock objects
        int i = 0;
        private Agency GetAgency()
        {
            return new Agency()
            {
                AgencyName = "example.org" + i++
            };
        }

        private Person GetPerson()
        {
            return new Person()
            {
                Username = "testuser",
                Name = "Jane Doe",
                Email = "jane@example.org"
            };
        }

        private Service GetService()
        {
            return new Service()
            {
                AssignmentId = Guid.NewGuid(),
                ServiceName = "colectica",
                Protocol = "tcp",
                TimeToLive = 86400,
                Priority = 0,
                Weight = 5,
                Port = 19893,
                Hostname = "colectica.example.org"
            };
        }

        private Delegation GetDelegation()
        {
            return new Delegation()
            {
                AssignmentId = Guid.NewGuid(),
                NameServer = "ns.example.org"
            };
        }

        private Assignment GetAssignment()
        {
            return new Assignment()
            {
                AgencyId = Guid.NewGuid(),
                Name = "demo.example.org",
                IsDelegated = true
            };
        }
        #endregion

        /// <summary>
        ///A test for Remove
        ///</summary>
        [TestMethod()]
        public void RemovePersonTest()
        {
            RegistryProvider target = new RegistryProvider();
            Person item = GetPerson();
            target.Add(item);

            Person person = target.GetPerson(item.PersonId);
            target.Remove(person);

            Assert.IsNull(target.GetPerson(person.PersonId));
        }

        /// <summary>
        ///A test for Remove
        ///</summary>
        [TestMethod()]
        public void RemoveAgencyTest()
        {
            RegistryProvider target = new RegistryProvider();
            Agency item = GetAgency();            
            target.Add(item);

            Agency result = target.GetAgency(item.AgencyId);
            target.Remove(result);

            Assert.IsNull(target.GetAgency(result.AgencyId));
        }

        /// <summary>
        ///A test for Remove
        ///</summary>
        [TestMethod()]
        public void RemoveServiceTest()
        {
            RegistryProvider target = new RegistryProvider();
            Service item = GetService();
            target.Add(item);

            Service result = target.GetService(item.ServiceId);
            target.Remove(result);

            Assert.IsNull(target.GetService(result.ServiceId));
        }

        /// <summary>
        ///A test for Remove
        ///</summary>
        [TestMethod()]
        public void RemoveAssignmentTest()
        {
            RegistryProvider target = new RegistryProvider();
            Assignment item = GetAssignment();
            target.Add(item);

            Assignment result = target.GetAssignment(item.AssignmentId);
            target.Remove(result);

            Assert.IsNull(target.GetAssignment(result.AssignmentId));
        }

        /// <summary>
        ///A test for Remove
        ///</summary>
        [TestMethod()]
        public void RemoveDelegationTest()
        {
            RegistryProvider target = new RegistryProvider();
            Delegation item = GetDelegation();
            target.Add(item);

            Delegation result = target.GetDelegation(item.DelegationId);
            target.Remove(result);

            Assert.IsNull(target.GetDelegation(result.DelegationId));
        }


        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateAgencyTest()
        {
            RegistryProvider target = new RegistryProvider();
            Agency item = GetAgency();
            target.Add(item);

            Agency toUpdate = target.GetAgency(item.AgencyId);
            toUpdate.ApprovalState = ApprovalState.Approved;
            target.Update(toUpdate);

            Agency result = target.GetAgency(item.AgencyId);
            Assert.AreEqual(toUpdate.ApprovalState, result.ApprovalState);

            target.Remove(result);
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateServiceTest()
        {
            RegistryProvider target = new RegistryProvider(); // TODO: Initialize to an appropriate value
            Service item = GetService();
            target.Add(item);

            Service toUpdate = target.GetService(item.ServiceId);
            toUpdate.Hostname = "new.host.example.org";
            target.Update(toUpdate);

            Service result = target.GetService(item.ServiceId);
            Assert.AreEqual(toUpdate.Hostname, result.Hostname);

            target.Remove(result);
        }

        /// <summary>
        ///A test for GetServicesForAssignment
        ///</summary>
        [TestMethod()]
        public void GetServicesForAssignmentTest()
        {
            RegistryProvider target = new RegistryProvider();
            Assignment assignment = GetAssignment();
            Service s1 = GetService();
            Service s2 = GetService();
            Service s3 = GetService();
            Collection<Service> expected = new Collection<Service>() { s1, s2, s3 };
            s1.AssignmentId = s2.AssignmentId = s3.AssignmentId = assignment.AssignmentId;
            target.Add(assignment);
            foreach (Service s in expected) { target.Add(s); }

            Service s4 = GetService();
            target.Add(s4);

            Collection<Service> actual;
            actual = target.GetServicesForAssignment(assignment.AssignmentId);

            Collection<Guid> actualIds = new Collection<Guid>();
            foreach (Service s in actual) { actualIds.Add(s.ServiceId); }

            Collection<Guid> expectedIds = new Collection<Guid>();
            foreach (Service s in expected) { expectedIds.Add(s.ServiceId); }

            CollectionAssert.AreEquivalent(actualIds, expectedIds);

            target.Remove(assignment);
            target.Remove(s4);
            foreach (Service s in expected) { target.Remove(s); }
        }

        /// <summary>
        ///A test for GetService
        ///</summary>
        [TestMethod()]
        public void GetServiceTest()
        {
            RegistryProvider target = new RegistryProvider();

            Service expected = GetService();
            target.Add(expected);

            Service actual = target.GetService(expected.ServiceId);
            Assert.AreEqual(expected.ServiceId, actual.ServiceId);

            target.Remove(expected);
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateAssignmentTest()
        {
            RegistryProvider target = new RegistryProvider(); // TODO: Initialize to an appropriate value
            Assignment item = GetAssignment();
            target.Add(item);

            Assignment toUpdate = target.GetAssignment(item.AssignmentId);
            toUpdate.Name = "new.example.org";
            target.Update(toUpdate);

            Assignment result = target.GetAssignment(item.AssignmentId);
            Assert.AreEqual(toUpdate.Name, result.Name);

            target.Remove(result);
        }

        /// <summary>
        ///A test for GetPerson
        ///</summary>
        [TestMethod()]
        public void GetPersonTest()
        {
            RegistryProvider target = new RegistryProvider(); // TODO: Initialize to an appropriate value
            Person expected = GetPerson();
            target.Add(expected);

            Person actual = target.GetPerson(expected.PersonId);
            Assert.AreEqual(expected.PersonId, actual.PersonId);

            target.Remove(expected);
        }

        /// <summary>
        ///A test for GetPeopleForAgency
        ///</summary>
        [TestMethod()]
        public void GetPeopleForUserTest()
        {
            RegistryProvider target = new RegistryProvider();
            string username = "test";
            Person s1 = GetPerson();
            Person s2 = GetPerson();
            Person s3 = GetPerson();
            Collection<Person> expected = new Collection<Person>() { s1, s2, s3 };
            s1.Username = s2.Username = s3.Username = username;
            foreach (Person s in expected) { target.Add(s); }

            Person s4 = GetPerson();
            target.Add(s4);

            Collection<Person> actual;
            actual = target.GetPeopleForUser(username);

            Collection<Guid> actualIds = new Collection<Guid>();
            foreach (Person s in actual) { actualIds.Add(s.PersonId); }

            Collection<Guid> expectedIds = new Collection<Guid>();
            foreach (Person s in expected) { expectedIds.Add(s.PersonId); }

            CollectionAssert.AreEquivalent(actualIds, expectedIds);

            target.Remove(s4);
            foreach (Person s in expected) { target.Remove(s); }
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdatePersonTest()
        {
            RegistryProvider target = new RegistryProvider(); // TODO: Initialize to an appropriate value
            Person item = GetPerson();
            target.Add(item);

            Person toUpdate = target.GetPerson(item.PersonId);
            toUpdate.Name = "John Doe";
            target.Update(toUpdate);

            Person result = target.GetPerson(item.PersonId);
            Assert.AreEqual(toUpdate.Name, result.Name);

            target.Remove(result);
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateDelegationTest()
        {
            RegistryProvider target = new RegistryProvider(); 
            Delegation item = GetDelegation();
            target.Add(item);

            Delegation toUpdate = target.GetDelegation(item.DelegationId);
            toUpdate.NameServer = "new.example.org";
            target.Update(toUpdate);

            Delegation result = target.GetDelegation(item.DelegationId);
            Assert.AreEqual(toUpdate.NameServer, result.NameServer);

            target.Remove(result);
        }

        /// <summary>
        ///A test for GetDelegationsForAssignment
        ///</summary>
        [TestMethod()]
        public void GetDelegationsForAssignmentTest()
        {
            RegistryProvider target = new RegistryProvider();
            Assignment agency = GetAssignment();
            Delegation s1 = GetDelegation();
            Delegation s2 = GetDelegation();
            Delegation s3 = GetDelegation();
            Collection<Delegation> expected = new Collection<Delegation>() { s1, s2, s3 };
            s1.AssignmentId = s2.AssignmentId = s3.AssignmentId = agency.AssignmentId;
            target.Add(agency);
            foreach (Delegation s in expected) { target.Add(s); }

            Delegation s4 = GetDelegation();
            target.Add(s4);

            Collection<Delegation> actual;
            actual = target.GetDelegationsForAssignment(agency.AssignmentId);

            Collection<Guid> actualIds = new Collection<Guid>();
            foreach (Delegation s in actual) { actualIds.Add(s.DelegationId); }

            Collection<Guid> expectedIds = new Collection<Guid>();
            foreach (Delegation s in expected) { expectedIds.Add(s.DelegationId); }

            CollectionAssert.AreEquivalent(actualIds, expectedIds);

            target.Remove(agency);
            target.Remove(s4);
            foreach (Delegation s in expected) { target.Remove(s); }
        }

        /// <summary>
        ///A test for GetDelegation
        ///</summary>
        [TestMethod()]
        public void GetDelegationTest()
        {
            RegistryProvider target = new RegistryProvider();
            Delegation expected = GetDelegation();
            target.Add(expected);

            Delegation actual = target.GetDelegation(expected.DelegationId);
            Assert.AreEqual(expected.DelegationId, actual.DelegationId);

            target.Remove(expected);
        }

        /// <summary>
        ///A test for GetAssignmentsForAgency
        ///</summary>
        [TestMethod()]
        public void GetAssignmentsForAgencyTest()
        {
            RegistryProvider target = new RegistryProvider();
            Agency agency = GetAgency();
            Assignment s1 = GetAssignment();
            Assignment s2 = GetAssignment();
            Assignment s3 = GetAssignment();
            Collection<Assignment> expected = new Collection<Assignment>() { s1, s2, s3 };
            s1.AgencyId = s2.AgencyId = s3.AgencyId = agency.AgencyId;
            target.Add(agency);
            foreach (Assignment s in expected) { target.Add(s); }

            Assignment s4 = GetAssignment();
            target.Add(s4);

            Collection<Assignment> actual;
            actual = target.GetAssignmentsForAgency(agency.AgencyId);

            Collection<Guid> actualIds = new Collection<Guid>();
            foreach (Assignment s in actual) { actualIds.Add(s.AssignmentId); }

            Collection<Guid> expectedIds = new Collection<Guid>();
            foreach (Assignment s in expected) { expectedIds.Add(s.AssignmentId); }

            CollectionAssert.AreEquivalent(actualIds, expectedIds);

            target.Remove(agency);
            target.Remove(s4);
            foreach (Assignment s in expected) { target.Remove(s); }
        }

        /// <summary>
        ///A test for GetAssignment
        ///</summary>
        [TestMethod()]
        public void GetAssignmentTest()
        {
            RegistryProvider target = new RegistryProvider();
            Assignment expected = GetAssignment();
            target.Add(expected);

            Assignment actual = target.GetAssignment(expected.AssignmentId);
            Assert.AreEqual(expected.AssignmentId, actual.AssignmentId);

            target.Remove(expected);
        }

        /// <summary>
        ///A test for GetAgency
        ///</summary>
        [TestMethod()]
        public void GetAgencyTest()
        {
            RegistryProvider target = new RegistryProvider();
            Agency expected = GetAgency();
            target.Add(expected);

            Agency actual = target.GetAgency(expected.AgencyId);
            Assert.AreEqual(expected.AgencyId, actual.AgencyId);

            target.Remove(expected);
        }

        /// <summary>
        ///A test for GetAgenciesForUser
        ///</summary>
        [TestMethod()]
        public void GetAgenciesForUserTest()
        {
            RegistryProvider target = new RegistryProvider();
            string username = "demo";
            Agency s1 = GetAgency();
            Agency s2 = GetAgency();
            Agency s3 = GetAgency();
            Collection<Agency> expected = new Collection<Agency>() { s1, s2, s3 };
            s1.Username = s2.Username = s3.Username = username;
            foreach (Agency s in expected) { target.Add(s); }

            Agency s4 = GetAgency();
            target.Add(s4);

            Collection<Agency> actual;
            actual = target.GetAgenciesForUser(username);

            Collection<Guid> actualIds = new Collection<Guid>();
            foreach (Agency s in actual) { actualIds.Add(s.AgencyId); }

            Collection<Guid> expectedIds = new Collection<Guid>();
            foreach (Agency s in expected) { expectedIds.Add(s.AgencyId); }

            CollectionAssert.AreEquivalent(actualIds, expectedIds);

            target.Remove(s4);
            foreach (Agency s in expected) { target.Remove(s); }
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void AddAgencyTest()
        {
            RegistryProvider target = new RegistryProvider();
            Agency item = GetAgency();
            target.Add(item);

            Agency result = target.GetAgency(item.AgencyId);
            Assert.AreEqual(item.AgencyId, result.AgencyId);

            target.Remove(result);
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void AddAssignmentTest()
        {
            RegistryProvider target = new RegistryProvider();
            Assignment item = GetAssignment();
            target.Add(item);

            Assignment result = target.GetAssignment(item.AssignmentId);
            Assert.AreEqual(item.AssignmentId, result.AssignmentId);

            target.Remove(result);
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void AddPersonTest()
        {
            RegistryProvider target = new RegistryProvider();
            Person item = GetPerson();
            target.Add(item);

            Person result = target.GetPerson(item.PersonId);
            Assert.AreEqual(item.PersonId, result.PersonId);

            target.Remove(result);
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void AddDelegationTest()
        {
            RegistryProvider target = new RegistryProvider();
            Delegation item = GetDelegation();
            target.Add(item);

            Delegation result = target.GetDelegation(item.DelegationId);
            Assert.AreEqual(item.DelegationId, result.DelegationId);

            target.Remove(result);
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void AddServiceTest()
        {
            RegistryProvider target = new RegistryProvider();
            Service item = GetService();
            target.Add(item);

            Service result = target.GetService(item.ServiceId);
            Assert.AreEqual(item.ServiceId, result.ServiceId);

            target.Remove(result);
        }
    }
}
