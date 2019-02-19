-- Copy membership users to new identity users
INSERT INTO "AspNetUsers"(
            "Id", 
            "UserName", 
            "NormalizedUserName", 
            "Email", 
            "NormalizedEmail", 
            "EmailConfirmed", 
            "PhoneNumberConfirmed", 
            "TwoFactorEnabled", 
            "LockoutEnabled", 
            "AccessFailedCount")
SELECT "pId", "Email", upper("Email"), "Email", upper("Email"), true , false, false, true, 0 FROM "Users";

-- update invalid records
UPDATE person set person_name='Tony Brown', email='toneybrown738@yahoo.com' where person_id='2454c03e-4694-4624-ae43-d8c2c80ffe0e';
UPDATE agency set username='John Shepherdson' where agency_name='uk.ukdataarchive';
-- delete duplicate wendy's in persons
DELETE FROM person where person_id IN ('bc60bb2b-7b04-4e24-971a-48f0190d5a5d', '47d76adb-988a-482d-a81a-8495b731264e');

-- copy person records that are not users to users (~23 new users)
INSERT INTO "AspNetUsers"(
            "Id", 
            "UserName", 
            "NormalizedUserName", 
            "Email", 
            "NormalizedEmail", 
            "EmailConfirmed", 
            "PhoneNumberConfirmed", 
            "TwoFactorEnabled", 
            "LockoutEnabled", 
            "AccessFailedCount")
SELECT person_id, email, upper(email), email, upper(email), true , false, false, true, 0 FROM person m where not exists
 (SELECT 1 from "AspNetUsers" i where i."NormalizedUserName"=upper(m.email));

 -- update all identity user records with content from old person records
 UPDATE "AspNetUsers" 
SET 
       "PhoneNumber"=m.phone, 
       "Name"=m.person_name, 
       "Organization"=m.organization, 
       "JobTitle"=m.job_title, 
       "StreetAddress"=m.street_address, 
       "City"=m.city, 
       "State"=m.state, 
       "Zip"=m.zip, 
       "Country"=m.country, 
       "HomePage"=m.homepage
 FROM person m     
 WHERE "AspNetUsers"."NormalizedUserName"=upper(m.email);

 -- copy current agency ids
 INSERT INTO "Agencies"(
            "AgencyId", 
            "DateCreated", 
            "LastModified", 
            "DateApproved", 
            "ApprovalState")
SELECT lower(agency_name), date_created, last_modified, date_approved, approval_state FROM agency;

-- set owner
UPDATE "Agencies" a SET 
            "CreatorId"=(SELECT "AspNetUsers"."Id" FROM "AspNetUsers" WHERE "NormalizedUserName"=
		(SELECT upper("Users"."Email") FROM "Users", agency where agency.username="Users"."Username" AND lower(agency.agency_name)=a."AgencyId" ));

-- set technical, and admin contacts
UPDATE "Agencies" a SET 
	"TechnicalContactId"=(SELECT "AspNetUsers"."Id" FROM "AspNetUsers" WHERE "NormalizedUserName"=
		(SELECT upper(person.email) FROM person, agency where agency.technical_id=person_id AND lower(agency.agency_name)=a."AgencyId" )),
	"AdminContactId"=(SELECT "AspNetUsers"."Id" FROM "AspNetUsers" WHERE "NormalizedUserName"=
		(SELECT upper(person.email) FROM person, agency where agency.admin_id=person_id AND lower(agency.agency_name)=a."AgencyId" ));

-- copy sub agency assignments
INSERT INTO "Assignments"("AssignmentId", "DateCreated", "LastModified", "AgencyId", "IsDelegated")
	SELECT lower(assignment_name), a.date_created, a.last_modified, lower(agency_name) , delegated FROM assignment a, agency
	WHERE a.agency_id=agency.agency_id;


-- set the agency label to the prior owners organization
UPDATE "Agencies" a SET "Label"=(SELECT "Organization" FROM "AspNetUsers" WHERE "Id"=a."CreatorId");

-- no delegations are present, skip for now in migration

-- export actions can be recreated

-- no real active services


-- update security tokens
UPDATE "AspNetUsers" SET "SecurityStamp"=md5(random()::text);

