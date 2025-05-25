# Supabase Database

This directory contains the database schema and setup instructions for the Supabase database used in the MovieTicketsNotifierV3 project.

## Database Schema

The database schema is defined in `supabase/Database.sql`. It includes:

1. Two main tables:
   - `registered_alerts_by_name`: For tracking alerts by movie name
   - `registered_alerts_by_id`: For tracking alerts by movie ID
   
2. Custom types:
   - `experiance_types`: IMAX, GOLD, ATMOS, Digital
   - `locations`: HCM, CCC, LIBERTY
   - `user_type`: ADMIN, USER

## Setting up the database

1. Create a new Supabase project from the [Supabase Dashboard](https://app.supabase.com/)
2. Go to the SQL Editor in your Supabase project
3. Paste the contents of `Database.sql` and run the script
4. Verify that the tables and types have been created correctly

## Connecting to the database

To connect to the Supabase database in the .NET app:

1. Get your Supabase URL and anon key from the Supabase dashboard
2. Add them to your configuration in `local.settings.json`:
   ```json
   {
     "Values": {
       "Supabase:Url": "https://your-project-id.supabase.co",
       "Supabase:Key": "your-supabase-anon-key"
     }
   }
   ```