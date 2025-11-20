# CPR Docker Services

This directory contains Docker Compose configuration for running CPR's development infrastructure.

## Services

### PostgreSQL Databases

**Development Database** (`db_dev`)
- Port: `5432`
- Container: `cpr_postgres_dev`
- Database: `cpr_dev`
- Username: `postgres`
- Password: `postgres`

**Test Database** (`db_test`)
- Port: `5433`
- Container: `cpr_postgres_test`
- Database: `cpr_test`
- Username: `postgres`
- Password: `postgres`

### SMTP Server for Email Testing

**smtp4dev** (`smtp4dev`)
- Web UI: http://localhost:3333
- SMTP Port: `1025`
- Container: `cpr_smtp4dev`

smtp4dev is a fake SMTP server for development and testing. It captures all emails sent from the application and displays them in a web interface.

#### Features:
- View sent emails in real-time
- Inspect HTML and plain text versions
- Download email attachments (like .ics calendar files)
- Test email sending without actually sending emails

## Quick Start

### Start All Services
```bash
docker-compose up -d
```

### Start Individual Services
```bash
# Development database only
docker-compose up -d db_dev

# Test database only
docker-compose up -d db_test

# SMTP server only
docker-compose up -d smtp4dev
```

### Stop All Services
```bash
docker-compose down
```

### Stop and Remove Volumes
```bash
docker-compose down -v
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f smtp4dev
```

## Accessing Services

- **Development Database**: `localhost:5432`
- **Test Database**: `localhost:5433`
- **smtp4dev Web UI**: http://localhost:3333
- **SMTP Server**: `localhost:1025`

## Email Testing Workflow

1. Start smtp4dev:
   ```bash
   docker-compose up -d smtp4dev
   ```

2. Open smtp4dev web UI: http://localhost:3333

3. Run your application (CPR API)

4. Trigger email notifications:
   - Create a new feedback request
   - Send a manual reminder
   - Wait for automatic reminders (or trigger Hangfire job manually)

5. View emails in smtp4dev:
   - All emails appear in real-time
   - Click to view HTML/text versions
   - Download .ics calendar attachments
   - Check email headers and metadata

## Configuration

The API is configured to use these services in `appsettings.Development.json`:

**Database:**
```json
"Database": {
  "Host": "localhost",
  "Port": "5432",
  "Name": "cpr_dev",
  "Username": "postgres",
  "Password": "postgres"
}
```

**Email:**
```json
"Email": {
  "SmtpHost": "localhost",
  "SmtpPort": "1025",
  "SmtpUsername": "",
  "SmtpPassword": "",
  "FromAddress": "noreply@cpr.local",
  "FromName": "CPR - Career Progress Registry"
}
```

## Troubleshooting

### Port Conflicts

If you get port conflict errors:

```bash
# Check what's using the port
netstat -ano | findstr :5432
netstat -ano | findstr :3333
netstat -ano | findstr :1025

# Stop the service using the port or change the port in docker-compose.yml
```

### Container Won't Start

```bash
# View detailed logs
docker-compose logs smtp4dev

# Restart the container
docker-compose restart smtp4dev

# Rebuild and restart
docker-compose up -d --force-recreate smtp4dev
```

### Reset Everything

```bash
# Stop all containers and remove volumes
docker-compose down -v

# Start fresh
docker-compose up -d
```

## Production Notes

⚠️ **smtp4dev is for development only!**

For production, configure a real SMTP server in `appsettings.Production.json`:
- Gmail SMTP
- SendGrid
- AWS SES
- Azure Communication Services
- Mailgun
- Or your organization's SMTP server
