# MovieTicketsNotifierV3
This project is for a ticket notifier app for a movie theater.

## Project Structure

- MovieTicketsNotifierV3FuncApp - contains the Azure fucntions backend code
- db - contains the code for setting up the supabase db
- app (TODO)- contains the android app that uses the function app as the backend

## Setting up Supabase

1. Create a Supabase project at https://supabase.com
2. Run the SQL commands from `db/supabase/Database.sql` in the Supabase SQL editor
3. Get your project URL and anon key from the Supabase dashboard
4. Update your `local.settings.json` file with the following keys:
   ```json
   {
     "Values": {
       "Supabase:Url": "https://your-project-id.supabase.co",
       "Supabase:Key": "your-supabase-anon-key"
     }
   }
   ```

## Database Structure

The application uses two main tables in Supabase:

1. `registered_alerts_by_name` - Stores alerts for movie names
2. `registered_alerts_by_id` - Stores alerts for specific movie IDs

See the full database schema in `db/supabase/Database.sql`