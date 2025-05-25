# MovieTicketsNotifierV3

This project is for a ticket notifier app for [Scope Cinemas](http://scopecinemas.com/).

- Users can register for notifications when tickets go live for movies they select. 
- Can specify which 
  - date
  - location(HCM, CCC, etc)
  - which experiances to notify (IMAX, ATMOS, etc.).

- A scheduled task (azure function app) will check every 15min and send out emails so you'd never miss the good seats :)

## Project Structure

- MovieTicketsNotifierV3FuncApp - contains the Azure fucntions backend code
- db - contains the code for setting up the supabase db
- app (TODO)- contains frontend app to register for notifications

## Setting up Supabase

1. Create a Supabase project at https://supabase.com
2. Run the SQL commands from `db/supabase/Database.sql` in the Supabase SQL editor
3. Get your project URL and anon key from the Supabase dashboard
4. Update your `local.settings.json` file with the following keys:
   ```json
   {
     "Supabase": {
       "Url": "https://your-project-id.supabase.co",
       "Key": "your-supabase-anon-key"
     }
   }
   ```

## Database Structure

The application uses two main tables in Supabase:

1. `registered_alerts_by_name` - Stores alerts for movie names
2. `registered_alerts_by_id` - Stores alerts for specific movie IDs

See the full database schema in `db/supabase/Database.sql`

## Scope API credentials

This is not a offcial integration. 

_But a curious developer might find decompiling the official android app is full of insights._