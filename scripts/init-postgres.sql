-- Create database if it doesn't exist (handled by POSTGRES_DB env var)
-- Enable PostGIS extension
CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS postgis_topology;

-- Enable fuzzy string matching extension
CREATE EXTENSION IF NOT EXISTS fuzzystrmatch;
