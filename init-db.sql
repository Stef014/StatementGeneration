-- Create databases
CREATE DATABASE accounts_statements;
CREATE DATABASE hangfire;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE accounts_statements TO postgres;
GRANT ALL PRIVILEGES ON DATABASE hangfire TO postgres;
