SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;


COMMENT ON SCHEMA "public" IS 'standard public schema';
CREATE EXTENSION IF NOT EXISTS "pg_graphql" WITH SCHEMA "graphql";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements" WITH SCHEMA "extensions";
CREATE EXTENSION IF NOT EXISTS "pgcrypto" WITH SCHEMA "extensions";
CREATE EXTENSION IF NOT EXISTS "supabase_vault" WITH SCHEMA "vault";
CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA "extensions";

CREATE TYPE "public"."experiance_types" AS ENUM (
    'IMAX',
    'GOLD',
    'ATMOS',
    'Digital'
);


ALTER TYPE "public"."experiance_types" OWNER TO "postgres";


CREATE TYPE "public"."locations" AS ENUM (
    'HCM',
    'CCC',
    'LIBERTY'
);


ALTER TYPE "public"."locations" OWNER TO "postgres";


CREATE TYPE "public"."user_type" AS ENUM (
    'ADMIN',
    'USER'
);


ALTER TYPE "public"."user_type" OWNER TO "postgres";

SET default_tablespace = '';

SET default_table_access_method = "heap";


CREATE TABLE IF NOT EXISTS "public"."registered_alerts_by_id" (
    "id" bigint NOT NULL,
    "created_at" timestamp with time zone DEFAULT "now"() NOT NULL,
    "active" boolean DEFAULT true NOT NULL,
    "email" "text" NOT NULL,
    "movie_id" "text" NOT NULL,
    "location" "public"."locations" NOT NULL,
    "experiance" "public"."experiance_types"[] NOT NULL,
    "date" "date" NOT NULL
);


ALTER TABLE "public"."registered_alerts_by_id" OWNER TO "postgres";


ALTER TABLE "public"."registered_alerts_by_id" ALTER COLUMN "id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "public"."registered_alerts_by_id_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


CREATE TABLE IF NOT EXISTS "public"."registered_alerts_by_name" (
    "id" bigint NOT NULL,
    "created_at" timestamp with time zone DEFAULT "now"() NOT NULL,
    "active" boolean DEFAULT true NOT NULL,
    "email" "text" NOT NULL,
    "movie_name" "text" NOT NULL,
    "location" "public"."locations" NOT NULL,
    "experiance" "public"."experiance_types"[] NOT NULL,
    "date" "date" NOT NULL
);


ALTER TABLE "public"."registered_alerts_by_name" OWNER TO "postgres";


ALTER TABLE "public"."registered_alerts_by_name" ALTER COLUMN "id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "public"."registered_alerts_by_name_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE ONLY "public"."registered_alerts_by_id"
    ADD CONSTRAINT "registered_alerts_by_id_pkey" PRIMARY KEY ("id");

ALTER TABLE ONLY "public"."registered_alerts_by_name"
    ADD CONSTRAINT "registered_alerts_by_name_pkey" PRIMARY KEY ("id");

ALTER TABLE "public"."registered_alerts_by_id" ENABLE ROW LEVEL SECURITY;

ALTER TABLE "public"."registered_alerts_by_name" ENABLE ROW LEVEL SECURITY;

ALTER PUBLICATION "supabase_realtime" OWNER TO "postgres";

GRANT USAGE ON SCHEMA "public" TO "postgres";
GRANT USAGE ON SCHEMA "public" TO "anon";
GRANT USAGE ON SCHEMA "public" TO "authenticated";
GRANT USAGE ON SCHEMA "public" TO "service_role";

GRANT ALL ON TABLE "public"."registered_alerts_by_id" TO "anon";
GRANT ALL ON TABLE "public"."registered_alerts_by_id" TO "authenticated";
GRANT ALL ON TABLE "public"."registered_alerts_by_id" TO "service_role";

GRANT ALL ON SEQUENCE "public"."registered_alerts_by_id_id_seq" TO "anon";
GRANT ALL ON SEQUENCE "public"."registered_alerts_by_id_id_seq" TO "authenticated";
GRANT ALL ON SEQUENCE "public"."registered_alerts_by_id_id_seq" TO "service_role";

GRANT ALL ON TABLE "public"."registered_alerts_by_name" TO "anon";
GRANT ALL ON TABLE "public"."registered_alerts_by_name" TO "authenticated";
GRANT ALL ON TABLE "public"."registered_alerts_by_name" TO "service_role";

GRANT ALL ON SEQUENCE "public"."registered_alerts_by_name_id_seq" TO "anon";
GRANT ALL ON SEQUENCE "public"."registered_alerts_by_name_id_seq" TO "authenticated";
GRANT ALL ON SEQUENCE "public"."registered_alerts_by_name_id_seq" TO "service_role";

ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON SEQUENCES  TO "postgres";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON SEQUENCES  TO "anon";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON SEQUENCES  TO "authenticated";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON SEQUENCES  TO "service_role";

ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON FUNCTIONS  TO "postgres";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON FUNCTIONS  TO "anon";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON FUNCTIONS  TO "authenticated";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON FUNCTIONS  TO "service_role";

ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON TABLES  TO "postgres";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON TABLES  TO "anon";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON TABLES  TO "authenticated";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON TABLES  TO "service_role";

RESET ALL;