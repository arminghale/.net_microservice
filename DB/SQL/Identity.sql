CREATE TABLE IF NOT EXISTS "Domain" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"Title" TEXT NOT NULL,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_Domain_Id
ON "Domain" ("Id");
----------------------------
CREATE TABLE IF NOT EXISTS "DomainValue" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"DomainId" int NOT NULL,
	CONSTRAINT FK_DomainValue_Domain_DomainId FOREIGN KEY ("DomainId")     
    		REFERENCES "Domain" ("Id") ON DELETE CASCADE,
	"Value" TEXT NOT NULL,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_DomainValue_Id
ON "DomainValue" ("Id");
CREATE INDEX IF NOT EXISTS IX_DomainValue_DomainId
ON "DomainValue" ("DomainId");
-----------------------
CREATE TABLE IF NOT EXISTS "SubDomainValue" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"ParentId" int NOT NULL,
	CONSTRAINT FK_SubDomainValue_DomainValue_ParentId FOREIGN KEY ("ParentId")     
    		REFERENCES "DomainValue" ("Id") ON DELETE CASCADE,
	"ChildId" int NOT NULL,
	CONSTRAINT FK_SubDomainValue_DomainValue_ChildId FOREIGN KEY ("ChildId")     
    		REFERENCES "DomainValue" ("Id")
);
CREATE INDEX IF NOT EXISTS IX_SubDomainValue_Id
ON "SubDomainValue" ("Id");
CREATE INDEX IF NOT EXISTS IX_SubDomainValue_ParentId
ON "SubDomainValue" ("ParentId");
CREATE INDEX IF NOT EXISTS IX_SubDomainValue_ChildId
ON "SubDomainValue" ("ChildId");
--------------------------------------
CREATE TABLE IF NOT EXISTS "Service" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"Title" TEXT NOT NULL,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_Service_Id
ON "Service" ("Id");
--------------------------------------
CREATE TABLE IF NOT EXISTS "ActionGroup" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"ServiceId" int NOT NULL,
	CONSTRAINT FK_ActionGroup_Service_ServiceId FOREIGN KEY ("ServiceId")     
    		REFERENCES "Service" ("Id") ON DELETE CASCADE,
	"Title" TEXT NOT NULL,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_ActionGroup_Id
ON "ActionGroup" ("Id");
CREATE INDEX IF NOT EXISTS IX_ActionGroup_ServiceId
ON "ActionGroup" ("ServiceId");
--------------------------------------
CREATE TABLE IF NOT EXISTS "Action" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"ActionGroupId" int NOT NULL,
	CONSTRAINT FK_Action_ActionGroup_ActionGroupId FOREIGN KEY ("ActionGroupId")     
    		REFERENCES "ActionGroup" ("Id") ON DELETE CASCADE,
	"Title" TEXT NOT NULL,
	"URL" TEXT NOT NULL,
	"Type" TEXT NOT NULL,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_Action_Id
ON "Action" ("Id");
CREATE INDEX IF NOT EXISTS IX_Action_ActionGroupId
ON "Action" ("ActionGroupId");
--------------------------------------
CREATE TABLE IF NOT EXISTS "Tenant" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"Title" TEXT NOT NULL,
	"AdminRoles" TEXT DEFAULT '',
	"RegisterRoles" TEXT DEFAULT '',
	"Additional" TEXT,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_Tenant_Id
ON "Tenant" ("Id");
--------------------------------------
CREATE TABLE IF NOT EXISTS "Role" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"Title" TEXT NOT NULL,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_Role_Id
ON "Role" ("Id");
----------------------------------------
CREATE TABLE IF NOT EXISTS "User" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"ParentId" int,
	CONSTRAINT FK_User_User_ParentId FOREIGN KEY ("ParentId")     
    		REFERENCES "User" ("Id") ON DELETE CASCADE,
	"Username" TEXT,
	"Password" TEXT,
	"Token" TEXT,
	"Email" TEXT,
	"EmailValidation" TEXT DEFAULT 'NO',
	"Phonenumber" TEXT,
	"PhonenumberValidation" TEXT DEFAULT 'NO',
	"Validation" TEXT DEFAULT 'NO',
	"RefrenceId" TEXT,
	"ProfilePic" TEXT,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW(),
	"LastLoginDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_User_Id
ON "User" ("Id");
--------------------------------------
CREATE TABLE IF NOT EXISTS "UserRole" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"RoleId" int NOT NULL,
	CONSTRAINT FK_UserRole_Role_RoleId FOREIGN KEY ("RoleId")     
    		REFERENCES "Role" ("Id") ,
	"UserId" int NOT NULL,
	CONSTRAINT FK_UserRole_User_UserId FOREIGN KEY ("UserId")     
    		REFERENCES "User" ("Id") ON DELETE CASCADE,
	"TenantId" int,
	CONSTRAINT FK_UserRole_Tenant_TenantId FOREIGN KEY ("TenantId")     
    		REFERENCES "Tenant" ("Id") ,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_UserRole_Id
ON "UserRole" ("Id");
CREATE INDEX IF NOT EXISTS IX_UserRole_TenantId
ON "UserRole" ("TenantId");
CREATE INDEX IF NOT EXISTS IX_UserRole_UserId
ON "UserRole" ("UserId");
CREATE INDEX IF NOT EXISTS IX_UserRole_RoleId
ON "UserRole" ("RoleId");
--------------------------------------
CREATE TABLE IF NOT EXISTS "RACC" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"RoleId" int NOT NULL,
	CONSTRAINT FK_RACC_Role_RoleId FOREIGN KEY ("RoleId")     
    		REFERENCES "Role" ("Id") ,
	"ActionId" int NOT NULL,
	CONSTRAINT FK_RACC_Action_UserId FOREIGN KEY ("ActionId")     
    		REFERENCES "Action" ("Id") ON DELETE CASCADE,
	"TenantId" int,
	CONSTRAINT FK_RACC_Tenant_TenantId FOREIGN KEY ("TenantId")     
    		REFERENCES "Tenant" ("Id") ,
	"type" int NOT NULL,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_RACC_Id
ON "RACC" ("Id");
CREATE INDEX IF NOT EXISTS IX_RACC_TenantId
ON "RACC" ("TenantId");
CREATE INDEX IF NOT EXISTS IX_RACC_ActionId
ON "RACC" ("ActionId");
CREATE INDEX IF NOT EXISTS IX_RACC_RoleId
ON "RACC" ("RoleId");
--------------------------------------
CREATE TABLE IF NOT EXISTS "UACC" (
	"Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
	"UserId" int NOT NULL,
	CONSTRAINT FK_UACC_User_UserId FOREIGN KEY ("UserId")     
    		REFERENCES "User" ("Id") ,
	"ActionId" int NOT NULL,
	CONSTRAINT FK_UACC_Action_UserId FOREIGN KEY ("ActionId")     
    		REFERENCES "Action" ("Id") ON DELETE CASCADE,
	"TenantId" int,
	CONSTRAINT FK_UACC_Tenant_TenantId FOREIGN KEY ("TenantId")     
    		REFERENCES "Tenant" ("Id") ,
	"type" int NOT NULL,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_UACC_Id
ON "UACC" ("Id");
CREATE INDEX IF NOT EXISTS IX_UACC_TenantId
ON "UACC" ("TenantId");
CREATE INDEX IF NOT EXISTS IX_UACC_ActionId
ON "UACC" ("ActionId");
CREATE INDEX IF NOT EXISTS IX_UACC_UserId
ON "UACC" ("UserId");
---------------------

---------------------
INSERT INTO "Role" ("Title") 
SELECT 'admin'
WHERE NOT EXISTS (SELECT 1 FROM "Role" WHERE "Title" = 'admin');

INSERT INTO "Role" ("Title") 
SELECT 'none'
WHERE NOT EXISTS (SELECT 1 FROM "Role" WHERE "Title" = 'none');

INSERT INTO "User" ("Username", "Password", "RefrenceId", "Token") 
SELECT 'admin', 'admin', 'admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'
WHERE NOT EXISTS (SELECT 1 FROM "User" WHERE "Username" = 'admin');

INSERT INTO "Tenant" ("Title","AdminRoles") 
SELECT 'default', (SELECT "Role"."Id" FROM "Role" WHERE "Title" = 'admin')
WHERE NOT EXISTS (SELECT 1 FROM "Tenant" WHERE "Title" = 'default');

INSERT INTO "UserRole" ("UserId","RoleId","TenantId") 
SELECT (SELECT "User"."Id" FROM "User" WHERE "Username" = 'admin'), (SELECT "Role"."Id" FROM "Role" WHERE "Title" = 'admin'), (SELECT "Tenant"."Id" FROM "Tenant" WHERE "Title" = 'default')
WHERE NOT EXISTS (SELECT 1 FROM "UserRole" WHERE "RoleId" = (SELECT "Role"."Id" FROM "Role" WHERE "Title" = 'admin') AND "UserId" = (SELECT "User"."Id" FROM "User" WHERE "Username" = 'admin') AND "TenantId" = (SELECT "Tenant"."Id" FROM "Tenant" WHERE "Title" = 'default'));


INSERT INTO "Service" ("Title") 
SELECT 'Identity'
WHERE NOT EXISTS (SELECT 1 FROM "Service" WHERE "Title" = 'Identity');

INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'Service'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'Service');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service'), 'List', '/api/v1/Service', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service') AND "Title" = 'List');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service'), 'Create', '/api/v1/Service', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service') AND "Title" = 'Create');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service'), 'Edit', '/api/v1/Service', 'PUT'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service') AND "Title" = 'Edit');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service'), 'Delete', '/api/v1/Service', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service') AND "Title" = 'Delete');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service'), 'UnMock', '/api/v1/Service/unmock', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Service') AND "Title" = 'UnMock');


INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'ActionGroup'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'ActionGroup');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup'), 'List', '/api/v1/ActionGroup', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup') AND "Title" = 'List');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup'), 'Create', '/api/v1/ActionGroup', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup') AND "Title" = 'Create');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup'), 'Edit', '/api/v1/ActionGroup', 'PUT'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup') AND "Title" = 'Edit');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup'), 'Delete', '/api/v1/ActionGroup', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup') AND "Title" = 'Delete');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup'), 'UnMock', '/api/v1/ActionGroup/unmock', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'ActionGroup') AND "Title" = 'UnMock');


INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'Action'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'Action');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action'), 'List', '/api/v1/Action', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action') AND "Title" = 'List');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action'), 'Create', '/api/v1/Action', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action') AND "Title" = 'Create');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action'), 'Edit', '/api/v1/Action', 'PUT'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action') AND "Title" = 'Edit');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action'), 'Delete', '/api/v1/Action', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action') AND "Title" = 'Delete');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action'), 'UnMock', '/api/v1/Action/unmock', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Action') AND "Title" = 'UnMock');


INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'Domain'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'Domain');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain'), 'List', '/api/v1/Domain', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain') AND "Title" = 'List');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain'), 'Create', '/api/v1/Domain', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain') AND "Title" = 'Create');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain'), 'Edit', '/api/v1/Domain', 'PUT'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain') AND "Title" = 'Edit');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain'), 'Delete', '/api/v1/Domain', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain') AND "Title" = 'Delete');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain'), 'UnMock', '/api/v1/Domain/unmock', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Domain') AND "Title" = 'UnMock');


INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'DomainValue'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'DomainValue');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue'), 'List', '/api/v1/DomainValue', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue') AND "Title" = 'List');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue'), 'Create', '/api/v1/DomainValue', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue') AND "Title" = 'Create');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue'), 'Edit', '/api/v1/DomainValue', 'PUT'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue') AND "Title" = 'Edit');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue'), 'Delete', '/api/v1/DomainValue', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue') AND "Title" = 'Delete');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue'), 'UnMock', '/api/v1/DomainValue/unmock', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'DomainValue') AND "Title" = 'UnMock');


INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'Role'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'Role');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role'), 'List', '/api/v1/Role', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role') AND "Title" = 'List');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role'), 'Create', '/api/v1/Role', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role') AND "Title" = 'Create');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role'), 'Edit', '/api/v1/Role', 'PUT'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role') AND "Title" = 'Edit');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role'), 'Delete', '/api/v1/Role', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role') AND "Title" = 'Delete');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role'), 'UnMock', '/api/v1/Role/unmock', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Role') AND "Title" = 'UnMock');


INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'User'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'User');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User'), 'List', '/api/v1/User', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User') AND "Title" = 'List');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User'), 'Create', '/api/v1/User', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User') AND "Title" = 'Create');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User'), 'Edit', '/api/v1/User', 'PUT'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User') AND "Title" = 'Edit');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User'), 'Delete', '/api/v1/User', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User') AND "Title" = 'Delete');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User'), 'UnMock', '/api/v1/User/unmock', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'User') AND "Title" = 'UnMock');


INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'Tenant'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'Tenant');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant'), 'List', '/api/v1/Tenant', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant') AND "Title" = 'List');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant'), 'Create', '/api/v1/Tenant', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant') AND "Title" = 'Create');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant'), 'Edit', '/api/v1/Tenant', 'PUT'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant') AND "Title" = 'Edit');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant'), 'Delete', '/api/v1/Tenant', 'DELETE'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant') AND "Title" = 'Delete');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant'), 'UnMock', '/api/v1/Tenant/unmock', 'GET'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'Tenant') AND "Title" = 'UnMock');


INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'RACC'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'RACC');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'RACC'), 'Update', '/api/v1/RACC/update', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'RACC') AND "Title" = 'Update');


INSERT INTO "ActionGroup" ("ServiceId","Title") 
SELECT (SELECT "Service"."Id" FROM "Service" WHERE "Title" = 'Identity'), 'UACC'
WHERE NOT EXISTS (SELECT 1 FROM "ActionGroup" WHERE "Title" = 'UACC');

INSERT INTO "Action" ("ActionGroupId","Title","URL","Type") 
SELECT (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'UACC'), 'Update', '/api/v1/UACC/update', 'POST'
WHERE NOT EXISTS (SELECT 1 FROM "Action" WHERE "ActionGroupId" = (SELECT "ActionGroup"."Id" FROM "ActionGroup" WHERE "Title" = 'UACC') AND "Title" = 'Update');


DO $$
DECLARE
    role_id int;
	tenant_id int;
    action_row RECORD;
BEGIN
    SELECT "Id" INTO role_id FROM "Role" WHERE "Title" = 'admin';
    SELECT "Id" INTO tenant_id FROM "Tenant" WHERE "Title" = 'default';
    FOR action_row IN SELECT "Id" FROM "Action" LOOP
        INSERT INTO "RACC" ("RoleId", "ActionId", "TenantId", "type")
        VALUES (role_id, action_row."Id", tenant_id, 1)
        ON CONFLICT DO NOTHING;
    END LOOP;
END $$;

CREATE DATABASE "Identity_Test" WITH TEMPLATE "Identity" OWNER admin;



