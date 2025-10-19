-- Challenger API - PostgreSQL DDL
-- Gera a modelagem conforme as entidades e configurações do SystemDbContext
-- Atenção: este script assume tipos e nomes inferidos do código. Ajuste conforme necessidades de produção.

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tabela Users
CREATE TABLE IF NOT EXISTS public."Users" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" text NOT NULL,
    "Email" text NOT NULL,
    "Password" text NOT NULL,
    "Role" int NOT NULL
);

-- Tabela Motorcycles
CREATE TABLE IF NOT EXISTS public."Motorcycles" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Identifier" text NOT NULL,
    "Year" int NOT NULL,
    "Model" text NOT NULL,
    "Plate" text NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Motorcycles_Plate ON public."Motorcycles" ("Plate");

-- Tabela DeliverymanProfiles
CREATE TABLE IF NOT EXISTS public."DeliverymanProfiles" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" uuid NOT NULL,
    "CnhNumber" text NOT NULL,
    "CnhType" text NOT NULL,
    "CnhImagePath" text NULL,
    "Cnpj" text NOT NULL,
    "BirthDate" timestamp with time zone NOT NULL,
    CONSTRAINT FK_DeliverymanProfiles_Users FOREIGN KEY ("UserId") REFERENCES public."Users" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_DeliverymanProfiles_CnhNumber ON public."DeliverymanProfiles" ("CnhNumber");
CREATE UNIQUE INDEX IF NOT EXISTS IX_DeliverymanProfiles_Cnpj ON public."DeliverymanProfiles" ("Cnpj");

-- Tabela Rentals (com constraint de StartDate = CreatedAt (dia) + 1 dia)
CREATE TABLE IF NOT EXISTS public."Rentals" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" uuid NOT NULL,
    "MotorcycleId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "StartDate" date NOT NULL,
    "ExpectedEndDate" date NOT NULL,
    "ActualEndDate" date NULL,
    "PlanDays" int NOT NULL,
    "DailyRate" numeric(10,2) NOT NULL,
    "TotalAmount" numeric(10,2) NOT NULL,
    "PenaltyFee" numeric(10,2) NULL,
    "ExtraFee" numeric(10,2) NULL,
    "FinalAmount" numeric(10,2) NOT NULL,
    "Status" int NOT NULL,
    CONSTRAINT FK_Rentals_Users FOREIGN KEY ("UserId") REFERENCES public."Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT FK_Rentals_Motorcycles FOREIGN KEY ("MotorcycleId") REFERENCES public."Motorcycles" ("Id") ON DELETE RESTRICT,
    CONSTRAINT CK_Rentals_StartDate_NextDay CHECK ("StartDate" = date_trunc('day', "CreatedAt") + interval '1 day')
);

-- Tabela Notifications
CREATE TABLE IF NOT EXISTS public."Notifications" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Message" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "MotorcycleId" uuid NOT NULL,
    CONSTRAINT FK_Notifications_Motorcycles FOREIGN KEY ("MotorcycleId") REFERENCES public."Motorcycles" ("Id") ON DELETE CASCADE
);

-- Tabela highlighted_motorcycles (snake_case conforme DbContext)
CREATE TABLE IF NOT EXISTS public.highlighted_motorcycles (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "MotorcycleId" uuid NOT NULL,
    "Plate" text NOT NULL,
    "Year" int NOT NULL,
    "Model" text NOT NULL,
    "HighlightedAt" timestamp with time zone NOT NULL,
    CONSTRAINT FK_Highlighted_Motorcycle FOREIGN KEY ("MotorcycleId") REFERENCES public."Motorcycles" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_highlighted_motorcycles_MotorcycleId ON public.highlighted_motorcycles ("MotorcycleId");
