CREATE TABLE IF NOT EXISTS "Subscription"(
    "Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
    "TenantId" int NOT NULL,
    "ReminderLimit" int DEFAULT 3,
    "Title" TEXT NOT NULL,
	"Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_Subscription_Id
ON "Subscription" ("Id");
----------------------------
CREATE TABLE IF NOT EXISTS "UserSubscription"(
    "Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
    "UserId" int NOT NULL,
    "SubscriptionId" int NOT NULL,
    CONSTRAINT FK_UserSubscription_Subscription_SubscriptionId FOREIGN KEY ("SubscriptionId")     
        REFERENCES "Subscription" ("Id") ON DELETE CASCADE,
    "Status" int DEFAULT 0,
    "Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_UserSubscription_Id
ON "UserSubscription" ("Id");
CREATE INDEX IF NOT EXISTS IX_UserSubscription_SubscriptionId
ON "UserSubscription" ("SubscriptionId");
----------------------------
CREATE TABLE IF NOT EXISTS "Reminder"(
    "Id" int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) NOT NULL PRIMARY KEY,
    "UserSubscriptionId" int NOT NULL,
    CONSTRAINT FK_Reminder_UserSubscription_UserSubscriptionId FOREIGN KEY ("UserSubscriptionId")     
        REFERENCES "UserSubscription" ("Id") ON DELETE CASCADE,
    "Date" TIMESTAMP NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT,
    "Status" int DEFAULT 0,
    "Delete" BOOLEAN DEFAULT FALSE,
	"CreateDate" TIMESTAMP DEFAULT NOW(),
	"LastUpdateDate" TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS IX_Reminder_Id
ON "Reminder" ("Id");
CREATE INDEX IF NOT EXISTS IX_Reminder_UserSubscriptionId
ON "Reminder" ("UserSubscriptionId");