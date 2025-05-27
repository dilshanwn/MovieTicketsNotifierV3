-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create movies table
CREATE TABLE movies (
    id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    poster_url TEXT,
    genre TEXT,
    runtime INTEGER,
    imdb_rating DECIMAL(3,1),
    synopsis TEXT,
    cast JSONB,
    trailer_url TEXT,
    release_date DATE,
    status VARCHAR(20) CHECK (status IN ('now_showing', 'upcoming')) NOT NULL DEFAULT 'upcoming',
    scope_movie_id VARCHAR(100), -- For mapping to Scope Cinema's movie ID
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create registered_alerts_by_name table
CREATE TABLE registered_alerts_by_name (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    movie_name VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(email, movie_name)
);

-- Create registered_alerts_by_id table
CREATE TABLE registered_alerts_by_id (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    movie_id INTEGER NOT NULL REFERENCES movies(id) ON DELETE CASCADE,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(email, movie_id)
);

-- Create notification_logs table for tracking sent notifications
CREATE TABLE notification_logs (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    movie_id INTEGER REFERENCES movies(id) ON DELETE SET NULL,
    movie_name VARCHAR(255),
    notification_type VARCHAR(50) NOT NULL, -- 'ticket_available', 'reminder', etc.
    sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    status VARCHAR(20) NOT NULL DEFAULT 'sent' -- 'sent', 'failed', 'pending'
);

-- Create indexes for better performance
CREATE INDEX idx_movies_status ON movies(status);
CREATE INDEX idx_movies_release_date ON movies(release_date);
CREATE INDEX idx_alerts_by_name_email ON registered_alerts_by_name(email);
CREATE INDEX idx_alerts_by_name_active ON registered_alerts_by_name(is_active);
CREATE INDEX idx_alerts_by_id_email ON registered_alerts_by_id(email);
CREATE INDEX idx_alerts_by_id_active ON registered_alerts_by_id(is_active);
CREATE INDEX idx_notification_logs_email ON notification_logs(email);
CREATE INDEX idx_notification_logs_sent_at ON notification_logs(sent_at);

-- Enable Row Level Security (RLS)
ALTER TABLE registered_alerts_by_name ENABLE ROW LEVEL SECURITY;
ALTER TABLE registered_alerts_by_id ENABLE ROW LEVEL SECURITY;
ALTER TABLE notification_logs ENABLE ROW LEVEL SECURITY;

-- Create RLS policies for data isolation by email
CREATE POLICY "Users can only see their own alerts by name" ON registered_alerts_by_name
    FOR ALL USING (auth.jwt() ->> 'email' = email);

CREATE POLICY "Users can only see their own alerts by id" ON registered_alerts_by_id
    FOR ALL USING (auth.jwt() ->> 'email' = email);

CREATE POLICY "Users can only see their own notification logs" ON notification_logs
    FOR ALL USING (auth.jwt() ->> 'email' = email);

-- Movies table should be readable by all authenticated users
ALTER TABLE movies ENABLE ROW LEVEL SECURITY;
CREATE POLICY "Movies are viewable by all authenticated users" ON movies
    FOR SELECT USING (auth.role() = 'authenticated');

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers for updated_at
CREATE TRIGGER update_movies_updated_at BEFORE UPDATE ON movies
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_alerts_by_name_updated_at BEFORE UPDATE ON registered_alerts_by_name
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_alerts_by_id_updated_at BEFORE UPDATE ON registered_alerts_by_id
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
